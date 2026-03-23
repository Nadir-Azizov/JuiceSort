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
        _LiquidTiltY ("Liquid Tilt Y", Float) = 0.0

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
                float _LiquidTiltY;
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
                float y = i.texcoord.y;

                // --- Compute total fill height (untilted) ---
                float totalFill = 0.0;
                int topBandIdx = 0;
                for (int p = 0; p < _LayerCount; p++)
                {
                    float bh = _FillLevels[p] * _MaxVisualFill;
                    if (bh <= 0.0) break;
                    totalFill += bh;
                    topBandIdx = p;
                }

                // --- Compute additive tilt offset at this pixel ---
                // Scaled by _MaxVisualFill (constant 0.80) instead of dynamic totalFill.
                // This prevents bottom bands from shifting position when only the top drains,
                // while keeping the tilt magnitude in a reasonable range.
                float tiltOffset = _MaxVisualFill * (_LiquidTilt * (i.texcoord.x - 0.5) + _LiquidTiltY * (y - 0.5));

                float wobbleOffset = _WobbleX * sin(i.texcoord.x * 6.2832);
                float surface = totalFill + tiltOffset - wobbleOffset;

                // Clamp surface: can't go below 0 (no negative liquid)
                surface = max(surface, 0.0);

                // --- Above tilted surface → empty ---
                if (y >= surface)
                {
                    // Inner glow above surface
                    if (surface > 0.0 && _GlowIntensity > 0.0)
                    {
                        float distAbove = y - surface;
                        float glowFalloff = saturate(1.0 - distAbove / 0.05);
                        glowFalloff *= glowFalloff;
                        if (glowFalloff > 0.001)
                        {
                            half4 glow = half4(_GlowColor.rgb * _GlowIntensity * glowFalloff, glowFalloff * 0.3);
                            return glow * i.color;
                        }
                    }
                    return half4(0, 0, 0, 0);
                }

                // --- Below tilted surface → in liquid ---
                // Band boundaries use the same additive tiltOffset so every
                // layer boundary is a parallel line at the same tilt angle.
                float cumFill = 0.0;
                half4 liquidColor = half4(0, 0, 0, 0);
                bool foundBand = false;

                for (int idx = 0; idx < _LayerCount; idx++)
                {
                    float bandHeight = _FillLevels[idx] * _MaxVisualFill;
                    if (bandHeight <= 0.0) break;

                    float bandBottom = cumFill + tiltOffset;
                    float bandTop = cumFill + bandHeight + tiltOffset;

                    if (y >= bandBottom && y < bandTop)
                    {
                        half4 bandColor = half4(_LayerColors[idx].rgb * _DimMultiplier, _LayerColors[idx].a);
                        liquidColor = bandColor;
                        foundBand = true;

                        // Smooth gradient blending at band boundaries
                        float blendZone = 0.015;
                        float distFromTop = bandTop - y;
                        if (distFromTop < blendZone && idx + 1 < _LayerCount && _FillLevels[idx + 1] > 0.0)
                        {
                            half4 nextColor = half4(_LayerColors[idx + 1].rgb * _DimMultiplier, _LayerColors[idx + 1].a);
                            float blendT = 1.0 - (distFromTop / blendZone);
                            liquidColor = lerp(bandColor, nextColor, blendT);
                        }
                        break;
                    }

                    cumFill += bandHeight;
                }

                // Pixel is below surface but not in any band (tilt shifted bands away).
                // Below first band's natural bottom → show bottom band color (gap on high side).
                // Above last band's natural top → show top band color (overflow on low side).
                if (!foundBand && _LayerCount > 0)
                {
                    if (y < tiltOffset)
                        liquidColor = half4(_LayerColors[0].rgb * _DimMultiplier, _LayerColors[0].a);
                    else
                        liquidColor = half4(_LayerColors[topBandIdx].rgb * _DimMultiplier, _LayerColors[topBandIdx].a);
                }

                // Inner glow near tilted surface
                if (_GlowIntensity > 0.0 && surface > 0.0)
                {
                    float distFromSurface = surface - y;
                    if (distFromSurface < 0.03)
                    {
                        float glowT = saturate(1.0 - distFromSurface / 0.03);
                        liquidColor.rgb += _GlowColor.rgb * _GlowIntensity * glowT * 0.5;
                    }
                }

                liquidColor *= i.color;
                return liquidColor;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
