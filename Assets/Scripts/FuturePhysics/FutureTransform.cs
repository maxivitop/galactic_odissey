using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformState : IEvolving<TransformState>
{
    public Vector3d position;

    public TransformState(Vector3d position)
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

    private void OnEnable()
    {
        initial = new TransformState(new Vector3d(transform.position));
    }

    public override void Step(int step)
    {
        transform.position = GetState(step).position.ToVector3();
    }

    protected override TransformState GetInitialState()
    {
        return initial;
    }

    public Vector3d GetFuturePosition(int step, double dt)
    {
        return GetState(step).position;
    }

    public int GetPriority()
    {
        return 0;
    }
}