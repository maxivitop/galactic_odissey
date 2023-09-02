using System;
using UnityEngine;

[RequireComponent(typeof(CircleFutureCollider))]
[RequireComponent(typeof(FutureRigidBody2D))]
[RequireComponent(typeof(FutureTransform))]
public class Projectile: FutureBehaviour
{
    [NonSerialized] public FutureTransform futureTransform;
    [NonSerialized] public CollisionDetector collisionDetector;
    public int trajLen;
    public int launchStep;
    
    private void Awake()
    {
        futureTransform = GetComponent<FutureTransform>();
        collisionDetector = GetComponent<CollisionDetector>();
    }

    public void Launch(int step, Vector3[] trajectory, int trajectoryLength)
    {
        launchStep = step;
        trajLen = trajectoryLength;
        futureTransform.position.Initialize(step, trajectory, trajectoryLength, ToString());
        futureTransform.Disable(step + trajectoryLength);
        collisionDetector.StartStep = step;
    }

    public override void Step(int step)
    {
        if (step < launchStep) return;
        var idx = step - launchStep;
        if (idx >= trajLen)
        {
            // Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
            return;
        }
        // transform.position = traj[idx];
        // foreach (var layer in FutureCollider.collisionTable[futureCollider.layer])
        // {
        //     foreach (var otherCollider in FutureCollider.layerToColliders[layer])
        //     {
        //         var other = otherCollider as CircleFutureCollider;
        //         var my = futureCollider;
        //         var dist = Vector3.SqrMagnitude(
        //             other!.futureTransform.GetFuturePosition(step)
        //             - traj[idx]);
        //         var collisionDistance = other.radius + my.radius;
        //         if (dist < collisionDistance * collisionDistance)
        //         {
        //             Instantiate(explosion, transform.position, Quaternion.identity);
        //             Destroy(gameObject);
        //             return;
        //         }
        //     }
        // }
    }

    public override bool CatchUpWithVirtualStep(int virtualStep)
    {
        return true;
    }
}
