using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Linq;
public class FuturePhysics
{
    public static int maxSteps = 20000;
    public static float deltaTime = 0.02f;
    public static int currentStep;
    public static volatile int lastVirtualStep;
    public static float G = 1f;
    public static UnityEvent<ResetParams> beforeReset = new();

    private static List<IFutureState> futureStates = new();
    private static Dictionary<Type, List<IFutureState>> TypeToFutureStates = new();
    private static object LOCK = new object();
    private static ResetParams lastResetParams = new ResetParams();

    public static void Step()
    {
        currentStep++;
        foreach(var state in futureStates)
        {
            state.Step(currentStep);
        }
    }

    public static void VirtualStep()
    {
        lock(LOCK)
        {
            foreach (var state in futureStates)
            {
                state.VirtualStep(lastVirtualStep);
            }
            lastVirtualStep++;
        }
    }

    public static void AddObject(Type type, IFutureState obj)
    {
        lock (LOCK)
        {
            futureStates.Add(obj);
            if (!TypeToFutureStates.ContainsKey(type))
            {
                TypeToFutureStates[type] = new();
            }
            TypeToFutureStates[type].Add(obj);
        }
    }
    public static void RemoveObject(Type type, IFutureState obj)
    {
        lock (LOCK)
        {
            futureStates.Remove(obj);
            if(TypeToFutureStates.ContainsKey(type))
            {
                TypeToFutureStates[type].Remove(obj);
            }
        }
    }
    public static IEnumerable<T> GetComponents<T>(int step) where T: class
    {
        return TypeToFutureStates[typeof(T)].Cast<T>();
    }
    public static void Reset(int step, GameObject cause)
    {
        if (step < currentStep)
        {
            Debug.LogError("reset on " + step + " with current step " + currentStep);
            return;
        }
        if (step == lastVirtualStep)
        {
            return;
        }
        lastResetParams.step = step;
        lastResetParams.cause = cause;
        beforeReset.Invoke(lastResetParams);
        lock (LOCK)
        {
            lastVirtualStep = step;
            foreach (var state in futureStates)
            {
                state.Reset(step);
            }
        }
    }

    public class ResetParams
    {
        public GameObject cause;
        public int step;
    }
}
