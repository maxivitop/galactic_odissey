using System;
using System.Collections.Generic;
using UnityEngine;

public enum CollisionLayer
{
    MyShip,
    EnemyShip,
    MyProjectile,
    EnemyProjectile,
    CelestialBody,
}

[RequireComponent(typeof(FutureTransform))]
public class FutureCollider : FutureBehaviour
{
    public static readonly Dictionary<CollisionLayer, HashSet<FutureCollider>> layerToColliders
        = new()
        {
            { CollisionLayer.MyShip, new HashSet<FutureCollider>() },
            { CollisionLayer.EnemyShip, new HashSet<FutureCollider>() },
            { CollisionLayer.MyProjectile, new HashSet<FutureCollider>() },
            { CollisionLayer.EnemyProjectile, new HashSet<FutureCollider>() },
            { CollisionLayer.CelestialBody, new HashSet<FutureCollider>() }
        };

    public static readonly Dictionary<CollisionLayer, HashSet<CollisionLayer>> collisionTable =
        new()
        {
            {
                CollisionLayer.MyShip, new HashSet<CollisionLayer>
                {
                    CollisionLayer.CelestialBody
                }
            },
            {
                CollisionLayer.EnemyShip, new HashSet<CollisionLayer>
                {
                    CollisionLayer.CelestialBody
                }
            },
            {
                CollisionLayer.MyProjectile, new HashSet<CollisionLayer>
                {
                    CollisionLayer.CelestialBody,
                    CollisionLayer.EnemyShip,
                }
            },
            {
                CollisionLayer.EnemyProjectile, new HashSet<CollisionLayer>
                { 
                    CollisionLayer.MyShip,
                    CollisionLayer.CelestialBody,
                }
            },
            {
                CollisionLayer.CelestialBody, new HashSet<CollisionLayer>()
            }
        };

    public CollisionLayer layer;
    [NonSerialized] public FutureTransform futureTransform;
    [NonSerialized] public bool isPassive = true;
    public bool registerSelf = true;

    private ICollisionEnterHandler[] collisionEnterHandlers;

    protected override void Register()
    {
        base.Register();
        collisionEnterHandlers = GetComponents<ICollisionEnterHandler>();
        futureTransform = GetComponent<FutureTransform>();
        if (registerSelf)
        { 
            layerToColliders[layer].Add(this);
        }
    }

    protected override void Unregister()
    {
        if (registerSelf)
        {
            layerToColliders[layer].Remove(this);
        }
        base.Unregister();
    }

    public void StepCollisionEnter(int step, FutureCollision collision)
    {
        foreach (var collisionEnterHandler in collisionEnterHandlers)
        {
            collisionEnterHandler.StepCollisionEnter(step, collision);
        }
    }

    public void VirtualStepCollisionEnter(int step, FutureCollision collision)
    {
        foreach (var collisionEnterHandler in collisionEnterHandlers)
        {
            collisionEnterHandler.VirtualStepCollisionEnter(step, collision);
        }
    }
}