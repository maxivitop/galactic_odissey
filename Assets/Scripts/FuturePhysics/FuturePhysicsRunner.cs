using System;
using System.Threading;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FuturePhysicsRunner : MonoBehaviour
{
    public static int timeScale = 0;
    private bool isThreadStarted;
    private readonly AutoResetEvent waitHandle = new(false);

    private void FixedUpdate()
    {
        if (!isThreadStarted)
        {
            var virtualRunner = new Thread(VirtualStepRunner);
            virtualRunner.Start();
            isThreadStarted = true;
        }

        for (var i = 0; i < timeScale; i++) FuturePhysics.Step();
        waitHandle.Set();
    }

    private void VirtualStepRunner()
    {
        while (true)
        {
            while (FuturePhysics.lastVirtualStep - FuturePhysics.currentStep <
                   FuturePhysics.MaxSteps) FuturePhysics.VirtualStep();
            waitHandle.WaitOne();
        }
    }
}