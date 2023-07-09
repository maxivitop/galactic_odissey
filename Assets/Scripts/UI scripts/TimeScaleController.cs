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

    private void Start()
    {
        timeScaleSlider.onValueChanged.AddListener(OnTimeScaleChanged);
    }

    private void OnTimeScaleChanged(float sliderValue)
    {
        FuturePhysicsRunner.timeScale =
            Mathf.RoundToInt(Mathf.Pow(timeScaleBase,
                sliderValue - 1)); // 0 -> 0, 1 -> 1, 2 -> 10, 3 -> 100
        timeScaleText.text = FuturePhysicsRunner.timeScale + "x";
        if (sliderValue != 0) lastNonZeroSliderValue = Mathf.RoundToInt(sliderValue);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            timeScaleSlider.value = FuturePhysicsRunner.timeScale == 0 ? lastNonZeroSliderValue : 0;
        }

        var availableSteps = FuturePhysics.lastVirtualStep - FuturePhysics.currentStep;
        if (availableSteps < FuturePhysicsRunner.stepsNextFrame*2 && timeScaleSlider.value > 0)
        {
            timeScaleSlider.value -= 1;
        }
    }
}