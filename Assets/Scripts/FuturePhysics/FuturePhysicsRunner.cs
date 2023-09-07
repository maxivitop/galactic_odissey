using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using ThreadPriority = System.Threading.ThreadPriority;

public class FuturePhysicsRunner : MonoBehaviour
{
    public delegate void ExecuteOnUpdateDelegate();

    public float stepsPerSecond = 50;

    public int waitedMs;
    public TextMeshProUGUI bgThreadWait;
    public TextMeshProUGUI fps;
    
    private static volatile Thread activeThread;
    private static volatile Thread bgThread;
    private static bool isQuitting;
    private static readonly AutoResetEvent mainThreadFinished = new(false);
    private static readonly AutoResetEvent bgThreadFinished = new(true);
    private static readonly List<ExecuteOnUpdateDelegate> executeOnUpdateQueue = new();

    private bool isThreadStarted;
    private IEnumerator mainThreadFinishedNotifier;
    private static volatile bool isMainThreadWaiting = true;
    private static volatile bool hasBgThreadWorked = true;
    private static volatile bool isBgThreadAlive = true;
    private float maxDeltaTimeThisSecond;
    private int trackedSecond;
    private float avgDeltaTimeSum;
    private int avgDeltaTimeCount;


    private void Awake()
    {
        activeThread = Thread.CurrentThread;
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
        if (Mathf.RoundToInt(Time.unscaledTime) != trackedSecond)
        {
            fps.text = Mathf.RoundToInt(avgDeltaTimeCount / avgDeltaTimeSum)
                       + " / " + String.Format("{0,3:###}",
                           Mathf.RoundToInt(1f / maxDeltaTimeThisSecond));
            trackedSecond = Mathf.RoundToInt(Time.unscaledTime);
            maxDeltaTimeThisSecond = Time.unscaledDeltaTime;
            avgDeltaTimeCount = 0;
            avgDeltaTimeSum = 0;
        }

        avgDeltaTimeSum += Time.unscaledDeltaTime;
        avgDeltaTimeCount++;
        maxDeltaTimeThisSecond = Mathf.Max(maxDeltaTimeThisSecond, Time.unscaledDeltaTime);
        bgThreadWait.text = waitedMs + " ms";
       
        hasBgThreadWorked = false;
        activeThread = Thread.CurrentThread;

        var copy = executeOnUpdateQueue.ToArray();
        executeOnUpdateQueue.Clear();
        foreach (var executeOnUpdate in copy)
        {
            executeOnUpdate.Invoke();
        }

        StartThreadIfNeeded();
        while (FuturePhysics.currentStep < Time.time * stepsPerSecond)
        {
            FuturePhysics.Step();
        }
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
        while (isBgThreadAlive)
        {
            if(isMainThreadWaiting)
            {
                bgThreadFinished.Set();
                mainThreadFinished.WaitOne();
                hasBgThreadWorked = true;
            }
            FuturePhysics.CatchUpWithStep(FuturePhysics.currentStep + FuturePhysics.MaxSteps);
        }
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
        isBgThreadAlive = false;
        bgThread?.Abort();
    }

    private void OnApplicationQuit()
    {
        isBgThreadAlive = false;
        bgThread?.Abort();
    }
}