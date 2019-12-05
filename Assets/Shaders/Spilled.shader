Shader "Unlit/Spilled"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }


			float getVal(float2 uv)
			{
				return length(tex2D(_MainTex, uv).xyz);
			}

			float2 getGrad(float2 uv, float delta)
			{
				float2 d = float2(delta, 0);
				return float2(getVal(uv + d.xy) - getVal(uv - d.xy), getVal(uv + d.yx) - getVal(uv - d.yx))/ delta;
			}




            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 col;
				float2 uv = i.uv / 5;
				float3 n = float3(getGrad(uv, 1.0 / 5), 150.0);
				n = normalize(n);
				col = float4(n, 1);

				float3 light = normalize(float3(1, 1, 2));
				float diff = clamp(dot(n, light), 0.5, 1.0);
				float spec = clamp(dot(reflect(light, n), float3(0, 0, -1)), 0.0, 1.0);
				spec = pow(spec, 36.0)*2.5;
				//spec=0.0;
				col = diff;

                return col;
            }
            ENDCG
        }
    }
}
