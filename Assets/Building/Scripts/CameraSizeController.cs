using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSizeController : MonoBehaviour
{
    public float animationDuration = 0.2f;
    private FloatAnimator animator;
    public float buildingCameraSize;
    public float battleCameraSize;

    private void Start()
    {
        animator = new FloatAnimator(animationDuration);
        ModeSwitcher.modeChanged.AddListener(mode =>
        {
            animator.Capture(Camera.main!.orthographicSize);
        });
        animator.Capture(Camera.main!.orthographicSize);
    }

    void Update()
    {
        animator.ForwardTime(Time.deltaTime);
        Camera.main!.orthographicSize = ModeSwitcher.CurrentMode switch
        {
            ModeSwitcher.Mode.Battle => animator.AnimateTowards(battleCameraSize),
            ModeSwitcher.Mode.Building => animator.AnimateTowards(buildingCameraSize),
            _ => Camera.main!.orthographicSize
        };
    }
}
