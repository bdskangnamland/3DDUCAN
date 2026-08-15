Shader "BrickKids/Ghost"
{
 Properties { _Color ("Color", Color) = (0.2,1,0.3,0.45) }
 SubShader
 {
  Tags { "RenderType"="Transparent" "Queue"="Transparent" }
  Blend SrcAlpha OneMinusSrcAlpha
  ZWrite Off Cull Back
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 2.0
   #include "UnityCG.cginc"
   fixed4 _Color;
   struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; };
   struct v2f { float4 pos:SV_POSITION; float3 normalWS:TEXCOORD0; };
   v2f vert(appdata v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.normalWS=UnityObjectToWorldNormal(v.normal); return o; }
   fixed4 frag(v2f i):SV_Target { float3 n=normalize(i.normalWS); float3 l=normalize(float3(-0.42,0.86,-0.30)); float diff=saturate(dot(n,l)); return fixed4(saturate(_Color.rgb*(0.72+diff*0.28)),_Color.a); }
   ENDCG
  }
 }
 FallBack "Unlit/Transparent"
}