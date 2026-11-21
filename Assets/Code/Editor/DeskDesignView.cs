using System.Linq;
using DetectiveDave.KeyboardInput;
using DetectiveDave.OverworldPlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    [InitializeOnLoad]
    static class DeskDesignView
    {
        const float SurfaceMargin = 1f;
        const float SurfaceDepth = 0.5f;
        const string SpriteShader = "Sprites/Default";

        static readonly Color KeyOutline = new Color(1f, 1f, 1f, 0.55f);
        static readonly Color StartColor = new Color(0.25f, 0.55f, 1f, 1f);
        static readonly Color ProblemColor = new Color(1f, 0.2f, 0.2f, 1f);
        static readonly Color NoCaseColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        static Material surfaceMaterial;

        static DeskDesignView() => SceneView.duringSceneGui += OnSceneGui;

        static void OnSceneGui(SceneView sceneView)
        {
            if (Event.current.type != EventType.Repaint || Application.isPlaying) return;
            var desk = CurrentDesk();
            var layout = KeyboardLayouts.Preview;
            if (!desk || layout == null) return;
            var design = DeskDesignInputs.DesignOn(desk, layout);
            DrawSurface(desk, layout);
            DrawKeys(desk, layout, design);
            DrawStartKey(desk, layout, design);
            DrawMarkers(desk, design);
            DrawLayoutName(layout);
            DrawRootWarning(desk);
        }

        static DeskLayout CurrentDesk()
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage) return stage.prefabContentsRoot.GetComponent<DeskLayout>();
            var selected = Selection.activeGameObject;
            if (!selected || EditorUtility.IsPersistent(selected)) return null;
            return selected.GetComponentInParent<DeskLayout>();
        }

        static void DrawSurface(DeskLayout desk, KeyboardLayout layout)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<OverworldSettings>(OverworldSetupPaths.SettingsPath)?.DeskTopSprite;
            if (!sprite) return;
            var material = SurfaceMaterial();
            if (!material) return;
            material.mainTexture = sprite.texture;
            var bounds = BoardBounds(layout);
            var size = bounds.size + Vector2.one * (SurfaceMargin * 2f);
            var local = Matrix4x4.TRS(new Vector3(bounds.center.x, bounds.center.y, SurfaceDepth), Quaternion.identity, new Vector3(size.x, size.y, 1f));
            material.SetPass(0);
            Graphics.DrawMeshNow(Resources.GetBuiltinResource<Mesh>("Quad.fbx"), desk.transform.localToWorldMatrix * local);
        }

        static Material SurfaceMaterial()
        {
            if (surfaceMaterial) return surfaceMaterial;
            var shader = Shader.Find(SpriteShader);
            if (!shader) return null;
            surfaceMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            return surfaceMaterial;
        }

        static Rect BoardBounds(KeyboardLayout layout)
        {
            var rects = DeskDesignInputs.Rects(layout).Values.SelectMany(group => group).ToArray();
            if (rects.Length == 0) return new Rect(Vector2.zero, Vector2.one);
            var min = new Vector2(rects.Min(rect => rect.xMin), rects.Min(rect => rect.yMin));
            var max = new Vector2(rects.Max(rect => rect.xMax), rects.Max(rect => rect.yMax));
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        static void DrawKeys(DeskLayout desk, KeyboardLayout layout, DeskDesign design)
        {
            var filled = design.Fills.ToDictionary(fill => fill.Key);
            foreach (var entry in DeskDesignInputs.Rects(layout))
            {
                var fill = filled.TryGetValue(entry.Key, out var found) ? (Color?)(found.IsShared ? DeskCaseColors.SharedKey : DeskCaseColors.Fill(found.CaseId)) : null;
                foreach (var rect in entry.Value) DrawRect(desk, rect, fill ?? Color.clear, KeyOutline);
            }
        }

        static void DrawStartKey(DeskLayout desk, KeyboardLayout layout, DeskDesign design)
        {
            var key = layout.UniqueKeys().FirstOrDefault(def => def.InputKey == DeskDesignInputs.StartKey);
            if (key == null) return;
            bool covered = design.Issues.Any(issue => issue.Kind == DeskIssueKind.CoversStartKey);
            var colour = covered ? ProblemColor : StartColor;
            foreach (var rect in KeyGeometry.Rects(layout, key)) DrawRect(desk, rect, Color.clear, colour);
            Label(desk.transform.TransformPoint(KeyGeometry.Center(layout, key)), covered ? "START (covered by a case!)" : "START", colour);
        }

        static void DrawMarkers(DeskLayout desk, DeskDesign design)
        {
            foreach (var marker in DeskDesignInputs.AllMarkers(desk))
            {
                if (!marker.CanBeMapped)
                {
                    Label(marker.transform.position, "No case assigned", NoCaseColor);
                    continue;
                }
                var deskMarker = marker.ToDeskMarker(desk.transform);
                bool coversNoKey = design.Issues.Any(issue => issue.Kind == DeskIssueKind.CoversNoKey && issue.CaseId == deskMarker.CaseId);
                var colour = DeskCaseColors.Outline(deskMarker.CaseId);
                DrawOutline(desk, deskMarker, colour);
                string name = string.IsNullOrWhiteSpace(marker.CaseFile.ClientName) ? deskMarker.CaseId : marker.CaseFile.ClientName;
                var warnings = MarkerWarnings(marker, coversNoKey);
                Label(marker.transform.position, string.Join("\n", new[] { name }.Concat(warnings)), warnings.Length > 0 ? ProblemColor : colour);
            }
        }

        static string[] MarkerWarnings(CaseMarker marker, bool coversNoKey) =>
            new[] { coversNoKey ? "Covers no key on this keyboard!" : null, CaseChainInputs.HasNoLevel(marker) ? "No level yet!" : null }.Where(text => text != null).ToArray();

        static void DrawOutline(DeskLayout desk, DeskMarker marker, Color colour)
        {
            var half = marker.Size * 0.5f;
            var turn = Quaternion.Euler(0f, 0f, marker.RotationDegrees);
            var corners = new[] { new Vector2(-half.x, -half.y), new Vector2(half.x, -half.y), new Vector2(half.x, half.y), new Vector2(-half.x, half.y) }
                .Select(corner => desk.transform.TransformPoint(marker.Centre + (Vector2)(turn * corner)))
                .ToArray();
            Handles.color = colour;
            Handles.DrawAAPolyLine(3f, corners.Concat(new[] { corners[0] }).ToArray());
        }

        static void DrawRect(DeskLayout desk, Rect rect, Color fill, Color outline)
        {
            var corners = new[]
            {
                desk.transform.TransformPoint(new Vector3(rect.xMin, rect.yMin)), desk.transform.TransformPoint(new Vector3(rect.xMax, rect.yMin)),
                desk.transform.TransformPoint(new Vector3(rect.xMax, rect.yMax)), desk.transform.TransformPoint(new Vector3(rect.xMin, rect.yMax)),
            };
            Handles.DrawSolidRectangleWithOutline(corners, fill, outline);
        }

        static void Label(Vector3 position, string text, Color colour)
        {
            var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
            style.normal.textColor = colour;
            Handles.Label(position, text, style);
        }

        static void DrawRootWarning(DeskLayout desk)
        {
            if (!DeskRoot.NeedsReset(desk)) return;
            Handles.BeginGUI();
            var style = new GUIStyle(EditorStyles.boldLabel) { wordWrap = true };
            style.normal.textColor = ProblemColor;
            GUI.Label(new Rect(10f, 44f, 420f, 60f), DeskRootCheck.Warning + " Reset it in the desk inspector.", style);
            Handles.EndGUI();
        }

        static void DrawLayoutName(KeyboardLayout layout)
        {
            Handles.BeginGUI();
            GUI.Label(new Rect(10f, 24f, 420f, 20f), "Desk preview on keyboard: " + layout.Name + " (change it in the Levels window or on the desk)", EditorStyles.boldLabel);
            Handles.EndGUI();
        }
    }
}
