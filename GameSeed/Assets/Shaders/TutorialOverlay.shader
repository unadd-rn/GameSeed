Shader "Unlit/TutorialOverlay"
{
    Properties
    {
        _Color ("Overlay Color", Color) = (0,0,0,0.5)
        _CenterX ("Center X (UV)", Float) = 0.5
        _CenterY ("Center Y (UV)", Float) = 0.5
        _Width ("Width (UV)", Float) = 0.2
        _Height ("Height (UV)", Float) = 0.1
        _CornerRadius ("Corner Radius (UV)", Float) = 0.01
        _Softness ("Edge Softness", Float) = 0.005
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Overlay"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "TutorialOverlay"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _CenterX;
                float _CenterY;
                float _Width;
                float _Height;
                float _CornerRadius;
                float _Softness;
            CBUFFER_END

            float RoundedBoxSDF(float2 uv, float2 center, float2 halfSize, float cornerRadius)
            {
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 p = (uv - center) * float2(aspect, 1.0);
                float2 size = halfSize * float2(aspect, 1.0);

                float2 d = abs(p) - size + cornerRadius;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0) - cornerRadius;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 center = float2(_CenterX, _CenterY);
                float2 halfSize = float2(_Width * 0.5, _Height * 0.5);

                float dist = RoundedBoxSDF(IN.uv, center, halfSize, _CornerRadius);
                float alpha = smoothstep(-_Softness, _Softness, dist);

                return half4(_Color.rgb, _Color.a * alpha);
            }
            ENDHLSL
        }
    }
}