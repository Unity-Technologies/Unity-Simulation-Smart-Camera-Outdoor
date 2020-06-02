// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/GlitchShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			float _GlitchAmount;

			/////////////////////

			float sat(float t) {
				return clamp(t, 0.0, 1.0);
			}

			float2 sat(float2 t) {
				return clamp(t, 0.0, 1.0);
			}
			// vec3 sat( vec3 v ) {
			// return clamp( v, 0.0f, 1.0f );
			// }

			//remaps inteval [a;b] to [0;1]
			float remap(float t, float a, float b) {
				return sat((t - a) / (b - a));
			}

			//note: /\ t=[0;0.5;1], y=[0;1;0]
			float linterp(float t) {
				return sat(1.0 - abs(2.0*t - 1.0));
			}

			//note: [0;1]
			float rand(float2 n) {
				return frac(sin(dot(n.xy, float2(12.9898, 78.233)))* 43758.5453);
			}

			//note: [-1;1]
			float srand(float2 n) {
				return rand(n) * 2.0 - 1.0;
			}

			float mytrunc(float x, float num_levels)
			{
				return floor(x*num_levels) / num_levels;
			}

			float2 mytrunc(float2 x, float2 num_levels)
			{
				return floor(x*num_levels) / num_levels;
			}

			float3 rgb2yuv(float3 rgb)
			{
				float3 yuv;
				yuv.x = dot(rgb, float3(0.299, 0.587, 0.114));
				yuv.y = dot(rgb, float3(-0.14713, -0.28886, 0.436));
				yuv.z = dot(rgb, float3(0.615, -0.51499, -0.10001));
				return yuv;
			}
			float3 yuv2rgb(float3 yuv)
			{
				float3 rgb;
				rgb.r = yuv.x + yuv.z * 1.13983;
				rgb.g = yuv.x + dot(float2(-0.39465, -0.58060), yuv.yz);
				rgb.b = yuv.x + yuv.y * 2.03211;
				return rgb;
			}

			////////////

			fixed4 frag (v2f i) : SV_Target
			{
				float THRESHOLD = clamp(_GlitchAmount,0,1);
				float time_s = fmod(_Time.y, 32.0);

				float glitch_threshold = 1.0 - THRESHOLD;
				const float max_ofs_siz = 0.1; //TOOD: input
				const float yuv_threshold = 0.5; //TODO: input, >1.0f == no distort
				const float time_frq = 16.0;

				float2 uv = i.uv.xy;
				//uv.y = 1.0 - uv.y;

				const float min_change_frq = 4.0;
				float ct = mytrunc(time_s, min_change_frq);
				float change_rnd = rand(mytrunc(uv.yy,float2(16,16)) + 150.0 * ct);

				float tf = time_frq*change_rnd;

				float t = 5.0 * mytrunc(time_s, tf);
				float vt_rnd = 0.5*rand(mytrunc(uv.yy + t, float2(11,11)));
				vt_rnd += 0.5 * rand(mytrunc(uv.yy + t, float2(7,7)));
				vt_rnd = vt_rnd*2.0 - 1.0;
				vt_rnd = sign(vt_rnd) * sat((abs(vt_rnd) - glitch_threshold) / (1.0 - glitch_threshold));

				float2 uv_nm = uv;
				uv_nm = sat(uv_nm + float2(max_ofs_siz*vt_rnd, 0));

				float rnd = rand(mytrunc(time_s, 8.0));
				uv_nm.y = (rnd>lerp(1.0, 0.975, sat(THRESHOLD))) ? 1.0 - uv_nm.y : uv_nm.y;

				float4 smpl = tex2D(_MainTex, uv_nm);
				float3 smpl_yuv = rgb2yuv(smpl.rgb);
				smpl_yuv.y /= 1.0 - 3.0*abs(vt_rnd) * sat(yuv_threshold - vt_rnd);
				smpl_yuv.z += 0.125 * vt_rnd * sat(vt_rnd - yuv_threshold);
				return float4(yuv2rgb(smpl_yuv), smpl.a);
			}
			ENDCG
		}
	}
}
