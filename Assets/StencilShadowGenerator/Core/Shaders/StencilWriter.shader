Shader "Hidden/ShadowVolumes/StencilWriter"
{
    Properties
    {
        _Direction ("Direction", Vector) = (1,0,0,0)
        _Extrude ("Extrude", Float) = 10
        _Bias ("_Bias", Float) = 0.01
    }

    SubShader
    {
        Name "ShadowVolume"
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry+1"
            "LightMode"="ShadowVolume"
        }
        ZWrite Off

        Pass
        {
            Cull Off
            Stencil
            {
                ZFailBack IncrWrap
                ZFailFront DecrWrap
            }

            ColorMask 0

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float3 _Direction;
            float _Extrude;
            float _Bias;

            struct Attributes
            {
                half3 normal : NORMAL;
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 worldNormal = UnityObjectToWorldNormal(IN.normal);
                float3 worldPos = mul(unity_ObjectToWorld, IN.positionOS.xyz);
                
                if (dot(worldNormal, _Direction) > 0) worldPos += normalize(_Direction) * _Extrude;
                else worldPos += normalize(_Direction) * _Bias;

                IN.positionOS.xyz = mul(unity_WorldToObject, worldPos);
                OUT.positionHCS = UnityObjectToClipPos(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag() : SV_Target
            {
                return 0;
            }
            ENDCG
        }
    }
}