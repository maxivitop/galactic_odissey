using System;
using System.Threading;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FuturePhysicsRunner : MonoBehaviour
{
    public static int timeScale = 0;
    private bool isThreadStarted = false;
    AutoResetEvent waitHandle = new AutoResetEvent(false);

    void FixedUpdate()
    {
        if (!isThreadStarted)
        {
            Thread virtualRunner = new Thread(VirtualStepRunner);
            virtualRunner.Start();
            isThreadStarted = true;
        }
        for (int i = 0; i < timeScale; i++)
        {
            FuturePhysics.Step();
        }
        waitHandle.Set();
    }

    void VirtualStepRunner()
    {
        while (true)
        {
            while (FuturePhysics.lastVirtualStep - FuturePhysics.currentStep < FuturePhysics.maxSteps)
            {
                FuturePhysics.VirtualStep();
            }
            waitHandle.WaitOne();
        }
       
    }
}
