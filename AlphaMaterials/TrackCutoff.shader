Shader "JamTools/TrackCutoff" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_TrackDefault ("Default Track Value", Float) = 0
	_Emission ("Emission", Float) = 0
	_TrackIgnoreValue ("Track Ignore Value", Float) = -1
	_TrackAdjustOffset ("Offset Track Adjustment", Float) = 0
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_TrackTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_Cutoff ("Alpha Track", Range(0,1)) = 0.5
	_Tolerance ("Alpha Tolerance", Range(0,1)) = 0.1
	_RCutoff ("Red Track", Range(0,1)) = 0.5
	_RTolerance ("Red Tolerance", Range(0,1)) = 0.1
	_GCutoff ("Green Track", Range(0,1)) = 0.5
	_GTolerance ("Green Tolerance", Range(0,1)) = 0.1
	_BCutoff ("Blue Track", Range(0,1)) = 0.5
	_BTolerance ("Blue Tolerance", Range(0,1)) = 0.1
}

SubShader {
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
	LOD 200
	
CGPROGRAM
#pragma surface surf Lambert

sampler2D _MainTex;
sampler2D _TrackTex;
fixed4 _Color;
fixed _Cutoff;
fixed _Tolerance;
fixed _RCutoff;
fixed _RTolerance;
fixed _GCutoff;
fixed _GTolerance;
fixed _BCutoff;
fixed _BTolerance;
fixed _TrackDefault;
fixed _TrackIgnoreValue;
fixed _TrackAdjustOffset;
fixed _Emission;

struct Input {
	float2 uv_MainTex;
	float2 uv_TrackTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	fixed4 t = tex2D(_TrackTex, IN.uv_TrackTex);
	fixed4 cut = {_TrackDefault,_TrackDefault,_TrackDefault,_TrackDefault};
	if(t.a != _TrackIgnoreValue)
		cut.a = min((t.a-_Cutoff+_TrackAdjustOffset),(-t.a+_Cutoff+_TrackAdjustOffset)) +_Tolerance;
	if(t.r != _TrackIgnoreValue)
		cut.r = min((t.r-_RCutoff+_TrackAdjustOffset),(-t.r+_RCutoff+_TrackAdjustOffset)) +_RTolerance;
	if(t.g != _TrackIgnoreValue)
		cut.g = min((t.g-_GCutoff+_TrackAdjustOffset),(-t.g+_GCutoff+_TrackAdjustOffset)) +_GTolerance;
	if(t.b != _TrackIgnoreValue)
		cut.b = min((t.b-_BCutoff+_TrackAdjustOffset),(-t.b+_BCutoff+_TrackAdjustOffset)) +_BTolerance;
	clip( max(max(cut.a, cut.r), max(cut.g, cut.b)));

	o.Albedo = c.rgb;
	o.Emission = c.rgb * _Emission;
	o.Alpha = 0;// c.a * _Color.a;
}
ENDCG
}

Fallback "Legacy Shaders/Transparent/Cutout/VertexLit"
}
