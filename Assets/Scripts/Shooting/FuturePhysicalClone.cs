using UnityEngine;

[RequireComponent(typeof(FutureRigidBody2D))]
[RequireComponent(typeof(FutureTransform))]
public class FuturePhysicalClone : FutureBehaviour
{
    public FutureRigidBody2D targetBody;
    private FutureTransform targetTransform;
    private FutureTransform futureTransform;
    private FutureRigidBody2D futureRigidBody2D;
    private const double Precision = 0.1f;
    private FutureBehaviour[] myFutureComponents;


    private void Start()
    {
        futureRigidBody2D = GetComponent<FutureRigidBody2D>();
        futureTransform = GetComponent<FutureTransform>();
        targetTransform = targetBody.GetComponent<FutureTransform>();
        myFutureComponents = GetComponents<FutureBehaviour>();
    }

    private void Update()
    {
        if (!targetTransform.IsAlive(FuturePhysics.currentStep))
        {
            Destroy(gameObject);
            return;
        }
        foreach (var myFutureComponent in myFutureComponents)
        {
            myFutureComponent.Disable(targetBody.disabledFromStep);
        }
        var step = FuturePhysics.currentStep;
        if (Vector3.SqrMagnitude(futureTransform.GetFuturePosition(step) -
                                  targetTransform.GetFuturePosition(step)) < Precision &&
            Vector2.SqrMagnitude(futureRigidBody2D.acceleration[step] -
                                  targetBody.acceleration[step]) < Precision &&
            Vector2.SqrMagnitude(futureRigidBody2D.velocity[step] -
                                  targetBody.velocity[step]) < Precision) return;
        FuturePhysics.Reset(step + 1, gameObject);
        CloneParams(step);
        CloneParams(step + 1);
    }

    protected override void VirtualStep(int step)
    {
        if (step > 0)
        {
            futureRigidBody2D.acceleration[step] =
                futureRigidBody2D.acceleration[step];
        }
    }

    private void CloneParams(int step)
    {
        futureTransform.SetFuturePosition(step, targetTransform.GetFuturePosition(step));
        futureRigidBody2D.acceleration[step] = targetBody.acceleration[step];
        futureRigidBody2D.velocity[step] = targetBody.velocity[step];
        futureRigidBody2D.orbit[step] = targetBody.orbit[step];
    }
}