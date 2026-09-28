Shader "Custom/FogOfWar"
{
    Properties
    {
        _FogColor ("Fog Color", Color) = (0, 0, 0, 0.7)
        _ViewDistance ("View Distance", Float) = 15
        _Fov ("FOV", Range(1, 360)) = 90
        _EdgeSoftness ("Edge Softness", Range(0.001, 5)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _FogColor;
            float _ViewDistance;
            float _Fov;
            float _EdgeSoftness;

            // 由 C# 每帧传入
            float4 _PlayerPos;
            float4 _PlayerForward;

            struct appdata
            {
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

            fixed4 frag(v2f i) : SV_Target
            {
                // 只考虑水平面
                float3 toPixel = i.worldPos - _PlayerPos.xyz;
                toPixel.y = 0;

                float dist = length(toPixel);
                if (dist < 0.001)
                    return fixed4(_FogColor.rgb, 0);

                // 距离衰减：超出视野距离则不可见
                float distFactor = 1.0 - smoothstep(
                    _ViewDistance - _EdgeSoftness,
                    _ViewDistance,
                    dist
                );

                // 角度衰减：超出视野角则不可见
                float3 forward = normalize(float3(_PlayerForward.x, 0, _PlayerForward.z));
                float3 dir = normalize(toPixel);
                float angle = degrees(acos(clamp(dot(forward, dir), -1.0, 1.0)));

                float angleFactor = 1.0 - smoothstep(
                    _Fov * 0.5 - _EdgeSoftness * 5.0,
                    _Fov * 0.5,
                    angle
                );

                // visible = 1 表示在视野锥内
                float visible = distFactor * angleFactor;

                float alpha = _FogColor.a * (1.0 - visible);
                return fixed4(_FogColor.rgb, alpha);
            }
            ENDCG
        }
    }
}