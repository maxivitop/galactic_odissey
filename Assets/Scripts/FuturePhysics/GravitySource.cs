using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TrajectoryProvider))]
[RequireComponent(typeof(CircleFutureCollider))]
[RequireComponent(typeof(FutureTransform))]
[RequireComponent(typeof(FutureRigidBody2D))]
public class GravitySource : FutureBehaviour
{
    [NonSerialized] public FutureRigidBody2D futureRigidBody2D;
    [NonSerialized] public IFuturePositionProvider futurePositionProvider;
    [NonSerialized] public FutureTransform futureTransform;
    [NonSerialized] public CircleFutureCollider futureCollider;

    protected void Awake()
    {
        futureCollider = GetComponent<CircleFutureCollider>();
        futureRigidBody2D = GetComponent<FutureRigidBody2D>();
        futurePositionProvider = IFuturePositionProvider.SelectFuturePositionProvider(gameObject);
        futureTransform = GetComponent<FutureTransform>();
    }

    public double CalculateDominatingGravityDistance(int step)
    {
        var myPosition = futurePositionProvider.GetFuturePosition(step);
        var center = OrbitUtils.FindBiggestGravitySource(myPosition, step);
        // G*m1*m / d1^2 = G*m2*m / d2^2; d1+d2=d;
        // m1/d1^2 = m2/d2^2
        // m1*d2^2 = m2*d1^2
        // m1*(d-d1)^2 = m2*d1^2
        // m1*(d^2 - 2*d*d1 + d1^2) = m2*d1^2
        // (m1-m2)*d1^2 - 2*m1*d*d1 + m1*d^2 = 0
        // d1 is my dominatingGravityDistance
        // m1 is my mass
        // m2 is other mass
        var m1 = futureRigidBody2D.mass[step];
        var m2 = center.futureRigidBody2D.mass[step];
        var d = (center.futurePositionProvider.GetFuturePosition(step) - myPosition).magnitude;
        var a = m1 - m2;
        var b = 2 * m1 * d;
        var c = m1 * d * d;
        var D = b * b - 4 * a * c;
        return (-b + Math.Sqrt(D)) / 2 / a;
    }
}