using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FutureRigidBody2D))]
public class CircularOrbit : FutureBehaviour, IFuturePositionProvider
{
    public GameObject center;
    private IFuturePositionProvider centerPositionProvider;
    public bool clockWise = true;
    private FutureRigidBody2D rigidBody;
    private FutureTransform futureTransform;
    private double radius;
    private double angleDelta;
    private double startAngle;

    private void Start()
    {
        centerPositionProvider = IFuturePositionProvider.SelectFuturePositionProvider(center);
        futureTransform = GetComponent<FutureTransform>();
        rigidBody = GetComponent<FutureRigidBody2D>();
        var radiusVector = new Vector3d(center.transform.position - transform.position);
        radius = radiusVector.magnitude;
        radiusVector /= radius;
        startAngle = Math.Atan2(-radiusVector.y, -radiusVector.x);
        var centerRigidBody = center.GetComponent<FutureRigidBody2D>();
        var mu = FuturePhysics.G * (rigidBody.initialMass + centerRigidBody.initialMass);
        var period = 2 * Mathf.PI * Math.Sqrt(radius * radius * radius / mu);
        var directionMultiplier = clockWise ? 1f : -1f;
        angleDelta = directionMultiplier * 2 * Math.PI / period * FuturePhysics.DeltaTime;
    }

    public override void VirtualStep(int step)
    {
        futureTransform.GetState(step).position = GetFuturePosition(step, 0f);
    }

    public Vector3d GetFuturePosition(int step, double dt)
    {
        var angle = startAngle + (step + dt) * angleDelta;
        return new Vector3d(
            radius * Math.Cos(angle),
            radius * Math.Sin(angle)
        ) + centerPositionProvider.GetFuturePosition(step, dt);
    }


    public int GetPriority()
    {
        return 1000;
    }
}