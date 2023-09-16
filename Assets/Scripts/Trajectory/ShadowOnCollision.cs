using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ShadowCloneProvider))]
public class ShadowOnCollision : FutureBehaviour, ICollisionEnterHandler
{
    private ShadowCloneProvider shadowCloneProvider;
    private readonly Dictionary<FutureCollider, CollisionInfo> colliderToCollisionInfo = new();
    public Material collisionMaterial;
    private readonly HashSet<FutureCollider> pendingDestroys = new();
    private float destroyTimer = float.PositiveInfinity;

    private void Start()
    {
        shadowCloneProvider = GetComponent<ShadowCloneProvider>();
    }

    private void Update()
    {
        destroyTimer -= Time.unscaledDeltaTime;
        if (destroyTimer <= 0f)
        {
            destroyTimer = float.PositiveInfinity;
            foreach (var coll in pendingDestroys)
            {
                DestroyShadowClone(coll);
            }
            pendingDestroys.Clear();
        }
    }

    public override void ResetToStep(int step, GameObject cause)
    {
        foreach (var kv in colliderToCollisionInfo)
        {
            if (step > kv.Value.collisionStep || kv.Key.gameObject != cause)
                continue;
            ReleaseShadowClone(kv.Key);
        }
    }

    public void VirtualStepCollisionEnter(int step, FutureCollision collision)
    {
        FuturePhysicsRunner.ExecuteOnUpdate(() =>
        {
            if (collision.other == null ||
                !collision.other.TryGetComponent<RequiresShadowOnCollision>(out _))
            {
                return;
            }

            var collisionInfo = new CollisionInfo(ObtainShadowClone(collision.other), step);
            collisionInfo.shadowClone.SetStep(step);
            collisionInfo.shadowClone.meshRenderer.material = collisionMaterial;
            collisionInfo.shadowClone.Activate();
            colliderToCollisionInfo[collision.other] = collisionInfo;
        });
    }

    public void StepCollisionEnter(int step, FutureCollision collision)
    {
        DestroyShadowClone(collision.other);
    }

    private void OnDestroy()
    {
        foreach (var collider in colliderToCollisionInfo.Keys.ToArray())
            // ToArray protects from concurrent modification
        {
            DestroyShadowClone(collider);
        }
    }
    
    private ShadowClone ObtainShadowClone(FutureCollider futureCollider)
    {
        if (pendingDestroys.Contains(futureCollider))
        {
            var result = colliderToCollisionInfo[futureCollider].shadowClone;
            pendingDestroys.Remove(futureCollider);
            colliderToCollisionInfo.Remove(futureCollider);
            return result;
        }
        return shadowCloneProvider.CreateShadowClone();
    }

    private void ReleaseShadowClone(FutureCollider futureCollider)
    {
        destroyTimer = 0.1f;
        pendingDestroys.Add(futureCollider);
    }
    
    private void DestroyShadowClone(FutureCollider futureCollider)
    {
        if (!colliderToCollisionInfo.ContainsKey(futureCollider))
        {
            return;
        }

        var collisionInfo = colliderToCollisionInfo[futureCollider];
        colliderToCollisionInfo.Remove(futureCollider);
        if (collisionInfo != null && collisionInfo.shadowClone != null)
        {
            Destroy(collisionInfo.shadowClone.gameObject);
        }
    }


    private class CollisionInfo
    {
        public readonly ShadowClone shadowClone;
        public readonly int collisionStep;

        public CollisionInfo(ShadowClone shadowClone, int collisionStep)
        {
            this.shadowClone = shadowClone;
            this.collisionStep = collisionStep;
        }

        protected bool Equals(CollisionInfo other)
        {
            return Equals(shadowClone, other.shadowClone) && collisionStep == other.collisionStep;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CollisionInfo)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(shadowClone, collisionStep);
        }

        public static bool operator ==(CollisionInfo left, CollisionInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CollisionInfo left, CollisionInfo right)
        {
            return !Equals(left, right);
        }
    }
}