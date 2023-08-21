using System;
using UnityEngine;

[RequireComponent(typeof(CircleFutureCollider))]
[RequireComponent(typeof(FutureRigidBody2D))]
[RequireComponent(typeof(FutureTransform))]
public class Projectile: FutureBehaviour
{
    [NonSerialized] public FutureTransform futureTransform;
    [NonSerialized] public FutureRigidBody2D futureRigidBody2D;
    [NonSerialized] public CircleFutureCollider futureCollider;
    public GameObject explosion;

    public Vector3[] traj;
    public int trajLen;
    public int launchStep;
    
    private void Awake()
    {
        futureRigidBody2D = GetComponent<FutureRigidBody2D>();
        futureTransform = GetComponent<FutureTransform>();
        futureCollider = GetComponent<CircleFutureCollider>();
        
    }

    public void Launch(int step, Vector3[] trajectory, int trajectoryLength)
    {
        launchStep = step;
        traj = (Vector3[])trajectory.Clone();
        trajLen = trajectoryLength;
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
        transform.position = traj[idx];
        foreach (var layer in FutureCollider.collisionTable[futureCollider.layer])
        {
            foreach (var otherCollider in FutureCollider.layerToColliders[layer])
            {
                var other = otherCollider as CircleFutureCollider;
                var my = futureCollider;
                var dist = Vector3.SqrMagnitude(
                    other!.futureTransform.GetFuturePosition(step).ToVector3()
                    - traj[idx]);
                var collisionDistance = other.radius + my.radius;
                if (dist < collisionDistance * collisionDistance)
                {
                    Instantiate(explosion, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }

    public override bool CatchUpWithVirtualStep(int virtualStep)
    {
        return true;
    }
}
