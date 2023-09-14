using System.Collections.Generic;
using UnityEngine;
using System;
// using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;

public class FuturePhysics
{
    public const int MaxSteps = 8000;
    public const float DeltaTime = 0.05f;
    public const float G = 1f;

    public static int currentStep;
    public static int lastVirtualStep;

    private static readonly List<IFutureObject> futureObjects = new();
    private static readonly List<IFutureObject> catchingUp = new();

    public static void Step()
    {
        FuturePhysicsRunner.CheckThread();
        currentStep++;
        foreach (var obj in GetAliveObjects(currentStep).ToArray())
            obj.Step(currentStep);
    }

    public static void CatchUpWithStep(int step)
    {
        var areAllCaughtUp = true;
        foreach (var obj in futureObjects) // first lightweight loop to check if all caught up
        {
            areAllCaughtUp &= obj.CatchUpWithVirtualStep(step);
        }
        if (areAllCaughtUp) return;
        catchingUp.Clear();
        for (var i = futureObjects.Count - 1; i >= 0; i--)
        {
            catchingUp.Add(futureObjects[i]);
        }
        // var times = new Dictionary<string, long>();
        // foreach (var obj in futureObjects)
        // {
        //     times[(obj as FutureBehaviour).myName] = 0;
        // }
        while (catchingUp.Count > 0)
        {
            for (var i = catchingUp.Count - 1; i >= 0; i--) // backwards loop allows to remove without breaking indexing
            {
                // var name = (catchingUp[i] as FutureBehaviour).myName;
                // var timer = Stopwatch.StartNew(); 
                if (catchingUp[i].CatchUpWithVirtualStep(step))
                    catchingUp.RemoveAt(i);
                // timer.Stop();
                // times[name] += timer.ElapsedTicks;
            }
        }
        //
        // Debug.Log(string.Join(
        //     "\n",
        //     times.OrderBy(kv => -kv.Value)
        //         .Select(kv => kv.Key + ": " + kv.Value)));
        
        lastVirtualStep = Mathf.Max(step, lastVirtualStep);
    }

    public static void AddObject(Type type, IFutureObject obj)
    {
        FuturePhysicsRunner.CheckThread();
        futureObjects.Add(obj);
    }

    public static void RemoveObject(IFutureObject obj)
    {
        FuturePhysicsRunner.CheckThread();
        futureObjects.Remove(obj);
    }

    public static void Reset(int step, GameObject whom)
    {
        FuturePhysicsRunner.CheckThread();
        if (step < currentStep)
        {
            Debug.LogError("reset on " + step + " with current step " + currentStep);
            return;
        }
        foreach (var state in futureObjects) state.ResetToStep(step, whom);
    }

    private static IEnumerable<IFutureObject> GetAliveObjects(int step)
    {
        return futureObjects.Where(obj => obj.IsAlive(step));
    }
}