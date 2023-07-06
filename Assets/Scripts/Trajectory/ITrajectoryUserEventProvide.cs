using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITrajectoryUserEventProvider
{
    bool isEnabled(int step);
    GameObject CreateUI(int step, TrajectoryMarker marker);
}
