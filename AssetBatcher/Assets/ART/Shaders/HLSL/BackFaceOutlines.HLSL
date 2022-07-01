#ifndef BACKFACEOUTLINES_INCLUDED
#define BACKFACEOUTLINES_INCLUDED

// Include helper functions from URP
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

// Data from the meshes
struct Attributes {
	float4 positionOS       : POSITION; // Position in object space
	float3 normalOS         : NORMAL; // Normal vector in object space
	#ifdef USE_PRECALCULATED_OUTLINE_NORMALS
	float3 smoothNormalOS   : TEXCOORD1; // Calculated "smooth" normals to extrude along in object space
	#endif
};

// Output from the vertex function and input to the fragment function
struct VertexOutput {
	float4 positionCS   : SV_POSITION; // Position in clip space
};

// Properties
float _Thickness;
float4 _Color;
float _DepthOffset;

VertexOutput Vertex(Attributes input) {
	VertexOutput output = (VertexOutput)0;

	float3 normalOS;
	#ifdef USE_PRECALCULATED_OUTLINE_NORMALS
	normalOS = input.smoothNormalOS;
	#else
	normalOS = input.normalOS;
	#endif

	// Extrude the object space position along a normal vector
	float3 posOS = input.positionOS.xyz + normalOS * _Thickness;
	// Convert this position to world and clip space
	output.positionCS = GetVertexPositionInputs(posOS).positionCS;

	float depthOffset = _DepthOffset;
	// If depth is reversed on this platform, reverse the offset
	#ifdef UNITY_REVERSED_Z
	depthOffset = -depthOffset;
	#endif
	output.positionCS.z += depthOffset;

	return output;
}

float4 Fragment(VertexOutput input) : SV_Target {
	return _Color;
}

static float2 sobelSamplePoints[9] = {
	float2(-1,1), float2(0,1), float2(1,1),
	float2(-1,0), float2(0,0), float2(1,1),
	float2(-1,-1), float2(0,-1), float2(1,-1),
};

static float sobelXMatrix[9] = {
	1, 0, -1,
	2, 0, -2,
	1, 0, -1
};

static float sobelYMatrix[9] = {
	1, 2, 1,
	0, 0, 0,
	-1, -2, -1
};

void DepthSobel_float(float2 UV, float Thickness, out float Out)
{
	float2 sobel = 0;
	[unroll] for(int i=0; i<9; i++)
	{
		float depth = SHADERGRPH_SAMPLE_SCENE_DEPTH(UV + sobelSamplePoints[i] * Thickness);
		sobel += depth * float2(sobelXMatrix[i], sobelYMatrix[i]);
	}
	Out = length(sobel);
}
#endif