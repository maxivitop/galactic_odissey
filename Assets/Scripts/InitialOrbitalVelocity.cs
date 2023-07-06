using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FutureRigidBody2D))]
public class InitialOrbitalVelocity : FutureBehaviour
{
    public FutureRigidBody2D center;
    [Range(0.5f, 2f)]
    public float aScale = 1f;
    public bool clockWise = true;
    FutureRigidBody2D rigidBody;
    // Start is called before the first frame update
    void Awake()
    {
        SetInitialVelocity();
    }

    public void SetInitialVelocity()
    {
        rigidBody = GetComponent<FutureRigidBody2D>();
        var r = transform.position - center.transform.position;
        Vector2 initialVelocity = clockWise ? PerpendicularCounterClockwise(r) : PerpendicularClockwise(r);
        initialVelocity.Normalize();
        var a = r.magnitude * aScale;
        var actualCoef = Mathf.Sqrt(center.initialMass * FuturePhysics.G * (2 / r.magnitude - 1 / a));
        initialVelocity *= actualCoef;

        rigidBody.initialVelocity = initialVelocity + center.initialVelocity;
    }

    private void Update()
    {
        
    }


    public static Vector2 PerpendicularClockwise(Vector2 vector2)
    {
        return new Vector2(vector2.y, -vector2.x);
    }

    public static Vector2 PerpendicularCounterClockwise(Vector2 vector2)
    {
        return new Vector2(-vector2.y, vector2.x);
    }
}
