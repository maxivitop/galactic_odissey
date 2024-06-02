using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipRotator : MonoBehaviour
{
    public float speed = 1;
    public float animationDuration = 0.2f;
    private FloatAnimator animator;

    private void Start()
    {
        animator = new FloatAnimator(animationDuration);
        ModeSwitcher.modeChanged.AddListener(mode =>
        {
            if (mode == ModeSwitcher.Mode.Building)
            {
                animator.Capture(ShipLayout.Instance.shipRotationDeg % 360);
            }
        });
    }

    void Update()
    {
        if (ModeSwitcher.CurrentMode == ModeSwitcher.Mode.Battle)
        {
            // ShipLayout.Instance.shipRotationDeg += Input.GetAxis("Horizontal") * speed;
            ShipLayout.Instance.shipRotationDeg = Quaternion.LookRotation(Vector3.forward, 
                Input.mousePosition - new Vector3(Screen.width / 2, Screen.height / 2)
                ).eulerAngles.z;
            // Debug.Log(Quaternion.LookRotation(Vector3.forward, 
            //     Input.mousePosition - new Vector3(Screen.width / 2, Screen.height / 2)
            // ).eulerAngles);
        }
        else
        {
            animator.ForwardTime(Time.deltaTime);
            ShipLayout.Instance.shipRotationDeg = animator.AnimateTowards(0f);
        }
    }
}
