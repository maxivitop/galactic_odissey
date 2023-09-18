using System;
using UnityEngine;
using UnityEngine.Serialization;

public class ConstantSizePx : MonoBehaviour
{
    public float sizePx = 100;
    public bool scaleTransform;
    public bool scaleLineRenderer;
    public float minScale;
    public Vector3 minScaleByAxis;
    private Vector3 initialScale;

    private LineRenderer lineRenderer;
    private bool hasLineRenderer;
    private float initialLineWidth;

    private void Start()
    {
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
        var size = Mathf.Max(minScale, sizePx * CameraMover.Instance.zoom / 2000);
        if (scaleTransform)
        {
            transform.localScale = new Vector3(
                    Mathf.Max(minScaleByAxis.x, size) * initialScale.x,
                    Mathf.Max(minScaleByAxis.y, size) * initialScale.y,
                    Mathf.Max(minScaleByAxis.z, size) * initialScale.z
                );
        }
        if (hasLineRenderer && scaleLineRenderer)
        {
            lineRenderer.widthMultiplier = initialLineWidth * size;
        }
    }
}
