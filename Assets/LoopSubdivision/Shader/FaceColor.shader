Shader "LoopSubdivide/FaceColor"
{
    Properties
    {
        // _NoiseTex("Noise Texture", 2D) = "white"
        _Triangle_Count("Triangle Count", int) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            // sampler2D _NoiseTex;
            // float4 _NoiseTex_ST;
            int _Triangle_Count;        // 三角面数量 //
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o = (v2f)0;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
                return o;
            }

            half4 frag (v2f i, uint primitiveID : SV_PrimitiveID) : SV_Target
            {
                // half3 noise = tex2D(_NoiseTex, i.uv).rgb;
                
                half3 color = (primitiveID % 255 / 255.0).xxx;
                return half4(color, 1);
            }
            ENDCG
        }
    }
}
