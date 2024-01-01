using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightController : MonoBehaviour
{
    public LensFlare lensFlare;

    public int timeMultiplier = 120;
    public float normalizedDayTime;

    public static void SetTime(TimeSpan value)
    {
        GameManager.Instance.currentTime = value;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTime();
        UpdateDayNightCycle();
    }

    private void UpdateTime()
    {
        GameManager.Instance.currentTime = GameManager.Instance.currentTime.Add(TimeSpan.FromSeconds(Time.deltaTime * timeMultiplier));
    }
    private void UpdateDayNightCycle()
    {
        normalizedDayTime = (float)((GameManager.Instance.currentTime.TotalSeconds% 86400) / 86400f);

        float angle = Mathf.Lerp(0f, 360f, normalizedDayTime);
        RenderSettings.sun.transform.rotation = Quaternion.Euler(angle, 45f, 0f);


    }




}

