using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(FutureTransform))]
[RequireComponent(typeof(FutureRigidBody2D))]
public class ProjectileLauncher : FutureBehaviour
{
    public Projectile projectile;
    public ComputeShader aimShader;
    public int maxSteps;
    private ComputeBuffer positions;
    private Vector3[] positionsData;
    private ComputeBuffer output;
    private Vector3[] outputData;
    private ComputeBuffer tmp;
    private ComputeBuffer tmpTraj;
    private ComputeBuffer trajLen;
    private int[] trajLenData = new int[1];
    private ComputeBuffer trajLens;
    private const int NumThreads = 64;
    public TrajectoryProvider target;
    private ComputeBuffer targetBuffer;
    private Vector3[] targetData;
    private float[] massesData;
    private ComputeBuffer masses;
    public float[] radiiData;
    private ComputeBuffer radii;
    public float initialSpeed;
    public int launchEachNSteps = 100;
    public int prelaunchSteps = 10;

    private FutureRigidBody2D futureRigidBody2D;
    private FutureTransform futureTransform;

    private void Start()
    {
        futureTransform = GetComponent<FutureTransform>();
        futureRigidBody2D = GetComponent<FutureRigidBody2D>();
        aimShader = Instantiate(aimShader);
        output = new ComputeBuffer(maxSteps, sizeof(float) * 3);
        aimShader.SetBuffer(0, "output", output);
        trajLen = new ComputeBuffer(1, sizeof(int));
        aimShader.SetBuffer(0, "traj_len", trajLen);


        tmp = new ComputeBuffer(NumThreads, sizeof(float));
        aimShader.SetBuffer(0, "tmp", tmp);
        tmpTraj = new ComputeBuffer(NumThreads * maxSteps, sizeof(float) * 3);
        aimShader.SetBuffer(0, "tmp_traj", tmpTraj);
        trajLens = new ComputeBuffer(NumThreads, sizeof(int));
        aimShader.SetBuffer(0, "traj_lens", trajLens);
        aimShader.SetInt("max_steps", maxSteps);
        targetBuffer = new ComputeBuffer(maxSteps, sizeof(float) * 3);
        aimShader.SetBuffer(0, "target", targetBuffer);
        targetData = new Vector3[maxSteps];
        outputData = new Vector3[maxSteps];
        aimShader.SetFloat("my_mass", (float)projectile
            .GetComponent<FutureRigidBody2D>().initialMass);
        aimShader.SetFloat("G", (float)FuturePhysics.G);
        aimShader.SetFloat("dt", (float)FuturePhysics.DeltaTime);
        aimShader.SetFloat("initial_speed", initialSpeed);
        aimShader.SetFloat("radius", projectile.GetComponent<CircleFutureCollider>().radius);
    }

    public override void Step(int step)
    {
        if ((step + prelaunchSteps) % launchEachNSteps == 0)
        {
            ShootStep(step + prelaunchSteps);
        }

        if (step % launchEachNSteps == 0)
        {
            output.GetData(outputData);
            trajLen.GetData(trajLenData);
            var projectileInstance = Instantiate(projectile);
            projectileInstance.Launch(step, outputData, trajLenData[0]);
        }
    }

    private void ShootStep(int step)
    {
        var trajStep = TrajectoryProvider.PhysicsStepToTrajectoryStep(step);
        if (trajStep < 0)
        {
            Debug.LogWarning("ShootStep with step="+step+": trajStep="+trajStep);
            step -= trajStep; // it is negative, so we are adding value
            trajStep = 0;
        }
        var gravitySources =
            FuturePhysics.GetComponents<GravitySource>(step).ToArray();
        var positionsCount = maxSteps * gravitySources.Length;
        if (positions == null || positionsCount != positions.count)
        {
            positions?.Release();
            positions = new ComputeBuffer(positionsCount, sizeof(float) * 3);
            positionsData = new Vector3[positionsCount];
            aimShader.SetBuffer(0, "positions", positions);
            masses?.Release();
            masses = new ComputeBuffer(gravitySources.Length, sizeof(float));
            massesData = new float[gravitySources.Length];
            aimShader.SetBuffer(0, "masses", masses);
            aimShader.SetInt("gravity_sources", gravitySources.Length);
            radii?.Release();
            radii = new ComputeBuffer(gravitySources.Length, sizeof(float));
            radiiData = new float[gravitySources.Length];
            aimShader.SetBuffer(0, "radii", radii);
            for (var i = 0; i < gravitySources.Length; i++)
            {
                var gravitySource = gravitySources[i];
                massesData[i] = (float)gravitySource.futureRigidBody2D.initialMass;
                radiiData[i] = gravitySource.futureCollider.radius;
            }

            masses.SetData(massesData);
            radii.SetData(radiiData);
        }

        for (var i = 0; i < gravitySources.Length; i++)
        {
            var gravitySource = gravitySources[i];
            var absTraj = gravitySource.trajectoryProvider.absoluteTrajectory;
            Array.Copy(
                absTraj.array,
                trajStep,
                positionsData,
                maxSteps * i,
                maxSteps
            );
        }

        positions.SetData(positionsData);
        Array.Copy(
            target.absoluteTrajectory.array,
            trajStep,
            targetData,
            0,
            maxSteps
        );
        targetBuffer.SetData(targetData);
        aimShader.SetVector("position", futureTransform.GetFuturePosition(step));
        aimShader.SetVector("initial_velocity", futureRigidBody2D.GetState(step).velocity);
        aimShader.Dispatch(0, 1, 1, 1);
    }

    private void OnDestroy()
    {
        positions?.Dispose();
        output.Dispose();
        tmp.Dispose();
        tmpTraj.Dispose();
        trajLen.Dispose();
        trajLens.Dispose();
        targetBuffer.Dispose();
        masses?.Dispose();
        radii?.Dispose();
    }
}