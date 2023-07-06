using System.Threading;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class FuturePhysicsRunner : MonoBehaviour
{
    private static volatile Thread activeThread;
    private static bool isQuitting;
    private static readonly Mutex mutex = new();

    public static int timeScale = 0;
    public float stepsPerSecond = 50;
    private bool isThreadStarted;
    private readonly AutoResetEvent waitHandle = new(false);
    private float timePerStep;
    private float unusedDeltaTime;
    private IEnumerator mutexReleaser;

    private void Awake()
    {
        activeThread = Thread.CurrentThread;
        
        timePerStep = 1f / stepsPerSecond;
        mutexReleaser = ReleaseMutex();
        mutex.WaitOne();
        StartCoroutine(mutexReleaser);
    }

    private void Update()
    {
        mutex.WaitOne();
        activeThread = Thread.CurrentThread;
        StartThreadIfNeeded();
        unusedDeltaTime += Time.deltaTime;
        var stepsThisFrame = Mathf.FloorToInt(stepsPerSecond * unusedDeltaTime);
        unusedDeltaTime -= stepsThisFrame * timePerStep;
        for (var i = 0; i < stepsThisFrame * timeScale; i++) FuturePhysics.Step();
        waitHandle.Set();
        // mutex is released by [ReleaseMutex] after all Updates.
    }
    
    // IEnumerator is called after all Updates
    private static IEnumerator ReleaseMutex()
    {
        while (true)
        {
            mutex.ReleaseMutex();
            yield return 0;
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private void StartThreadIfNeeded()
    {
        if (isThreadStarted) return;
        var virtualRunner = new Thread(VirtualStepRunner);
        virtualRunner.Start();
        isThreadStarted = true;
    }

    private void VirtualStepRunner()
    {
        while (true)
        {
            while (FuturePhysics.lastVirtualStep - FuturePhysics.currentStep <
                   FuturePhysics.MaxSteps)
            {
                mutex.WaitOne();
                activeThread = Thread.CurrentThread;
                FuturePhysics.VirtualStep();
                mutex.ReleaseMutex();
            }
            waitHandle.WaitOne();
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    public static void CheckThread()
    {
        if (isQuitting)
        {
            return;
        }
        Assert.AreEqual(activeThread, Thread.CurrentThread);
    }
}