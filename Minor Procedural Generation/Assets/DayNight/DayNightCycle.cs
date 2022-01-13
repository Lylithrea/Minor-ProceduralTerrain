using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DayNightCycle : MonoBehaviour
{
    public Light directionalLight;
    public DayNightSettings settings;

    [Range(0,24f)] public float timeofDay;

    [Range(0f,1f)]public float timeSpeed;


    // Start is called before the first frame update

    private void updateLightning(float timePercent)
    {
        RenderSettings.ambientLight = settings.ambientColour.Evaluate(timePercent);
        RenderSettings.fogColor = settings.fogColour.Evaluate(timePercent);
        directionalLight.color = settings.directionalColour.Evaluate(timePercent);
        directionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170, 0));
    }

    // Update is called once per frame
    void Update()
    {
        timeofDay += Time.deltaTime * timeSpeed;
        //clamp between 24, bcz 24 hours in a day
        timeofDay %= 24;
        updateLightning(timeofDay / 24f );
    }
}
