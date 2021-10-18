Shader "Hidden/ShadowVolumes/StencilWriter"
{
    Properties
    {
        _ShadowZOffset ("Shadow ZOffset", Range(0,1)) = 0.01
    }

    SubShader
    {
        Name "ShadowVolume"
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry+1"
        }
        // we use offset to change z-testing. 
        // our shadow volume is capped at both ends using the mesh geometry
        // and this causes z-fighting if we dont offset the z-test
        // this can be removed if the shadow volume implementation removes the front cap
        Offset [_ShadowZOffset], [_ShadowZOffset]
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