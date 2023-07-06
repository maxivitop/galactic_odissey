using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Linq;

public class FuturePhysics
{
    public const int MaxSteps = 20000;
    public const float DeltaTime = 0.02f;
    public static int currentStep;
    public static volatile int lastVirtualStep;
    public const float G = 1f;
    public static readonly UnityEvent<ResetParams> beforeReset = new();

    private static readonly List<IFutureState> futureStates = new();
    private static readonly Dictionary<Type, List<IFutureState>> typeToFutureStates = new();
    private static readonly object @lock = new();
    private static readonly ResetParams lastResetParams = new();

    public static void Step()
    {
        currentStep++;
        foreach (var state in futureStates) state.Step(currentStep);
    }

    public static void VirtualStep()
    {
        lock (@lock)
        {
            foreach (var state in futureStates) state.VirtualStep(lastVirtualStep);

            // ReSharper disable once NonAtomicCompoundOperator
            lastVirtualStep++;
        }
    }

    public static void AddObject(Type type, IFutureState obj)
    {
        lock (@lock)
        {
            futureStates.Add(obj);
            if (!typeToFutureStates.ContainsKey(type))
                typeToFutureStates[type] = new List<IFutureState>();

            typeToFutureStates[type].Add(obj);
        }
    }

    public static void RemoveObject(Type type, IFutureState obj)
    {
        lock (@lock)
        {
            futureStates.Remove(obj);
            if (typeToFutureStates.TryGetValue(type, out var states)) states.Remove(obj);
        }
    }

    public static IEnumerable<T> GetComponents<T>(int step) where T : class
    {
        return typeToFutureStates[typeof(T)].Cast<T>();
    }

    public static void Reset(int step, GameObject cause)
    {
        if (step < currentStep)
        {
            Debug.LogError("reset on " + step + " with current step " + currentStep);
            return;
        }

        if (step == lastVirtualStep) return;

        lastResetParams.step = step;
        lastResetParams.cause = cause;
        beforeReset.Invoke(lastResetParams);
        lock (@lock)
        {
            lastVirtualStep = step;
            foreach (var state in futureStates) state.Reset(step);
        }
    }

    public class ResetParams
    {
        public GameObject cause;
        public int step;
    }
}