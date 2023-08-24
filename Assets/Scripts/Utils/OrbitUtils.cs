using System;
using UnityEngine;

public class OrbitUtils
{
    
    public static GravitySource FindBiggestGravitySource(
        Vector3 position,
        int step=0,
        float dt=0.0f
        )
    {
        var maxGravity = 0.0f;
        GravitySource closestSource = null;
        foreach (var gravitySource in FuturePhysics.GetComponents<GravitySource>(step))
        {
            var sourcePosition = gravitySource.futurePositionProvider.GetFuturePosition(step, dt);
            Vector2 direction = sourcePosition - position;
            if (direction.sqrMagnitude < 1e-6f) continue; // dont return self
            // F = ma
            // F = G*m1*m2/r^2
            // a = F/m = G*m2/r^2
            var gravity = FuturePhysics.G * gravitySource.futureRigidBody2D.mass[step]
                          / direction.sqrMagnitude;
            if (gravity < maxGravity) continue;
            maxGravity = gravity;
            closestSource = gravitySource;
        }

        return closestSource;
    }

    public static Vector2 CalculateGravityVector(
        GravitySource gravitySource,
        Vector3 position,
        float myMass,
        int step=0,
        float dt=0.0f
    )
    {
        var gsPosition = gravitySource.futurePositionProvider.GetFuturePosition(step, dt);
        var direction = gsPosition - position;
        return FuturePhysics.G * (gravitySource.futureRigidBody2D.mass[step] + myMass) *
             direction.normalized / direction.sqrMagnitude;
    }
    
    // Taken from Vallado source code site. (Also used in LambertUniversal)
    public static void FindC2C3(double znew, out double c2new, out double c3new) {
        double small, sqrtz;
        small = 0.00000001;

        // -------------------------  implementation   -----------------
        if (znew > small) {
            sqrtz = Math.Sqrt(znew);
            c2new = (1.0 - Math.Cos(sqrtz)) / znew;
            c3new = (sqrtz - Math.Sin(sqrtz)) / (sqrtz * sqrtz * sqrtz);
        } else {
            if (znew < -small) {
                sqrtz = Math.Sqrt(-znew);
                c2new = (1.0 - Math.Cosh(sqrtz)) / znew;
                c3new = (Math.Sinh(sqrtz) - sqrtz) / (sqrtz * sqrtz * sqrtz);
            } else {
                c2new = 0.5;
                c3new = 1.0 / 6.0;
            }
        }
    }  // findc2c3
}