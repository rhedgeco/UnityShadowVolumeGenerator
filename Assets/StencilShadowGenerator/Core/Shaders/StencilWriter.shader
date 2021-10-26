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
            "LightMode"="ShadowVolume"
        }
        ZWrite Off

        Pass
        {
            // draw front and back faces
            Cull Off

            // depth fail stencil logic
            Stencil
            {
                ZFailBack IncrWrap
                ZFailFront DecrWrap
            }

            // dont draw any color
            ColorMask 0

            // shader code to extrude mesh
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            CBUFFER_START(UnityPerMaterial)
            float3 _Direction;
            float _Extrude;
            float _Bias;
            CBUFFER_END

            struct Attributes
            {
                float3 normal : NORMAL;
                float4 position : POSITION;
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 worldNormal = UnityObjectToWorldNormal(IN.normal);
                float3 worldPos = mul(unity_ObjectToWorld, IN.position.xyz);
                
                if (dot(worldNormal, _Direction) > 0) worldPos += normalize(_Direction) * _Extrude;

                // we use an interesting take on "shadow bias" here
                // shadow bias is usually pushing the depth at which a shadow appears
                // so basically nudging the shadow along the axis that the light is casting
                // but since we are using meshes here, we can nudge in any direction
                // in this case, we nudge in the direction of the camera view
                // this almost always prevents any sort of z-fighting from appearing
                float3 viewDir = mul((float3x3)unity_CameraToWorld, float3(0,0,1));
                worldPos += normalize(viewDir) * _Bias;

                IN.position.xyz = mul(unity_WorldToObject, worldPos);
                OUT.position = UnityObjectToClipPos(IN.position.xyz);
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