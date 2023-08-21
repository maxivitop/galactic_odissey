using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBody2DState : IEvolving<RigidBody2DState>
{
    public Vector2 velocity;
    public float mass;
    public Vector2 acceleration = Vector2.zero;
    public EllipticalOrbit orbit;

    public RigidBody2DState(Vector2 velocity, float mass)
    {
        this.velocity = velocity;
        this.mass = mass;
    }


    public void AddAcceleration(Vector2 acceleration)
    {
        this.acceleration += acceleration;
    }

    public void AddForce(Vector2 force)
    {
        AddAcceleration(force / mass);
    }

    public RigidBody2DState Next()
    {
        return new RigidBody2DState(velocity, mass)
        {
            orbit = orbit
        };
    }
}


[RequireComponent(typeof(FutureTransform))]
public class FutureRigidBody2D : FutureStateBehaviour<RigidBody2DState>
{
    public Vector2 initialVelocity;
    public float initialMass;

    protected override RigidBody2DState GetInitialState()
    {
        return new RigidBody2DState(initialVelocity, initialMass);
    }
}