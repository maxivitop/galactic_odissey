using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFuturePositionProvider 
{
    Vector3 GetFuturePosition(int step, float Dt);
    int GetPriority();
}
