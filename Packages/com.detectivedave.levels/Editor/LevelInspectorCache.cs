using System;
using UnityEditor;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>
    /// Marks an inspector dirty when anything a check depends on changes: tiles painted, undo/redo, hierarchy or project edits.
    /// The inspector recomputes in the next Layout pass only. Dispose it in OnDisable.
    /// </summary>
    internal sealed class LevelInspectorCache : IDisposable
    {
        readonly Action repaint;

        public bool Dirty { get; private set; } = true;

        public LevelInspectorCache(Action repaint)
        {
            this.repaint = repaint;
            Tilemap.tilemapTileChanged += OnTilesChanged;
            Undo.undoRedoPerformed += MarkDirty;
            EditorApplication.hierarchyChanged += MarkDirty;
            EditorApplication.projectChanged += MarkDirty;
        }

        public void Dispose()
        {
            Tilemap.tilemapTileChanged -= OnTilesChanged;
            Undo.undoRedoPerformed -= MarkDirty;
            EditorApplication.hierarchyChanged -= MarkDirty;
            EditorApplication.projectChanged -= MarkDirty;
        }

        public void MarkDirty()
        {
            Dirty = true;
            repaint();
        }

        public void MarkClean() => Dirty = false;

        void OnTilesChanged(Tilemap tilemap, Tilemap.SyncTile[] tiles) => MarkDirty();
    }
}
