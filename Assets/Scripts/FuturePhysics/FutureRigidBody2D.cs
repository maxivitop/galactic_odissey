using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBody2DState : IEvolving<RigidBody2DState>
{
    public Vector2d velocity;
    public double mass;
    public Vector2d acceleration = Vector2d.zero;

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
        return new RigidBody2DState(velocity, mass);
    }
}


[RequireComponent(typeof(FutureTransform))]
public class FutureRigidBody2D : FutureStateBehaviour<RigidBody2DState>
{
    private FutureTransform futureTransform;
    public Vector2 initialVelocity;
    public double initialMass;
    public IAccelerationProvider accelerationProvider;

    private void Start()
    {
        futureTransform = GetComponent<FutureTransform>();
    }

    public override void VirtualStep(int step)
    {
        var stepState = GetState(step);
        Vector2d stepDistance;
        if (accelerationProvider == null)
        {
            // D = v0*t + 1/2*a*t^2
            stepDistance = stepState.velocity * FuturePhysics.DeltaTime + 
                           stepState.acceleration *
                           (0.5f * FuturePhysics.DeltaTime * FuturePhysics.DeltaTime);
            // v1 = v0 + a*t
            stepState.velocity += stepState.acceleration * FuturePhysics.DeltaTime;
        }
        else
        {
            // Runge-Kutta 4th order simulation
            var position = (Vector2d)futureTransform.GetState(step).position;
            var v1 = stepState.velocity;
            var a1 = accelerationProvider.CalculateAcceleration(step, 0f, position) +
                     stepState.acceleration;
            var v2 = v1 + a1 * (0.5 * FuturePhysics.DeltaTime);
            var a2 = accelerationProvider.CalculateAcceleration(step, 0.5,
                position + v1 * (0.5 * FuturePhysics.DeltaTime)) + stepState.acceleration;
            var v3 = v1 + a2 * (0.5 * FuturePhysics.DeltaTime);
            var a3 = accelerationProvider.CalculateAcceleration(step, 0.5,
                position + v2 * (0.5 * FuturePhysics.DeltaTime)) + stepState.acceleration;
            var v4 = v1 + a3 * FuturePhysics.DeltaTime;
            var a4 = accelerationProvider.CalculateAcceleration(step, 1.0,
                position + v3 * FuturePhysics.DeltaTime) + stepState.acceleration;

            stepState.velocity += (a1 + 2 * a2 + 2 * a3 + a4) * (FuturePhysics.DeltaTime / 6.0f);
            stepDistance = (v1 + 2 * v2 + 2 * v3 + v4) * (FuturePhysics.DeltaTime / 6.0f);
        }

        futureTransform.GetState(step).position.x += stepDistance.x;
        futureTransform.GetState(step).position.y += stepDistance.y;
    }


    protected override RigidBody2DState GetInitialState()
    {
        return new RigidBody2DState(new Vector2d(initialVelocity), initialMass);
    }
}