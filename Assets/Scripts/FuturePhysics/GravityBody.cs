using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(FutureRigidBody2D))]
public class GravityBody : FutureBehaviour
{
    private FutureTransform futureTransform;
    private FutureRigidBody2D futureRB;
    private GravitySource myGravitySource;
    public float ellipticalOrbitThreshold = 1.5f;
    [Range(0, 1)] public float ellipticalOrbitDominatingGravityDistanceThreshold = 0.5f;

    private void Awake()
    {
        futureTransform = GetComponent<FutureTransform>();
        futureRB = GetComponent<FutureRigidBody2D>();
        TryGetComponent(out myGravitySource);
        hasVirtualStep = true;
    }

    protected override void VirtualStep(int step)
    {
        var position = futureTransform.position[step];
        if (futureRB.acceleration[step].sqrMagnitude > Mathd.Epsilon)
        {
            IntegrateStep(step, position);
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
            if ((centerPosition - position)
                .sqrMagnitude > // != r0, r0 is after integration
                distanceThresh * distanceThresh)
            {
                IntegrateStep(step, position);
                return;
            }
        }

        if (futureRB.orbit[step] == null || futureRB.orbit[step].center != closestGravitySource)
        {
            IntegrateStep(step, position);
            var r0 = futureTransform.GetFuturePosition(step) - centerPosition;
            var v0 = futureRB.velocity[step] -
                     closestGravitySource.futureRigidBody2D.velocity[step];
            futureRB.orbit[step] =
                new EllipticalOrbit(closestGravitySource, futureRB.mass[step], step, r0, v0);
            return;
        }
        
        // Use elliptical orbit
        if (futureRB.orbit[step].Evolve(step, 0f, out var rNew, out var vNew))
        {
            if (vNew.sqrMagnitude > 1e6)
            {
                vNew = futureRB.velocity[step];
                rNew = position + vNew * FuturePhysics.DeltaTime;
            }

            futureTransform.SetFuturePosition(step, rNew);
            futureRB.velocity[step] = vNew;
        }
        else
        {
            IntegrateStep(step, position);
        }
    }

    private void IntegrateStep(int step, Vector2 position)
    {
        futureRB.orbit[step] = null;
        // Runge-Kutta 4th order simulation
        var v1 = futureRB.velocity[step];
        var a1 = CalculateAcceleration(step, 0, position);
        var v2 = v1 + a1 * (0.5f * FuturePhysics.DeltaTime);
        var a2 = CalculateAcceleration(step, 0.5f,
            position + v1 * (0.5f * FuturePhysics.DeltaTime));
        var v3 = v1 + a2 * (0.5f * FuturePhysics.DeltaTime);
        var a3 = CalculateAcceleration(step, 0.5f,
            position + v2 * (0.5f * FuturePhysics.DeltaTime));
        var v4 = v1 + a3 * FuturePhysics.DeltaTime;
        var a4 = CalculateAcceleration(step, 1.0f,
            position + v3 * FuturePhysics.DeltaTime);

        futureRB.velocity[step] += (a1 + 2 * a2 + 2 * a3 + a4) * (FuturePhysics.DeltaTime / 6.0f);
        Vector3 stepDistance = (v1 + 2 * v2 + 2 * v3 + v4) * (FuturePhysics.DeltaTime / 6.0f);

        futureTransform.position[step] += stepDistance;
    }

    private Vector2 CalculateGravityAcceleration(int step, float dt, Vector3 position)
    {
        var gravity = Vector2.zero;
        foreach (var gravitySource in GravitySource.All)
        {
            if (gravitySource == myGravitySource) continue;
            gravity += OrbitUtils.CalculateGravityVector(
                gravitySource, position, futureRB.mass[step], step, dt);
        }

        return gravity;
    }

    private Vector2 CalculateGravityAccelerationFromClosest(int step,
        float dt, Vector3 position, out GravitySource closest)
    {
        closest = OrbitUtils.FindBiggestGravitySource(position, step, dt);
        return OrbitUtils.CalculateGravityVector(closest, position,
            futureRB.mass[step], step, dt);
    }

    private Vector2 CalculateAcceleration(int step, float dt, Vector3 position)
    {
        return CalculateGravityAcceleration(step, dt, position) +
               futureRB.acceleration[step];
    }
}