Shader "ShadowVolume/StencilWriter"{
  Properties{
    _ShadowZOffset ("Shadow ZOffset", Float) = 0.01
  }

  SubShader{
    Tags{ "RenderType"="Opaque" "Queue"="Geometry+1" }
    // we use offset to change z-testing. 
    // our shadow volume is capped at both ends using the mesh geometry
    // and this causes z-fighting if we dont offset the z-test
    // this can be removed if the shadow volume implementation removes the front cap
    Offset [_ShadowZOffset], [_ShadowZOffset]
    ZWrite Off
    LOD 100
    
    CGINCLUDE
    #include "UnityCG.cginc"
    
    struct appdata{
      float4 vertex : POSITION;
    };

    struct v2f{
      float4 position : SV_POSITION;
    };
    
    v2f vert(appdata v){
      v2f o;
      o.position = UnityObjectToClipPos(v.vertex);
      return o;
    }
    
    fixed4 frag(v2f i) : SV_TARGET{
      return 0;
    }
    ENDCG

    Pass{
      Cull Off
      Stencil{
        Ref 0
        PassBack DecrWrap
        PassFront IncrWrap
      }
      ColorMask 0
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      ENDCG
    }
    
    Pass{
      Cull Back
      Stencil{
        Ref 0
        Comp Less
      }
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      ENDCG
    }
  }
}