// This shader is what computes the depth 
// of the objects such that we can develop 
// depth maps for the scene 
Shader "Custom/WorldSpaceDepth"
{
    // Defines the rendering pipeline for this shader
    SubShader
    {
        // This is for opaque objects (objects we can see)
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                // Get the vertex position in object space
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            // Compute the depth and output as grayscale
            float4 frag(v2f i) : SV_Target
            {
                // Distance 
                float d = length(_WorldSpaceCameraPos - i.worldPos); 
                // Normalize 
                d = saturate(d / 10.0); 
                // Grayscale 
                return float4(d, d, d, 1);
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
