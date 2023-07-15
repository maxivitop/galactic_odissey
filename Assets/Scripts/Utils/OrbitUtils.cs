using System;
using UnityEngine;

public class OrbitUtils
{
    
    public static GravitySource FindBiggestGravitySource(
        Vector3d position,
        int step=0,
        double dt=0.0
        )
    {
        var maxGravity = 0.0;
        GravitySource closestSource = null;
        foreach (var gravitySource in FuturePhysics.GetComponents<GravitySource>(step))
        {
            var sourcePosition = gravitySource.futurePositionProvider.GetFuturePosition(step, dt);
            Vector2d direction = sourcePosition - position;
            if (direction.sqrMagnitude < 1e-6) continue; // dont return self
            // F = ma
            // F = G*m1*m2/r^2
            // a = F/m = G*m2/r^2
            var gravity = FuturePhysics.G * gravitySource.futureRigidBody2D.initialMass
                          / direction.sqrMagnitude;
            if (gravity < maxGravity) continue;
            maxGravity = gravity;
            closestSource = gravitySource;
        }

        return closestSource;
    }

    public static Vector2d CalculateGravityVector(
        GravitySource gravitySource,
        Vector3d position,
        double myMass,
        int step=0,
        double dt=0.0
    )
    {
        var gsPosition = gravitySource.futurePositionProvider.GetFuturePosition(step, dt);
        var direction = gsPosition - position;
        return FuturePhysics.G * (gravitySource.futureRigidBody2D.initialMass + myMass) *
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