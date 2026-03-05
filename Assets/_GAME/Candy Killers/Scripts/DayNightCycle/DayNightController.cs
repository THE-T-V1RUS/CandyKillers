using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class DayNightController : MonoBehaviour
{
    [Range(0f, 1f)]
    public float timeOfDay = 0f; // 0 = Day, 1 = Night

    [Header("Lighting")]
    public Light directionalLight;
    public Gradient lightColor;       // Gradient to control light color over time
    public AnimationCurve lightIntensity; // Curve to control light intensity over time
    public Vector3 dayRotation = new Vector3(50, 0, 0);
    public Vector3 nightRotation = new Vector3(-30, 180, 0);

    [Header("Skybox")]
    public Material blendedSkybox; // Material using the BlendedSkybox shader

    [Header("Post Processing")]
    public Volume volume;
    public float maxPostExposure = 1.0f;
    public float minPostExposure = 0.25f;
    private ColorAdjustments colorAdjustments;

    [Header("Time Progression")]
    [Tooltip("How many seconds it takes to go from 0 (day) to 1 (night).")]
    public float duration = 60f;

    [Tooltip("If true, timeOfDay will automatically progress.")]
    public bool isAdvancing = false;

    public List<MeshRenderer> lightRenderers;
    public List<Material> lightMaterials;
    public GameObject ExteriorLights;
    [Range(0f, 1f)]
    public float OutdoorLightsThreshhold;
    private bool isOutdoorLightsEnabled = false;

    private void Start()
    {
        // Make sure the volume is not null
        if (volume != null && volume.profile.TryGet(out colorAdjustments))
        {
            // Successfully grabbed post-processing settings
        }
        else
        {
            Debug.LogWarning("Color Adjustments not found in volume profile.");
        }
    }

    void Update()
    {
        if (isAdvancing)
        {
            timeOfDay += Time.deltaTime / duration;
            timeOfDay = Mathf.Clamp01(timeOfDay); // Prevents going over 1
        }

        UpdateLighting();
        UpdateSkybox();
        if (colorAdjustments != null)
        {
            if (timeOfDay <= 0.5f)
            {
                colorAdjustments.postExposure.value = maxPostExposure;
            }
            else
            {
                float t = Mathf.InverseLerp(0.5f, 1.0f, timeOfDay);
                colorAdjustments.postExposure.value = Mathf.Lerp(maxPostExposure, minPostExposure, t);
            }
        }
    }

    void UpdateLighting()
    {
        if (directionalLight != null)
        {
            // Rotate sun based on time of day
            directionalLight.transform.rotation = Quaternion.Lerp(
                Quaternion.Euler(dayRotation),
                Quaternion.Euler(nightRotation),
                timeOfDay
            );

            // Light color & intensity based on gradient/curve
            directionalLight.color = lightColor.Evaluate(timeOfDay);
            directionalLight.intensity = lightIntensity.Evaluate(timeOfDay);
        }

        if(timeOfDay > OutdoorLightsThreshhold && !isOutdoorLightsEnabled)
        {
            isOutdoorLightsEnabled = true;
            foreach(MeshRenderer renderer in lightRenderers)
            {
                renderer.material = lightMaterials[1];
            }
            ExteriorLights.SetActive(true);
        }
        else if (timeOfDay <= OutdoorLightsThreshhold && isOutdoorLightsEnabled)
        {
            isOutdoorLightsEnabled = false;
            foreach (MeshRenderer renderer in lightRenderers)
            {
                renderer.material = lightMaterials[0];
            }
            ExteriorLights.SetActive(false);
        }
    }

    void UpdateSkybox()
    {
        if (blendedSkybox != null)
        {
            float blendValue = 0f;

            if (timeOfDay > 0.5f)
            {
                float t = Mathf.InverseLerp(0.5f, 1.0f, timeOfDay);
                blendValue = Mathf.Clamp01(t);
            }

            blendedSkybox.SetFloat("_Blend", blendValue);
            RenderSettings.skybox = blendedSkybox;
            DynamicGI.UpdateEnvironment();
        }
    }
}
