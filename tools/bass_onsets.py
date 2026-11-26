import json
import math
import os
import struct
import sys
import wave

import numpy as np

HOP_SECONDS = 0.005
WINDOW_SECONDS = 0.02
MIN_GAP_SECONDS = 0.08
LOOK_BACK_SECONDS = 0.06
RISE_DB = 6.0
FLOOR_DB_BELOW_PEAK = 40.0
CHORD_TEMPO_BPM = 75.0
BEATS_PER_BAR = 4
LOOP_BARS = 8


def read_extended(raw):
    exponent = ((raw[0] & 0x7F) << 8) | raw[1]
    mantissa = int.from_bytes(raw[2:10], "big")
    if exponent == 0 and mantissa == 0:
        return 0.0
    sign = -1.0 if raw[0] & 0x80 else 1.0
    return sign * mantissa * 2.0 ** (exponent - 16383 - 63)


def decode_pcm(data, bits, channels, big_endian):
    width = bits // 8
    count = len(data) // (width * channels)
    data = data[: count * width * channels]
    if width == 2:
        samples = np.frombuffer(data, dtype=">i2" if big_endian else "<i2").astype(np.float64) / 32768.0
    elif width == 3:
        b = np.frombuffer(data, dtype=np.uint8).reshape(-1, 3).astype(np.int64)
        value = (b[:, 0] << 16 | b[:, 1] << 8 | b[:, 2]) if big_endian else (b[:, 2] << 16 | b[:, 1] << 8 | b[:, 0])
        value = np.where(value >= 1 << 23, value - (1 << 24), value)
        samples = value.astype(np.float64) / 8388608.0
    elif width == 4:
        samples = np.frombuffer(data, dtype=">i4" if big_endian else "<i4").astype(np.float64) / 2147483648.0
    else:
        raise ValueError("unsupported bit depth %d" % bits)
    return samples.reshape(-1, channels)


def read_aiff(path):
    with open(path, "rb") as handle:
        blob = handle.read()
    if blob[0:4] != b"FORM" or blob[8:12] not in (b"AIFF", b"AIFC"):
        raise ValueError("not an AIFF file")
    position = 12
    rate = bits = channels = None
    sound = None
    while position + 8 <= len(blob):
        name = blob[position:position + 4]
        size = struct.unpack(">I", blob[position + 4:position + 8])[0]
        body = blob[position + 8:position + 8 + size]
        if name == b"COMM":
            channels = struct.unpack(">h", body[0:2])[0]
            bits = struct.unpack(">h", body[6:8])[0]
            rate = read_extended(body[8:18])
        elif name == b"SSND":
            offset = struct.unpack(">I", body[0:4])[0]
            sound = body[8 + offset:]
        position += 8 + size + (size & 1)
    if rate is None or sound is None:
        raise ValueError("AIFF is missing COMM or SSND")
    return decode_pcm(sound, bits, channels, True), int(round(rate))


def read_wav(path):
    with wave.open(path, "rb") as handle:
        channels = handle.getnchannels()
        bits = handle.getsampwidth() * 8
        rate = handle.getframerate()
        data = handle.readframes(handle.getnframes())
    return decode_pcm(data, bits, channels, False), rate


def read_audio(path):
    extension = os.path.splitext(path)[1].lower()
    if extension in (".aif", ".aiff"):
        return read_aiff(path)
    if extension == ".wav":
        return read_wav(path)
    raise ValueError("only .aif, .aiff and .wav are supported")


def envelope_db(mono, rate):
    kernel = max(1, int(rate * 0.05))
    slow = np.convolve(mono, np.ones(kernel) / kernel)[: len(mono)]
    filtered = mono - slow
    hop = max(1, int(rate * HOP_SECONDS))
    window = max(hop, int(rate * WINDOW_SECONDS))
    frames = (len(filtered) - window) // hop
    squares = filtered * filtered
    csum = np.concatenate(([0.0], np.cumsum(squares)))
    starts = np.arange(frames) * hop
    energy = (csum[starts + window] - csum[starts]) / window
    return 10.0 * np.log10(energy + 1e-12), hop / rate


def detect_onsets(mono, rate):
    db, hop_seconds = envelope_db(mono, rate)
    if len(db) == 0:
        return []
    floor = db.max() - FLOOR_DB_BELOW_PEAK
    back = max(1, int(LOOK_BACK_SECONDS / hop_seconds))
    gap = max(1, int(MIN_GAP_SECONDS / hop_seconds))
    onsets = []
    last = -gap
    for i in range(back, len(db) - 2):
        if db[i] < floor:
            continue
        reference = db[i - back:i - 1].min()
        rise = db[i] - reference
        if rise < RISE_DB:
            continue
        if i - last < gap:
            continue
        j = i
        while j + 1 < len(db) and db[j + 1] > db[j] and j - i < 6:
            j += 1
        onsets.append(i * hop_seconds + WINDOW_SECONDS / 2.0)
        last = j
    return onsets


def to_mono(samples):
    return samples.mean(axis=1)


def analyse(path):
    samples, rate = read_audio(path)
    mono = to_mono(samples)
    return {
        "durationSeconds": len(mono) / rate,
        "sampleRate": rate,
        "onsetsSeconds": [round(t, 4) for t in detect_onsets(mono, rate)],
    }


def report(result):
    duration = result["durationSeconds"]
    onsets = result["onsetsSeconds"]
    chord_loop = LOOP_BARS * BEATS_PER_BAR * 60.0 / CHORD_TEMPO_BPM
    bars_at_chart = duration * CHORD_TEMPO_BPM / 60.0 / BEATS_PER_BAR
    implied_bpm = LOOP_BARS * BEATS_PER_BAR * 60.0 / duration
    beat_seconds = 60.0 / CHORD_TEMPO_BPM
    print("duration %.4f s, sample rate %d" % (duration, result["sampleRate"]))
    print("chord chart loop %.4f s (%g BPM, %d bars)" % (chord_loop, CHORD_TEMPO_BPM, LOOP_BARS))
    print("audio is %.3f bars at chart tempo; if it is %d bars the tempo is %.3f BPM" % (bars_at_chart, LOOP_BARS, implied_bpm))
    print("onsets: %d" % len(onsets))
    print("seconds:", " ".join("%.3f" % t for t in onsets))
    print("beats at chart tempo:", " ".join("%.2f" % (t / beat_seconds) for t in onsets))
    print("onsets per bar: %.2f" % (len(onsets) / max(bars_at_chart, 1e-9)))


def main(argv):
    if len(argv) < 2:
        print("usage: bass_onsets.py <audio file> [output json]")
        return 1
    path = argv[1]
    result = analyse(path)
    report(result)
    if len(argv) > 2:
        result["sourceFile"] = os.path.basename(path)
        ordered = {"sourceFile": result["sourceFile"], "durationSeconds": result["durationSeconds"],
                   "sampleRate": result["sampleRate"], "onsetsSeconds": result["onsetsSeconds"]}
        with open(argv[2], "w") as handle:
            json.dump(ordered, handle, indent=2)
            handle.write("\n")
        print("wrote", argv[2])
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
