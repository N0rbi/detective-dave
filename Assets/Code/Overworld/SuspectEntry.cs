using System;
using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    [Serializable]
    public struct SuspectEntry
    {
        [SerializeField] string key;
        [SerializeField] string value;

        public SuspectEntry(string key, string value)
        {
            this.key = key;
            this.value = value;
        }

        public string Key => key;
        public string Value => value;
    }
}
