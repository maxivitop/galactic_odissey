using System;
using UnityEngine;

public class FutureTransform : FutureBehaviour, IFuturePositionProvider
{
    public readonly FutureArray<Vector3> position = new();

    private void Awake()
    {
        var pos = transform.position;
        position.Initialize(startStep, pos, ToString());
    }

    private void Update()
    {
        if (disabledFromStep <= FuturePhysicsRunner.renderFrameNextStep || FuturePhysicsRunner.renderFramePrevStep < startStep)
        {
            return;
        }
        for(int i = position.capacityArray.size; i< FuturePhysicsRunner.renderFrameNextStep; i++)
            GetFuturePosition(i);
        transform.position = GetFuturePosition(
            FuturePhysicsRunner.renderFramePrevStep,
            FuturePhysicsRunner.renderFrameStepPart);
    }
    
    public Vector3 GetFuturePosition(int step, float dt=0)
    {
        if (dt == 0)
        {
            return position[step];
        }
        if (step < startStep)
        {
            return position[startStep];
        }     
        var posPrevStep= position[step];
        if (step >= disabledFromStep)
        {
            return position[disabledFromStep - 1];
        }
        var posNextStep = position[step+1];
        return posPrevStep + (posNextStep - posPrevStep) * dt;
    }
    
    public void SetFuturePosition(int step, Vector3 value)
    {
        position[step] = value;
    }

    public int GetPriority()
    {
        return 0;
    }
    
    public override void ResetToStep(int step, GameObject cause)
    {
        base.ResetToStep(step, cause);
        if (cause != gameObject) return;
        position.ResetToStep(step);
    }
}