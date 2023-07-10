using UnityEngine;

public interface IAccelerationProvider
{
    Vector2d CalculateAcceleration(int step, double dt, Vector3d pos);
}