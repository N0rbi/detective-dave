using DetectiveDave.OverworldPlay;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    static class OverworldDeskBuilder
    {
        public const string NewDeskPath = OverworldSetupPaths.OverworldFolder + "/New Desk.prefab";

        public static CaseMarker AddMarker(Transform parent, CaseFile caseFile, Sprite art, Vector3 localPosition, float localScale)
        {
            var go = new GameObject(caseFile ? caseFile.name : "Case Marker");
            if (parent) go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = new Vector3(localScale, localScale, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = art;
            var marker = go.AddComponent<CaseMarker>();
            var serialized = new SerializedObject(marker);
            serialized.FindProperty("caseFile").objectReferenceValue = caseFile;
            serialized.FindProperty("art").objectReferenceValue = renderer;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return marker;
        }

        public static CaseMarker AddToDesk(DeskLayout desk, CaseFile caseFile, Vector2? localPosition = null)
        {
            var art = AssetDatabase.LoadAssetAtPath<Sprite>(OverworldSetupPaths.FolderPath);
            float scale = CentredScale(art);
            var position = localPosition ?? FreeSpot(desk, art, scale);
            var marker = AddMarker(desk.transform, caseFile, art, new Vector3(position.x, position.y, 0f), scale);
            Undo.RegisterCreatedObjectUndo(marker.gameObject, "Add Case to Desk");
            Selection.activeGameObject = marker.gameObject;
            return marker;
        }

        static Vector2 FreeSpot(DeskLayout desk, Sprite art, float scale)
        {
            var layout = KeyboardLayouts.Preview;
            if (layout == null) return Vector2.zero;
            var size = art ? (Vector2)art.bounds.size * scale : Vector2.one;
            return DeskFreeSpot.Find(DeskDesignInputs.MappedMarkers(desk), DeskDesignInputs.Centres(layout), DeskDesignInputs.StartKey, size);
        }

        public static GameObject SaveDesk(string path, CaseFile caseFile, Sprite art, float localScale)
        {
            var root = new GameObject(System.IO.Path.GetFileNameWithoutExtension(path));
            root.AddComponent<DeskLayout>();
            AddMarker(root.transform, caseFile, art, Vector3.zero, localScale);
            var saved = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return saved;
        }

        // One case file about two and a half keys wide in the middle of the keyboard (grid fitted into the scene's default 15 x 4 area).
        public static float CentredScale(Sprite art)
        {
            var layout = KeyboardLayouts.Preview;
            float keyScale = layout == null ? 1f : KeyboardGrid.GridUtils.FitScale(layout, new Vector2(15f, 4f));
            return art ? 2.5f * keyScale / art.bounds.size.x : 1f;
        }

        [MenuItem("Window/Detective Dave/Create Overworld Desk")]
        static void CreateDesk()
        {
            var error = OverworldSetup.ApplyOrError();
            if (error != null)
            {
                PlayerSetup.ShowProblem("Create Overworld Desk", error);
                return;
            }
            var art = AssetDatabase.LoadAssetAtPath<Sprite>(OverworldSetupPaths.FolderPath);
            var path = AssetDatabase.GenerateUniqueAssetPath(NewDeskPath);
            var saved = SaveDesk(path, null, art, CentredScale(art));
            Selection.activeObject = saved;
            EditorGUIUtility.PingObject(saved);
            Debug.Log("[Overworld] Desk created at " + path + ". Open it, move and scale the case file, and drag a Case asset onto it. Press F5 while it is open to play it.");
        }

        [MenuItem("GameObject/Detective Dave/Case Marker", false, 10)]
        static void CreateMarker(MenuCommand command)
        {
            var parent = MarkerParent.Find(command.context as GameObject);
            if (!parent)
            {
                EditorUtility.DisplayDialog("Case Marker", "Open a desk in Prefab Mode, or select a desk in the scene, so the case marker has a desk to go on.", "OK");
                return;
            }
            var art = AssetDatabase.LoadAssetAtPath<Sprite>(OverworldSetupPaths.FolderPath);
            var marker = AddMarker(parent, null, art, Vector3.zero, CentredScale(art));
            Undo.RegisterCreatedObjectUndo(marker.gameObject, "Create Case Marker");
            Selection.activeGameObject = marker.gameObject;
        }
    }
}
