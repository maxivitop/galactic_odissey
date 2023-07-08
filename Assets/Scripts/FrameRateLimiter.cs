using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Reduces framerate in editor so trajectory has time to render between frames.
 */
public class FrameRateLimiter : MonoBehaviour
{
    public int targetFrameRate = 60;
#if UNITY_EDITOR
    private void OnEnable()
    {
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = targetFrameRate;
    }
#endif
}
