using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class ReferenceFrameHost : MonoBehaviour, IPointerClickHandler
{
    public static readonly Event<ReferenceFrameHost> referenceFrameChangeOld = new();
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

    private float lastClickTime;

    private void Start()
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
        if (Time.timeSinceLevelLoad - lastClickTime < 0.5f) ReferenceFrame = this;
        lastClickTime = Time.timeSinceLevelLoad;
    }
}