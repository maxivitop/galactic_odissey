using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBody2DState : Evolving<RigidBody2DState>
{
    public Vector2 velocity;
    public float mass;
    public Vector2 acceleration = Vector2.zero;

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

    public RigidBody2DState next()
    {
        return new RigidBody2DState(velocity, mass);
    }
}

public interface AccelerationProvider
{
    Vector2 CalculateAcceleration(int step, Vector3 pos);
}


[RequireComponent(typeof(FutureTransform))]
public class FutureRigidBody2D : FutureStateBehaviour<RigidBody2DState>
{
    private FutureTransform futureTransform;
    public Vector2 initialVelocity;
    public float initialMass;
    public AccelerationProvider accelerationProvider;

    void Start()
    {
        futureTransform = GetComponent<FutureTransform>();
    }

    public override void VirtualStep(int step)
    {
        var stepState = GetState(step);
        Vector2 stepDistance;
        if (accelerationProvider == null)
        {
            // D = v0*t + 1/2*a*t^2
            stepDistance = stepState.velocity * FuturePhysics.deltaTime + 0.5f * stepState.acceleration * FuturePhysics.deltaTime * FuturePhysics.deltaTime;       
            // v1 = v0 + a*t
            stepState.velocity += stepState.acceleration * FuturePhysics.deltaTime;
        }
        else
        {
            // Runge-Knutta 4th order simulation
            var position = (Vector2)futureTransform.GetState(step).position;
            var v1 = stepState.velocity;
            var a1 = accelerationProvider.CalculateAcceleration(step, position) + stepState.acceleration;
            var v2 = v1 + a1 * 0.5f * FuturePhysics.deltaTime;
            var a2 = accelerationProvider.CalculateAcceleration(step, position + v1 * 0.5f * FuturePhysics.deltaTime) + stepState.acceleration;
            var v3 = v1 + a2 * 0.5f * FuturePhysics.deltaTime;
            var a3 = accelerationProvider.CalculateAcceleration(step, position + v2 * 0.5f * FuturePhysics.deltaTime) + stepState.acceleration;
            var v4 = v1 + a3 * FuturePhysics.deltaTime;
            var a4 = accelerationProvider.CalculateAcceleration(step, position + v3 * FuturePhysics.deltaTime) + stepState.acceleration;

            stepState.velocity += (a1 + 2 * a2 + 2 * a3 + a4) / 6.0f * FuturePhysics.deltaTime;
            stepDistance = (v1 + 2 * v2 + 2 * v3 + v4) / 6.0f * FuturePhysics.deltaTime;
        }

        futureTransform.GetState(step).position.x += stepDistance.x;
        futureTransform.GetState(step).position.y += stepDistance.y;
    }


    protected override RigidBody2DState GetInitialState()
    {
        return new RigidBody2DState(initialVelocity, initialMass);
    }
}
