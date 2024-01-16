using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DayNightController : MonoBehaviour
{

    public int timeMultiplier = 120;
    public float normalizedDayTime;
    public float normalizedDayCycleTime; // The Time in the Current Cycle (Day-Cycle, Night-Cycle)
    public AnimationCurve sunIntensity;
    [SerializeField]
    public Gradient sunColor;
    public float sunAngleOffset = -90;

    [Header("References")]
    public Light sun;
    public LensFlareComponentSRP lensFlare;

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
        GameUI.Instance.timeText.text = $"Time:{GameManager.Instance.currentTime.Hours.ToString("D2")}:{GameManager.Instance.currentTime.Minutes.ToString("D2")}\n" +
            $"Days: {GameManager.Instance.currentTime.TotalDays.ToString("F0")}";
    }
    private void UpdateDayNightCycle()
    {
        normalizedDayTime = (float)(((GameManager.Instance.currentTime.TotalSeconds% 86400) / 86400f)+0.75f) % 1;
        bool isNight = normalizedDayTime < 0.5f;

        float angle = Mathf.Lerp(0f, 360f, normalizedDayTime);
        sun.transform.rotation = Quaternion.Euler(sunAngleOffset + angle, 169.6f, 0f);
        sun.intensity = sunIntensity.Evaluate(normalizedDayTime);
        sun.color = sunColor.Evaluate(normalizedDayTime);
        if (normalizedDayTime <= 0.6f)
        {
            lensFlare.enabled = true;
        }
        else
        {
            lensFlare.enabled = false;
        }
    }

   



}

