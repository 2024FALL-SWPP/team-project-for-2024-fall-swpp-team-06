using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class LightController : MonoBehaviour
{
    public TimeManager timeManager;
    private float dayPeriod;
    public Material skyboxMaterial;
    private float midnightSkyTintHue = 220 / 360f;
    private float noonSkyTintHue = 80 / 360f;
    // public float currentTime = 0f;

    private float lightControlPeriod = 0.2f;

    private Light lightProperty;
    
    private Quaternion initialRotation = Quaternion.Euler(5f, -74f, 0f);
    private Vector3 rotAxis;
    
    private float sunAttenuationPeriod;
    private float sunResizePeriod;

    private const float sunSize = 0.025f;
    private const float lightIntensity = 1.2f;
    private const float dayPlanetExposure = 1.2f;
    private const float nightPlanetExposure = 0.55f;
    private const float dayStarExposure = 1f;
    private const float nightStarExposure = 2f;
    
    private float rotSpeed;
    private bool resetLightIntensity = false;
    private bool resetSunSize = false;
    private bool resetStarExposure = false;
    private bool resetPlanetExposure = false;
    private float dayStartTime;
    private float nightStartTime;
    
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        dayPeriod = timeManager.dayPeriod;
        dayStartTime = timeManager.dayStartTime;
        nightStartTime = timeManager.nightStartTime;
        
        rotSpeed = 360 / dayPeriod;
        rotAxis = (initialRotation * -Vector3.forward + 2 * Vector3.up + 1.2f * Vector3.forward).normalized;
        lightProperty = GetComponent<Light>();
        sunAttenuationPeriod = dayPeriod / 6;
        sunResizePeriod = dayPeriod / 3;
        skyboxMaterial.SetFloat("_SunSize", sunSize);

        StartCoroutine(LightControlRoutine());

    }

    IEnumerator LightControlRoutine()
    {
        while (true)
        {
            float currentTime = timeManager.currentTime;
            
            // control light intensity
            if (Mathf.Abs(currentTime - dayPeriod / 6) < sunAttenuationPeriod / 2)
            {
                lightProperty.intensity = 0.75f * lightIntensity - 0.25f * lightIntensity *
                    Mathf.Cos(2 * Mathf.PI * (currentTime - dayPeriod / 6) / sunAttenuationPeriod);
                resetLightIntensity = false;
            }
            else if (!resetLightIntensity)
            {
                lightProperty.intensity = lightIntensity;
                resetLightIntensity = true;
            }
            
            // control sunsize
            if (currentTime > 4.4f * dayPeriod / 6 || currentTime < 0.4f * dayPeriod / 6)
            {
                skyboxMaterial.SetFloat("_SunSize",
                    2.5f * sunSize +
                    1.5f * sunSize * Mathf.Cos(2 * Mathf.PI * (currentTime - 5.4f * dayPeriod / 6) / sunResizePeriod));
                resetSunSize = false;
            }
            else if (!resetSunSize)
            {
                skyboxMaterial.SetFloat("_SunSize", sunSize);
                resetSunSize = true;
            }
            
            transform.rotation = Quaternion.AngleAxis(rotSpeed * (currentTime - dayPeriod / 6), rotAxis) * initialRotation;
            
            
            // control planet background exposure
            if (currentTime > dayStartTime && currentTime <= (dayStartTime + nightStartTime) / 2)
            {
                float planetExposure =
                    (dayPlanetExposure - nightPlanetExposure) * Mathf.Log(currentTime - dayStartTime + 1,
                        (nightStartTime - dayStartTime) / 2 + 1) + nightPlanetExposure;
                skyboxMaterial.SetFloat("_ExposureCube3", planetExposure);
            } else if (currentTime > (dayStartTime + nightStartTime) / 2 && currentTime <= nightStartTime)
            {
                float planetExposure =
                    (dayPlanetExposure - nightPlanetExposure) * Mathf.Log(nightStartTime - currentTime + 1,
                        (nightStartTime - dayStartTime) / 2 + 1) + nightPlanetExposure;
                skyboxMaterial.SetFloat("_ExposureCube3", planetExposure);
            } else if (!resetPlanetExposure)
            {
                skyboxMaterial.SetFloat("_ExposureCube3", nightPlanetExposure);
                resetPlanetExposure = true;
            }
            
            // control star background exposure
            if (currentTime > nightStartTime || currentTime <= (dayStartTime + nightStartTime - dayPeriod) / 2)
            {
                float deltaTime = currentTime - nightStartTime + (currentTime > nightStartTime ? 0 : dayPeriod);

                float starExposure =
                    (nightStarExposure - dayStarExposure) *
                    Mathf.Log(deltaTime + 1, (dayStartTime + dayPeriod - nightStartTime) / 2 + 1) + dayStarExposure;
                skyboxMaterial.SetFloat("_ExposureCube2", starExposure);

            } else if (currentTime <= dayStartTime)
            {
                float starExposure =
                    (nightStarExposure - dayStarExposure) * Mathf.Log(dayStartTime - currentTime + 1,
                        (dayStartTime + dayPeriod - nightStartTime) / 2 + 1) + dayStarExposure;
                skyboxMaterial.SetFloat("_ExposureCube2", starExposure);
                resetStarExposure = false;
            }
            else if (!resetStarExposure)
            {
                skyboxMaterial.SetFloat("_ExposureCube2", dayStarExposure);
                resetStarExposure = true;
            }
            
            // control sky tint hue
            if (currentTime >= dayPeriod / 2)
            {
                float hue = Mathf.Pow(midnightSkyTintHue - noonSkyTintHue + 1, 2 * currentTime / dayPeriod - 1) +
                    noonSkyTintHue - 1;
                
                skyboxMaterial.SetColor("_SkyTint", Color.HSVToRGB(hue, 1, 1));
            }
            else
            {
                float hue = Mathf.Pow(midnightSkyTintHue - noonSkyTintHue + 1, 1 - 2 * currentTime / dayPeriod) +
                    noonSkyTintHue - 1;
                skyboxMaterial.SetColor("_SkyTint", Color.HSVToRGB(hue, 1, 1));
            }
            
            yield return new WaitForSeconds(lightControlPeriod);
        }
    }
    
