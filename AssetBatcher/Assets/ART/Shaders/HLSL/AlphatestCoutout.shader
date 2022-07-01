Shader "Obliy/AlphatestCoutout"
{
	Properties
	{
		_TintColor("Test Color", color) = (1, 1, 1, 1)
		_Intensity("Range Sample", Range(0, 1)) = 0.5
		_MainTex("Main Texture", 2D) = "white" {}
		_Alpha("AlphaCut", Range(0,1)) = 0.5
	}  

	SubShader
	{  	
		Tags
		{
			"RenderPipeline"="UniversalPipeline"
			"RenderType"="TransparentCutout"          
			"Queue"="Alphatest"
		}
    	Pass
		{  		
			Name "Universal Forward"
			Tags {"LightMode" = "UniversalForward"}
			HLSLPROGRAM
        	#pragma prefer_hlslcc gles
        	#pragma exclude_renderers d3d11_9x

        	#pragma vertex vert
        	#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			half4 _TintColor;
			float _Intensity;
			float _Alpha;

			float4 _MainTex_ST;
			Texture2D _MainTex;
			SamplerState sampler_MainTex;	
         	
         	struct VertexInput
         	{
         		float4 vertex : POSITION;
         		float2 uv       : TEXCOORD0;
         	};

			struct VertexOutput
			{
				float4 vertex  	: SV_POSITION;
				float2 uv      	: TEXCOORD0;
			};

			VertexOutput vert(VertexInput v)
			{
				VertexOutput o;
				o.vertex = TransformObjectToHClip(v.vertex.xyz);
				o.uv = v.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				return o;
			}

        	half4 frag(VertexOutput i) : SV_Target
        	{
        		float4 color = _MainTex.Sample(sampler_MainTex, i.uv);
        		color.rgb *= _TintColor * _Intensity;
        		clip(color.a - _Alpha);
        		return color;
        	}
			ENDHLSL  
		}
	}
}
