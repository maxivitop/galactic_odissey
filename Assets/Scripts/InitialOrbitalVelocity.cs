using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FutureRigidBody2D))]
public class InitialOrbitalVelocity : FutureBehaviour
{
    public FutureRigidBody2D center;
    [Range(0.5f, 2f)] public float aScale = 1f;
    public bool clockWise = true;
    public bool addCenterVelocity;

    private FutureRigidBody2D rigidBody;
    private bool hasSet;

    // Start is called before the first frame update
    private void Start()
    {
        if (isActiveAndEnabled)
        {
            SetInitialVelocity();
        }
    }

    private void SetInitialVelocity()
    {
        if (hasSet) return;
        hasSet = true;
        
        rigidBody = GetComponent<FutureRigidBody2D>();
        if (center == null)
        {
            center = OrbitUtils.FindBiggestGravitySource(transform.position, FuturePhysics.currentStep)
                .futureRigidBody2D;
        }

        var centerVelocity = Vector2.zero;
        if (addCenterVelocity)
        {
            center.TryGetComponent<InitialOrbitalVelocity>(out var centerInitialVelocity);
            if (centerInitialVelocity != null)
            {
                centerInitialVelocity.SetInitialVelocity();
            }

            centerVelocity = center.InitialVelocity;
        }
        
        
        var r = transform.position - center.transform.position;
        var initialVelocity =
            clockWise ? PerpendicularCounterClockwise(r) : PerpendicularClockwise(r);
        initialVelocity.Normalize();
        var a = r.magnitude * aScale;
        var actualCoefficient =
            Math.Sqrt(center.initialMass * FuturePhysics.G * (2 / r.magnitude - 1 / a));
        initialVelocity *= (float) actualCoefficient;

        rigidBody.InitialVelocity = initialVelocity + centerVelocity;
    }

    private static Vector2 PerpendicularClockwise(Vector2 v2)
    {
        return new Vector2(v2.y, -v2.x);
    }

    private static Vector2 PerpendicularCounterClockwise(Vector2 v2)
    {
        return new Vector2(-v2.y, v2.x);
    }
}