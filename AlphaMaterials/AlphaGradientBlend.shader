Shader "Custom/AlphaGradientBlend" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_GradientRamp ("Gradient Ramp (Alpha)", 2D) = "white" {}
		_RampHeight ("Ramp Height", Range(-1,1)) = 0.0
	}

	SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 200

CGPROGRAM
#pragma surface surf Lambert alpha

sampler2D _MainTex;
sampler2D _GradientRamp;
		half _RampHeight;
		fixed4 _Color;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	fixed4 c2 = tex2D(_GradientRamp, IN.uv_MainTex);
	o.Alpha = c.a * min(1, c2.rgba - _RampHeight);
	o.Albedo = c.rgb * o.Alpha;
}
ENDCG
}

Fallback "Transparent/VertexLit"
}

