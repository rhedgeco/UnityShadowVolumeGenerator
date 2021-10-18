Shader "Hidden/ShadowVolumes/StencilWriter"
{
    SubShader
    {
        Name "ShadowVolume"
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry+1"
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
        }
    }
}