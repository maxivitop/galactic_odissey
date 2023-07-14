using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnCollision : MonoBehaviour, ICollisionEnterHandler
{
    
    public void VirtualStepCollisionEnter(int step, FutureCollision collision)
    {
        Debug.Log("VirtualStepCollisionEnter");
    }

    public void StepCollisionEnter(int step, FutureCollision collision)
    {
        Debug.Log("StepCollisionEnter " + gameObject.name + " " + collision.other.gameObject.name);
    }
}
