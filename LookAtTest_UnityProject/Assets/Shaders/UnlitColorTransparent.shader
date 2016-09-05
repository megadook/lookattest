// Unlit Transparency shader
// - no lighting
// - no lightmap support
// - no texture

Shader "Custom/Unlit/ColorTransparent" 
{
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
	}

	SubShader 
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100
	
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 

		Pass 
		{  
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
			
				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float4 color : COLOR;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					float4 color : COLOR;
					UNITY_FOG_COORDS(0)
				};

				fixed4 _Color;
			
				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.color = v.color * _Color;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}
			
				fixed4 frag (v2f i) : COLOR
				{
					fixed4 col = i.color;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
			ENDCG
		}
	}
}