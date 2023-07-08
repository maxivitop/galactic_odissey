using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FutureTransform))]
[RequireComponent(typeof(FutureRigidBody2D))]
public class GravityBody : FutureBehaviour
{
    public FutureTransform futureTransform;
    public FutureRigidBody2D futureRigidBody2D;
    private GravitySource myGravitySource;

    private void Start()
    {
        futureTransform = GetComponent<FutureTransform>();
        futureRigidBody2D = GetComponent<FutureRigidBody2D>();
        myGravitySource = GetComponent<GravitySource>();
        futureRigidBody2D.accelerationProvider =
            new GravityAccelerationProvider(myGravitySource);
    }

    private class GravityAccelerationProvider : IAccelerationProvider
    {
        private readonly GravitySource myGravitySource;

        public GravityAccelerationProvider(
            GravitySource myGravitySource
        )
        {
            this.myGravitySource = myGravitySource;
        }

        public Vector2 CalculateAcceleration(int step, float dt, Vector3 position)
        {
            var gravity = Vector2.zero;
            var gravitySources = FuturePhysics.GetComponents<GravitySource>(step);
            foreach (var gravitySource in gravitySources)
            {
                if (gravitySource == myGravitySource) continue;
                Vector2 direction =
                    gravitySource.futurePositionProvider.GetFuturePosition(step, dt) - position;
                // F = ma
                // F = G*m1*m2/r^2
                // a = F/m = G*m2/r^2
                gravity += FuturePhysics.G * gravitySource.futureRigidBody2D.initialMass *
                    direction.normalized / direction.sqrMagnitude;
            }

            return gravity;
        }
    }
}