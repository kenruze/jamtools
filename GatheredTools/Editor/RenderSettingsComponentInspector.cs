﻿using UnityEngine;
using UnityEditor;
using System.Collections;


[CustomEditor(typeof(RenderSettingsComponent))]
class RenderSettingsComponentInspector : Editor 
{

    public override void OnInspectorGUI() 
    {
        if(GUILayout.Button("Apply To Scene")) ApplyToScene();
        if(GUILayout.Button("Copy From Scene")) CopyFromScene();

        DrawDefaultInspector();
    }

    void ApplyToScene()
    {
        RenderSettingsComponent myTarget=(RenderSettingsComponent) target;

        UnityEngine.RenderSettings.fog=myTarget.fog;

        UnityEngine.RenderSettings.fogColor = myTarget.fogColor;
        UnityEngine.RenderSettings.fogMode = myTarget.fogMode;
        UnityEngine.RenderSettings.fogDensity = myTarget.fogDensity;

        UnityEngine.RenderSettings.fogStartDistance = myTarget.linearFogStart;
        UnityEngine.RenderSettings.fogEndDistance = myTarget.linearFogEnd;

        UnityEngine.RenderSettings.ambientLight = myTarget.ambientLight;

        RenderSettings.ambientEquatorColor = myTarget.ambientEquatorColor;
        RenderSettings.ambientGroundColor=myTarget.ambientGroundColor;
        RenderSettings.ambientIntensity=myTarget.ambientIntensity;
        RenderSettings.ambientMode=myTarget.ambientMode;
        RenderSettings.ambientProbe=myTarget.ambientProbe;
        RenderSettings.ambientSkyColor=myTarget.ambientSkyColor;

        UnityEngine.RenderSettings.skybox = myTarget.skyboxMaterial;

        UnityEngine.RenderSettings.haloStrength = myTarget.haloStrength;

        UnityEngine.RenderSettings.flareStrength = myTarget.flareStrength;
    }
    void CopyFromScene()
    {
        RenderSettingsComponent myTarget=(RenderSettingsComponent) target;

        myTarget.fog=UnityEngine.RenderSettings.fog;

        myTarget.fogColor=UnityEngine.RenderSettings.fogColor;
        myTarget.fogMode=UnityEngine.RenderSettings.fogMode;
        myTarget.fogDensity=UnityEngine.RenderSettings.fogDensity;

        myTarget.linearFogStart=UnityEngine.RenderSettings.fogStartDistance;
        myTarget.linearFogEnd=UnityEngine.RenderSettings.fogEndDistance;

        myTarget.ambientLight=UnityEngine.RenderSettings.ambientLight;
        myTarget.ambientEquatorColor=RenderSettings.ambientEquatorColor;
        myTarget.ambientGroundColor = RenderSettings.ambientGroundColor;
        myTarget.ambientIntensity = RenderSettings.ambientIntensity;
        myTarget.ambientMode = RenderSettings.ambientMode;
        myTarget.ambientProbe = RenderSettings.ambientProbe;
        myTarget.ambientSkyColor = RenderSettings.ambientSkyColor;
        myTarget.skyboxMaterial=UnityEngine.RenderSettings.skybox;

        myTarget.haloStrength=UnityEngine.RenderSettings.haloStrength;

    }
}