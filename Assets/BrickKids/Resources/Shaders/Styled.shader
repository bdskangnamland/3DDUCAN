Shader "BrickKids/Styled"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Metallic ("Metallic", Range(0,1)) = 0.03
        _Smoothness ("Smoothness", Range(0,1)) = 0.62
        _Style ("Style", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 280
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        fixed4 _Color;
        half _Metallic;
        half _Smoothness;
        float _Style;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        float hash31(float3 p)
        {
            p = frac(p * 0.1031);
            p += dot(p, p.yzx + 33.33);
            return frac((p.x + p.y) * p.z);
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float3 baseColor = _Color.rgb;
            float variation = 1.0;
            float style = floor(_Style + 0.5);

            // Wood: subtle directional grain.
            if (style == 4.0)
            {
                float grain = sin(IN.worldPos.z * 11.0 + sin(IN.worldPos.x * 2.8) * 2.0);
                grain = grain * 0.5 + 0.5;
                variation = lerp(0.72, 1.12, grain);
            }
            // Concrete: fine speckle.
            else if (style == 5.0)
            {
                float n = hash31(floor(IN.worldPos * 18.0));
                variation = lerp(0.82, 1.08, n);
            }
            // Brick: mortar-like grid in world space.
            else if (style == 6.0)
            {
                float2 uv = IN.worldPos.xz * 1.55;
                float row = floor(uv.y);
                uv.x += fmod(abs(row), 2.0) * 0.5;
                float2 cell = frac(uv);
                float mortar = step(0.07, cell.x) * step(0.08, cell.y);
                variation = lerp(0.48, 1.05, mortar);
            }
            // Stone: broader natural variation.
            else if (style == 7.0)
            {
                float n1 = hash31(floor(IN.worldPos * 3.5));
                float n2 = hash31(floor(IN.worldPos * 8.0 + 4.7));
                variation = lerp(0.72, 1.16, n1 * 0.7 + n2 * 0.3);
            }

            o.Albedo = saturate(baseColor * variation);
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Occlusion = 1.0;
            o.Alpha = 1.0;
        }
        ENDCG
    }
    FallBack "Specular"
}
