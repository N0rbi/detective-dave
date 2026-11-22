using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>The Level inspector: checks with fix buttons on top (live while painting), then the fields, then a map of the shape.</summary>
    [CustomEditor(typeof(Level))]
    internal sealed class LevelEditor : UnityEditor.Editor
    {
        LevelInspectorCache cache;
        List<LevelIssue> issues;
        LevelTerrain preview;

        void OnEnable()
        {
            cache = new LevelInspectorCache(Repaint);
            LightingPreview.Changed += Repaint;
        }

        void OnDisable()
        {
            LightingPreview.Changed -= Repaint;
            LightingPreview.Stop();
            cache?.Dispose();
        }

        public override void OnInspectorGUI()
        {
            var level = (Level)target;
            if (issues == null || (cache.Dirty && Event.current.type == EventType.Layout))
            {
                issues = LevelValidator.Validate(level);
                preview = level.BuildTerrain();
                cache.MarkClean();
            }

            LevelEditorGUI.Header("Checks");
            if (LevelEditorGUI.DrawIssues(issues)) cache.MarkDirty();

            LevelEditorGUI.Header("Level");
            EditorGUI.BeginChangeCheck();
            DrawPropertiesExcluding(serializedObject, "m_Script", "lighting");
            LevelEditorGUI.DrawEndKey(level);
            if (EditorGUI.EndChangeCheck()) cache.MarkDirty();
            serializedObject.ApplyModifiedProperties();

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            LightingEditorGUI.Draw(serializedObject.FindProperty("lighting"), level.Lighting);
            if (EditorGUI.EndChangeCheck()) cache.MarkDirty();
            serializedObject.ApplyModifiedProperties();

            LevelEditorGUI.Header("Shape");
            LevelEditorGUI.DrawTerrainPreview(preview);
        }
    }
}
