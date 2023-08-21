using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformState : IEvolving<TransformState>
{
    public Vector3 position;

    public TransformState(Vector3 position)
    {
        this.position = position;
    }

    public TransformState Next()
    {
        return new TransformState(position);
    }
}

public class FutureTransform : FutureStateBehaviour<TransformState>, IFuturePositionProvider
{
    private TransformState initial;

    private void Awake()
    {
        initial = new TransformState(transform.position);
    }

    public override void Step(int step)
    {
        transform.position = GetState(step).position;
    }

    protected override TransformState GetInitialState()
    {
        return initial;
    }

    public Vector3 GetFuturePosition(int step, float dt=0)
    {
        return GetState(step).position;
    }
    
    public void SetFuturePosition(int step, Vector3 value)
    {
        GetState(step).position = value;
    }

    public int GetPriority()
    {
        return 0;
    }
}