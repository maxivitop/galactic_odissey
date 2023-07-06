using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformState: Evolving<TransformState>
{
    public Vector3 position;

    public TransformState(Vector3 position)
    {
        this.position = position;
    }

    public TransformState next()
    {
        return new TransformState(position);
    }
}
public class FutureTransform : FutureStateBehaviour<TransformState>, IFuturePositionProvider
{
    TransformState initial;
    void OnEnable()
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

    public Vector3 GetFuturePosition(int step, float Dt)
    {
        return GetState(step).position;
    }

    public int GetPriority()
    {
        return 0;
    }
}
