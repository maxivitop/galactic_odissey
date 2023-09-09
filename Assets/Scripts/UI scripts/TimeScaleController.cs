using System;
using System.Threading;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeScaleController : MonoBehaviour
{
    private readonly float[] timeScales = { 0, 0.1f, 1, 10, 100 };
    public int lastNonZeroSliderValue = 1;
    public float timeScaleBase = 10f;
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
        if (sliderValue != 0) lastNonZeroSliderValue = Mathf.RoundToInt(sliderValue);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            timeScaleSlider.value = Time.timeScale == 0 ? lastNonZeroSliderValue : 0;
        }
    }
}