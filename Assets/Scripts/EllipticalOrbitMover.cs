using System;
using UnityEngine;

[RequireComponent(typeof(FutureRigidBody2D))]
public class EllipticalOrbitMover : FutureBehaviour, IFuturePositionProvider
{
    public GravitySource center;

    private IFuturePositionProvider centerPositionProvider;
    private FutureRigidBody2D rigidBody;
    private FutureTransform futureTransform;
    private EllipticalOrbit ellipticalOrbit;

    private readonly CapacityArray<Vector3> positionsCache = new(FuturePhysics.MaxSteps * 2 + 2);
    private readonly CapacityArray<Vector3> velocitiesCache = new(FuturePhysics.MaxSteps * 2 + 2);
    private int lastCacheStep = -1;

    private void Start()
    {
        futureTransform = GetComponent<FutureTransform>();
        rigidBody = GetComponent<FutureRigidBody2D>();
        if (center == null)
        {
            center = OrbitUtils.FindBiggestGravitySource(transform.position);
        }
        var r0 = transform.position - center.transform.position;
        var v0 =  rigidBody.initialVelocity;
        ellipticalOrbit = new EllipticalOrbit(center, rigidBody.initialMass, 0, r0, v0); 
    }

    protected override void VirtualStep(int step)
    {
        var (position, velocity) = GetPositionAndVelocity(step, 0);
        futureTransform.SetFuturePosition(step, position);
        rigidBody.GetState(step).velocity = velocity;
    }

    public Vector3 GetFuturePosition(int step, float dt)
    {
        if (ellipticalOrbit == null) // not initialized yet
        {
            return transform.position;
        }

        return GetPositionAndVelocity(step, dt).Item1;
    }

    private (Vector3, Vector3) GetPositionAndVelocity(int step, float dt)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (dt == 1.0f)
        {
            return GetPositionAndVelocity(step + 1, 0);
        }
        var canBeCached = dt is 0 or 0.5f;
        if (!canBeCached)
        {
            Debug.LogWarning("Cannot use cache dt="+dt);
            ellipticalOrbit.Evolve(step, dt, out var position, out var velocity);
            return (position, velocity);
        }
        if (step > lastCacheStep + 1)
        {
            Debug.LogWarning("Cannot use cache step="+step+" my last step="+lastCacheStep);
            ellipticalOrbit.Evolve(step, dt, out var position, out var velocity);
            return (position, velocity);
        }

        if (step > lastCacheStep)
        {
            ellipticalOrbit.Evolve(step, 0, out var position, out var velocity);
            ellipticalOrbit.Evolve(step, 0.5f, out var positionH, out var velocityH);
            positionsCache[step * 2] = position;
            positionsCache[step * 2 + 1] = positionH;
            velocitiesCache[step * 2] = velocity;
            velocitiesCache[step * 2 + 1] = velocityH;
            lastCacheStep++;
        }
        var cachePosition = step * 2;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (dt == 0.5f) cachePosition++;
        return (positionsCache[cachePosition], velocitiesCache[cachePosition]);
    }


    public int GetPriority()
    {
        return 1010;
    }
}