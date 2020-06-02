Shader "Indago/Unlit/Transparent Cutout Position Color" {
	Properties{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
		_Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
	}
		SubShader{
			Tags {"Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout"}
			LOD 100

			Lighting Off

			Pass {
				CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					#pragma target 2.0
					#pragma multi_compile_fog

					#include "UnityCG.cginc"

					struct appdata_t {
						float4 vertex : POSITION;
						float2 texcoord : TEXCOORD0;
						UNITY_VERTEX_INPUT_INSTANCE_ID
					};

					struct v2f {
						float4 vertex : SV_POSITION;
						float2 texcoord : TEXCOORD0;
						float4 color :COLOR;
						UNITY_FOG_COORDS(1)
						UNITY_VERTEX_OUTPUT_STEREO
					};

					sampler2D _MainTex;
					float4 _MainTex_ST;
					fixed _Cutoff;
					fixed4 _Color;

					float rand(float3 co)
					{
						return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
					}

					v2f vert(appdata_t v)
					{
						v2f o;
						UNITY_INITIALIZE_OUTPUT(v2f, o);
						UNITY_SETUP_INSTANCE_ID(v);
						UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

						o.vertex = UnityObjectToClipPos(v.vertex);
						o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
						float4 pos = mul(unity_ObjectToWorld, v.vertex);
						float colorR = rand(pos)*0.1f;
						o.color.r = colorR;

						UNITY_TRANSFER_FOG(o, o.vertex);
						return o;
					}

					fixed4 frag(v2f i) : SV_Target
					{


						fixed4 col = tex2D(_MainTex, i.texcoord);
						clip(col.a - _Cutoff - 0.1f);
						col.r += i.color;

						float edgeHeight = 0.015;

						col = lerp(col, fixed4(0, 4, 2, 1), step(col.a, _Cutoff + edgeHeight))*_Color;


						UNITY_APPLY_FOG(i.fogCoord, col);
						UNITY_OPAQUE_ALPHA(col.a);
						return col;
					}
				ENDCG
			}
		}

}