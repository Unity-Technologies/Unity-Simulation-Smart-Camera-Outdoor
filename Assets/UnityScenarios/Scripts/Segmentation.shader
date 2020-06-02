Shader "Unlit/Segmentation"
{
    Properties
    {
        _SegmentColor ("Segment Color", Color) = (1,1,1,1)
    }

    CGINCLUDE
    #include "UnityCG.cginc"
    ENDCG

    SubShader
    {
        Tags 
        {
            "RenderType"="Opaque"
            "Queue" = "Overlay"  
        }

        Pass 
        {
            Cull Back
            CGPROGRAM
            #pragma vertex vert             
            #pragma fragment frag
         
            fixed4 _SegmentColor;
            
            struct vertInput {
                float4 pos : POSITION;
            };  

            struct vertOutput {
                float4 pos : SV_POSITION;
            };

            vertOutput vert(vertInput input) {
                vertOutput o;
                o.pos = UnityObjectToClipPos(input.pos);
                return o;
            }

            fixed4 frag(vertOutput output) : COLOR {
                return _SegmentColor;
            }
            ENDCG
        }  
   }
}