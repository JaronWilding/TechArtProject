Shader "Custom/TextureBomber"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Resolution("Resolution", Range(1, 10)) = 1
		[MaterialToggle] _UseHash("Use Hash", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _Resolution;
			float _UseHash;

			float4 hash4(float2 _pos)
			{
				float a = 1.0 + dot(_pos, float2(37.0, 17.0));
				float b = 2.0 + dot(_pos, float2(11.0, 47.0));
				float c = 3.0 + dot(_pos, float2(41.0, 29.0));
				float d = 4.0 + dot(_pos, float2(23.0, 31.0));
				return frac(sin(float4(a, b, c, d) * 103.0));
			}


			fixed4 TextureBomberTech1(sampler2D _tex, in float2 uv)
			{
				float2 iuv = floor(uv);
				float2 fuv = frac(uv);

				float4 offsetA = hash4(iuv + float2(0.0, 0.0));
				float4 offsetB = hash4(iuv + float2(1.0, 0.0));
				float4 offsetC = hash4(iuv + float2(0.0, 1.0));
				float4 offsetD = hash4(iuv + float2(1.0, 1.0));

				float2 dx = ddx(uv);
				float2 dy = ddy(uv);

				offsetA.zw = sign(offsetA.zw - 0.5);
				offsetB.zw = sign(offsetB.zw - 0.5);
				offsetC.zw = sign(offsetC.zw - 0.5);
				offsetD.zw = sign(offsetD.zw - 0.5);

				float2 uvA = uv * offsetA.zw + offsetA.xy;
				float2 uvB = uv * offsetB.zw + offsetB.xy;
				float2 uvC = uv * offsetC.zw + offsetC.xy;
				float2 uvD = uv * offsetD.zw + offsetD.xy;

				float2 dxA = dx * offsetA.zw;
				float2 dxB = dx * offsetB.zw;
				float2 dxC = dx * offsetC.zw;
				float2 dxD = dx * offsetD.zw;

				float2 dyA = dy * offsetA.zw;
				float2 dyB = dy * offsetB.zw;
				float2 dyC = dy * offsetC.zw;
				float2 dyD = dy * offsetD.zw;

				float2 blender = smoothstep(0.25, 0.7f, fuv);

				float4 mixA = lerp(tex2D(_tex, uvA, dxA, dyA), tex2D(_tex, uvB, dxB, dyB), blender.x);
				float4 mixB = lerp(tex2D(_tex, uvC, dxC, dyC), tex2D(_tex, uvD, dxD, dyD), blender.x);
				fixed4 col = lerp(mixA, mixB, blender.y);


				return col;
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float2 uv = i.uv * _Resolution;
				fixed4 col;
				if (_UseHash == 1)
				{
					col = TextureBomberTech1(_MainTex, uv);
				}
				else
				{
					col = tex2D(_MainTex, uv);
				}
                
                return col;
            }
            ENDCG
        }
    }
}
