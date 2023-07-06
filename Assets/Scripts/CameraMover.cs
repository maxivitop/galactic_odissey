using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraMover : MonoBehaviour
{
    [FormerlySerializedAs("speedOfMovment")]
    public float speedOfMovement = 1;

    public float minPos;
    public float maxPos;
    public float minZoom;
    public float maxZoom;
    private Vector3? previousMousePosition;
    private Vector3 relativePosition;

    private void Start()
    {
        relativePosition = transform.position;
        ReferenceFrameHost.referenceFrameChangeOld.AddListener((ReferenceFrameHost old) =>
        {
            relativePosition += old.transform.position -
                                ReferenceFrameHost.ReferenceFrame.transform.position;
        });
    }

    private void Update()
    {
        var z = relativePosition.z;
        var previousPosition = transform.position;
        previousPosition.z = 0;
        var mousePosition = Utils.WorldMousePosition;
        relativePosition += new Vector3(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical"),
            Input.mouseScrollDelta.y
        ) * speedOfMovement;
        relativePosition.z = Mathf.Clamp(relativePosition.z, minZoom, maxZoom);

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (z != relativePosition.z)
        {
            var k = z / (z - relativePosition.z);
            relativePosition += (mousePosition - previousPosition) / k;
        }

        if (Input.GetMouseButton(1))
        {
            if (previousMousePosition.HasValue)
                relativePosition += previousMousePosition.Value - mousePosition;
        }
        else
        {
            previousMousePosition = null;
        }

        relativePosition.x = Mathf.Clamp(relativePosition.x, minPos, maxPos);
        relativePosition.y = Mathf.Clamp(relativePosition.y, minPos, maxPos);
        transform.position =
            ReferenceFrameHost.ReferenceFrame.transform.position + relativePosition;
        if (Input.GetMouseButton(1))
        {
            previousMousePosition = Utils.WorldMousePosition;
        }
    }
}