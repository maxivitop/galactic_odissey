using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFutureState
{
    public void Step(int step);

    public void VirtualStep(int step);
    public void Reset(int step);
}
