Shader "Custom/ParticleSys_SS"
{
	Properties
	{
		_colour("Colour", color) = (1.0, 0.0, 0.0, 1.0)
		_A("A", Range(100, 800)) = 400
		_B("B", Range(100, 800)) = 400
		_C("C", Range(100, 800)) = 400
		_I("Intensity", Range(1.0, 5.0)) = 3.0
		[Toggle] _V("Variant 2", Float) = 0
    }
    SubShader
    {
        Pass
        {
			ZTest Always
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

			struct Point 
			{
				float3 pos;
			};

			float _A, _B, _C, _I, _V;
			float4 _colour;
			StructuredBuffer<Point> cloud;


			float hash(float2 n)
			{
				return frac(sin(dot(n, float2(12.9897, 4.1414))) * 43758.5453);
			}
			
			float noise(float2 p)
			{
				float2 k = floor(p);
				float2 u = frac(p);
				u = u * u * (3.0 - 2.0 * u);
				float a = hash(k + float2(0.0, 0.0));
				float b = hash(k + float2(1.0, 0.0));
				float c = hash(k + float2(0.0, 1.0));
				float d = hash(k + float2(1.0, 1.0));
				float t = lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
				return t * t;
			}

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float3 variable : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (uint id : SV_VertexID)
            {
				v2f vs;
				Point T = cloud[id];
				float x = noise(float2(T.pos.yz*_Time.g*0.0001)) * _A;
				float y = noise(float2(T.pos.yz*_Time.g*0.0001)) * _B;
				float z = noise(float2(T.pos.yz*_Time.g*0.0001)) * _C;
				vs.variable = mul(unity_ObjectToWorld, T.pos);
				T.pos += float3(x, y, z);
				vs.vertex = UnityObjectToClipPos(float4(T.pos, 1.0));
				return vs;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float h = pow(length(i.variable) * 0.003, _I);
				if (_V == 0) 
				{
					return float4(_colour.rgb*h, 1.0);
				}
				else 
				{
					return float4(i.variable, 1.0);
				}
            }
            ENDCG
        }
    }
}
