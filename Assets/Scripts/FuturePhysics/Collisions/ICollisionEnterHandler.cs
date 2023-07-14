using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollisionEnterHandler
{
    void VirtualStepCollisionEnter(int step, FutureCollision collision);
    
    void StepCollisionEnter(int step, FutureCollision collision);
}
