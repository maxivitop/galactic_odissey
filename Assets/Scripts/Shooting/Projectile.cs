using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleFutureCollider))]
[RequireComponent(typeof(FutureRigidBody2D))]
[RequireComponent(typeof(FutureTransform))]
public class Projectile: MonoBehaviour
{
    [NonSerialized] public FutureTransform futureTransform;
    private int lastStep;
    
    private void Awake()
    {
        futureTransform = GetComponent<FutureTransform>();
    }

    public void Launch(int step, Vector3[] trajectory, int trajectoryLength)
    {
        var lostSteps = FuturePhysics.currentStep - step;
        if (lostSteps > trajectoryLength)
        {
            Debug.LogWarning("Dispatched Launch too late lostSteps=" + lostSteps 
                +", trajectoryLength="+trajectoryLength
                +" step="+step 
                +" currentStep="+FuturePhysics.currentStep);
            DestroySelf();
            return;
        }
        lastStep = step + trajectoryLength;
        futureTransform.position.Initialize(step, trajectory, trajectoryLength, ToString());    
        foreach (var futureBehaviour in GetComponents<MonoBehaviour>())
        {
            futureBehaviour.enabled = true;
        }
        foreach (var futureBehaviour in GetComponents<FutureBehaviour>())
        {
            futureBehaviour.Disable(lastStep);
        }
    }

    private void Update()
    {
        if (FuturePhysics.currentStep > lastStep)
        {
            DestroySelf();
        }
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}
