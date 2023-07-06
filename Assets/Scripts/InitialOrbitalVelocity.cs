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
        var r = transform.position - center.transform.position;
        var initialVelocity =
            clockWise ? PerpendicularCounterClockwise(r) : PerpendicularClockwise(r);
        initialVelocity.Normalize();
        var a = r.magnitude * aScale;
        var actualCoefficient =
            Mathf.Sqrt(center.initialMass * FuturePhysics.G * (2 / r.magnitude - 1 / a));
        initialVelocity *= actualCoefficient;

        rigidBody.initialVelocity = initialVelocity + center.initialVelocity;
    }

    private static Vector2 PerpendicularClockwise(Vector2 vector2)
    {
        return new Vector2(vector2.y, -vector2.x);
    }

    private static Vector2 PerpendicularCounterClockwise(Vector2 vector2)
    {
        return new Vector2(-vector2.y, vector2.x);
    }
}