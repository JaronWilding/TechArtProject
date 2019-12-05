Shader "Custom/MyTextureBomber"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Contrast("Contrast", Range(0, 10)) = 1
		_Resolution("Resolution", Range(1, 10)) = 1
		_Seed("Seed", Range(1,100)) = 1
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
			half _Contrast;
			half _Resolution;
			half _Seed;

			// Custom functions

			half3 ContrastCurve(half3 colour, half contrast)
			{
			#if !UNITY_COLORSPACE_GAMMA
				colour = LinearToGammaSpace(colour);
			#endif
				colour = saturate(lerp(half3(0.5, 0.5, 0.5), colour, contrast));
			#if !UNITY_COLORSPACE_GAMMA
				colour = GammaToLinearSpace(colour);
			#endif

				return colour;
			}

			half2 N22(half2 _pos)
			{
				half3 a = frac(_pos.xyx * half3(123.34, 234.34, 345.65));
				a += dot(a, a + 34.45);
				return frac(half2(a.x * a.y, a.y * a.z));
			}

			// Default functions

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

                fixed3 col = tex2D(_MainTex, uv);


				float m = 0;
				float t = _Time.y * 0.1;

				float minDist = 100;
				float cellIndex = 0;

				if (false)
				{
					for (float ii = 0; ii < 200; ii++)
					{
						float2 n = N22(half2(ii, ii)); //noise
						float2 p = sin(n * t) * _Resolution; //particle point

						float d = length(uv - p);
						m += smoothstep(0.02, 0.01, d);

						if (d < minDist)
						{
							minDist = d;
							cellIndex = ii;
						}
					}
				}
				else 
				{
					uv *= 3;

					float2 gv = frac(uv) - 0.5;
					float2 id = floor(uv);

					float2 cellID = float2(0, 0);

					for (float yy = -1; yy <= 1; yy++)
					{
						for (float xx = -1; xx <= 1; xx++)
						{
							float2 offs = float2(xx, yy);
							
							float2 n = N22(half2(id + offs));
							float2 p = offs + sin(n * _Seed) * 0.5;

							p -= gv;

							float ed = length(p);
							float md = abs(p.x) + abs(p.y);

							float d = ed;

							if (d < minDist)
							{
								minDist = d;
								//cellIndex = ii;
								cellID = id + offs;
							}

						}
					}
					
					col.rgb = minDist.r ;
				}


				//col = cellIndex/ 200;
				

				//col.rgb = ContrastCurve(col.ggg, _Contrast);


                return fixed4(col, 1.0);
            }
            ENDCG
        }
    }
}
