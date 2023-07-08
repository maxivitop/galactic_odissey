using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FutureTransform))]
[RequireComponent(typeof(FutureRigidBody2D))]
public class GravitySource : FutureBehaviour
{
    public FutureRigidBody2D futureRigidBody2D;
    public IFuturePositionProvider futurePositionProvider;

    private void Start()
    {
        futureRigidBody2D = GetComponent<FutureRigidBody2D>();
        futurePositionProvider = IFuturePositionProvider.SelectFuturePositionProvider(gameObject);
    }
}