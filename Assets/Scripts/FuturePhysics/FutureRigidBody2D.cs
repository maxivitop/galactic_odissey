using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBody2DState : IEvolving<RigidBody2DState>
{
    public Vector2d velocity;
    public double mass;
    public Vector2d acceleration = Vector2d.zero;
    public EllipticalOrbit orbit;

    public RigidBody2DState(Vector2d velocity, double mass)
    {
        this.velocity = velocity;
        this.mass = mass;
    }


    public void AddAcceleration(Vector2d acceleration)
    {
        this.acceleration += acceleration;
    }

    public void AddForce(Vector2d force)
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
    public double initialMass;
    public bool processCollisions;

    protected override RigidBody2DState GetInitialState()
    {
        return new RigidBody2DState(new Vector2d(initialVelocity), initialMass);
    }
}