using System.Linq;
using DetectiveDave.OverworldPlay;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    [InitializeOnLoad]
    static class DeskDropTarget
    {
        static DeskDropTarget() => SceneView.duringSceneGui += OnSceneGui;

        static void OnSceneGui(SceneView sceneView)
        {
            var current = Event.current;
            if (current.type != EventType.DragUpdated && current.type != EventType.DragPerform) return;
            var desk = DeskDesignView.CurrentDesk();
            var dropped = DragAndDrop.objectReferences.OfType<CaseFile>().Where(file => !IsOnDesk(desk, file)).ToArray();
            if (!desk || EditorUtility.IsPersistent(desk) || dropped.Length == 0) return;
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (current.type == EventType.DragPerform) Place(desk, dropped, current.mousePosition);
            current.Use();
        }

        static bool IsOnDesk(DeskLayout desk, CaseFile file) => desk && DeskDesignInputs.AllMarkers(desk).Any(marker => marker.CaseFile == file);

        static void Place(DeskLayout desk, CaseFile[] files, Vector2 mousePosition)
        {
            DragAndDrop.AcceptDrag();
            OverworldDeskBuilder.AddToDesk(desk, files[0], LocalPoint(desk, mousePosition));
            foreach (var file in files.Skip(1)) OverworldDeskBuilder.AddToDesk(desk, file);
        }

        static Vector2 LocalPoint(DeskLayout desk, Vector2 mousePosition)
        {
            var ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            var plane = new Plane(desk.transform.forward, desk.transform.position);
            var world = plane.Raycast(ray, out float distance) ? ray.GetPoint(distance) : ray.origin;
            return desk.transform.InverseTransformPoint(world);
        }
    }
}
