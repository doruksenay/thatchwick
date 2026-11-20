// Su parıltısı (belge 57 §10): su karolarının üstüne ikinci bir katman olarak çizilir. Karonun kendisini boyamaz;
// yalnızca suyun üstünde yavaşça kayan, piksel ızgarasına oturmuş açık dalga çizgilerini çıkarır (geri kalan saydam).
// Çizgiler dünya konumuna göre hesaplanır, böylece karo sınırında kesilmez. Güç küresel _TwWaterAmp (0 kapalı;
// efekt yoğunluğu ayarı), rüzgâr küresel _TwLean'den yön ve hız alır. Resources altında: Shader.Find ile bulunur.
Shader "Thatchwick/WaterShimmer"
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
            float _TwWaterAmp;
            float _TwLean;

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
                float2 world : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS);
                o.color = input.color * unity_SpriteColor;
                o.uv = input.uv;
                o.world = TransformObjectToWorld(input.positionOS).xy;
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                // 32 piksel = 1 birim: konumu piksel ızgarasına oturt, çizgiler yarım piksele düşmesin.
                float2 px = floor(input.world * 32.0);
                // Yalnızca her üçüncü piksel satırında yatay çizgi (izometrik suyun yatay parıltısı).
                if (fmod(abs(px.y), 3.0) > 0.5) return half4(0, 0, 0, 0);
                float dir = _TwLean < 0.0 ? -1.0 : 1.0;
                float speed = 0.35 + min(abs(_TwLean), 3.0) * 0.12;
                float a = frac(px.x / 37.0 + px.y / 11.0 + dir * _Time.y * speed);
                float b = frac(px.x / 23.0 - px.y / 17.0 - dir * _Time.y * speed * 0.7 + 0.5);
                float band = step(a, 0.07) + step(b, 0.05) * 0.6;
                return half4(1.0, 1.0, 1.0, saturate(band) * tex.a * 0.22 * _TwWaterAmp);
            }
            ENDHLSL
        }
    }
}
