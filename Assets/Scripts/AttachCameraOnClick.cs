using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(FutureTransform))]
public class AttachCameraOnClick : MonoBehaviour, IPointerClickHandler
{

    private FutureTransform futureTransform;
    private CameraMover cameraMover;

    private void Start()
    {
        futureTransform = GetComponent<FutureTransform>();
        cameraMover = Camera.main!.GetComponent<CameraMover>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (cameraMover.followee == futureTransform)
        {
            var targetPos = transform.position;
            targetPos.z = cameraMover.transform.position.z;
            cameraMover.MoveToPosition(targetPos);
        }
        cameraMover.Follow(futureTransform);
    }
}