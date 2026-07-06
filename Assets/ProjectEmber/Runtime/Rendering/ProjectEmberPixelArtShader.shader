Shader "ProjectEmber/Pixel Art"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _PixelSize ("Pixel Size", Float) = 1.0
        _PaletteColors ("Palette Colors", ColorArray) = (0,0,0,0)
        _PaletteSize ("Palette Size", Int) = 0
        _DitherStrength ("Dither Strength", Range(0,1)) = 0.5
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", Range(0,1)) = 0.1
        _ShadowColor ("Shadow Color", Color) = (0,0,0,0.3)
        _HighlightColor ("Highlight Color", Color) = (1,1,1,0.2)
    }
    
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "PixelArt2D"
            Tags { "LightMode" = "Universal2D" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS :_sv_position;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            float4 _MainTex_ST;
            float4 _Color;
            float _PixelSize;
            float4 _PaletteColors[16];
            int _PaletteSize;
            float _DitherStrength;
            float4 _OutlineColor;
            float _OutlineThickness;
            float4 _ShadowColor;
            float4 _HighlightColor;

            // Bayer matrix for dithering
            static const float BayerMatrix[16] = 
            {
                0.0/16.0, 8.0/16.0, 2.0/16.0, 10.0/16.0,
                12.0/16.0, 4.0/16.0, 14.0/16.0, 6.0/16.0,
                3.0/16.0, 11.0/16.0, 1.0/16.0, 9.0/16.0,
                15.0/16.0, 7.0/16.0, 13.0/16.0, 5.0/16.0
            };

            float GetBayerValue(int x, int y)
            {
                return BayerMatrix[(x % 4) + (y % 4) * 4];
            }

            float3 ApplyPalette(float3 color)
            {
                if (_PaletteSize <= 0)
                    return color;
                
                float minDist = 1000.0;
                float3 closestColor = color;
                
                for (int i = 0; i < _PaletteSize && i < 16; i++)
                {
                    float3 paletteColor = _PaletteColors[i].rgb;
                    float dist = distance(color, paletteColor);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestColor = paletteColor;
                    }
                }
                
                return closestColor;
            }

            float3 ApplyDithering(float3 color, int x, int y)
            {
                float bayer = GetBayerValue(x, y);
                float dither = (bayer - 0.5) * _DitherStrength * 0.1;
                return clamp(color + dither, 0.0, 1.0);
            }

            float3 ApplyOutline(float3 color, float2 uv, float2 texelSize)
            {
                float2 offset = texelSize * _OutlineThickness;
                
                float4 sampleLeft = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-offset.x, 0));
                float4 sampleRight = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(offset.x, 0));
                float4 sampleUp = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, offset.y));
                float4 sampleDown = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, -offset.y));
                
                float4 center = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                
                float edge = 0.0;
                edge += step(center.a, 0.5) * step(0.5, sampleLeft.a);
                edge += step(center.a, 0.5) * step(0.5, sampleRight.a);
                edge += step(center.a, 0.5) * step(0.5, sampleUp.a);
                edge += step(center.a, 0.5) * step(0.5, sampleDown.a);
                
                if (edge > 0.5 && center.a > 0.5)
                {
                    return lerp(color, _OutlineColor.rgb, _OutlineColor.a);
                }
                
                return color;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 texelSize = 1.0 / float2(textureSize(_MainTex, 0));
                float2 pixelatedUV = floor(input.uv / texelSize * _PixelSize) / _PixelSize * texelSize;
                
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, pixelatedUV);
                float3 finalColor = texColor.rgb * _Color.rgb * input.color.rgb;
                
                // Apply palette quantization
                finalColor = ApplyPalette(finalColor);
                
                // Apply dithering
                int2 pixelCoord = int2(input.positionCS.xy);
                finalColor = ApplyDithering(finalColor, pixelCoord.x, pixelCoord.y);
                
                // Apply outline
                finalColor = ApplyOutline(finalColor, pixelatedUV, texelSize);
                
                // Add subtle shadow and highlight
                float shadow = 1.0 - (pixelCoord.x % 3) * 0.05;
                float highlight = 1.0 + (pixelCoord.y % 3) * 0.03;
                finalColor *= shadow * highlight;
                
                return half4(finalColor, texColor.a * _Color.a * input.color.a);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "PixelArtForward"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 normalWS : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            float4 _MainTex_ST;
            float4 _Color;
            float _PixelSize;
            float4 _PaletteColors[16];
            int _PaletteSize;
            float _DitherStrength;
            float4 _OutlineColor;
            float _OutlineThickness;

            static const float BayerMatrix[16] = 
            {
                0.0/16.0, 8.0/16.0, 2.0/16.0, 10.0/16.0,
                12.0/16.0, 4.0/16.0, 14.0/16.0, 6.0/16.0,
                3.0/16.0, 11.0/16.0, 1.0/16.0, 9.0/16.0,
                15.0/16.0, 7.0/16.0, 13.0/16.0, 5.0/16.0
            };

            float GetBayerValue(int x, int y)
            {
                return BayerMatrix[(x % 4) + (y % 4) * 4];
            }

            float3 ApplyPalette(float3 color)
            {
                if (_PaletteSize <= 0)
                    return color;
                
                float minDist = 1000.0;
                float3 closestColor = color;
                
                for (int i = 0; i < _PaletteSize && i < 16; i++)
                {
                    float3 paletteColor = _PaletteColors[i].rgb;
                    float dist = distance(color, paletteColor);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestColor = paletteColor;
                    }
                }
                
                return closestColor;
            }

            float3 ApplyDithering(float3 color, int x, int y)
            {
                float bayer = GetBayerValue(x, y);
                float dither = (bayer - 0.5) * _DitherStrength * 0.1;
                return clamp(color + dither, 0.0, 1.0);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 texelSize = 1.0 / float2(textureSize(_MainTex, 0));
                float2 pixelatedUV = floor(input.uv / texelSize * _PixelSize) / _PixelSize * texelSize;
                
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, pixelatedUV);
                float3 finalColor = texColor.rgb * _Color.rgb * input.color.rgb;
                
                // Apply palette quantization
                finalColor = ApplyPalette(finalColor);
                
                // Apply dithering
                int2 pixelCoord = int2(input.positionCS.xy);
                finalColor = ApplyDithering(finalColor, pixelCoord.x, pixelCoord.y);
                
                // Simple lighting
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(input.normalWS, mainLight.direction));
                finalColor *= mainLight.color * (NdotL * 0.5 + 0.5);
                
                return half4(finalColor, texColor.a * _Color.a * input.color.a);
            }
            ENDHLSL
        }
    }
    
    FallBack "Sprites/Default"
}
