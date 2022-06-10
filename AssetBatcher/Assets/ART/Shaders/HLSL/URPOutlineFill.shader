Shader "Obliy/URPOutlineFill"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0

        _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineWidth("Outline Width", Range(0, 10)) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent+110"
            "DisableBatching" = "True"
        }
        Pass
        {
            Name "Universal Forward"
            Cull Off
            ZTest[_ZTest]
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGB
            
            Stencil {
                Ref 1
                Comp NotEqual
            }
            
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            half4  _OutlineColor;
            float _OutlineWidth;

            struct VertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct VertexOutput
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
            };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;
                float3 viewPosition = v.vertex.xyz;
                float3 viewNormal =  TransformObjectToWorldNormal(v.normal);
                o.vertex = TransformObjectToHClip(viewPosition + viewNormal  * _OutlineWidth / 10.0);
                return o;
            }

            half4 frag(VertexOutput i) : SV_Target
            {
                float4 color = _OutlineColor;
                return color;
            }
            ENDHLSL
        }
    }
}