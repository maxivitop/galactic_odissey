using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FutureCollision
{
    public FutureCollider my;
    public FutureCollider other;

    public FutureCollision(FutureCollider my, FutureCollider other)
    {
        this.my = my;
        this.other = other;
    }

    private bool Equals(FutureCollision another)
    {
        return Equals(my, another.my) && Equals(other, another.other) ||
               Equals(other, another.my) && Equals(my, another.other);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((FutureCollision)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(my, other) + HashCode.Combine(other, my);
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
public class CollisionProcessor : FutureStateBehaviour<FutureCollisions>
{
    public override void Step(int step)
    {
        var prevStepCollisions = step == 0
            ? new HashSet<FutureCollision>()
            : GetState(step - 1).collisions;
        foreach (var collision in GetState(step).collisions)
        {
            if (prevStepCollisions.Contains(collision)) continue;
            collision.my.StepCollisionEnter(step, collision);
            collision.other.StepCollisionEnter(step, collision);
        }
    }

    public override void VirtualStep(int step)
    {
        var prevStepCollisions = step == 0
            ? new HashSet<FutureCollision>()
            : GetState(step - 1).collisions;
        var colliders = FuturePhysics.GetComponents<FutureCollider>(step).ToList();
        for (var i = 0; i < colliders.Count; i++)
        {
            var coll1 = colliders[i];
            if (coll1.isPassive || !coll1.IsAlive(step)) continue;
            for (var j = 0; j < colliders.Count; j++)
            {
                var coll2 = colliders[j];
                if (!coll2.isPassive && i <= j || !coll2.IsAlive(step)) continue; // coll2 already checked collision with coll1
                if (!CheckCollision(step, coll1, coll2)) continue;
                
                var collision = new FutureCollision(coll1, coll2);
                if (!prevStepCollisions.Contains(collision))
                {
                    coll1.VirtualStepCollisionEnter(step, collision);
                    coll2.VirtualStepCollisionEnter(step, collision);
                } // OnCollisionStay goes in else if needed 
                GetState(step).collisions.Add(collision);
            }
        }
    }

    protected override FutureCollisions GetInitialState()
    {
        return new FutureCollisions();
    }
    
    private static bool CheckCollision(int step,  FutureCollider lhs, FutureCollider rhs)
    {
        var left = lhs as CircleFutureCollider;
        var right = rhs as CircleFutureCollider;
        var dist = Vector3d.Distance(
            left!.futureTransform.GetFuturePosition(step),
            right!.futureTransform.GetFuturePosition(step));
        var collisionDistance = left.radius + right.radius;
        return dist < collisionDistance;
    }
}
