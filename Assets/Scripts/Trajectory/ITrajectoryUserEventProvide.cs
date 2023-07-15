using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITrajectoryUserEventProvider
{
    bool IsEnabled(int step);
    GameObject CreateUI(int step, TrajectoryMarker marker);
    
    void Destroy(TrajectoryMarker marker);
}