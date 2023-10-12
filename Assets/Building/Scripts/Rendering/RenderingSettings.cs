using System;
using UnityEngine;

public class RenderingSettings: MonoBehaviour
{
    public static RenderingSettings Instance;
    public float side = 2.8f;

    private void Awake()
    {
        Instance = this;
    }
}