// Draws the keyboard_*.png masks: R = edge, G = key, B = text, A = accent coverage, layered over the panel color.
// The rounded panel corners are cut here, so the texture itself has no transparency.
// Code mirror of the properties below: KeyboardGlyphShader.cs.
Shader "Game/Keyboard Glyph"
{
    Properties
    {
        [PerRendererData] _MainTex ("Mask", 2D) = "white" {}
        _Panel ("Panel", Color) = (0.0784, 0.0902, 0.1255, 1)
        _Key ("Key", Color) = (0.1882, 0.2118, 0.2745, 1)
        _Edge ("Edge", Color) = (0.3294, 0.3608, 0.4549, 1)
        _Text ("Text", Color) = (0.9098, 0.9255, 0.9569, 1)
        _Accent ("Accent", Color) = (0.9490, 0.7216, 0.2941, 1)
        _CornerRadius ("Corner Radius (texture pixels)", Float) = 18
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        CBUFFER_START(UnityPerMaterial)
            half4 _Panel;
            half4 _Key;
            half4 _Edge;
            half4 _Text;
            half4 _Accent;
            float _CornerRadius;
        CBUFFER_END

        struct Attributes
        {
            float3 positionOS : POSITION;
            float2 uv : TEXCOORD0;
            half4 color : COLOR;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            half4 color : COLOR;
        };

        Varyings vert(Attributes input)
        {
            Varyings output;
            output.positionCS = TransformObjectToHClip(input.positionOS);
            output.uv = input.uv;
            output.color = input.color;
            return output;
        }

        half4 frag(Varyings input) : SV_Target
        {
            half4 mask = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
            half3 c = _Panel.rgb;
            c = lerp(c, _Edge.rgb, mask.r);
            c = lerp(c, _Key.rgb, mask.g);
            c = lerp(c, _Text.rgb, mask.b);
            c = lerp(c, _Accent.rgb, mask.a);

            // rounded rectangle over the whole texture, anti-aliased over one texture pixel
            float w, h;
            _MainTex.GetDimensions(w, h);
            float2 size = float2(w, h);
            float2 q = abs(input.uv * size - size * 0.5) - (size * 0.5 - _CornerRadius);
            float d = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - _CornerRadius;
            half alpha = saturate(0.5 - d);

            return half4(c, alpha * input.color.a);
        }
        ENDHLSL

        Pass
        {
            Tags { "LightMode" = "Universal2D" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" "Queue" = "Transparent" "RenderType" = "Transparent" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}
