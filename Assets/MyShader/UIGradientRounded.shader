Shader "Custom/UIGradientRounded"
{
    Properties
    {
        _Color1 ("Left Color", Color) = (1,0,0,1) // 左侧颜色
        _Color2 ("Right Color", Color) = (0,0,1,1) // 右侧颜色
        _Radius ("Corner Radius", Range(0,0.5)) = 0.1 // 圆角半径
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha  // 透明混合
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
            fixed4 _Color1;
            fixed4 _Color2;
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
                // 计算颜色渐变 (基于 UV.x 轴)
                fixed4 gradientColor = lerp(_Color1, _Color2, i.uv.x);

                // 计算圆角 (基于 UV)
                float2 uv = i.uv * 2 - 1; // 归一化 (-1,1)
                float dist = length(max(abs(uv) - (1 - _Radius), 0));
                if (dist > _Radius) discard; // 剔除超出圆角区域的部分

                return gradientColor;
            }
            ENDCG
        }
    }
}
