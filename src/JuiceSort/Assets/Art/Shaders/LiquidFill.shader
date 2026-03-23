Shader "JuiceSort/LiquidFill"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _MaxVisualFill ("Max Visual Fill", Float) = 0.80
        _DimMultiplier ("Dim Multiplier", Float) = 1.0
        _WobbleX ("Wobble X", Float) = 0.0
        _WobbleZ ("Wobble Z", Float) = 0.0
        _GlowColor ("Glow Color", Color) = (1,0.9,0.6,1)
        _GlowIntensity ("Glow Intensity", Float) = 0.15
        _LiquidTilt ("Liquid Tilt", Float) = 0.0

        // SpriteMask stencil support (set automatically by Unity based on maskInteraction)
        _StencilRef ("Stencil Ref", Float) = 0
        _StencilComp ("Stencil Comp", Float) = 8
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_StencilRef]
            Comp [_StencilComp]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // Per-material properties
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _MaxVisualFill;
                float _DimMultiplier;
                float _WobbleX;
                float _WobbleZ;
                float4 _GlowColor;
                float _GlowIntensity;
                float _LiquidTilt;
            CBUFFER_END

            // HLSL arrays — set from C# via SetFloatArray / SetVectorArray
            // Cannot be in CBUFFER with SRP Batcher due to variable sizing
            float _FillLevels[6];
            float4 _LayerColors[6];
            int _LayerCount;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // Y coordinate: 0 = bottom of sprite, 1 = top
                float y = i.texcoord.y;

                // Apply liquid tilt: when bottle tilts, liquid flows toward the low side
                // _LiquidTilt > 0 means liquid pools toward x=1 (right), < 0 toward x=0 (left)
                // This tilts the fill line so liquid surface is angled like real gravity
                float tiltOffset = _LiquidTilt * (i.texcoord.x - 0.5);
                y -= tiltOffset;

                // Apply wobble offset to the sampling position
                float wobbleOffset = _WobbleX * sin(i.texcoord.x * 6.2832); // full sine wave across width
                y -= wobbleOffset;

                // Walk through bands from bottom up
                float cumFill = 0.0;
                half4 liquidColor = half4(0, 0, 0, 0);
                bool inLiquid = false;

                for (int idx = 0; idx < _LayerCount; idx++)
                {
                    float bandHeight = _FillLevels[idx] * _MaxVisualFill;
                    if (bandHeight <= 0.0)
                        break;

                    float bandBottom = cumFill;
                    float bandTop = cumFill + bandHeight;

                    if (y >= bandBottom && y < bandTop)
                    {
                        half4 bandColor = half4(_LayerColors[idx].rgb * _DimMultiplier, _LayerColors[idx].a);
                        liquidColor = bandColor;
                        inLiquid = true;

                        // Smooth gradient blending at band boundaries (top edge)
                        float blendZone = 0.015; // ~1.5% of sprite height
                        float distFromTop = bandTop - y;
                        if (distFromTop < blendZone && idx + 1 < _LayerCount && _FillLevels[idx + 1] > 0.0)
                        {
                            // Blend with next band color
                            half4 nextColor = half4(_LayerColors[idx + 1].rgb * _DimMultiplier, _LayerColors[idx + 1].a);
                            float blendT = 1.0 - (distFromTop / blendZone);
                            liquidColor = lerp(bandColor, nextColor, blendT);
                        }

                        break;
                    }

                    cumFill = bandTop;
                }

                // Empty region above liquid: fully transparent
                if (!inLiquid)
                {
                    // Inner glow: subtle additive color near the liquid surface (in empty region)
                    if (cumFill > 0.0 && _GlowIntensity > 0.0)
                    {
                        float distAboveLiquid = y - cumFill;
                        float glowFalloff = saturate(1.0 - distAboveLiquid / 0.05); // 5% falloff zone
                        glowFalloff *= glowFalloff; // quadratic falloff for soft edge
                        if (glowFalloff > 0.001)
                        {
                            half4 glow = half4(_GlowColor.rgb * _GlowIntensity * glowFalloff, glowFalloff * 0.3);
                            return glow * i.color;
                        }
                    }
                    return half4(0, 0, 0, 0);
                }

                // Inner glow: boost liquid color near the surface for a subtle luminous effect
                if (_GlowIntensity > 0.0 && cumFill > 0.0)
                {
                    float distFromSurface = cumFill - y;
                    if (distFromSurface < 0.03)
                    {
                        float glowT = saturate(1.0 - distFromSurface / 0.03);
                        liquidColor.rgb += _GlowColor.rgb * _GlowIntensity * glowT * 0.5;
                    }
                }

                // Apply sprite vertex color tinting
                liquidColor *= i.color;

                return liquidColor;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
