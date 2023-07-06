using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover: MonoBehaviour
{
    public float speedOfMovment = 1;
    public float minPos;
    public float maxPos;
    public float minZoom;
    public float maxZoom;
    private Vector3? previousMousePosition = null;
    private Vector3 relativePositon;

    private void Start()
    {
        relativePositon = transform.position;
        ReferenceFrameHost.ReferenceFrameChangeOld.AddListener((ReferenceFrameHost old) =>
        {
            relativePositon += old.transform.position - ReferenceFrameHost.ReferenceFrame.transform.position;
        });
    }
    void Update()
    {
        float z = relativePositon.z;
        Vector3 previousPosition = transform.position;
        previousPosition.z = 0;
        Vector3 MousePosition = Utils.worldMousePosition;
        relativePositon += new Vector3(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical"), 
            Input.mouseScrollDelta.y
        ) * speedOfMovment;
        relativePositon.z = Mathf.Clamp(relativePositon.z, minZoom, maxZoom);

        if (z != relativePositon.z)
        {
            float k = z / (z - relativePositon.z);
            relativePositon += (MousePosition - previousPosition) / k;
        }
        if (Input.GetMouseButton(1))
        {
            if (previousMousePosition.HasValue)
            {
                relativePositon += previousMousePosition.Value - MousePosition;
            }
        }
        else
        {
            previousMousePosition = null;
        }
        relativePositon.x = Mathf.Clamp(relativePositon.x, minPos, maxPos);
        relativePositon.y = Mathf.Clamp(relativePositon.y, minPos, maxPos);
        transform.position = ReferenceFrameHost.ReferenceFrame.transform.position + relativePositon;
        if (Input.GetMouseButton(1))
        {
            previousMousePosition = Utils.worldMousePosition;
        }
    }
}
