Shader "Unlit/StencilOutline"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_OutlineColor("outline Color",Color) = (0,0,0,0)
		_OutlineWidth("outline Width",Range(0,5)) = 0.01
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100

			Pass
			{
				Tags{"Queue" = "Gemetry + 1"}
				Stencil
				{
					Ref 1
					Comp Always
					Pass Replace
				}

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
			// apply fog
			UNITY_APPLY_FOG(i.fogCoord, col);
			return col;
		}
		ENDCG
	}
	pass
	{
		Tags{"Queue" = "Gemetry + 2"}
		Stencil
		{
			Ref 1
			Comp NotEqual
			Pass keep
		}

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			fixed4 _OutlineColor;
			float _OutlineWidth;

			v2f vert(appdata v)
			{
				v2f o;
				//在模型空间里直接膨胀模型，也可以在别的空间里做
				//需要注意法线的变换，还要注意在别的空间里膨胀的话，顶点变换到裁剪空间时不是MVP了
				v.vertex.xyz += v.normal * _OutlineWidth;
				o.vertex = UnityObjectToClipPos(v.vertex);

				//// 在裁剪空间里膨胀，可以解决描边近大远小的问题
				//// 先把法线从模型空间转到相机空间
				//// 这里直接使用unity给我们提供的宏,等价于 vNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal))
				//// 为什么是(float3x3)UNITY_MATRIX_IT_MV，而不是 (float3x3)UNITY_MATRIX_MV，冯乐乐美女的4.7节是有这个解释的
				// fixed3 vNormal = COMPUTE_VIEW_NORMAL;
				//// 再把 vNormal 的xy转化到裁剪空间里去，z不要了
				// fixed2 pNormal = TransformViewToProjection(vNormal.xy);
				//// 开始膨胀模型
				// o.vertex.xy += pNormal * _OutlineWidth;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return _OutlineColor;
			}
			ENDCG
		}
		}
}