using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFutureObject
{
    public void Step(int step);
    public void VirtualStep(int step);
    public void ResetToStep(int step, GameObject cause);

    public bool IsAlive(int step);
    
    public bool IsObsolete(int step);
}