Shader "Tether Rush/NeonSurface"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.1,0.8,0.8,1)
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,1)
        _Smoothness ("Smoothness", Range(0,1)) = .55
        _RimStrength ("Edge light", Range(0,1)) = .18
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "NeonSurface"
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _EmissionColor;
                float _Smoothness;
                float _RimStrength;
            CBUFFER_END
            float4 _TRFogColor;
            float _TRFogStart, _TRFogEnd;
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; float3 positionWS : TEXCOORD1; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half3 n = normalize(input.normalWS);
                half3 v = normalize(GetWorldSpaceViewDir(input.positionWS));
                half3 l = normalize(half3(-.35,.8,-.45));
                half diffuse = saturate(dot(n,l));
                half sky = saturate(n.y * .5h + .5h);
                half3 ambient = lerp(half3(.20,.24,.36),half3(.46,.56,.67),sky);
                half rim = pow(1 - saturate(dot(n,v)),3) * _RimStrength;
                half specular = pow(saturate(dot(n,normalize(l+v))),lerp(12,96,_Smoothness)) * (.16h + _Smoothness * .35h);
                half3 color = _BaseColor.rgb * (ambient + diffuse * .65h);
                color += half3(.69,.83,1) * specular + _BaseColor.rgb * rim + _EmissionColor.rgb;
                float fog = _TRFogEnd > _TRFogStart ? saturate((distance(_WorldSpaceCameraPos,input.positionWS)-_TRFogStart)/max(.001,_TRFogEnd-_TRFogStart)) : 0;
                fog = fog * fog * (3 - 2 * fog);
                return half4(lerp(color,_TRFogColor.rgb,fog),1);
            }
            ENDHLSL
        }
    }
}
