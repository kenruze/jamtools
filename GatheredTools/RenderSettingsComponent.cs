//Source
//http://answers.unity3d.com/questions/248079/global-render-settings.html

using UnityEngine;

public class RenderSettingsComponent : MonoBehaviour {

    public bool fog;
    public Color fogColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
    public FogMode fogMode = FogMode.ExponentialSquared;
    public float fogDensity = 0.01f;

    public float linearFogStart = 0.0f;
    public float linearFogEnd = 300.0f;

    public Color ambientLight = new Color(0.2f, 0.2f, 0.2f, 1.0f);
    public Color ambientEquatorColor;
    public Color ambientGroundColor;
    public float ambientIntensity;
    public UnityEngine.Rendering.AmbientMode ambientMode;
    public UnityEngine.Rendering.SphericalHarmonicsL2 ambientProbe;
    public Color ambientSkyColor;


    public Material skyboxMaterial;

    public float haloStrength = 0.5f;

    public float flareStrength = 1.0f;

    void Awake() {
        RenderSettings.fog = fog;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = fogMode;
        RenderSettings.fogDensity = fogDensity;

        RenderSettings.fogStartDistance = linearFogStart;
        RenderSettings.fogEndDistance = linearFogEnd;

        RenderSettings.ambientEquatorColor = ambientEquatorColor;
        RenderSettings.ambientGroundColor=ambientGroundColor;
        RenderSettings.ambientIntensity=ambientIntensity;
        RenderSettings.ambientLight = ambientLight;
        RenderSettings.ambientMode=ambientMode;
        RenderSettings.ambientProbe=ambientProbe;
        RenderSettings.ambientSkyColor=ambientSkyColor;

        RenderSettings.skybox = skyboxMaterial;

        RenderSettings.haloStrength = haloStrength;

        RenderSettings.flareStrength = flareStrength;
    }

}