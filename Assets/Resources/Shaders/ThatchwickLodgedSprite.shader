// Yatmış tahıl (belge 57d §2): fırtınanın yatırdığı ekin. Sway shader'ının kopyası; sprite tabanı yerinde kalır,
// üst satırlar rüzgâr yönüne kayar ve boy kısalır. Kayma 1/32 birime yuvarlanır (piksel satırları yatay kalır).
// Yön küresel _TwLodgeSide (+1 sağa, -1 sola; verilmemişse sağa).
Shader "Thatchwick/LodgedSprite"
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
            // Rüzgâr yönü ve hamle (belge 57 §8): _TwLean rüzgâr yönüne kalıcı eğim (piksel, işaretli);
            // _TwGustPos haritayı dolaşan hamlenin konumu (dünya x + 0,5 y), _TwGustWidth genişliği, _TwGustAmp gücü (piksel, işaretli).
            float _TwLean;
            float _TwGustPos;
            float _TwGustWidth;
            float _TwGustAmp;
            float _TwLodgeSide;

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
                float along = origin.x + origin.y * 0.5;
                float gust = saturate(1.0 - abs(along - _TwGustPos) / max(_TwGustWidth, 0.5)) * _TwGustAmp;
                float h = max(input.positionOS.y, 0.0);
                float side = _TwLodgeSide < 0.0 ? -1.0 : 1.0;
                float px = round(h * 32.0 * 0.85) * side + round((wave * _TwWind * 0.3 + gust * 0.3) * lift);
                world.x += px / 32.0;
                world.y -= h * 0.5;
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
