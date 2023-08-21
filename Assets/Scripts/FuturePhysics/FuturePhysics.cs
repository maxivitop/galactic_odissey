using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class FuturePhysics
{
    public const int MaxSteps = 10000;
    public const float DeltaTime = 0.04f;
    public const float G = 1f;

    public static int currentStep;
    public static int lastVirtualStep;
    public static bool upToDateWithLastStep;

    private static readonly HashSet<IFutureObject> futureObjects = new();
    private static readonly Dictionary<IFutureObject, ISet<Type>> objToTypes = new();
    private static readonly Dictionary<Type, ISet<IFutureObject>> typeToObjs = new();

    public static void Step()
    {
        FuturePhysicsRunner.CheckThread();
        currentStep++;
        CatchUpWithStep(currentStep);
        foreach (var obj in GetAliveObjects(currentStep).ToArray())
            obj.Step(currentStep);
    }

    public static void MakeVirtualCalculations()
    {
        FuturePhysicsRunner.CheckThread();
        if (upToDateWithLastStep && lastVirtualStep - currentStep < MaxSteps)
        {
            lastVirtualStep++;
        }
        upToDateWithLastStep = true;
        foreach (var obj in futureObjects)
            upToDateWithLastStep &= obj.CatchUpWithVirtualStep(lastVirtualStep);
    }

    public static void CatchUpWithStep(int step, bool isFromBg = false)
    {
        var minVirtualStep =
            futureObjects.Select(obj => obj.RequiredVirtualStepForStep(step)).Max();
        var logWarning = !isFromBg;
        while (true)
        {
            var areAllPrerequisiteVirtualStepsComplete = true;
            foreach (var obj in futureObjects)
            {
                var isComplete = obj.CatchUpWithVirtualStep(minVirtualStep);
                if (!isComplete && logWarning)
                {
                    Debug.LogWarning(obj + "Did not complete in time step="
                                         +step+     
                                         " minVirtualStep=" + minVirtualStep
                                         + " startStep=" + (obj as FutureBehaviour).StartStep);
                    logWarning = false;
                }
                areAllPrerequisiteVirtualStepsComplete &= isComplete;
            }
            if (areAllPrerequisiteVirtualStepsComplete) break;
        }
    }

    public static void AddObject(Type type, IFutureObject obj)
    {
        upToDateWithLastStep = false;
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
        if (step == lastVirtualStep) return;
        upToDateWithLastStep = false;
        foreach (var state in futureObjects) state.ResetToStep(step, whom);
    }

    private static IEnumerable<IFutureObject> GetAliveObjects(int step)
    {
        return futureObjects.Where(obj => obj.IsAlive(step));
    }
}