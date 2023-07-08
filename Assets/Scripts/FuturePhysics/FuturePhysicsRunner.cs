using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThreadPriority = System.Threading.ThreadPriority;

public class FuturePhysicsRunner : MonoBehaviour
{
    public delegate void ExecuteOnUpdateDelegate();
    
    public static int timeScale = 0;
    public static readonly Event<int> onBgThreadIdle = new();
    public const float StepsPerSecond = 50;
    public static int stepsNextFrame;
    
    private static volatile Thread activeThread;
    private static volatile Thread bgThread;
    private static bool isQuitting;
    private static readonly AutoResetEvent mainThreadFinished = new(false);
    private static readonly AutoResetEvent bgThreadFinished = new(true);
    private static readonly List<ExecuteOnUpdateDelegate> executeOnUpdateQueue = new();
    
    private bool isThreadStarted;
    private float timePerStep;
    private float unusedDeltaTime;
    private IEnumerator mainThreadFinishedNotifier;
    private static volatile bool isMainThreadWaiting = true;


    private void Awake()
    {
        activeThread = Thread.CurrentThread;
        
        timePerStep = 1f / StepsPerSecond;
        mainThreadFinishedNotifier = MainThreadFinishedNotifier();
        StartCoroutine(mainThreadFinishedNotifier);
    }

    private void Update()
    {
        isMainThreadWaiting = true;
        bgThreadFinished.WaitOne(100);
        activeThread = Thread.CurrentThread;
        
        foreach (var executeOnUpdate in executeOnUpdateQueue)
        {
            executeOnUpdate.Invoke();
        }
        executeOnUpdateQueue.Clear();
        
        StartThreadIfNeeded();
        var stepsThisFrame = stepsNextFrame;
        unusedDeltaTime += Time.deltaTime;
        var stepsPerNextFrame = Mathf.FloorToInt(StepsPerSecond * unusedDeltaTime);
        unusedDeltaTime -= stepsPerNextFrame * timePerStep;
        stepsNextFrame = stepsPerNextFrame * timeScale;
        for (var i = 0; i < stepsThisFrame; i++) FuturePhysics.Step();
        // lock is released by [ReleaseLock] after all Updates.
    }
    
    // IEnumerator is called after all Updates
    private static IEnumerator MainThreadFinishedNotifier()
    {
        yield return 0;
        while (true)
        {
            isMainThreadWaiting = false;
            activeThread = bgThread;
            mainThreadFinished.Set();
            yield return 0;
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private void StartThreadIfNeeded()
    {
        if (isThreadStarted) return;
        bgThread = new Thread(VirtualStepRunner)
        {
            Priority = ThreadPriority.AboveNormal
        };
        bgThread.Start();
        isThreadStarted = true;
    }

    private void VirtualStepRunner()
    {
        mainThreadFinished.WaitOne();
        while (true)
        {
            while (FuturePhysics.lastVirtualStep - FuturePhysics.currentStep <
                   FuturePhysics.MaxSteps)
            {
                if(isMainThreadWaiting)
                {
                    bgThreadFinished.Set();
                    mainThreadFinished.WaitOne();
                }
                FuturePhysics.VirtualStep();
            }
            onBgThreadIdle.Invoke(FuturePhysics.lastVirtualStep);
            bgThreadFinished.Set();
            mainThreadFinished.WaitOne();
        }
        // ReSharper disable once FunctionNeverReturns
    }

    public static void ExecuteOnUpdate(ExecuteOnUpdateDelegate executeOnUpdateDelegate)
    {
        executeOnUpdateQueue.Add(executeOnUpdateDelegate);
    }


    public static void CheckThread()
    {
        if (activeThread == Thread.CurrentThread)
        {
            return;
        }
        Debug.LogError("Incorrect thread access from " +
            (Thread.CurrentThread == bgThread ? "bg" : "main") +
            " thread, isMainThreadWaiting=" + isMainThreadWaiting);
    }
}