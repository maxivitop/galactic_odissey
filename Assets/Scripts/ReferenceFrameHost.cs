using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ReferenceFrameHost : MonoBehaviour, IPointerClickHandler
{
    public static readonly SingleEvent<ReferenceFrameHost> referenceFrameChangeOld = new();
    private static ReferenceFrameHost referenceFrame;

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
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (Vector3.Distance(CameraMover.Instance.CenterPosition, transform.position) < 1f) // sufficiently close
        {
            ReferenceFrame = this;
        }
        CameraMover.Instance.Follow(futureTransform);
    }
}