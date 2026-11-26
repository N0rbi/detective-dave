using System;
using UnityEngine;

namespace DetectiveDave.MusicPlay
{
    [Serializable]
    public sealed class BassOnsetFile
    {
        public string sourceFile;
        public float durationSeconds;
        public int sampleRate;
        public float[] onsetsSeconds = new float[0];

        public static BassOnsetFile Parse(string json)
        {
            BassOnsetFile file;
            try
            {
                file = JsonUtility.FromJson<BassOnsetFile>(json);
            }
            catch (ArgumentException exception)
            {
                throw new ArgumentException("The file is not valid JSON: " + exception.Message);
            }
            if (file == null || file.onsetsSeconds == null) throw new ArgumentException("The file has no onsetsSeconds list.");
            if (file.durationSeconds <= 0f) throw new ArgumentException("durationSeconds must be above 0.");
            return file;
        }
    }
}
