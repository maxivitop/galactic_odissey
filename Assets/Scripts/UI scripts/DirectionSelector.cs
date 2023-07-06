using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class DirectionSelector : MonoBehaviour, IDragHandler, IPointerClickHandler
{
    public class DirectionEvent : UnityEvent<Vector2>
    {
    }

    public GameObject arrow;
    public readonly DirectionEvent OnValueChanged = new();
    private Vector2 value = Vector2.up;

    public Vector2 Value
    {
        get => value;
        set
        {
            this.value = value.normalized;
            OnValueChanged.Invoke(this.value);
            arrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0,
                -Mathf.Rad2Deg * Mathf.Atan2(this.value.x, this.value.y)));
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnArrowMove(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnArrowMove(eventData);
    }

    private void OnArrowMove(PointerEventData eventData)
    {
        var clickPosition = eventData.position;

        var thisRect = transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(thisRect, clickPosition, null,
            out var result);
        Value = result;
    }
}