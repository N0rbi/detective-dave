using System.Collections.Generic;
using System.Linq;
using DetectiveDave.KeyboardInput;
using DetectiveDave.OverworldPlay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.EditorTools
{
    static class DeskDesignInputs
    {
        public const Key StartKey = Key.LeftShift;

        public static CaseMarker[] AllMarkers(DeskLayout desk) => desk.GetComponentsInChildren<CaseMarker>(true);

        public static DeskMarker[] MappedMarkers(DeskLayout desk) =>
            AllMarkers(desk).Where(marker => marker.CanBeMapped).Select(marker => marker.ToDeskMarker(desk.transform)).ToArray();

        public static Dictionary<Key, Vector2> Centres(KeyboardLayout layout) =>
            layout.UniqueKeys().ToDictionary(def => def.InputKey.Value, def => KeyGeometry.Center(layout, def));

        public static Dictionary<Key, Rect[]> Rects(KeyboardLayout layout) =>
            layout.UniqueKeys().ToDictionary(def => def.InputKey.Value, def => KeyGeometry.Rects(layout, def));

        public static IEnumerable<LayoutKeyCentres> EveryLayout() =>
            KeyboardLayouts.All.Select(layout => new LayoutKeyCentres(layout.Name, Centres(layout)));

        public static DeskDesign DesignOn(DeskLayout desk, KeyboardLayout layout) =>
            new DeskDesign(MappedMarkers(desk), Centres(layout), Rects(layout), StartKey);
    }
}
