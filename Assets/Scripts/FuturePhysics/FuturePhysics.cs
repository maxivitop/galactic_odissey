using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class FuturePhysics
{
    public const int MaxSteps = 10000;
    public const double DeltaTime = 0.06f;
    public const double G = 1f;

    public static int currentStep;
    public static int lastVirtualStep;
    public static readonly SingleEvent<ResetParams> beforeReset = new();
    public static readonly SingleEvent<int> afterVirtualStep = new();

    private static readonly HashSet<IFutureObject> futureObjects = new();
    private static readonly Dictionary<IFutureObject, ISet<Type>> objToTypes = new();
    private static readonly Dictionary<Type, ISet<IFutureObject>> typeToObjs = new();
    private static readonly ResetParams lastResetParams = new();
    private static readonly List<IFutureObject> obsoleteObjs = new();

    public static void Step()
    {
        FuturePhysicsRunner.CheckThread();
        currentStep++;
        while (currentStep > lastVirtualStep)
        {
            VirtualStep();
        }
        foreach (var obj in GetAliveObjects(currentStep)) obj.Step(currentStep);
        foreach (var obj in futureObjects)
        {
            if (!obj.IsObsolete(currentStep)) continue;
            obsoleteObjs.Add(obj);
        }
        foreach (var obsolete in obsoleteObjs)
        {
            RemoveObject(obsolete);
        }
        obsoleteObjs.Clear();
    }

    public static void VirtualStep()
    {
        FuturePhysicsRunner.CheckThread();
        foreach (var obj in GetAliveObjects(lastVirtualStep))
            obj.VirtualStep(lastVirtualStep);

        afterVirtualStep.Invoke(lastVirtualStep);
        lastVirtualStep++;
    }

    public static void AddObject(Type type, IFutureObject obj)
    {
        FuturePhysicsRunner.CheckThread();

        futureObjects.Add(obj);
        if (!typeToObjs.ContainsKey(type))
            typeToObjs[type] = new HashSet<IFutureObject>();
        if (!objToTypes.ContainsKey(obj))
            objToTypes[obj] = new HashSet<Type>();

        typeToObjs[type].Add(obj);
        objToTypes[obj].Add(type);
    }

    public static void RemoveObject(IFutureObject obj)
    {
        FuturePhysicsRunner.CheckThread();
        if (!futureObjects.Remove(obj)) return;
        
        foreach (var type in objToTypes[obj])
        {
            if (typeToObjs.TryGetValue(type, out var objs)) objs.Remove(obj);
        }
        objToTypes.Remove(obj);
    }

    public static IEnumerable<T> GetComponents<T>(int step) where T : class
    {
        return typeToObjs[typeof(T)].Cast<T>();
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
        foreach (var state in futureObjects) state.ResetToStep(step, cause);
    }

    private static IEnumerable<IFutureObject> GetAliveObjects(int step)
    {
        return futureObjects.Where(obj => obj.IsAlive(step));
    }

    public class ResetParams
    {
        public GameObject cause;
        public int step;
    }
}