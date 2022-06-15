Shader "URPUnlit/URPZtest"
{
    Properties
    {
        _Color("Color", color) = (1,1,1,1)
        
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
    }
    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"="Opaque"
            "Queue" = "Geometry"
        }
        Pass
        {
            Name "Universal Forward"
            Tags {"LightMode" = "UniversalForward"}
            
            ZTest [_ZTest]

            
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            half4 _Color;

            struct VertexInput
            {
                float4 vertex : POSITION;
            };

            struct VertexOutput
            {
                float4 vertex : SV_POSITION;
            };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                return o;
            }

            half4 frag(VertexOutput i) : SV_Target
            {
                float4 color =  _Color;
                return color;
            }
            ENDHLSL
        }
    }
}
