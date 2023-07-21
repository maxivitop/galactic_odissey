
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
    public MeshFilter target;
    [NonSerialized]
    public TrajectoryProvider trajectoryProvider;
    private int step;

    private void OnEnable()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        if (target == null) return;
        meshFilter.mesh = target.mesh;
        transform.localScale = target.transform.lossyScale;
    }

    public void SetStep(int step)
    {
        this.step = step;
    }

    public void Update()
    {
        var trajStep = TrajectoryProvider.PhysicsStepToTrajectoryStep(step);
        
        if (trajectoryProvider.trajectory.size <= trajStep || trajStep < 0)
        {
            transform.position = new Vector3(1e10f, 1e10f);
            return;
        }
        transform.position = trajectoryProvider.trajectory.array[trajStep];
    }
}