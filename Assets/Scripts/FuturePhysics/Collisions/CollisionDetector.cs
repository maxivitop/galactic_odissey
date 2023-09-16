using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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

public class CollisionDetector : FutureBehaviour
{
    public enum Mode
    {
        Virtual,
        JustInTime,
    }
    
    public enum Type
    {
        Discrete,
        Continuous,
    }

    public Mode mode;
    public Type type;

    private CollisionLayer myLayer;
    private HashSet<CollisionLayer> collidesWithLayers;
    private readonly HashSet<FutureCollider> myColliders = new();
    public readonly FutureArray<HashSet<FutureCollision>> collisions = new();
    private CircleFutureCollider worldBoundary;
    private readonly HashSet<FutureCollision> emptyHashset = new();

    private void Awake()
    {
        worldBoundary = GameObject.FindWithTag("WorldBoundary").GetComponent<CircleFutureCollider>();
        hasVirtualStep = mode == Mode.Virtual;
        if (mode == Mode.Virtual)
        {
            collisions.Initialize(startStep, emptyHashset, ToString());
        }

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
        if (mode == Mode.JustInTime)
        {
            if (step >= disabledFromStep)
            {
                return; // in just in time mode disabling from step should work as expected.
            }
            if (collisions.capacityArray.size < step)
            {
                collisions.Initialize(step, new HashSet<FutureCollision>(), ToString());
            }

            DetectVirtualStepCollisions(step);
        }

        var prevStepCollisions = collisions[step - 1]
                                 ?? new HashSet<FutureCollision>();
        foreach (var collision in collisions[step])
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
        if (mode == Mode.Virtual)
        {
            DetectVirtualStepCollisions(step);
        }
    }

    private void DetectVirtualStepCollisions(int step)
    {
        var prevStepCollisions = collisions[step];
        collisions[step] = new HashSet<FutureCollision>();
        foreach (var myCollider in myColliders)
        {
            if (!myCollider.IsAlive(step)) continue;
            var position = myCollider.futureTransform.position[step];
            if (!float.IsNormal(position.x) || position.sqrMagnitude > worldBoundary.radius*worldBoundary.radius)
            {
                AddCollision(new FutureCollision(myCollider, worldBoundary), step-1, prevStepCollisions);
            }
        }
        foreach (var layer in collidesWithLayers)
        {
            var colliders = FutureCollider.layerToColliders[layer];
            foreach (var otherCollider in colliders)
            {
                if (!otherCollider.IsAlive(step)) continue;
                if (myColliders.Contains(otherCollider)) continue;
                foreach (var myCollider in myColliders)
                {
                    if (!myCollider.IsAlive(step)) continue;
                    if (!CheckCollision(step, myCollider, otherCollider)) continue;
                    AddCollision(new FutureCollision(myCollider, otherCollider), step, prevStepCollisions);
                }
            }
        }
    }

    private void AddCollision(FutureCollision collision, int step, ISet<FutureCollision> prevStepCollisions)
    {
        if (!prevStepCollisions.Contains(collision))
        {
            collision.my.VirtualStepCollisionEnter(step, collision);
            if (collision.other.isPassive)
            {
                var otherCollision = new FutureCollision(collision.other, collision.my);
                collision.other.VirtualStepCollisionEnter(step, otherCollision);
            }
            collisions[step].Add(collision);
        } // OnCollisionStay goes in else if needed 
    } 

    private bool CheckCollision(int step, FutureCollider lhs, FutureCollider rhs)
    {
        var left = lhs as CircleFutureCollider;
        var right = rhs as CircleFutureCollider;
        var lhsPos = left!.futureTransform.GetFuturePosition(step);
        var rhsPos = right!.futureTransform.GetFuturePosition(step);
        
        var r = left.radius + right.radius;

        if (type == Type.Discrete)
        {
            var dist = (lhsPos - rhsPos).sqrMagnitude;
            return dist < r*r;
        }
        var lhsNextPos = left!.futureTransform.GetFuturePosition(step, 1f);
        var rhsNextPos = right!.futureTransform.GetFuturePosition(step, 1f);
        
        var direction = lhsNextPos - lhsPos - (rhsNextPos - rhsPos);
        var centerOrigin = rhsPos - lhsPos;
        
        var magnitudeMax = direction.magnitude;
        direction /= magnitudeMax; // normalize
        //Do projection from the point but clamp it
        var dotP = Vector3.Dot(centerOrigin, direction);
        dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
        return (direction * dotP - centerOrigin).sqrMagnitude < r*r;
    }

    public override void ResetToStep(int step, GameObject cause)
    {
        base.ResetToStep(step, cause);
        if (cause != gameObject) return;
        collisions.ResetToStep(step);
        emptyHashset.Clear();
    }
}