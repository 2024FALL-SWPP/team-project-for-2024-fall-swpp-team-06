using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public float dayPeriod { get; private set; } = 60 * 10;
    public float currentTime { get; private set; } = 300;
    public float dayStartTime { get; private set; }
    public float nightStartTime { get; private set; }
    public bool isNight { get; private set; } = false;

    // Start is called before the first frame update
    void Start()
    {
        dayStartTime = dayPeriod / 6;
        nightStartTime = 5.5f * dayPeriod / 6;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= dayPeriod)
        {
            currentTime -= dayPeriod;
        }

        if (!isNight)
        {
            if (currentTime > nightStartTime)
            {
                isNight = true;
            }
        }
        else
        {
            if (currentTime > dayStartTime && currentTime < nightStartTime)
            {
                isNight = false;
            }
        }
    }
}
