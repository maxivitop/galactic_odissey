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
    private FutureTransform futureTransform;
    private GravitySource myGravitySource;
    public Vector2 initialVelocity;
    public double initialMass;
    public float ellipticalOrbitThreshold = 1.5f;
    [Range(0, 1)]
    public float ellipticalOrbitDominatingGravityDistanceThreshold = 0.5f;
    public bool affectedByGravity;

    private void Start()
    {
        futureTransform = GetComponent<FutureTransform>();
        TryGetComponent(out myGravitySource);
    }

    public override void VirtualStep(int step)
    {
        if (!affectedByGravity) return;
        var stepState = GetState(step);
        var position = (Vector2d)futureTransform.GetState(step).position;
        if (stepState.acceleration.sqrMagnitude > Mathd.Epsilon)
        {
            IntegrateStep(step, position, stepState);
            return;
        }
        
        var closestGravity = CalculateGravityAccelerationFromClosest(
            step, 0f, position, out var closestGravitySource);
        var stepStartGravity = CalculateGravityAcceleration(step, 0f, position);
        
        var centerPosition =
            closestGravitySource.futurePositionProvider.GetFuturePosition(step);
        if ((closestGravity - stepStartGravity).sqrMagnitude > 
            ellipticalOrbitThreshold * ellipticalOrbitThreshold)
        {
            var centerDominatingGravity =
                closestGravitySource.CalculateDominatingGravityDistance(step);
            var distanceThresh = centerDominatingGravity *
                              ellipticalOrbitDominatingGravityDistanceThreshold;
            if (((Vector2d)centerPosition - position).sqrMagnitude > // != r0, r0 is after integration
                distanceThresh * distanceThresh)
            {
                IntegrateStep(step, position, stepState);
                return;
            }
        }
        
        if (stepState.orbit == null || stepState.orbit.center != closestGravitySource)
        {
            IntegrateStep(step, position, stepState);
            var r0 = futureTransform.GetState(step).position - centerPosition;
            var v0 = stepState.velocity - 
                     closestGravitySource.futureRigidBody2D.GetState(step).velocity;
            stepState.orbit =
                new EllipticalOrbit(closestGravitySource, stepState.mass, step, r0, v0);
            return;
        }
        
        // Use elliptical orbit
        if (stepState.orbit.Evolve(step, 0f, out var rNew, out var vNew))
        {
            if (rNew.sqrMagnitude > 1e6)
            {
                IntegrateStep(step, position, stepState);
                return;
            }
            futureTransform.GetState(step).position = rNew;
            stepState.velocity = vNew;
        }
        else
        {
            IntegrateStep(step, position, stepState);
        }
    }

    private void IntegrateStep(int step, Vector2d position, RigidBody2DState stepState)
    {
        stepState.orbit = null;
        // Runge-Kutta 4th order simulation
        var v1 = stepState.velocity;
        var a1 = CalculateAcceleration(step, 0f, position);
        var v2 = v1 + a1 * (0.5 * FuturePhysics.DeltaTime);
        var a2 = CalculateAcceleration(step, 0.5,
            position + v1 * (0.5 * FuturePhysics.DeltaTime));
        var v3 = v1 + a2 * (0.5 * FuturePhysics.DeltaTime);
        var a3 = CalculateAcceleration(step, 0.5,
            position + v2 * (0.5 * FuturePhysics.DeltaTime));
        var v4 = v1 + a3 * FuturePhysics.DeltaTime;
        var a4 = CalculateAcceleration(step, 1.0,
            position + v3 * FuturePhysics.DeltaTime);

        stepState.velocity += (a1 + 2 * a2 + 2 * a3 + a4) * (FuturePhysics.DeltaTime / 6.0f);
        Vector3d stepDistance = (v1 + 2 * v2 + 2 * v3 + v4) * (FuturePhysics.DeltaTime / 6.0f);
    
        futureTransform.GetState(step).position += stepDistance;
    }

    private Vector2d CalculateGravityAcceleration(int step, double dt, Vector3d position)
    {
        var gravity = Vector2d.zero;
        var gravitySources = FuturePhysics.GetComponents<GravitySource>(step);
        foreach (var gravitySource in gravitySources)
        {
            if (gravitySource == myGravitySource) continue;
            gravity += OrbitUtils.CalculateGravityVector(gravitySource, position, initialMass, step, dt);
        }
        return gravity;
    }

    private Vector2d CalculateGravityAccelerationFromClosest(int step,
        double dt, Vector3d position, out GravitySource closest)
    {
        closest = OrbitUtils.FindBiggestGravitySource(position, step, dt);
        return OrbitUtils.CalculateGravityVector(closest, position, initialMass, step, dt);
    }
    
    private Vector2d CalculateAcceleration(int step, double dt, Vector3d position)
    {
        return CalculateGravityAcceleration(step, dt, position) + GetState(step).acceleration;
    }


    protected override RigidBody2DState GetInitialState()
    {
        return new RigidBody2DState(new Vector2d(initialVelocity), initialMass);
    }
}