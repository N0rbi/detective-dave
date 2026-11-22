using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Levels
{
    public enum StartSide
    {
        [InspectorName("Left Shift")] Left,
        [InspectorName("Right Shift")] Right
    }

    /// <summary>
    /// The root of a level prefab in Assets/Levels: one file holds the whole level. Children: a Grid with the Shape tilemap
    /// (painted with <see cref="TileType"/> tiles, one tile per cell, a key is the cells under it) and a Background sprite renderer.
    /// The game reads the shape straight from the tilemap (<see cref="BuildTerrain"/>); nothing is baked.
    /// <para>
    /// Board space is the local space of this object's parent (the keyboard object the level is placed under, at its local origin),
    /// or of this object when it has no parent, as when the prefab is open for editing.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Detective Dave/Level")]
    [HelpURL(LevelsDocs.GuideUrl)]
    public sealed class Level : MonoBehaviour
    {
        const string CloneSuffix = "(Clone)";

        [Tooltip("The name players see. Leave empty to use the file name.")]
        [SerializeField] string displayName;

        [Tooltip("How the keys look in game. Duplicate a Key Art to give this level its own look.")]
        [SerializeField] KeyArt keyArt;

        [Tooltip("The tilemap that holds the level's shape: which keys are floor, wall or gap. The two Shift keys are the Start and End. Paint it with the Level Tiles palette.")]
        [SerializeField] Tilemap shape;

        [Tooltip("Which Shift key the player starts on. The other Shift key is the End.")]
        [SerializeField] StartSide startKey = StartSide.Left;

        [SerializeField, HideInInspector] float drawnHeightStretch = 1f;

        [Tooltip("Drawn behind the keyboard. Its sprite and colour are the level's background; with no sprite the Key Art's default background is used.")]
        [SerializeField] SpriteRenderer background;

        [Tooltip("How the 2D lights look in this level: their brightness and colour, and an optional flicker.")]
        [SerializeField] LevelLightingSettings lighting = new LevelLightingSettings();

        /// <summary>The name players see: the display name, or the object (file) name without the "(Clone)" Unity adds to instances.</summary>
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(displayName)) return displayName;
                var objectName = name;
                return objectName.EndsWith(CloneSuffix) ? objectName.Substring(0, objectName.Length - CloneSuffix.Length).TrimEnd() : objectName;
            }
        }

        /// <summary>How the keys look, or null when not set.</summary>
        public KeyArt KeyArt => keyArt;

        /// <summary>The tilemap with the level's shape, or null when not set.</summary>
        public Tilemap Shape => shape;

        /// <summary>The Shift key the player starts on; the other Shift key is the End.</summary>
        public StartSide StartKey => startKey;

        /// <summary>How the 2D lights look while this level plays.</summary>
        public LevelLightingSettings Lighting => lighting ??= new LevelLightingSettings();

        public float DrawnHeightStretch => drawnHeightStretch;

        public StartSide EndKey => startKey == StartSide.Left ? StartSide.Right : StartSide.Left;

        /// <summary>The renderer that draws the background, or null when the level has none.</summary>
        public SpriteRenderer BackgroundRenderer => background;

        /// <summary>The background sprite: the renderer's, else the Key Art's default. Null when neither has one.</summary>
        public Sprite Background => background && background.sprite ? background.sprite : keyArt ? keyArt.DefaultBackground : null;

        /// <summary>The tint that goes with <see cref="Background"/>.</summary>
        public Color BackgroundTint => background && background.sprite ? background.color : keyArt ? keyArt.DefaultBackgroundTint : Color.white;

        /// <summary>The transform whose local space is board space: the parent, or this object at the root.</summary>
        public Transform BoardSpace => transform.parent ? transform.parent : transform;

        /// <summary>
        /// Reads the Shape tilemap into a terrain in board space (see <see cref="LevelTerrain.FromTilemap"/>). Allocates and reads every
        /// cell: call it when the level is loaded or changed, not every frame. Empty when there is no Shape.
        /// </summary>
        public LevelTerrain BuildTerrain() => shape ? LevelTerrain.FromTilemap(shape, BoardSpace) : new LevelTerrain();
    }
}
