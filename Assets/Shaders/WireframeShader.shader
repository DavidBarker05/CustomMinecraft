Shader "Custom/WireframeShader"
{
    Properties
    {
        [MainColor] _LineColour ("Line Colour", Color) = (1, 1, 1, 1)
        _LineThickness ("Line Thickness", Float) = 0.1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }
        
        ZWrite On
        Cull Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 positionOS : TEXCOORD0;
                float3 normalOS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _LineColour;
                float _LineThickness;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                ZERO_INITIALIZE(Varyings, OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionOS = IN.positionOS;
                OUT.normalOS = normalize(IN.normalOS);
                return OUT;
            }

            float3 ProjectVectorOnPlane(float3 v, float3 n)
            {
                n = normalize(n);
                return v - n * dot(v, n);
            }

            float Line(float axis)
            {
                if (abs(axis) >= 0.5 - _LineThickness) return 1;
                return 0;
            }

            float Square(float4 positionOS)
            {
                float x = Line(positionOS.x);
                float y = Line(positionOS.y);
                float z = Line(positionOS.z);
                return saturate(x + y + z);
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // Per face allows for line on each face rather than covering each face since technically all sides fit one requirement
                // I did this in my grid shader
                float4 postionOSFace = float4(ProjectVectorOnPlane(IN.positionOS.xyz, IN.normalOS), 1);
                float square = Square(postionOSFace);
                clip(square - 0.5);
                return _LineColour;
            }
            ENDHLSL
        }
    }
}
