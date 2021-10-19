Shader "Hidden/ShadowVolumes/ShadowRender"
{
    SubShader
    {
        Stencil
        {
            Ref 0
            Comp Less
        }
        Color (0,0,0,1)
        Pass {}
    }
}