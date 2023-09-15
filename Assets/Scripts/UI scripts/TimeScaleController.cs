using System;
using System.Threading;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeScaleController : MonoBehaviour
{
    private readonly float[] timeScales = { 0, 0.1f, 1, 10, 100 };
    private const float DefaultPlayTimescale = 1f;
    public TextMeshProUGUI timeScaleText;
    public Slider timeScaleSlider;
    
    private float fixedDeltaTime;

    private void Start()
    {
        fixedDeltaTime = Time.fixedDeltaTime;
        Time.timeScale = 0f;
        timeScaleSlider.maxValue = timeScales.Length-1;
        timeScaleSlider.onValueChanged.AddListener(OnTimeScaleChanged);
    }

    private void OnTimeScaleChanged(float sliderValue)
    {
        Time.timeScale = timeScales[Mathf.RoundToInt(sliderValue)];
        Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale;
        timeScaleText.text = Time.timeScale + "x";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            timeScaleSlider.value = Time.timeScale == 0 ? Array.IndexOf(timeScales, DefaultPlayTimescale) : 0;
        }
    }
}