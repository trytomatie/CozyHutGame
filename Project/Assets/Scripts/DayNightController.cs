using AtmosphericHeightFog;
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
    public Gradient heightFogColorStart;
    public Gradient heightFogColorEnd;
    public float sunAngleOffset = -90;

    [Header("References")]
    public Light sun;
    public LensFlareComponentSRP lensFlare;
    public HeightFogGlobal fog;

    public static DayNightController Instance { get; protected set; }

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {

            Instance = this;
        }
        DontDestroyOnLoad(this);
    }

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

    private int NormalziedTimeAnimatorHash = Animator.StringToHash("NormalizedTime");
    private void UpdateTime()
    {
        string ampm = (GameManager.Instance.currentTime.Hours % 24) < 12 ? "AM" : "PM";
        GameManager.Instance.currentTime = GameManager.Instance.currentTime.Add(TimeSpan.FromSeconds(Time.deltaTime * timeMultiplier));
        GameUI.Instance.timeText.text = $"{(GameManager.Instance.currentTime.Hours % 12).ToString("D2")}:{GameManager.Instance.currentTime.Minutes.ToString("D2")} {ampm}";
        GameUI.Instance.timeIconAnmiator.SetFloat(NormalziedTimeAnimatorHash, normalizedDayTime);
    }
    private void UpdateDayNightCycle()
    {
        normalizedDayTime = (float)(((GameManager.Instance.currentTime.TotalSeconds% 86400) / 86400f)+0.75f) % 1;
        bool isNight = normalizedDayTime < 0.5f;

        float angle = Mathf.Lerp(0f, 360f, normalizedDayTime);
        sun.transform.rotation = Quaternion.Euler(sunAngleOffset + angle, 169.6f, 0f);
        sun.intensity = sunIntensity.Evaluate(normalizedDayTime);
        sun.color = sunColor.Evaluate(normalizedDayTime);
        fog.fogColorStart = heightFogColorStart.Evaluate(normalizedDayTime);
        fog.fogColorEnd = heightFogColorEnd.Evaluate(normalizedDayTime);
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

