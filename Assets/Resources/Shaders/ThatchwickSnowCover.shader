// Kar örtüsü (belge 57 §9): dış mekân zemin karolarının üstüne ikinci katman olarak çizilir. Kar yağdıkça küresel
// _TwSnow 0'dan 1'e çıkar; örtü önce kümeler hâlinde (piksel ızgarasına oturmuş gürültü eşiği) belirir, sonra bütün
// zemini kaplar; erirken aynı kümeler en son gider. Renk soğuk beyaz. Resources altında: Shader.Find ile bulunur.
Shader "Thatchwick/SnowCover"
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
            float _TwSnow;

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

            // Piksel ızgarasında değer gürültüsü (karo sınırında kesilmez).
            float Hash(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }
            float Noise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hash(i), Hash(i + float2(1, 0)), f.x), lerp(Hash(i + float2(0, 1)), Hash(i + float2(1, 1)), f.x), f.y);
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                if (_TwSnow <= 0.001) return half4(0, 0, 0, 0);
                float2 px = floor(input.world * 32.0);
                float n = Noise(px / 9.0) * 0.65 + Noise(px / 3.0) * 0.35;
                // Kar azken yalnızca gürültünün yüksek yerleri örtülür; 1'de her yer.
                float cover = step(1.0 - _TwSnow * 1.15, n);
                half shade = 0.94 + 0.06 * step(0.5, Noise(px / 2.0));
                return half4(shade, shade + 0.02, 1.0, cover * tex.a * 0.92);
            }
            ENDHLSL
        }
    }
}
