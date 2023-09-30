using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using ThreadPriority = System.Threading.ThreadPriority;

public class FuturePhysicsRunner : MonoBehaviour
{
    public static FuturePhysicsRunner Instance;
    public delegate void ExecuteOnUpdateDelegate();

    public const float StepsPerSecond = 40f;

    public int waitedMs;
    public TextMeshProUGUI bgThreadWait;
    public TextMeshProUGUI fps;
    
    private volatile Thread activeThread;
    private volatile Thread bgThread;
    private bool isQuitting;
    private readonly AutoResetEvent mainThreadFinished = new(false);
    private readonly AutoResetEvent bgThreadFinished = new(true);
    private readonly List<ExecuteOnUpdateDelegate> executeOnUpdateQueue = new();

    private bool isThreadStarted;
    private IEnumerator mainThreadFinishedNotifier;
    private volatile bool isMainThreadWaiting = true;
    private volatile bool hasBgThreadWorked = true;
    private volatile bool isBgThreadAlive = true;
    private float maxDeltaTimeThisSecond;
    private int trackedSecond;
    private float avgDeltaTimeSum;
    private int avgDeltaTimeCount;
    public float renderFrame;
    public int renderFrameNextStep;
    public int renderFramePrevStep;
    public float renderFrameStepPart;


    private void Awake()
    {
        Instance = this;
        FuturePhysics.FullReset();
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

        renderFrame = Time.time * StepsPerSecond;
        renderFrameNextStep = Mathf.CeilToInt(renderFrame);
        renderFramePrevStep = Mathf.FloorToInt(renderFrame);
        renderFrameStepPart = renderFrame - renderFramePrevStep;
       
        hasBgThreadWorked = false;
        activeThread = Thread.CurrentThread;

        var copy = executeOnUpdateQueue.ToArray();
        executeOnUpdateQueue.Clear();
        foreach (var executeOnUpdate in copy)
        {
            executeOnUpdate.Invoke();
        }

        StartThreadIfNeeded();
        while (FuturePhysics.currentStep < renderFrameNextStep)
        {
            FuturePhysics.Step();
        }
    }
    
    // IEnumerator is called after all Updates
    private IEnumerator MainThreadFinishedNotifier()
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
        Instance.executeOnUpdateQueue.Add(executeOnUpdateDelegate);
    }


    public static void CheckThread()
    {
        if (Instance.activeThread == Thread.CurrentThread)
        {
            return;
        }
        Debug.LogError("Incorrect thread access from " +
            (Thread.CurrentThread == Instance.bgThread ? "bg" : "main") +
            " thread, isMainThreadWaiting=" + Instance.isMainThreadWaiting);
    }

    private void OnDestroy()
    {
        isBgThreadAlive = false;
        mainThreadFinished.Set();
        bgThread?.Abort();
    }

    private void OnApplicationQuit()
    {
        isBgThreadAlive = false;
        mainThreadFinished.Set();
        bgThread?.Abort();
    }
}