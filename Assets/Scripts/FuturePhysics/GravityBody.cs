using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(FutureRigidBody2D))]
public class GravityBody: FutureBehaviour
{
    private FutureTransform futureTransform;
    private FutureRigidBody2D futureRigidBody2D;
    private GravitySource myGravitySource;
    public float ellipticalOrbitThreshold = 1.5f;
    [Range(0, 1)]
    public float ellipticalOrbitDominatingGravityDistanceThreshold = 0.5f;

    private void Start()
    {
        futureTransform = GetComponent<FutureTransform>();
        futureRigidBody2D = GetComponent<FutureRigidBody2D>();
        TryGetComponent(out myGravitySource);
    }

    public override void VirtualStep(int step)
    {
        var stepState = futureRigidBody2D.GetState(step);
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
            gravity += OrbitUtils.CalculateGravityVector(
                gravitySource, position, futureRigidBody2D.initialMass, step, dt);
        }
        return gravity;
    }

    private Vector2d CalculateGravityAccelerationFromClosest(int step,
        double dt, Vector3d position, out GravitySource closest)
    {
        closest = OrbitUtils.FindBiggestGravitySource(position, step, dt);
        return OrbitUtils.CalculateGravityVector(closest, position,
            futureRigidBody2D.initialMass, step, dt);
    }
    
    private Vector2d CalculateAcceleration(int step, double dt, Vector3d position)
    {
        return CalculateGravityAcceleration(step, dt, position) +
               futureRigidBody2D.GetState(step).acceleration;
    }
}