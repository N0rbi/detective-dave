using System.Collections.Generic;
using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public sealed class DeskLayout : MonoBehaviour
    {
        CaseMarker[] markers;

        public IReadOnlyList<CaseMarker> Markers => markers ?? (markers = GetComponentsInChildren<CaseMarker>(true));
    }
}
