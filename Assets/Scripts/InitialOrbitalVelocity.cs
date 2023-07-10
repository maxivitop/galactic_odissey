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

    private FutureRigidBody2D rigidBody;

    // Start is called before the first frame update
    private void Awake()
    {
        SetInitialVelocity();
    }

    private void SetInitialVelocity()
    {
        rigidBody = GetComponent<FutureRigidBody2D>();
        var r = new Vector3d(transform.position - center.transform.position);
        var initialVelocity =
            clockWise ? PerpendicularCounterClockwise(r) : PerpendicularClockwise(r);
        initialVelocity.Normalize();
        var a = r.magnitude * aScale;
        var actualCoefficient =
            Math.Sqrt(center.initialMass * FuturePhysics.G * (2 / r.magnitude - 1 / a));
        initialVelocity *= actualCoefficient;

        rigidBody.initialVelocity = initialVelocity + center.initialVelocity;
    }

    private static Vector2d PerpendicularClockwise(Vector2d v2)
    {
        return new Vector2d(v2.y, -v2.x);
    }

    private static Vector2d PerpendicularCounterClockwise(Vector2d v2)
    {
        return new Vector2d(-v2.y, v2.x);
    }
}