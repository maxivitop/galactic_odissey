using UnityEngine;

public interface IAccelerationProvider
{
    Vector2 CalculateAcceleration(int step, Vector3 pos);
}