/*
    // Update is called once per frame
    void Update()
    {
        
        currentTime += Time.deltaTime;
        if (currentTime >= dayPeriod)
        {
            currentTime -= dayPeriod;
        }

        // control light intensity
        if (Mathf.Abs(currentTime - dayPeriod / 6) < sunAttenuationPeriod / 2)
        {
            lightProperty.intensity = 0.75f * lightIntensity - 0.25f * lightIntensity *
                Mathf.Cos(2 * Mathf.PI * (currentTime - dayPeriod / 6) / sunAttenuationPeriod);
            resetLightIntensity = false;
        }
        else if (!resetLightIntensity)
        {
            lightProperty.intensity = lightIntensity;
            resetLightIntensity = true;
        }
        
        // control sunsize
        if (currentTime > 4.4f * dayPeriod / 6 || currentTime < 0.4f * dayPeriod / 6)
        {
            skyboxMaterial.SetFloat("_SunSize",
                2.5f * sunSize +
                1.5f * sunSize * Mathf.Cos(2 * Mathf.PI * (currentTime - 5.4f * dayPeriod / 6) / sunResizePeriod));
            resetSunSize = false;
        }
        else if (!resetSunSize)
        {
            skyboxMaterial.SetFloat("_SunSize", sunSize);
            resetSunSize = true;
        }
        
        transform.rotation = Quaternion.AngleAxis(rotSpeed * (currentTime - dayPeriod / 6), rotAxis) * initialRotation;
        
        
        // control planet background exposure
        if (currentTime > dayStartTime && currentTime <= (dayStartTime + nightStartTime) / 2)
        {
            float planetExposure =
                (dayPlanetExposure - nightPlanetExposure) * Mathf.Log(currentTime - dayStartTime + 1,
                    (nightStartTime - dayStartTime) / 2 + 1) + nightPlanetExposure;
            skyboxMaterial.SetFloat("_ExposureCube3", planetExposure);
        } else if (currentTime > (dayStartTime + nightStartTime) / 2 && currentTime <= nightStartTime)
        {
            float planetExposure =
                (dayPlanetExposure - nightPlanetExposure) * Mathf.Log(nightStartTime - currentTime + 1,
                    (nightStartTime - dayStartTime) / 2 + 1) + nightPlanetExposure;
            skyboxMaterial.SetFloat("_ExposureCube3", planetExposure);
        } else if (!resetPlanetExposure)
        {
            skyboxMaterial.SetFloat("_ExposureCube3", nightPlanetExposure);
            resetPlanetExposure = true;
        }
        
        // control star background exposure
        if (currentTime > nightStartTime || currentTime <= (dayStartTime + nightStartTime - dayPeriod) / 2)
        {
            float deltaTime = currentTime - nightStartTime + (currentTime > nightStartTime ? 0 : dayPeriod);

            float starExposure =
                (nightStarExposure - dayStarExposure) *
                Mathf.Log(deltaTime + 1, (dayStartTime + dayPeriod - nightStartTime) / 2 + 1) + dayStarExposure;
            skyboxMaterial.SetFloat("_ExposureCube2", starExposure);

        } else if (currentTime <= dayStartTime)
        {
            float starExposure =
                (nightStarExposure - dayStarExposure) * Mathf.Log(dayStartTime - currentTime + 1,
                    (dayStartTime + dayPeriod - nightStartTime) / 2 + 1) + dayStarExposure;
            skyboxMaterial.SetFloat("_ExposureCube2", starExposure);
            resetStarExposure = false;
        }
        else if (!resetStarExposure)
        {
            skyboxMaterial.SetFloat("_ExposureCube2", dayStarExposure);
            resetStarExposure = true;
        }
        
        // control sky tint hue
        if (currentTime >= dayPeriod / 2)
        {
            float hue = Mathf.Pow(midnightSkyTintHue - noonSkyTintHue + 1, 2 * currentTime / dayPeriod - 1) +
                noonSkyTintHue - 1;
            
            skyboxMaterial.SetColor("_SkyTint", Color.HSVToRGB(hue, 1, 1));
        }
        else
        {
            float hue = Mathf.Pow(midnightSkyTintHue - noonSkyTintHue + 1, 1 - 2 * currentTime / dayPeriod) +
                noonSkyTintHue - 1;
            skyboxMaterial.SetColor("_SkyTint", Color.HSVToRGB(hue, 1, 1));
        }
    }*/
}
