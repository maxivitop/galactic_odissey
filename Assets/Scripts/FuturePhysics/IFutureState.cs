using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFutureState
{
    public void Step(int step);
    public void VirtualStep(int step);
    public void ResetToStep(int step, GameObject cause);
}