using System;
using UnityEngine;
using UnityEngine.Serialization;

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
    public float targetPositionAnimationDuration = 0.3f;
    [NonSerialized]
    public FutureTransform followee;
    private Vector3Animator targetPositionAnimator;
    [Range(0, 2)]
    public float lookRotationBias = 0.7f;
    [Range(0, 85)]
    public float maxAngle = 20f;

    private float horizontalAxis;
    private float verticalAxis;

    private void Start()
    {
        var position = transform.position;
        relativePosition = position;
        startPosZ = position.z;
        followee = ReferenceFrameHost.ReferenceFrame.futureTransform;
        targetPositionAnimator = new Vector3Animator(targetPositionAnimationDuration);
    }

    private void Update()
    {
        var z = relativePosition.z;
        var previousPosition = transform.position;
        previousPosition.z = 0;
        MouseHandler.UpdateWorldMousePosition();
        var mousePosition = MouseHandler.WorldMousePosition;
        var movementSpeed = speedOfMovement * z / startPosZ;
        GetSmoothRawAxis("Horizontal", ref horizontalAxis);
        GetSmoothRawAxis("Vertical", ref verticalAxis);
        var input = new Vector3(
            horizontalAxis * movementSpeed,
            verticalAxis * movementSpeed,
            Input.mouseScrollDelta.y * speedOfZoom
        );
        if (targetPositionAnimator.HasFinished())
        {
            targetRelativePosition = null;
        }
        if (input.sqrMagnitude > Mathf.Epsilon)
        {
            targetRelativePosition = null;
        }
        if (targetRelativePosition.HasValue)
        {
            targetPositionAnimator.ForwardTime(Time.unscaledTime);
            relativePosition = targetPositionAnimator.AnimateTowards(targetRelativePosition.Value);
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
        var referencePos = GetReferencePosition(followee);
        transform.position = referencePos + relativePosition;
        
        var groundRelativePos = relativePosition;
        groundRelativePos.z = 0;
      
        var lookTarget = referencePos + groundRelativePos * lookRotationBias;
        Quaternion initialRotation = transform.rotation;
        transform.LookAt(lookTarget);
        Quaternion targetRotation = // limit rotation to be no more than maxAngle from identity
            Quaternion.RotateTowards(Quaternion.identity, transform.rotation, maxAngle);
        transform.rotation = // animate for smooth rotation channges
            Quaternion.RotateTowards(initialRotation, targetRotation, 
               maxAngle / targetPositionAnimationDuration * Time.unscaledTime);

        if (Input.GetMouseButton(1))
        {
            MouseHandler.UpdateWorldMousePosition();
            previousMousePosition = MouseHandler.WorldMousePosition;
        }
    }

    public void MoveToPosition(Vector3 target)
    {
        targetRelativePosition = target - GetReferencePosition(followee);
        targetPositionAnimator.Capture(relativePosition);
    }
    
    public void Follow(FutureTransform followee)
    {
        relativePosition += GetReferencePosition(this.followee) - GetReferencePosition(followee);
        this.followee = followee;
    }

    private Vector3 GetReferencePosition(FutureTransform futureTransform)
    {
        if (futureTransform == null)
        {
            return transform.position - relativePosition;
        }

        return futureTransform.GetFuturePosition(TrajectoryProvider.TrajectoryStepToPhysicsStep(0));
    }
    
    private static void GetSmoothRawAxis(string name, ref float axis, float sensitivity=3f, float gravity=3f)
    {
        var r = Input.GetAxisRaw(name);
        var s = sensitivity;
        var g = gravity;
        var t = Time.unscaledDeltaTime;

        if (r != 0)
        {
            axis = Mathf.Clamp(axis + r * s * t, -1f, 1f);
        }
        else
        {
            axis = Mathf.Clamp01(Mathf.Abs(axis) - g * t) * Mathf.Sign(axis);
        }
    }
}