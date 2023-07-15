using System;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    public float speedOfMovement = 1;
    public float speedOfZoom = 10;

    public float minPos;
    public float maxPos;
    public float minZoom;
    public float maxZoom;
    private Vector3? previousMousePosition;
    private Vector3 relativePosition;
    private float startPosZ;
    private Vector3? targetRelativePosition;
    private Vector3 targetPositionAnimationStartRelativePosition;
    private float targetPositionAnimationTime;
    public float targetPositionAnimationDuration = 0.3f;
    [NonSerialized]
    public FutureTransform followee;

    private void Start()
    {
        var position = transform.position;
        relativePosition = position;
        startPosZ = position.z;
        followee = ReferenceFrameHost.ReferenceFrame.futureTransform;
    }

    private void Update()
    {
        var z = relativePosition.z;
        var previousPosition = transform.position;
        previousPosition.z = 0;
        MouseHandler.UpdateWorldMousePosition();
        var mousePosition = MouseHandler.WorldMousePosition;
        var movementSpeed = speedOfMovement * z / startPosZ;
        var input = new Vector3(
            Input.GetAxis("Horizontal") * movementSpeed,
            Input.GetAxis("Vertical") * movementSpeed,
            Input.mouseScrollDelta.y * speedOfZoom
        );
        if (targetPositionAnimationTime >= targetPositionAnimationDuration)
        {
            targetRelativePosition = null;
        }
        if (input.sqrMagnitude > Mathf.Epsilon)
        {
            targetRelativePosition = null;
        }
        if (targetRelativePosition.HasValue)
        {
            targetPositionAnimationTime += Time.deltaTime;
            var progress = Mathf.Min(
                targetPositionAnimationTime / targetPositionAnimationDuration, 1f);
            relativePosition = targetPositionAnimationStartRelativePosition + progress * (
                        targetRelativePosition.Value - targetPositionAnimationStartRelativePosition);
        }
        
        relativePosition += input;
        relativePosition.z = Mathf.Clamp(relativePosition.z, minZoom, maxZoom);

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (z != relativePosition.z && !targetRelativePosition.HasValue)
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
        transform.position = GetReferencePosition(followee) + relativePosition;
        if (Input.GetMouseButton(1))
        {
            MouseHandler.UpdateWorldMousePosition();
            previousMousePosition = MouseHandler.WorldMousePosition;
        }
    }

    public void MoveToPosition(Vector3 target)
    {
        targetRelativePosition = target - GetReferencePosition(followee);
        targetPositionAnimationStartRelativePosition = relativePosition;
        targetPositionAnimationTime = 0f;
    }
    
    public void Follow(FutureTransform followee)
    {
        relativePosition += GetReferencePosition(this.followee) - GetReferencePosition(followee);
        this.followee = followee;
    }

    private Vector3 GetReferencePosition(FutureTransform futureTransform)
    {
        return futureTransform.GetState(TrajectoryProvider.TrajectoryStepToPhysicsStep(0))
            .position.ToVector3();
    }
}