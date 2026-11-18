// Ortam ışığı ve parıltı için ışıksız sprite (görsel analiz, 18.11.2026).
// Karışım modu malzemeden seçilir: çarpma (DstColor, Zero) gece ve hava tonu için,
// toplama (SrcAlpha, One) ocak ve mum parıltısı için. Resources altında olduğu için
// derlemeye girer ve Shader.Find ile bulunur.
// Unity 6'da SpriteRenderer rengi köşe rengiyle değil unity_SpriteColor ile gelir (Core2D.hlsl);
// URP'nin Sprite-Unlit-Default'u gibi ikisi çarpılır.
Shader "Thatchwick/BlendSprite"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Kaynak karışım", Float) = 2
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Hedef karışım", Float) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }
        Cull Off
        ZWrite Off
        Blend [_SrcBlend] [_DstBlend]

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

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
                o.positionCS = TransformObjectToHClip(input.positionOS);
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
