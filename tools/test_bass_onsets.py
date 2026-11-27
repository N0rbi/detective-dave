import os
import struct
import sys
import tempfile
import unittest
import wave

import numpy as np

sys.path.insert(0, os.path.dirname(__file__))
import bass_onsets

RATE = 44100


def tone(seconds_list, total, frequency=55.0):
    signal = np.zeros(int(total * RATE))
    for start in seconds_list:
        n = int(0.4 * RATE)
        t = np.arange(n) / RATE
        burst = np.sin(2 * np.pi * frequency * t) * np.exp(-t * 6.0)
        begin = int(start * RATE)
        signal[begin:begin + n] += burst[: len(signal) - begin]
    return signal * 0.5


def write_wav(path, signal):
    with wave.open(path, "wb") as handle:
        handle.setnchannels(1)
        handle.setsampwidth(2)
        handle.setframerate(RATE)
        handle.writeframes((signal * 32767).astype("<i2").tobytes())


def extended(rate):
    exponent = 16383 + int(np.floor(np.log2(rate)))
    mantissa = int(rate * 2 ** (63 - int(np.floor(np.log2(rate)))))
    return struct.pack(">HQ", exponent, mantissa)


def write_aiff(path, signal):
    pcm = (signal * 32767).astype(">i2").tobytes()
    comm = struct.pack(">hIh", 1, len(signal), 16) + extended(RATE)
    ssnd = struct.pack(">II", 0, 0) + pcm
    body = b"AIFF" + b"COMM" + struct.pack(">I", len(comm)) + comm + b"SSND" + struct.pack(">I", len(ssnd)) + ssnd
    with open(path, "wb") as handle:
        handle.write(b"FORM" + struct.pack(">I", len(body)) + body)


class BassOnsetTests(unittest.TestCase):
    starts = [0.5, 1.3, 2.1, 3.5]

    def check(self, found):
        self.assertEqual(len(found), len(self.starts))
        for expected, actual in zip(self.starts, found):
            self.assertAlmostEqual(expected, actual, delta=0.03)

    def test_wav(self):
        with tempfile.TemporaryDirectory() as folder:
            path = os.path.join(folder, "bass.wav")
            write_wav(path, tone(self.starts, 4.5))
            result = bass_onsets.analyse(path)
            self.check(result["onsetsSeconds"])
            self.assertAlmostEqual(result["durationSeconds"], 4.5, places=3)

    def test_aiff_reads_the_sample_rate_and_the_notes(self):
        with tempfile.TemporaryDirectory() as folder:
            path = os.path.join(folder, "bass.aif")
            write_aiff(path, tone(self.starts, 4.5))
            result = bass_onsets.analyse(path)
            self.assertEqual(result["sampleRate"], RATE)
            self.check(result["onsetsSeconds"])

    def test_silence_has_no_onsets(self):
        self.assertEqual(bass_onsets.detect_onsets(np.zeros(RATE * 2), RATE), [])

    def test_notes_closer_than_the_minimum_gap_count_once(self):
        found = bass_onsets.detect_onsets(tone([1.0, 1.04], 3.0), RATE)
        self.assertEqual(len(found), 1)


if __name__ == "__main__":
    unittest.main()
