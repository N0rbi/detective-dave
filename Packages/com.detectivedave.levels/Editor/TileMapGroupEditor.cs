using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>
    /// The Tile Map Group inspector: checks, the target level with a Bake button and an up-to-date status, the layer list with
    /// helpers to fill it, and a live map of what would be baked.
    /// </summary>
    [CustomEditor(typeof(TileMapGroup))]
    internal sealed class TileMapGroupEditor : UnityEditor.Editor
    {
        const float BakeButtonHeight = 28f;

        SerializedProperty level;
        SerializedProperty layers;
        SerializedProperty boardGuide;

        // Baking and validating read every tile and scan the open scenes, so they run only when something changed,
        // and only in the Layout pass (the Repaint pass that follows draws the same state).
        List<LevelIssue> issues;
        LevelTerrain preview;
        bool upToDate;
        bool dirty = true;

        void OnEnable()
        {
            level = serializedObject.FindProperty("level");
            layers = serializedObject.FindProperty("layers");
            boardGuide = serializedObject.FindProperty("boardGuide");
            Tilemap.tilemapTileChanged += OnTilesChanged;
            Undo.undoRedoPerformed += MarkDirty;
            EditorApplication.hierarchyChanged += MarkDirty;
            EditorApplication.projectChanged += MarkDirty;
        }

        void OnDisable()
        {
            Tilemap.tilemapTileChanged -= OnTilesChanged;
            Undo.undoRedoPerformed -= MarkDirty;
            EditorApplication.hierarchyChanged -= MarkDirty;
            EditorApplication.projectChanged -= MarkDirty;
        }

        void OnTilesChanged(Tilemap tilemap, Tilemap.SyncTile[] tiles) => MarkDirty();

        void MarkDirty()
        {
            dirty = true;
            Repaint();
        }

        void Recompute(TileMapGroup group)
        {
            preview = LevelBaker.Bake(group);
            issues = LevelValidator.Validate(group, LevelBaker.FindOpenGroups(), preview);
            upToDate = !group.Level || preview.SameAs(group.Level.Terrain);
            dirty = false;
        }

        public override void OnInspectorGUI()
        {
            var group = (TileMapGroup)target;
            if (issues == null || (dirty && Event.current.type == EventType.Layout)) Recompute(group);
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            LevelEditorGUI.Header("Checks");
            LevelEditorGUI.DrawIssues(issues);

            DrawLevel(group);
            DrawLayers(group);

            LevelEditorGUI.Header("Scene View");
            EditorGUILayout.PropertyField(boardGuide);
            EditorGUILayout.LabelField($"Board space: local space of '{group.BoardSpace.name}'.", EditorStyles.miniLabel);

            if (EditorGUI.EndChangeCheck()) dirty = true;
            serializedObject.ApplyModifiedProperties();

            LevelEditorGUI.Header("Preview (what Bake would write)");
            LevelEditorGUI.DrawTerrainPreview(preview);
        }

        void DrawLevel(TileMapGroup group)
        {
            LevelEditorGUI.Header("Level");
            EditorGUILayout.PropertyField(level);
            var chosen = level.objectReferenceValue as LevelDefinition;
            if (!chosen)
            {
                if (GUILayout.Button(new GUIContent("Create Level…", "Creates a new Level Definition asset and assigns it here.")))
                    CreateLevel(group);
                return;
            }

            bool current = chosen == group.Level && upToDate;
            EditorGUILayout.LabelField(current ? "Up to date." : "Out of date: press Bake.", current ? EditorStyles.miniLabel : EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button($"Bake Into '{chosen.name}'", GUILayout.Height(BakeButtonHeight)))
                {
                    serializedObject.ApplyModifiedProperties();
                    Bake(group);
                    dirty = true;
                }
                if (GUILayout.Button("Select Level", GUILayout.Height(BakeButtonHeight))) Selection.activeObject = chosen;
            }
        }

        static void Bake(TileMapGroup group)
        {
            var source = LevelBaker.DescribeSource(group);
            var bakedFrom = group.Level.BakedFrom;
            if (!string.IsNullOrEmpty(bakedFrom) && bakedFrom != source && !group.Level.Terrain.IsEmpty &&
                !EditorUtility.DisplayDialog("Replace level shape?",
                    $"'{group.Level.name}' was baked from {bakedFrom}. Bake from {source} instead?", "Bake", "Cancel"))
                return;
            LevelBaker.BakeInto(group);
            AssetDatabase.SaveAssetIfDirty(group.Level);
        }

        void DrawLayers(TileMapGroup group)
        {
            LevelEditorGUI.Header("Layers (highest priority first)");
            EditorGUILayout.PropertyField(layers, true);
            bool editable = !EditorUtility.IsPersistent(group);
            using (new EditorGUI.DisabledScope(!editable))
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("Add Child Tilemaps", "Adds every Tilemap under this object that is not listed yet.")))
                {
                    AddChildTilemaps(group);
                    dirty = true;
                }
                if (GUILayout.Button(new GUIContent("Add Tilemap Per Tile Type", "Creates one child Tilemap per Tile Type asset that has no layer yet, each a layer of that type.")))
                {
                    AddTilemapPerType(group);
                    dirty = true;
                }
            }
            if (!editable)
                EditorGUILayout.LabelField("Open the prefab to add tilemaps.", EditorStyles.miniLabel);
        }

        void AddChildTilemaps(TileMapGroup group)
        {
            var listed = Enumerable.Range(0, layers.arraySize)
                .Select(i => layers.GetArrayElementAtIndex(i).FindPropertyRelative("tilemap").objectReferenceValue)
                .ToList();
            foreach (var tilemap in group.GetComponentsInChildren<Tilemap>(true).Where(tilemap => !listed.Contains(tilemap)))
                AppendLayer(tilemap, null);
        }

        void AddTilemapPerType(TileMapGroup group)
        {
            var typed = Enumerable.Range(0, layers.arraySize)
                .Select(i => layers.GetArrayElementAtIndex(i).FindPropertyRelative("type").objectReferenceValue)
                .ToList();
            foreach (var type in LevelAssets.FindAllTileTypes().Where(type => !typed.Contains(type)))
            {
                var child = new GameObject(type.name, typeof(Tilemap), typeof(TilemapRenderer));
                Undo.RegisterCreatedObjectUndo(child, "Add Tilemap Per Tile Type");
                child.transform.SetParent(group.transform, false);
                AppendLayer(child.GetComponent<Tilemap>(), type);
            }
        }

        void AppendLayer(Tilemap tilemap, TileType type)
        {
            layers.arraySize++;
            var element = layers.GetArrayElementAtIndex(layers.arraySize - 1);
            element.FindPropertyRelative("tilemap").objectReferenceValue = tilemap;
            element.FindPropertyRelative("type").objectReferenceValue = type;
        }

        void CreateLevel(TileMapGroup group)
        {
            var sceneName = group.gameObject.scene.name;
            var path = EditorUtility.SaveFilePanelInProject("Create Level", string.IsNullOrEmpty(sceneName) ? "NewLevel" : sceneName, "asset",
                "Where should the new Level Definition go?");
            if (string.IsNullOrEmpty(path)) return;
            var created = LevelAssets.CreateLevel(path);
            level.objectReferenceValue = created;
            serializedObject.ApplyModifiedProperties();
            LevelBaker.BakeInto(group);
            AssetDatabase.SaveAssetIfDirty(created);
            dirty = true;
        }
    }
}
