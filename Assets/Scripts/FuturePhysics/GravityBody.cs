using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FutureTransform))]
[RequireComponent(typeof(FutureRigidBody2D))]
public class GravityBody : FutureBehaviour
{
    public GravitySource[] sources;
    public FutureTransform futureTransform;
    public FutureRigidBody2D futureRigidBody2D;
    private GravitySource myGravitySource;
    private void Start()
    {
        futureTransform = GetComponent<FutureTransform>();
        futureRigidBody2D = GetComponent<FutureRigidBody2D>();
        myGravitySource = GetComponent<GravitySource>();
        futureRigidBody2D.accelerationProvider = new GravityAccelerationProvider(futureTransform, myGravitySource);
    }

    class GravityAccelerationProvider : AccelerationProvider
    {

        FutureTransform futureTransform;
        GravitySource myGravitySource;
        public GravityAccelerationProvider(
            FutureTransform futureTransform,
            GravitySource myGravitySource
        )
        {
            this.futureTransform = futureTransform;
            this.myGravitySource = myGravitySource;
        }
        public Vector2 CalculateAcceleration(int step, Vector3 position)
        {
            Vector2 gravity = Vector2.zero;
            IEnumerable<GravitySource> gravitySources = FuturePhysics.GetComponents<GravitySource>(step);
            foreach (var gravitySource in gravitySources)
            {
                if (gravitySource == myGravitySource)
                {
                    continue;
                }
                Vector2 direction = gravitySource.futureTransform.GetState(step).position - position;
                // F = ma
                // F = G*m1*m2/r^2
                // a = F/m = G*m2/r^2
                gravity += FuturePhysics.G * gravitySource.futureRigidBody2D.initialMass * direction.normalized / direction.sqrMagnitude;
            }
            return gravity;
        }
    }
   /* public override void VirtualStep(int step)
    {
        Vector2 gravity = Vector2.zero;
        IEnumerable<GravitySource> gravitySources = sources.Length == 0 ? FuturePhysics.GetComponents<GravitySource>(step) : sources;
        Vector3 position = futureTransform.GetState(step).position;
        foreach (var gravitySource in gravitySources)
        {
            if (gravitySource == myGravitySource)
            {
                continue;
            }
            Vector2 direction = gravitySource.futureTransform.GetState(step).position - position;
            // F = ma
            // F = G*m1*m2/r^2
            // a = F/m = G*m2/r^2
            gravity += FuturePhysics.G * gravitySource.futureRigidBody2D.initialMass * direction.normalized / direction.sqrMagnitude;
        }
        futureRigidBody2D.GetState(step).AddAcceleration(gravity);
    }*/

}
