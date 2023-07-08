using UnityEngine;

public interface IAccelerationProvider
{
    Vector2 CalculateAcceleration(int step, float dt, Vector3 pos);
}