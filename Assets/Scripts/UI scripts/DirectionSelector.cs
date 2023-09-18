using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class DirectionSelector : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
   
    [SerializeField]
    private SpriteRenderer arrowHead;
    [SerializeField]
    private SpriteRenderer arrowTail;
    [SerializeField] private float minArrowSize = 1;
    [SerializeField] private float maxArrowSize = 2;
    public readonly UnityEvent OnValueChanged = new();
    [FormerlySerializedAs("value")]
    [SerializeField] private Vector2 direction = Vector2.up;
    [SerializeField] private float magnitude;
    private Vector3 initialLocalScale;
    private bool isDragging;
    
    public Color Color
    {
        get => arrowHead.color;
        set {
            arrowHead.color = value;
            arrowTail.color = value;
        }
    }

    public Vector2 Direction
    {
        get => direction;
        set
        {
            if (direction == value)
            {
                return;
            }
            direction = value;
            UpdateArrow();
        }
    }
    
    public float Magnitude
    {
        get => magnitude;
        set
        {
            if (magnitude == value)
            {
                return;
            }
            magnitude = value;
            UpdateArrow();
        }
    }

    private void UpdateArrow()
    {
        if (initialLocalScale == Vector3.zero)
        {
            initialLocalScale = arrowTail.transform.localScale;
        }
        arrowHead.transform.rotation = Quaternion.Euler(new Vector3(0, 0,
            -Mathf.Rad2Deg * Mathf.Atan2(direction.x, direction.y)));
        var scale = transform.localScale.y;
        arrowHead.transform.position = (Vector2)transform.position 
                                       + direction * (minArrowSize * scale)
                                       + direction * ((maxArrowSize - minArrowSize) * magnitude * scale);
        arrowTail.transform.rotation = arrowHead.transform.rotation;
        arrowTail.transform.position = (transform.position + arrowHead.transform.position) / 2;
        arrowTail.transform.localScale = new Vector3(
            initialLocalScale.x, 
            initialLocalScale.y * (magnitude * (maxArrowSize - minArrowSize) + minArrowSize),
            initialLocalScale.z);
        OnValueChanged.Invoke();
    }
    
    public void OnDrag(PointerEventData eventData) { } // required for begin/end drag to work

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
        isDragging = true;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
        isDragging = false;
    }

    private void Update()
    {
        if (isDragging)
        {
            OnArrowMove();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
        OnArrowMove();
    }

    private void OnArrowMove()
    {
        var v = (MouseHandler.WorldMousePosition - transform.position) / transform.lossyScale.y;
        var vLen = v.magnitude;
        Direction = v / vLen;

        vLen -= minArrowSize;
        vLen /= maxArrowSize - minArrowSize;
        vLen = Mathf.Clamp01(vLen);
        Magnitude = vLen;
    }
}