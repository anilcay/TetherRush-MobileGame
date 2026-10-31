Shader "Tether Rush/NeonGlow"
{
    Properties { [HDR] _BaseColor ("Glow color", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+1" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha One
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
            CBUFFER_END
            float _TRFogStart, _TRFogEnd;
            struct Attributes { float4 positionOS : POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; float distance : TEXCOORD1; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 world = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(world);
                output.color = input.color; output.uv = input.uv;
                output.distance = distance(_WorldSpaceCameraPos,world);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half4 color = _BaseColor * input.color;
                half edge = saturate(1-abs(input.uv.y*2-1));
                color.a *= edge*edge;
                float fog = _TRFogEnd > _TRFogStart ? saturate((input.distance-_TRFogStart)/max(.001,_TRFogEnd-_TRFogStart)) : 0;
                color.a *= 1-fog;
                return color;
            }
            ENDHLSL
        }
    }
}
