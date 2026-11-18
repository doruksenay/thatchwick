// Rüzgârda sallanan sprite (animasyon partisi 1, 18.11.2026): ağaç, fidan ve ekinler.
// Tabandan yukarı doğru artan yatay kayma; kayma 1/32 birime (bir sanat pikseline) yuvarlanır,
// böylece piksel ızgarası bozulmaz. Güç GameRoot'tan küresel _TwWind (piksel) ile gelir; her nesne
// dünya konumundan kendi evresini alır, orman aynı anda sallanmaz. Normal saydamlık karışımı.
Shader "Thatchwick/SwaySprite"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float _TwWind;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                Varyings o;
                float3 world = TransformObjectToWorld(input.positionOS);
                float3 origin = TransformObjectToWorld(float3(0, 0, 0));
                float lift = saturate(input.positionOS.y * 0.6);
                float phase = origin.x * 1.7 + origin.y * 2.3;
                float wave = sin(_Time.y * 1.5 + phase) + 0.35 * sin(_Time.y * 3.7 + phase * 1.3);
                float px = round(wave * _TwWind * lift * lift);
                world.x += px / 32.0;
                o.positionCS = TransformWorldToHClip(world);
                o.color = input.color * unity_SpriteColor;
                o.uv = input.uv;
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
            }
            ENDHLSL
        }
    }
}
