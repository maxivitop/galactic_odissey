using System;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    public static CameraMover Instance;
    public float mouseMovementSmoothingSpeed = 1f;
    public float speedOfMovement = 1;
    public float rotationXSpeed = 1;
    public float rotationYSpeed = 1;
    public float speedOfZoom = 10;
    public float zoomPerUnit = 10;
    public float maxPos;
    public float minZoom;
    public float maxZoom;
    public float targetPositionAnimationDuration = 0.3f;
    [Range(0, 85)] public float minAngle = 20f;
    [Range(0, 180)] public float maxAngle = 90f;

    [NonSerialized] public float zoom;

    private FutureTransform followee;

    private Vector3 referencePos; // position of reference celestial body
    private Vector3 centerPosition; // vector from referencePos to center of rotation

    private Vector3
        relativePosition; // vector from (centerPosition + referencePos) to camera position

    private Vector3 lastReferencePos;

    private Transform targetCenter; // for animation
    private Vector3Animator targetPositionAnimator;
    private bool animating;

    private float horizontalAxis;
    private float verticalAxis;
    private float mouseXAxis;
    private float mouseYAxis;
    private float mouseZAxis;
    private float unusedScroll;
    private float unusedXMouseMovement;
    private float unusedYMouseMovement;

    public Vector3 CenterPosition =>
        referencePos + centerPosition;

    private void Awake()
    {
        Instance = this;
        var position = transform.position;
        centerPosition = Vector3.zero;
        relativePosition = position;
        zoom = relativePosition.magnitude;
        targetPositionAnimator = new Vector3Animator(targetPositionAnimationDuration);
    }

    private void Start()
    {
        followee = ReferenceFrameHost.ReferenceFrame.futureTransform;
        lastReferencePos = GetReferencePosition(followee);
    }

    private void Update()
    {
        mouseXAxis = Input.GetAxisRaw("Mouse X");
        mouseYAxis = Input.GetAxisRaw("Mouse Y");
        mouseZAxis = Input.GetAxisRaw("Mouse ScrollWheel");
        GetSmoothRawAxis("Horizontal", ref horizontalAxis);
        GetSmoothRawAxis("Vertical", ref verticalAxis);

        referencePos = GetReferencePosition(followee);
        transform.position += referencePos - lastReferencePos;
        
        RotateAroundByMouse();

        var moved = MoveCenterByInput();
        var zoomed = ZoomFromMouseWheel();
        if (moved || zoomed)
        {
            animating = false;
        }

        if (animating)
        {
            targetPositionAnimator.ForwardTime(Time.unscaledDeltaTime);
            centerPosition = targetPositionAnimator.AnimateTowards(Vector3.zero);
        }

        transform.position = referencePos + centerPosition + relativePosition;
        lastReferencePos = referencePos;
    }

    public void Follow(FutureTransform followee)
    {
        centerPosition += GetReferencePosition(this.followee) - GetReferencePosition(followee);
        targetPositionAnimator.Capture(centerPosition);
        animating = true;
        unusedScroll = 0f;
        lastReferencePos = GetReferencePosition(followee);
        this.followee = followee;
    }

    private Vector3 GetReferencePosition(FutureTransform futureTransform)
    {
        if (futureTransform == null)
        {
            return transform.position - relativePosition;
        }

        return futureTransform.transform.position;
    }

    private void RotateAroundByMouse()
    {
        if(Input.GetMouseButton(1))
        {
            unusedXMouseMovement += mouseXAxis;
            unusedYMouseMovement += mouseYAxis;
        }
        var xMovement = GetSmoothedValue(ref unusedXMouseMovement, mouseMovementSmoothingSpeed);
        var yMovement = GetSmoothedValue(ref unusedYMouseMovement, mouseMovementSmoothingSpeed);
        var yRot = yMovement * rotationYSpeed;
        var groundPos = relativePosition;
        groundPos.z = 0;
        var angleY = Vector3.Angle(groundPos, relativePosition);
        if (angleY - yRot < minAngle)
        {
            yRot = angleY - minAngle;
        }

        if (angleY - yRot > maxAngle)
        {
            yRot = angleY - maxAngle;
        }
        transform.RotateAround(referencePos + centerPosition, -Vector3.forward,
            xMovement * rotationXSpeed);
        transform.RotateAround(referencePos + centerPosition, -transform.right, yRot);
        relativePosition = transform.position - centerPosition - referencePos;
    }

    private bool MoveCenterByInput()
    {
        var movementSpeed = speedOfMovement * relativePosition.magnitude;

        var groundRelativePosition = relativePosition;
        groundRelativePosition.z = 0;
        var input =
            (horizontalAxis * transform.right
             - verticalAxis * groundRelativePosition.normalized
            ) * (movementSpeed * Time.unscaledDeltaTime);
        centerPosition += input;


        if (centerPosition.sqrMagnitude > maxPos * maxPos)
        {
            centerPosition = centerPosition.normalized * maxPos;
        }

        return input.sqrMagnitude > Mathf.Epsilon;
    }

    private float GetSmoothedValue(ref float accumulator, float speed)
    {
        var valueThisFrame = Time.unscaledDeltaTime * Mathf.Sign(accumulator) * speed;
        if (Mathf.Abs(valueThisFrame) > Mathf.Abs(accumulator))
        {
            valueThisFrame = accumulator;
        }
        accumulator -= valueThisFrame;
        return valueThisFrame;
    }

    private bool ZoomFromMouseWheel()
    {
        unusedScroll += mouseZAxis;
        var zoomThisFrame = GetSmoothedValue(ref unusedScroll, speedOfZoom);
        var zoomAmount = zoomThisFrame * zoomPerUnit;
        if (zoomAmount >= relativePosition.magnitude)
        {
            relativePosition = relativePosition.normalized * minZoom;
            zoomAmount = 0;
        }

        relativePosition -= relativePosition.normalized * zoomAmount;
        if (relativePosition.sqrMagnitude < minZoom * minZoom)
        {
            relativePosition = relativePosition.normalized * minZoom;
        }

        if (relativePosition.sqrMagnitude > maxZoom * maxZoom)
        {
            relativePosition = relativePosition.normalized * maxZoom;
        }

        zoom = relativePosition.magnitude;
        return mouseZAxis * mouseZAxis > Mathf.Epsilon;
    }
    
    private static void GetSmoothRawAxis(string name, ref float axis, float sensitivity = 3f,
        float gravity = 3f)
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