using UnityEngine;

public class PlanetRotator: MonoBehaviour
{
    public Vector3 up = Vector3.back;
    public float speed;
    private float rotation;
    public bool rotateIndependently = true;

    private void Update()
    {
        if (!rotateIndependently) return;
        SetStep(FuturePhysicsRunner.renderFrame);
    }

    public void SetStep(float step)
    {
        rotation = speed * step / FuturePhysicsRunner.StepsPerSecond;
        transform.rotation = Quaternion.AngleAxis(rotation, up);
    }

    public void CopyInto(PlanetRotator other)
    {
        other.up = up;
        other.speed = speed;
        other.rotateIndependently = rotateIndependently;
    }
}