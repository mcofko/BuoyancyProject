// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/RippleWaterShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Bumpmap", 2D) = "bump" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_Scale("Scale", float) = 1
		_Freq("Freq", float) = 1
		_Speed("Speed", float) = 1
		_WaveAmpl1("WaveAmpl1", float) = 0
		_WaveAmpl2("WaveAmpl2", float) = 0
		_WaveAmpl3("WaveAmpl3", float) = 0
		_WaveAmpl4("WaveAmpl4", float) = 0
		_WaveAmpl5("WaveAmpl5", float) = 0
		_WaveAmpl6("WaveAmpl6", float) = 0
		_WaveAmpl7("WaveAmpl7", float) = 0
		_WaveAmpl8("WaveAmpl8", float) = 0

		_TestX("TestX", float) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 customValue;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _Scale;
		float _Speed;
		float _Freq; 
		float _TestX;
		float _WaveAmpl1, _WaveAmpl2, _WaveAmpl3, _WaveAmpl4, _WaveAmpl5, _WaveAmpl6, _WaveAmpl7, _WaveAmpl8;
		float _OffsetX1, _OffsetZ1, _OffsetX2, _OffsetZ2, _OffsetX3, _OffsetZ3, _OffsetX4, _OffsetZ4, _OffsetX5, _OffsetZ5, _OffsetX6, _OffsetZ6, _OffsetX7, _OffsetZ7, _OffsetX8, _OffsetZ8;
		float _Distance1, _Distance2, _Distance3, _Distance4, _Distance5, _Distance6, _Distance7, _Distance8;
		float _xImpact1, _zImpact1, _xImpact2, _zImpact2, _xImpact3, _zImpact3, _xImpact4, _zImpact4, _xImpact5, _zImpact5, _xImpact6, _zImpact6,
			_xImpact7, _zImpact7, _xImpact8, _zImpact8;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			if (_TestX == 0) _TestX = v.vertex.x;

			half offsetvert = (v.vertex.x * v.vertex.x) + (v.vertex.z * v.vertex.z);
			half offsetvert2 = v.vertex.x * v.vertex.z;

			half value0 = _Scale * sin(_Time.w * _Speed + offsetvert2 * _Freq);

			half value1 = _Scale * sin(_Time.w * _Speed + offsetvert * _Freq + (v.vertex.x * _OffsetX1) + (v.vertex.z * _OffsetZ1));
			half value2 = _Scale * sin(_Time.w * _Speed + offsetvert * _Freq + (v.vertex.x * _OffsetX2) + (v.vertex.z * _OffsetZ2));
			half value3 = _Scale * sin(_Time.w * _Speed + offsetvert * _Freq + (v.vertex.x * _OffsetX3) + (v.vertex.z * _OffsetZ3));
			half value4 = _Scale * sin(_Time.w * _Speed + offsetvert * _Freq + (v.vertex.x * _OffsetX4) + (v.vertex.z * _OffsetZ4));
			half value5 = _Scale * sin(_Time.w * _Speed + offsetvert * _Freq + (v.vertex.x * _OffsetX5) + (v.vertex.z * _OffsetZ5));
			half value6 = _Scale * sin(_Time.w * _Speed + offsetvert * _Freq + (v.vertex.x * _OffsetX6) + (v.vertex.z * _OffsetZ6));
			half value7 = _Scale * sin(_Time.w * _Speed + offsetvert * _Freq + (v.vertex.x * _OffsetX7) + (v.vertex.z * _OffsetZ7));
			half value8 = _Scale * sin(_Time.w * _Speed + offsetvert * _Freq + (v.vertex.x * _OffsetX8) + (v.vertex.z * _OffsetZ8));
			
			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

			v.vertex.y += value0;
			o.customValue.y += value0;

			if (sqrt( pow(worldPos.x - _xImpact1, 2) + pow(worldPos.z - _zImpact1, 2) ) < _Distance1) {
				v.vertex.y += value1 * _WaveAmpl1;
				o.customValue += value1 * _WaveAmpl1;
			}
			
			if (sqrt(pow(worldPos.x - _xImpact2, 2) + pow(worldPos.z - _zImpact2, 2)) < _Distance2) {
				v.vertex.y += value2 * _WaveAmpl2;
				o.customValue += value2 * _WaveAmpl2;
			}

			if (sqrt(pow(worldPos.x - _xImpact3, 2) + pow(worldPos.z - _zImpact3, 2)) < _Distance3) {
				v.vertex.y += value3 * _WaveAmpl3;
				o.customValue += value3 * _WaveAmpl3;
			}

			if (sqrt(pow(worldPos.x - _xImpact4, 2) + pow(worldPos.z - _zImpact4, 2)) < _Distance4) {
				v.vertex.y += value4 * _WaveAmpl4;
				o.customValue += value4 * _WaveAmpl4;
			}

			if (sqrt(pow(worldPos.x - _xImpact5, 2) + pow(worldPos.z - _zImpact5, 2)) < _Distance5) {
				v.vertex.y += value5 * _WaveAmpl5;
				o.customValue += value5 * _WaveAmpl5;
			}

			if (sqrt(pow(worldPos.x - _xImpact6, 2) + pow(worldPos.z - _zImpact6, 2)) < _Distance6) {
				v.vertex.y += value6 * _WaveAmpl6;
				o.customValue += value6 * _WaveAmpl6;
			}

			if (sqrt(pow(worldPos.x - _xImpact7, 2) + pow(worldPos.z - _zImpact7, 2)) < _Distance7) {
				v.vertex.y += value7 * _WaveAmpl7;
				o.customValue += value7 * _WaveAmpl7;
			}

			if (sqrt(pow(worldPos.x - _xImpact8, 2) + pow(worldPos.z - _zImpact8, 2)) < _Distance8) {
				v.vertex.y += value8 * _WaveAmpl8;
				o.customValue += value8 * _WaveAmpl8;
			}
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			o.Normal.y += IN.customValue;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			
		}
		ENDCG
	}
	FallBack "Diffuse"
}
