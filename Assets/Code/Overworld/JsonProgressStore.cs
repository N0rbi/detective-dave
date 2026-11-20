using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public sealed class JsonProgressStore : IProgressStore
    {
        const string FileName = "progress.json";

        readonly string path;

        public JsonProgressStore(string path)
        {
            this.path = path;
        }

        public static JsonProgressStore AtDefaultPath() => new JsonProgressStore(Path.Combine(Application.persistentDataPath, FileName));

        public IReadOnlyCollection<string> LoadSolved()
        {
            if (!File.Exists(path)) return new string[0];
            try
            {
                var data = JsonUtility.FromJson<ProgressData>(File.ReadAllText(path));
                return data?.solved ?? new List<string>();
            }
            catch (ArgumentException)
            {
                return new string[0];
            }
        }

        public void SaveSolved(IReadOnlyCollection<string> solvedIds)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(new ProgressData { solved = new List<string>(solvedIds) }));
        }

        [Serializable]
        sealed class ProgressData
        {
            public List<string> solved = new List<string>();
        }
    }
}
