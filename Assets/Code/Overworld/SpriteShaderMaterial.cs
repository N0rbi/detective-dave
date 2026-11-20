using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public static class SpriteShaderMaterial
    {
        const string UrpSpriteShader = "Universal Render Pipeline/2D/Sprite-Unlit-Default";
        const string BuiltInSpriteShader = "Sprites/Default";

        public static Material Create()
        {
            var shader = Shader.Find(UrpSpriteShader);
            if (!shader) shader = Shader.Find(BuiltInSpriteShader);
            return new Material(shader);
        }
    }
}
