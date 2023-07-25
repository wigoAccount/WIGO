// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/TextureRotateShader"
{
     Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        [KeywordEnum(Off, On)] Flip_Horizontal ("Flip Horizontal", Int) = 0
        [KeywordEnum(None, D90, D270)] Rotate ("Rotate", Int) = 0
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile ROTATE_NONE ROTATE_D90 ROTATE_D270
            #pragma multi_compile FLIP_HORIZONTAL_OFF FLIP_HORIZONTAL_ON

            struct appdata
            {
                float4 vertex : POSITION; // vertex position
                float2 uv : TEXCOORD0; // texture coordinate
            };

            struct v2f
            {
                float2 uv : TEXCOORD0; // texture coordinate
                float4 vertex : SV_POSITION; // clip space position
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                #if ROTATE_D90
                o.uv = float2(1 - v.uv.y, v.uv.x);
                #elif ROTATE_D270
                o.uv = float2(v.uv.y, 1 - v.uv.x);
                #else
                o.uv = v.uv;
                #endif

                #if FLIP_HORIZONTAL_ON && (ROTATE_D90 || ROTATE_D270)
                o.uv = float2(o.uv.x, 1 - o.uv.y);
                #endif
                
                #if FLIP_HORIZONTAL_ON && ROTATE_NONE
                o.uv = float2(1 - o.uv.x, o.uv.y);
                #endif
                
                return o;
            }
            
            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
