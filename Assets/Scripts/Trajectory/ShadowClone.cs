
using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(PlanetRotator))]
public class ShadowClone: MonoBehaviour
{
    [NonSerialized]
    public MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    [NonSerialized]
    public MeshFilter targetMesh;
    [NonSerialized]
    public GameObject targetGameObject;
    [NonSerialized]
    public TrajectoryProvider trajectoryProvider;
    private int step = -1;
    [NonSerialized]
    public PlanetRotator myPlanetRotator;
    private float destroyCountDown;
    private Vector3 initialScale;
    public float scaleMult = 1f;

    private void Awake()
    {
        myPlanetRotator = GetComponent<PlanetRotator>();
        if (initialScale == Vector3.zero)
        {
            initialScale = transform.lossyScale;
        }
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }
    
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (targetGameObject == null) return;
        Transform scaleReference;
        if (targetMesh == null)
        {
            scaleReference = targetGameObject.transform;
        }
        else
        {
            var meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh = targetMesh.mesh;
            scaleReference = targetMesh.transform;
        }
        transform.localScale = new Vector3(
            initialScale.x * scaleReference.lossyScale.x,
            initialScale.y * scaleReference.lossyScale.y,
            initialScale.z * scaleReference.lossyScale.z
        ) * scaleMult;
    }

    public void SetStep(int step)
    {
        this.step = step;
        myPlanetRotator.SetStep(step);
    }

    public void Update()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }
        if (ReferenceFrameHost.ReferenceFrame.gameObject == targetGameObject)
        {
            var targetTransform = targetMesh != null ? targetMesh.transform : targetGameObject.transform;
            transform.position = targetTransform.position;
            transform.rotation = targetTransform.rotation;
            return;
        } 
        var trajStep = TrajectoryProvider.PhysicsStepToTrajectoryStep(step);
        
        if (trajStep < 0 || trajectoryProvider.trajectory.size <= trajStep)
        {
            transform.position = new Vector3(1e10f, 1e10f);
            return;
        }
        transform.position = trajectoryProvider.trajectory[trajStep];
    }
}