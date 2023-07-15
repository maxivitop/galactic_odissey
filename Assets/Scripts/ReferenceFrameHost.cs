using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class ReferenceFrameHost : MonoBehaviour, IPointerClickHandler
{
    public static readonly SingleEvent<ReferenceFrameHost> referenceFrameChangeOld = new();
    private static ReferenceFrameHost referenceFrame;
    [FormerlySerializedAs("cameraScale")] public float cameraHeight;

    public static ReferenceFrameHost ReferenceFrame
    {
        get => referenceFrame;
        private set
        {
            var oldReferenceFrame = referenceFrame;
            referenceFrame = value;
            if (oldReferenceFrame != null) referenceFrameChangeOld.Invoke(oldReferenceFrame);
        }
    }

    public bool isActiveInitially;

    [NonSerialized]
    public FutureTransform futureTransform;
    [NonSerialized]
    public TrajectoryProvider trajectoryProvider;
    private CameraMover cameraMover;
    
    private void Awake()
    {
        if (isActiveInitially)
        {
            if (ReferenceFrame != null)
                Debug.LogError("Having more than one reference frame: " + gameObject.name +
                               " and " + ReferenceFrame.gameObject.name);
            ReferenceFrame = this;
        }

        futureTransform = GetComponent<FutureTransform>();
        trajectoryProvider = GetComponent<TrajectoryProvider>();
        cameraMover = Camera.main!.GetComponent<CameraMover>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ReferenceFrame == this)
        {
            var targetPos = transform.position;
            targetPos.z = cameraHeight;
            cameraMover.MoveToPosition(targetPos);
        }
        else
        {
            ReferenceFrame = this;
            cameraMover.Follow(futureTransform);
        }
    }
}