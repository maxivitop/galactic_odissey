using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ReferenceFrameHost : MonoBehaviour, IPointerClickHandler 
{
    public static UnityEvent<ReferenceFrameHost> ReferenceFrameChangeOld = new UnityEvent<ReferenceFrameHost>();
    private static ReferenceFrameHost _ReferenceFrame = null;
    public static ReferenceFrameHost ReferenceFrame
    {
        get { return _ReferenceFrame; }
        set
        {
            if (_ReferenceFrame != null)
            {
                _ReferenceFrame.isActive = false;
            }
            var oldReferenceFrame = _ReferenceFrame;
            _ReferenceFrame = value;
            _ReferenceFrame.isActive = true;
            if (oldReferenceFrame != null)
            {
                ReferenceFrameChangeOld.Invoke(oldReferenceFrame);
            }
        }
    }

    public bool isActiveInitially = false;
    public bool isActive;
    public FutureTransform FutureTransform;

    private float lastClickTime = 0;

    void Start()
    {
        if (isActiveInitially)
        {
            if (ReferenceFrame != null)
            {
                Debug.LogError("Having more than one reference frame: " + gameObject.name + " and " + ReferenceFrame.gameObject.name);
            }
            ReferenceFrame = this;
        }
        FutureTransform = GetComponent<FutureTransform>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Time.timeSinceLevelLoad - lastClickTime < 0.5f)
        {
            ReferenceFrame = this;
        }
        lastClickTime = Time.timeSinceLevelLoad;
    }
}
