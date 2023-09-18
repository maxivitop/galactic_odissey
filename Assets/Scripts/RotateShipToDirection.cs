using System;
using UnityEngine;

[RequireComponent(typeof(FutureRigidBody2D))]
[RequireComponent(typeof(TrajectoryProvider))]
public class RotateShipToDirection : MonoBehaviour
{
    public float rotationTimeSeconds = 1;
    public Transform transformToRotate;
    private FutureTransform futureTransform;
    private FutureRigidBody2D futureRigidBody2D;
    private TrajectoryProvider trajectoryProvider;


    private void Start()
    {
        futureTransform = GetComponent<FutureTransform>();
        trajectoryProvider = GetComponent<TrajectoryProvider>();
        futureRigidBody2D = GetComponent<FutureRigidBody2D>();
    }

    private void Update()
    {
        var acceleration = Vector2.zero;
        var lookaheadSteps = Mathf.Min(
            futureRigidBody2D.acceleration.capacityArray.size -  FuturePhysics.currentStep,
            Mathf.CeilToInt(FuturePhysicsRunner.StepsPerSecond / rotationTimeSeconds)
        );
        for (var i = FuturePhysics.currentStep; i < FuturePhysics.currentStep + lookaheadSteps; i++)
        {
            if (futureRigidBody2D.acceleration[i] != Vector2.zero)
            {
                acceleration = futureRigidBody2D.acceleration[i];
                lookaheadSteps = i - FuturePhysics.currentStep;
                break;
            }
        }
        Quaternion targetRotation;
        float anglePerSecond;
        if (acceleration != Vector2.zero)
        {
            var direction = acceleration;
            targetRotation = Quaternion.LookRotation(Vector3.forward, direction);
            if (lookaheadSteps == 0)
            {
                transformToRotate.rotation = targetRotation;
                return;
            }
            anglePerSecond = Quaternion.Angle(transformToRotate.rotation, targetRotation) 
                * FuturePhysicsRunner.StepsPerSecond
                / lookaheadSteps;
        }
        else
        {
            var closestGravity = OrbitUtils.FindBiggestGravitySource(transform.position, FuturePhysics.currentStep);
            var relativePosNext = futureTransform.position[FuturePhysics.currentStep + 1]
                              - closestGravity.futureTransform.position[FuturePhysics.currentStep + 1];
            var relativePos = futureTransform.position[FuturePhysics.currentStep]
                                  - closestGravity.futureTransform.position[FuturePhysics.currentStep];
            
            var direction = relativePosNext - relativePos;
            targetRotation = Quaternion.LookRotation(Vector3.forward, direction);

            anglePerSecond = 360 / rotationTimeSeconds;
        }
        
        transformToRotate.rotation = Quaternion.RotateTowards(
            transformToRotate.rotation,
            targetRotation,
            Time.deltaTime * anglePerSecond
        );
    }
}