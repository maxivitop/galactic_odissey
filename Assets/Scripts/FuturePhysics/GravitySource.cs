using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FutureTransform))]
[RequireComponent(typeof(FutureRigidBody2D))]
public class GravitySource : FutureBehaviour
{
    public FutureTransform futureTransform;
    public FutureRigidBody2D futureRigidBody2D;

    private void Start()
    {
        futureTransform = GetComponent<FutureTransform>();
        futureRigidBody2D = GetComponent<FutureRigidBody2D>();
    }
}