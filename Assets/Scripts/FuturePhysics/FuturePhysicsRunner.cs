using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = System.Random;
using ThreadPriority = System.Threading.ThreadPriority;

public class FuturePhysicsRunner : MonoBehaviour
{
    public delegate void ExecuteOnUpdateDelegate();

    public int waitedMs;
    public TextMeshProUGUI bgThreadWait;
    public static int timeScale = 0;
    public static readonly SingleEvent<int> onBgThreadIdle = new();
    public const float StepsPerSecond = 50;
    public static int stepsNextFrame;
    
    private static volatile Thread activeThread;
    private static volatile Thread bgThread;
    private static bool isQuitting;
    private static readonly AutoResetEvent mainThreadFinished = new(false);
    private static readonly AutoResetEvent bgThreadFinished = new(true);
    private static readonly List<ExecuteOnUpdateDelegate> executeOnUpdateQueue = new();
    public static int nextFrameStep;

    private bool isThreadStarted;
    private bool caughtUpThisFrame = false;
    private float timePerStep;
    private float unusedDeltaTime;
    private IEnumerator mainThreadFinishedNotifier;
    private static volatile bool isMainThreadWaiting = true;
    private static volatile bool hasBgThreadWorked = true;


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
        if (!hasBgThreadWorked)
        {
            Debug.LogWarning("Scheduler is slow");
        }
        var start = Time.realtimeSinceStartupAsDouble;
        bgThreadFinished.WaitOne(1000);
        var end = Time.realtimeSinceStartupAsDouble;
        waitedMs = Mathd.RoundToInt((end - start) * 1000);
        bgThreadWait.text = waitedMs.ToString();
        hasBgThreadWorked = false;
        activeThread = Thread.CurrentThread;

        var copy = executeOnUpdateQueue.ToArray();
        executeOnUpdateQueue.Clear();
        foreach (var executeOnUpdate in copy)
        {
            executeOnUpdate.Invoke();
        }

        StartThreadIfNeeded();
        var stepsThisFrame = stepsNextFrame;
        unusedDeltaTime += Time.deltaTime;
        var stepsPerNextFrame = Mathf.FloorToInt(StepsPerSecond * unusedDeltaTime);
        unusedDeltaTime -= stepsPerNextFrame * timePerStep;
        stepsNextFrame = stepsPerNextFrame * timeScale;
        for (var i = 0; i < stepsThisFrame; i++) FuturePhysics.Step();
        nextFrameStep = FuturePhysics.currentStep + stepsNextFrame;
        caughtUpThisFrame = false;
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
            Priority = ThreadPriority.Highest
        };
        bgThread.Start();
        isThreadStarted = true;
    }

    private void VirtualStepRunner()
    {
        mainThreadFinished.WaitOne();
        hasBgThreadWorked = true;
        while (true)
        {
            if(isMainThreadWaiting)
            {
                bgThreadFinished.Set();
                mainThreadFinished.WaitOne();
                hasBgThreadWorked = true;
            }
            if (stepsNextFrame != 0 && !caughtUpThisFrame)
            {
                FuturePhysics.CatchUpWithStep(nextFrameStep, isFromBg:true);
                caughtUpThisFrame = true;
            } 
            FuturePhysics.MakeVirtualCalculations();
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

    private void OnDestroy()
    {
        bgThread?.Abort();
    }
}