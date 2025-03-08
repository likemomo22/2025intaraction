Shader "Custom/UIGradient"
{
    Properties
    {
        _Color1 ("Left Color", Color) = (1,0,0,1) // 左侧颜色
        _Color2 ("Right Color", Color) = (0,0,1,1) // 右侧颜色
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha  // 透明混合，支持半透明
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color1;
            fixed4 _Color2;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return lerp(_Color1, _Color2, i.uv.x); // 根据 UV.x 进行颜色插值
            }
            ENDCG
        }
    }
}
