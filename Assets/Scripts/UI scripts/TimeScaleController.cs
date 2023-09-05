using System;
using System.Threading;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeScaleController : MonoBehaviour
{
    public int lastNonZeroSliderValue = 1;
    public float timeScaleBase = 10f;
    public TextMeshProUGUI timeScaleText;
    public Slider timeScaleSlider;
    
    private float fixedDeltaTime;

    private void Start()
    {
        fixedDeltaTime = Time.fixedDeltaTime;
        Time.timeScale = 0f;
        timeScaleSlider.onValueChanged.AddListener(OnTimeScaleChanged);
    }

    private void OnTimeScaleChanged(float sliderValue)
    {
        Time.timeScale =
            Mathf.RoundToInt(Mathf.Pow(timeScaleBase,
                sliderValue - 1)); // 0 -> 0, 1 -> 1, 2 -> 10, 3 -> 100
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

        var availableSteps = FuturePhysics.lastVirtualStep - FuturePhysics.currentStep;
        if (availableSteps < FuturePhysicsRunner.stepsNextFrame*2 && timeScaleSlider.value > 0)
        {
            timeScaleSlider.value -= 1;
        }
    }
}