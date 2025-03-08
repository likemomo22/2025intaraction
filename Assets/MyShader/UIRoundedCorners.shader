Shader "Custom/UIRoundedCorners"
{
    Properties
    {
        _Radius ("Corner Radius", Range(0, 1)) = 0.1
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Radius;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv * 2 - 1; // 归一化 UV (-1,1)
                float cornerRadius = _Radius;

                // 计算四角的圆角区域
                float dist = length(max(abs(uv) - (1 - cornerRadius), 0));

                if (dist > cornerRadius) discard; // 剔除超出圆角区域的部分

                return tex2D(_MainTex, i.uv); // 采样 UI 纹理
            }
            ENDCG
        }
    }
}
