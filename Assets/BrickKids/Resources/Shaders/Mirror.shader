Shader "BrickKids/Mirror"
{
    Properties
    {
        _Color ("Color", Color) = (0.72,0.80,0.86,1)
        _Metallic ("Metallic", Range(0,1)) = 0.94
        _Smoothness ("Smoothness", Range(0,1)) = 0.96
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 250
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        fixed4 _Color;
        half _Metallic;
        half _Smoothness;
        struct Input { float3 worldPos; };
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = _Color.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Occlusion = 1.0;
            o.Alpha = 1.0;
        }
        ENDCG
    }
    FallBack "Specular"
}
