using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Linq;
using UnityEngine.Assertions;

public class FuturePhysics
{
    public const int MaxSteps = 20000;
    public const double DeltaTime = 0.02f;
    public const double G = 1f;
    
    public static int currentStep;
    public static int lastVirtualStep;
    public static readonly Event<ResetParams> beforeReset = new();

    private static readonly List<IFutureState> futureStates = new();
    private static readonly Dictionary<Type, List<IFutureState>> typeToFutureStates = new();
    private static readonly ResetParams lastResetParams = new();

    public static void Step()
    {
        FuturePhysicsRunner.CheckThread();
        currentStep++;
        while (currentStep >  lastVirtualStep)
        {
            VirtualStep();
        }
        foreach (var state in futureStates) state.Step(currentStep);
    }

    public static void VirtualStep()
    {
        FuturePhysicsRunner.CheckThread();
        foreach (var state in futureStates) state.VirtualStep(lastVirtualStep);
        
        lastVirtualStep++;
    }

    public static void AddObject(Type type, IFutureState obj)
    {
        FuturePhysicsRunner.CheckThread();

        futureStates.Add(obj);
        if (!typeToFutureStates.ContainsKey(type))
            typeToFutureStates[type] = new List<IFutureState>();

        typeToFutureStates[type].Add(obj);
    }

    public static void RemoveObject(Type type, IFutureState obj)
    {
        FuturePhysicsRunner.CheckThread();
        futureStates.Remove(obj);
        if (typeToFutureStates.TryGetValue(type, out var states)) states.Remove(obj);
    }

    public static IEnumerable<T> GetComponents<T>(int step) where T : class
    {
        return typeToFutureStates[typeof(T)].Cast<T>();
    }

    public static void Reset(int step, GameObject cause)
    {
        FuturePhysicsRunner.CheckThread();
        if (step < currentStep)
        {
            Debug.LogError("reset on " + step + " with current step " + currentStep);
            return;
        }

        if (step == lastVirtualStep) return;

        lastResetParams.step = step;
        lastResetParams.cause = cause;
        beforeReset.Invoke(lastResetParams);
        lastVirtualStep = step; 
        foreach (var state in futureStates) state.ResetToStep(step, cause);
    }

    public class ResetParams
    {
        public GameObject cause;
        public int step;
    }
}