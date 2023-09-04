Shader "HighlightersURP/AlphaBlit"
{
   HLSLINCLUDE

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		TEXTURE2D(_BlurredObjectsBoth);
		SAMPLER(sampler_BlurredObjectsBoth);

		TEXTURE2D(_ObjectsInfo);
		SAMPLER(sampler_ObjectsInfo);

		float4 _ColorFront;
		float4 _ColorBack;

		float4 _RenderingBounds;

		struct Attributes
		{
			float4 positionOS : POSITION;
			float2 uv : TEXCOORD0;
			uint vertexID : SV_VertexID;

			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct Varyings
		{
			float4 positionCS : SV_POSITION;
			float2 uv : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
		};

		Varyings VertexSimple(Attributes input)
		{
			Varyings output = (Varyings)0;

			UNITY_SETUP_INSTANCE_ID(input);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

			float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
			float2 uv = GetFullScreenTriangleTexCoord(input.vertexID);

			output.positionCS = pos;
			output.uv = uv;

			return output;
		}

		
		bool ShouldRender(float2 positionVS)
		{
			//return true;

			if(_RenderingBounds.x < positionVS.x && _RenderingBounds.y < positionVS.y && _RenderingBounds.z > positionVS.x && _RenderingBounds.w > positionVS.y)
			{
				return true;
			}

			return false;
		}

		float4 frontBlitFrag(Varyings i) : SV_TARGET
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
			
			//return float4(1, 0, 0, 0.5);

			if(!ShouldRender(i.uv)) return float4(0,0,0,0);

			float2 uv = UnityStereoTransformScreenSpaceTex(i.uv);

			float4 mask = SAMPLE_TEXTURE2D(_ObjectsInfo, sampler_ObjectsInfo, uv);

			if (mask.r > 0 || mask.g >0)
			{
				return float4(0,0,0,0);
			}

            float2 blurredObjects = SAMPLE_TEXTURE2D(_BlurredObjectsBoth, sampler_BlurredObjectsBoth, uv).rg;

			return float4( _ColorFront.rgb,saturate(blurredObjects.g * _ColorFront.a));
        }

		float4 backBlitFrag(Varyings i) : SV_TARGET
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		
			if(!ShouldRender(i.uv)) return float4(0,0,0,0);

			float2 uv = UnityStereoTransformScreenSpaceTex(i.uv);

			float4 mask = SAMPLE_TEXTURE2D(_ObjectsInfo, sampler_ObjectsInfo, uv);

			if (mask.r > 0 || mask.g >0)
			{
				return float4(0,0,0,0);
			}

            float2 blurredObjects = SAMPLE_TEXTURE2D(_BlurredObjectsBoth, sampler_BlurredObjectsBoth, uv).rg;

			return float4( _ColorBack.rgb,saturate(blurredObjects.r * _ColorBack.a));

        }
	ENDHLSL

	SubShader
	{
		Tags { "RenderPipeline" = "UniversalPipeline" }

		ZWrite Off
		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency


		Pass
		{
			Name "FrontBlitPass"

			HLSLPROGRAM
	        #pragma multi_compile_instancing

            #pragma vertex VertexSimple
            #pragma fragment frontBlitFrag
            ENDHLSL
		}

		Pass
		{
			Name "BackBlitPass"

			HLSLPROGRAM
	        #pragma multi_compile_instancing

            #pragma vertex VertexSimple
            #pragma fragment backBlitFrag
            ENDHLSL
		}
	}
}
