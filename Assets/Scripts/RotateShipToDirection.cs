using System;
using UnityEngine;

[RequireComponent(typeof(FutureRigidBody2D))]
[RequireComponent(typeof(TrajectoryProvider))]
public class RotateShipToDirection : MonoBehaviour
{
    public float speed = 10;
    private FutureRigidBody2D futureRigidBody2D;
    private TrajectoryProvider trajectoryProvider;


    private void Start()
    {
        trajectoryProvider = GetComponent<TrajectoryProvider>();
        futureRigidBody2D = GetComponent<FutureRigidBody2D>();
    }

    private void Update()
    {
        var acceleration = futureRigidBody2D.GetState(FuturePhysics.currentStep).acceleration;
        var direction = Vector3.up;
        if (acceleration.sqrMagnitude > Mathf.Epsilon)
        {
            direction = acceleration.ToVector2();
        }
        else
        {
            var trajStep =
                TrajectoryProvider.PhysicsStepToTrajectoryStep(FuturePhysics.currentStep);
            if (trajectoryProvider.trajectory.size > trajStep + 1)
            {
                direction = trajectoryProvider.trajectory.array[trajStep + 1]
                            - trajectoryProvider.trajectory.array[trajStep];
            }
        }
        transform.rotation = Quaternion.Lerp( // not perfect, but does the job
            transform.rotation,
            Quaternion.LookRotation(Vector3.forward, direction),
            Time.deltaTime * speed * FuturePhysicsRunner.timeScale
        );
    }
}