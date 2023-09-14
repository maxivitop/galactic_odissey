using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(FutureTransform))]
public class AttachCameraOnClick : MonoBehaviour, IPointerClickHandler
{

    private FutureTransform futureTransform;

    private void Start()
    {
        futureTransform = GetComponent<FutureTransform>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        CameraMover.Instance.Follow(futureTransform);
    }
}