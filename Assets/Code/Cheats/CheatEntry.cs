using System;
using UnityEngine;

namespace DetectiveDave.CheatPlay
{
    [Serializable]
    public sealed class CheatEntry
    {
        [SerializeField] [Tooltip("The word the player types. Letters only, any case.")] string code;
        [SerializeField] [Tooltip("What happens when the word is typed.")] CheatAction action;

        public CheatEntry(string code, CheatAction action)
        {
            this.code = code;
            this.action = action;
        }

        public string Code => code;

        public CheatAction Action => action;
    }
}
