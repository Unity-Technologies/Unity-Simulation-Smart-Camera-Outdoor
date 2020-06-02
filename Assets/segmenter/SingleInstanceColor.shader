Shader "Unlit/SingleInstanceColor"
{
     Properties
    {
        _InstanceColor ("Instance Color", Color) = (1,1,1,1)
        
    }
    
    CGINCLUDE
    #include "UnityCG.cginc"
    ENDCG
    
    
    // Opaque Overlay
    SubShader
    {
        Tags 
        {
            "RenderType"="Opaque"
            "Queue" = "Overlay"  
        }
       
       
        // Colour pass
        Pass {
            Cull Back
            CGPROGRAM
            #pragma vertex vert             
            #pragma fragment frag
         
            fixed4 _InstanceColor;
            
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
                return _InstanceColor;
            }
            ENDCG
        }  
   }
   // Transparent Overlay
    SubShader
    {
        Tags 
        {
            "RenderType"="Transparent"
            "Queue" = "Overlay"  
        }
       
       
        // Colour pass
        Pass {
            Cull Back
            CGPROGRAM
            #pragma vertex vert             
            #pragma fragment frag
         
            fixed4 _InstanceColor;
            
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
                return _InstanceColor;
            }
            ENDCG
        }  
   }


      // Nature Prefab
    SubShader
    {
        Tags 
        {
            "RenderType"="TreeLeaf"
            "Queue" = "Geometry"  
        }
       
       
        // Colour pass
        Pass {
            Cull Back
            CGPROGRAM
            #pragma vertex vert             
            #pragma fragment frag
         
            fixed4 _InstanceColor;
            
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
                return _InstanceColor;
            }
            ENDCG
        }  
   }
}
