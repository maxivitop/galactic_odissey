using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleWithCamera : MonoBehaviour
{
    public float referenceCameraHeight = -30;
    public bool scaleSelf;
    private LineRenderer lineRenderer;
    private bool hasLineRenderer;
    private Vector3 initialScale;

    private float initialLineWidth;

    private void Start()
    {
        hasLineRenderer = TryGetComponent(out lineRenderer);
        initialScale = transform.localScale;
        if (hasLineRenderer)
        {
            initialLineWidth = lineRenderer.widthMultiplier;
        }
        UpdateScale();
    }

    private void Update()
    {
        UpdateScale();
    }

    private void UpdateScale()
    {
        var scale = Camera.main!.transform.position.z / referenceCameraHeight;
        if (scaleSelf)
        {
            transform.localScale = initialScale * scale;
        }
        if (hasLineRenderer)
        {
            lineRenderer.widthMultiplier = initialLineWidth * scale;
        }
    }
}
