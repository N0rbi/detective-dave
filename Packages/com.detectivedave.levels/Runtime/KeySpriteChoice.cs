using UnityEngine;

namespace DetectiveDave.Levels
{
    public enum KeySpriteKind
    {
        None,
        Stretched,
        Sliced,
    }

    public readonly struct KeySpriteChoice
    {
        public static readonly KeySpriteChoice None = new KeySpriteChoice(KeySpriteKind.None, null);

        public readonly KeySpriteKind Kind;
        public readonly Sprite Sprite;

        public KeySpriteChoice(KeySpriteKind kind, Sprite sprite)
        {
            Kind = kind;
            Sprite = sprite;
        }
    }
}
