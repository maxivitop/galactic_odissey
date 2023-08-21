using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FutureCollision
{
    public readonly FutureCollider my;
    public readonly FutureCollider other;

    public FutureCollision(FutureCollider my, FutureCollider other)
    {
        this.my = my;
        this.other = other;
    }

    protected bool Equals(FutureCollision other)
    {
        return Equals(my, other.my) && Equals(this.other, other.other);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((FutureCollision)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(my, other);
    }

    public static bool operator ==(FutureCollision left, FutureCollision right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(FutureCollision left, FutureCollision right)
    {
        return !Equals(left, right);
    }
}

public class FutureCollisions: IEvolving<FutureCollisions>
{
    public readonly HashSet<FutureCollision> collisions;

    public FutureCollisions()
    {
        collisions = new HashSet<FutureCollision>();
    }

    public FutureCollisions Next()
    {
        return new FutureCollisions();
    }
}
public class CollisionDetector : FutureStateBehaviour<FutureCollisions>
{
    private CollisionLayer myLayer;
    private HashSet<CollisionLayer> collidesWithLayers;
    private readonly HashSet<FutureCollider> myColliders = new();
    private void Awake()
    {
        myColliders.UnionWith(GetComponents<FutureCollider>());
        var i = 0;
        foreach (var futureCollider in myColliders)
        {
            if (i != 0 && futureCollider.layer != myLayer)
            {
                Debug.LogError(
                    "Different collision layers are not supported on the same object." +
                    " Layers are " + myLayer + " and " + futureCollider.layer
                );
            }
            myLayer = futureCollider.layer;
            futureCollider.isPassive = false;
            i++;
        }

        collidesWithLayers = FutureCollider.collisionTable[myLayer];
    }

    public override bool IsAlive(int step)
    {
        return step >= startStep; // Needs to dispatch collision events
    }

    public override void Step(int step)
    {
        var prevStepCollisions = step == startStep
            ? new HashSet<FutureCollision>()
            : GetState(step - 1).collisions;
        foreach (var collision in GetState(step).collisions)
        {
            if (prevStepCollisions.Contains(collision)) continue;
            collision.my.StepCollisionEnter(step, collision);
            if (!collision.other.isPassive) continue;
            var otherCollision = new FutureCollision(collision.other, collision.my);
            collision.other.StepCollisionEnter(step, otherCollision);
        }
    }

    protected override void VirtualStep(int step)
    {
        var prevStepCollisions = step == startStep
            ? new HashSet<FutureCollision>()
            : GetState(step - 1).collisions;
        foreach (var layer in collidesWithLayers)
        {
            var colliders = FutureCollider.layerToColliders[layer];
            foreach(var otherCollider in colliders)
            {
                if (!otherCollider.IsAlive(step)) continue;
                if (myColliders.Contains(otherCollider)) continue;
                foreach (var myCollider in myColliders)
                {
                    if (!myCollider.IsAlive(step)) continue;
                    if (!CheckCollision(step, myCollider, otherCollider)) continue;
                    var collision = new FutureCollision(myCollider, otherCollider);
                    if (!prevStepCollisions.Contains(collision))
                    {
                        myCollider.VirtualStepCollisionEnter(step, collision);
                        if (otherCollider.isPassive)
                        {
                            var otherCollision = new FutureCollision(otherCollider, myCollider);
                            otherCollider.VirtualStepCollisionEnter(step, otherCollision);
                        }
                    } // OnCollisionStay goes in else if needed 
                    GetState(step).collisions.Add(collision);
                }
            }
        }
    }

    protected override FutureCollisions GetInitialState()
    {
        return new FutureCollisions();
    }
    
    public static bool CheckCollision(int step, FutureCollider lhs, FutureCollider rhs)
    {
        var left = lhs as CircleFutureCollider;
        var right = rhs as CircleFutureCollider;
        var dist = Vector3.SqrMagnitude(
            left!.futureTransform.GetFuturePosition(step)-
            right!.futureTransform.GetFuturePosition(step));
        var collisionDistance = left.radius + right.radius;
        return dist < collisionDistance * collisionDistance;
    }
}
