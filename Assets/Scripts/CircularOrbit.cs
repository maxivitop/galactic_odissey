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
    private float radius;
    private float angleDelta;
    private float startAngle;

    private void Start()
    {
        centerPositionProvider = IFuturePositionProvider.SelectFuturePositionProvider(center);
        futureTransform = GetComponent<FutureTransform>();
        rigidBody = GetComponent<FutureRigidBody2D>();
        var radiusVector = center.transform.position - transform.position;
        radius = radiusVector.magnitude;
        radiusVector /= radius;
        startAngle = Mathf.Atan2(-radiusVector.y, -radiusVector.x);
        var centerRigidBody = center.GetComponent<FutureRigidBody2D>();
        var mu = FuturePhysics.G * (rigidBody.initialMass + centerRigidBody.initialMass);
        var period = 2 * Mathf.PI * Mathf.Sqrt(radius * radius * radius / mu);
        var directionMultiplier = clockWise ? 1f : -1f;
        angleDelta = directionMultiplier * 2 * Mathf.PI / period * FuturePhysics.DeltaTime;
    }

    public override void VirtualStep(int step)
    {
        var angle = startAngle + step * angleDelta;
        futureTransform.GetState(step).position = new Vector3(
            radius * Mathf.Cos(angle),
            radius * Mathf.Sin(angle)
        ) + centerPositionProvider.GetFuturePosition(step, 0);
    }

    public Vector3 GetFuturePosition(int step, float dt)
    {
        var angle = startAngle + (step + dt) * angleDelta;
        return new Vector3(
            radius * Mathf.Cos(angle),
            radius * Mathf.Sin(angle)
        ) + centerPositionProvider.GetFuturePosition(step, dt);
    }


    public int GetPriority()
    {
        return 1000;
    }
}