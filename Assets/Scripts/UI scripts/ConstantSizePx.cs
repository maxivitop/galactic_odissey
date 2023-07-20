using System;
using UnityEngine;
using UnityEngine.Serialization;

public class ConstantSizePx : MonoBehaviour
{
    public float sizePx = 100;
    public bool scaleTransform;
    public bool scaleLineRenderer;
    private Canvas canvas;
    private Camera mainCamera;
    private Vector3 initialScale;

    private LineRenderer lineRenderer;
    private bool hasLineRenderer;
    private float initialLineWidth;

    private void Start()
    {
        canvas = FindObjectOfType<Canvas>();
        mainCamera = Camera.main!;
        var parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        var localScale = transform.localScale;
        initialScale = new Vector3(
            1 / parentScale.x * localScale.x,
            1 / parentScale.y * localScale.y,
            1 / parentScale.z * localScale.z
        );
        hasLineRenderer = TryGetComponent(out lineRenderer);
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
        var scale = canvas.scaleFactor;
        var scaledSizePx = sizePx * scale;
        var position = transform.position;
        var screenPos = mainCamera.WorldToScreenPoint(position);
        var screenUp = screenPos + Vector3.up * scaledSizePx;
        screenUp.z = -mainCamera.transform.position.z;
        var size = Vector3.Distance(position,  mainCamera.ScreenToWorldPoint(screenUp));
        if (scaleTransform)
        {
            transform.localScale = initialScale * size;
        }
        if (hasLineRenderer && scaleLineRenderer)
        {
            lineRenderer.widthMultiplier = initialLineWidth * size;
        }
    }
}
