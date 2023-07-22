
using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
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
        meshFilter = GetComponent<MeshFilter>();
        if (targetMesh == null) return;
        meshFilter.mesh = targetMesh.mesh;
        transform.localScale = targetMesh.transform.lossyScale;
    }

    public void SetStep(int step)
    {
        this.step = step;
    }

    public void Update()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }
        var trajStep = TrajectoryProvider.PhysicsStepToTrajectoryStep(step);
        
        if (trajStep < 0 || trajectoryProvider.trajectory.size <= trajStep)
        {
            transform.position = new Vector3(1e10f, 1e10f);
            return;
        }
        transform.position = trajectoryProvider.trajectory.array[trajStep];
    }
}