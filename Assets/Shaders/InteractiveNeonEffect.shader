Shader "UI/InteractiveNeonPulseFixed"
{
    Properties
    {
        [HideInInspector]_MainTex ("Texture", 2D) = "white" {}
        _Mouse ("Mouse Position", Vector) = (0.5, 0.5, 0, 0)
        _Aspect ("Panel Aspect", Float) = 1.0 // Новое свойство
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        ZWrite Off
        Blend One One 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _Mouse;
            float _Aspect; // Получаем из скрипта

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 st = i.uv;
                float2 mouse = _Mouse.xy;

                // ВАЖНО: Вычитаем координаты ДО применения аспекта
                float2 diff = st - mouse;

                // Корректируем X только для расчета дистанции по аспекту ПАНЕЛИ
                diff.x *= _Aspect;

                float dist = length(diff);
                float3 color = float3(0.0, 0.0, 0.0);
                float time = _Time.y;

                for(float j = 1.0; j < 4.0; j++) {
                    float wave = abs(sin(dist * 15.0 * j - time * 3.0));
                    float glow = 0.008 / wave;
                    float3 ringColor = float3(0.1 * j, 0.5 * sin(time + j), 0.8);
                    color += ringColor * glow;
                }

                // Ограничиваем эффект, чтобы он не "заливал" всю панель
                color *= smoothstep(1.0, 0.0, dist);

                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}
