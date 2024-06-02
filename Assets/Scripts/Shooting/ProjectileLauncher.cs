using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

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
    private ComputeBuffer trajLenAndDistance;
    private int[] trajLenData = new int[2];
    private ComputeBuffer trajLens;
    private const int NumThreads = 64;
    public FutureTransform target;
    private ComputeBuffer targetBuffer;
    private Vector3[] targetData;
    private float[] massesData;
    private ComputeBuffer masses;
    public float[] radiiData;
    private ComputeBuffer radii;
    public float initialSpeed;
    public float collisionOffset;
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
        trajLenAndDistance = new ComputeBuffer(2, sizeof(int));
        aimShader.SetBuffer(0, "traj_len", trajLenAndDistance);


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
        aimShader.SetFloat("my_mass", projectile.GetComponent<FutureRigidBody2D>().initialMass);
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
            var trajLenRequest = AsyncGPUReadback.Request(trajLenAndDistance, request =>
            {
                var trajLenNativeData = request.GetData<int>();
                trajLenNativeData.CopyTo(trajLenData);
            });
            AsyncGPUReadback.Request(output, request => {
                var outputNativeData = request.GetData<Vector3>();
                outputNativeData.CopyTo(outputData);
                FuturePhysicsRunner.ExecuteOnUpdate(() =>
                {
                    trajLenRequest.WaitForCompletion();
                    if (trajLenData[1] != 0) // not close
                    {
                        return;
                    }
                    var projectileInstance = Instantiate(projectile, outputData[0], Quaternion.identity);
                    projectileInstance.Launch(step, outputData, trajLenData[0]);
                });
            });
        }
    }

    private void ShootStep(int step)
    {
        var trajStep = TrajectoryProvider.PhysicsStepToTrajectoryStep(step);
        if (trajStep < 0)
        {
            Debug.LogWarning("ShootStep with step="+step+": trajStep="+trajStep);
            step -= trajStep; // it is negative, so we are adding value
        }
        var gravitySources = GravitySource.All;
        var positionsCount = maxSteps * gravitySources.Count;
        if (positions == null || positionsCount != positions.count)
        {
            positions?.Release();
            positions = new ComputeBuffer(positionsCount, sizeof(float) * 3);
            positionsData = new Vector3[positionsCount];
            aimShader.SetBuffer(0, "positions", positions);
            masses?.Release();
            masses = new ComputeBuffer(gravitySources.Count, sizeof(float));
            massesData = new float[gravitySources.Count];
            aimShader.SetBuffer(0, "masses", masses);
            aimShader.SetInt("gravity_sources", gravitySources.Count);
            radii?.Release();
            radii = new ComputeBuffer(gravitySources.Count, sizeof(float));
            radiiData = new float[gravitySources.Count];
            aimShader.SetBuffer(0, "radii", radii);
            for (var i = 0; i < gravitySources.Count; i++)
            {
                var gravitySource = gravitySources[i];
                massesData[i] = gravitySource.futureRigidBody2D.mass[step];
                radiiData[i] = gravitySource.futureCollider.radius + collisionOffset;
            }

            masses.SetData(massesData);
            radii.SetData(radiiData);
        }

        for (var i = 0; i < gravitySources.Count; i++)
        {
            var gravitySource = gravitySources[i];
            var absTraj = gravitySource.futureTransform.position;
            absTraj.capacityArray.CopyInto(
                positionsData,
                step,
                maxSteps,
                maxSteps * i
            );
        }

        positions.SetData(positionsData);
        target.position.capacityArray.CopyInto(
            targetData,
            step,
            maxSteps,
            0
        );
        targetBuffer.SetData(targetData);
        aimShader.SetVector("position", futureTransform.GetFuturePosition(step));
        aimShader.SetVector("initial_velocity", futureRigidBody2D.velocity[step]);

        var commandBuffer = new CommandBuffer();
        commandBuffer.SetExecutionFlags(CommandBufferExecutionFlags.AsyncCompute);
        commandBuffer.DispatchCompute(
            aimShader, 0, 1, 1, 1);
        commandBuffer.name = "aimShaderBuffer";
        Graphics.ExecuteCommandBufferAsync(commandBuffer, ComputeQueueType.Background);
        aimShader.Dispatch(0, 1, 1, 1);
    }

    private void OnDestroy()
    {
        positions?.Dispose();
        output?.Dispose();
        tmp.Dispose();
        tmpTraj.Dispose();
        trajLenAndDistance.Dispose();
        trajLens.Dispose();
        targetBuffer.Dispose();
        masses?.Dispose();
        radii?.Dispose();
    }
}