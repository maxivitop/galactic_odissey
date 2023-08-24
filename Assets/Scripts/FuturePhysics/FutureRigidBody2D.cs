using UnityEngine;



[RequireComponent(typeof(FutureTransform))]
public class FutureRigidBody2D : FutureBehaviour
{
    [SerializeField] public float initialMass;
    [SerializeField] private Vector2 initialVelocity;

    public Vector2 InitialVelocity
    {
        get => initialVelocity;
        set
        {
            initialVelocity = value;
            velocity.Initialize(startStep, initialVelocity, myName);
        }
    }

    public readonly FutureArray<Vector2> velocity = new();
    public readonly FutureArray<float> mass = new();
    public readonly FutureArray<Vector2> acceleration = new();
    public readonly FutureArray<EllipticalOrbit> orbit = new();

    private void Awake()
    {
        myName = ToString();
        velocity.Initialize(startStep, initialVelocity, myName);
        mass.Initialize(startStep, initialMass, myName);
        acceleration.Initialize(startStep, Vector2.zero, myName);
        orbit.Initialize(startStep, null, myName);
    }
    
    protected override void VirtualStep(int step)
    {
        acceleration[step] = Vector2.zero;
    }

    public void AddForce(int step, Vector2 force)
    {
        acceleration[step] += force / mass[step];
    }
    
    public override void ResetToStep(int step, GameObject cause)
    {
        base.ResetToStep(step, cause);
        if (cause != gameObject) return;
        velocity.ResetToStep(step);
        mass.ResetToStep(step);
        acceleration.ResetToStep(step);
        orbit.ResetToStep(step);
    }
}