using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
public class ClickHelper: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Color highlightedColor;
    private SpriteRenderer spriteRenderer;
    private Color initialColor;
    private Transform mainCamera;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialColor = spriteRenderer.color;
        mainCamera = Camera.main!.transform;
    }

    private void Update()
    {
        transform.LookAt(mainCamera);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        spriteRenderer.color = highlightedColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        spriteRenderer.color = initialColor; 
    }
}