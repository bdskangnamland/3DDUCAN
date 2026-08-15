Shader "BrickKids/Plastic"
{
 Properties { _Color ("Color", Color) = (1,1,1,1) }
 SubShader
 {
  Tags { "RenderType"="Opaque" "Queue"="Geometry" }
  Pass
  {
   Cull Back ZWrite On
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 2.0
   #include "UnityCG.cginc"
   fixed4 _Color;
   struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; };
   struct v2f { float4 pos:SV_POSITION; float3 normalWS:TEXCOORD0; float3 worldPos:TEXCOORD1; };
   v2f vert(appdata v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.normalWS=UnityObjectToWorldNormal(v.normal); o.worldPos=mul(unity_ObjectToWorld,v.vertex).xyz; return o; }
   fixed4 frag(v2f i):SV_Target
   {
    float3 n=normalize(i.normalWS); float3 l=normalize(float3(-0.42,0.86,-0.30)); float diff=saturate(dot(n,l));
    float3 v=normalize(_WorldSpaceCameraPos.xyz-i.worldPos); float3 h=normalize(l+v);
    float spec=pow(saturate(dot(n,h)),36.0)*0.24; float rim=pow(1.0-saturate(dot(n,v)),3.0)*0.07;
    float3 col=_Color.rgb*(0.68+diff*0.32)+float3(spec,spec,spec)+float3(rim,rim,rim); return fixed4(saturate(col),1.0);
   }
   ENDCG
  }
 }
 FallBack "Unlit/Color"
}