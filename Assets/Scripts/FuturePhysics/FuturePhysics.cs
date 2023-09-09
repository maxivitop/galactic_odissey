using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class FuturePhysics
{
    public const int MaxSteps = 8000;
    public const float DeltaTime = 0.05f;
    public const float G = 1f;

    public static int currentStep;
    public static int lastVirtualStep;

    private static readonly HashSet<IFutureObject> futureObjects = new();
    private static readonly Dictionary<IFutureObject, ISet<Type>> objToTypes = new();
    private static readonly Dictionary<Type, ISet<IFutureObject>> typeToObjs = new();
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
        catchingUp.AddRange(futureObjects.Reverse());
        while (catchingUp.Count > 0)
        {
            for (var i = catchingUp.Count - 1; i >= 0; i--) // backwards loop allows to remove without breaking indexing
            {
                if (catchingUp[i].CatchUpWithVirtualStep(step))
                    catchingUp.RemoveAt(i);
            }
        }
        
        lastVirtualStep = Mathf.Max(step, lastVirtualStep);
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