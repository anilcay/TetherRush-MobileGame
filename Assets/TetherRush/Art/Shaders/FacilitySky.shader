Shader "Tether Rush/FacilitySky"
{
    Properties
    {
        _BaseColor ("Base tint", Color) = (1,1,1,1)
        _HorizonColor ("Horizon", Color) = (.065,.12,.19,1)
        _ZenithColor ("Zenith", Color) = (.012,.025,.06,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Background" "RenderType"="Background" }
        Pass
        {
            Cull Off
            ZWrite Off
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"
            float4 _BaseColor, _HorizonColor, _ZenithColor;
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 direction : TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = UnityObjectToClipPos(input.positionOS);
                // Skybox cube vertices encode a view direction, never a finite world position.
                output.direction = input.positionOS.xyz;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half horizon = exp2(-abs(normalize(input.direction).y) * 5);
                half3 color = lerp(_ZenithColor.rgb,_HorizonColor.rgb,horizon);
                return half4(color * _BaseColor.rgb,1);
            }
            ENDCG
        }
    }
}
