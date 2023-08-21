using UnityEngine;


[RequireComponent(typeof(FutureRigidBody2D))]
public class GravityBody : FutureBehaviour
{
    private FutureTransform futureTransform;
    private FutureRigidBody2D futureRigidBody2D;
    private GravitySource myGravitySource;
    public float ellipticalOrbitThreshold = 1.5f;
    [Range(0, 1)] public float ellipticalOrbitDominatingGravityDistanceThreshold = 0.5f;

    private void Awake()
    {
        futureTransform = GetComponent<FutureTransform>();
        futureRigidBody2D = GetComponent<FutureRigidBody2D>();
        TryGetComponent(out myGravitySource);
    }

    protected override void VirtualStep(int step)
    {
        var stepState = futureRigidBody2D.GetState(step);
        var position = futureTransform.GetFuturePosition(step);
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
            if ((centerPosition - position)
                .sqrMagnitude > // != r0, r0 is after integration
                distanceThresh * distanceThresh)
            {
                IntegrateStep(step, position, stepState);
                return;
            }
        }

        if (stepState.orbit == null || stepState.orbit.center != closestGravitySource)
        {
            IntegrateStep(step, position, stepState);
            var r0 = futureTransform.GetFuturePosition(step) - centerPosition;
            var v0 = stepState.velocity -
                     closestGravitySource.futureRigidBody2D.GetState(step).velocity;
            stepState.orbit =
                new EllipticalOrbit(closestGravitySource, stepState.mass, step, r0, v0);
            return;
        }
        
        // Use elliptical orbit
        if (stepState.orbit.Evolve(step, 0f, out var rNew, out var vNew))
        {
            if (vNew.sqrMagnitude > 1e6)
            {
                if (!double.IsNormal(vNew.sqrMagnitude))
                {
                    vNew = stepState.velocity;
                }
                vNew = Vector2.ClampMagnitude(vNew, 1e3f);
                rNew = position + vNew * FuturePhysics.DeltaTime;
            }

            futureTransform.SetFuturePosition(step, rNew);
            stepState.velocity = vNew;
        }
        else
        {
            IntegrateStep(step, position, stepState);
        }
    }

    private void IntegrateStep(int step, Vector2 position, RigidBody2DState stepState)
    {
        stepState.orbit = null;
        // Runge-Kutta 4th order simulation
        var v1 = stepState.velocity;
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

        stepState.velocity += (a1 + 2 * a2 + 2 * a3 + a4) * (FuturePhysics.DeltaTime / 6.0f);
        Vector3 stepDistance = (v1 + 2 * v2 + 2 * v3 + v4) * (FuturePhysics.DeltaTime / 6.0f);

        futureTransform.SetFuturePosition(step,
            futureTransform.GetFuturePosition(step) + stepDistance);
    }

    private Vector2 CalculateGravityAcceleration(int step, float dt, Vector3 position)
    {
        var gravity = Vector2.zero;
        var gravitySources = FuturePhysics.GetComponents<GravitySource>(step);
        foreach (var gravitySource in gravitySources)
        {
            if (gravitySource == myGravitySource) continue;
            gravity += OrbitUtils.CalculateGravityVector(
                gravitySource, position, futureRigidBody2D.initialMass, step, dt);
        }

        return gravity;
    }

    private Vector2 CalculateGravityAccelerationFromClosest(int step,
        float dt, Vector3 position, out GravitySource closest)
    {
        closest = OrbitUtils.FindBiggestGravitySource(position, step, dt);
        return OrbitUtils.CalculateGravityVector(closest, position,
            futureRigidBody2D.initialMass, step, dt);
    }

    private Vector2 CalculateAcceleration(int step, float dt, Vector3 position)
    {
        return CalculateGravityAcceleration(step, dt, position) +
               futureRigidBody2D.GetState(step).acceleration;
    }
}