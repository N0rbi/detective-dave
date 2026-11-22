using System;
using UnityEngine;

namespace DetectiveDave.MusicPlay
{
    [Serializable]
    public sealed class ChordChartFile
    {
        public float tempoBpm = 75f;
        public int beatsPerBar = 4;
        public int loopLengthBars = 8;
        public float chartBeatAtLoopStart;
        public ChordSegment[] segments = new ChordSegment[0];
        public ClipPitchEntry[] clips = new ClipPitchEntry[0];

        public static ChordChartFile Parse(string json) => JsonUtility.FromJson<ChordChartFile>(json);

        [Serializable]
        public sealed class ClipPitchEntry
        {
            public string clipName;
            public string label;
            public int[] pitchClasses = new int[0];
        }
    }
}
