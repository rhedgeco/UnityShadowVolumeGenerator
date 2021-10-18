Shader "Hidden/ShadowVolumes/ShadowRender"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Stencil
        {
            Ref 0
            Comp Less
        }
        Color [_Color]

        Pass {}
    }
}