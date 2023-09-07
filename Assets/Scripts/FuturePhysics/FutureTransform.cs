using UnityEngine;

public class FutureTransform : FutureBehaviour, IFuturePositionProvider
{
    public readonly FutureArray<Vector3> position = new();

    private void Awake()
    {
        var pos = transform.position;
        position.Initialize(startStep, pos, ToString());
    }

    public override void Step(int step)
    {
        transform.position = position[step];
    }

    public Vector3 GetFuturePosition(int step, float dt=0)
    {
        return position[step];
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