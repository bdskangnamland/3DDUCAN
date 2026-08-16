Shader "BrickKids/Glass"
{
    Properties
    {
        _Color ("Tint", Color) = (0.62,0.86,1.0,0.35)
        _Metallic ("Metallic", Range(0,1)) = 0.05
        _Smoothness ("Smoothness", Range(0,1)) = 0.92
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 250
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        CGPROGRAM
        #pragma surface surf Standard alpha:fade
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
            o.Alpha = _Color.a;
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
