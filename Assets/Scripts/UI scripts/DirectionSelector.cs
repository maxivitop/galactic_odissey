using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class DirectionSelector : MonoBehaviour, IDragHandler, IPointerClickHandler
{
    public class DirectionEvent : UnityEvent<Vector2> { }

    public GameObject arrow;
    public DirectionEvent OnValueChanged = new();
    private Vector2 _value = Vector2.up;
    public Vector2 Value
    {
        get{return _value;}
        set
        {
            _value = value.normalized;
            OnValueChanged.Invoke(_value);
            arrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -Mathf.Rad2Deg * Mathf.Atan2(_value.x, _value.y)));
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
        Vector2 result;
        Vector2 clickPosition = eventData.position;

        RectTransform thisRect = transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(thisRect, clickPosition, null, out result);
        Value = result;
    }
}
