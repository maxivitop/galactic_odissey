using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrusterMarker : MonoBehaviour
{
    public void SetConfig(Thruster.Config config, Thruster thruster)
    {
        transform.localScale = Vector3.one * config.thrust;
        transform.rotation = Quaternion.LookRotation(config.direction);
    }
}