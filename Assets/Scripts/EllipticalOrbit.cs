using System;
using UnityEditor;
using UnityEngine;

public class EllipticalOrbit
{
    public GravitySource center;
    private int step0;
    private double eccentricity;
    private double mu;
    private double p; //! semi-parameter that defines orbit size

    private Vector3d r0;
    private Vector3d v0;
    private const double Small = 1E-6;
    private const double ConvergenceThreshold = 1E-3;
    private const int MaxIter = 20;
    private const double VerySmall = 1E-9; // 1E-13;
    private const double HalfPi = Math.PI * 0.5;

    public EllipticalOrbit(GravitySource center, float myMass, int step0, Vector3 r0, Vector3 v0)
    {
        this.center = center;
        this.step0 = step0;
        this.r0 = new Vector3d(r0);
        this.v0 = new Vector3d(v0);
        mu = FuturePhysics.G * (myMass + center.futureRigidBody2D.mass[step0]);
        InitializeFromRv();
    }

    //RVtoCOE
    private void InitializeFromRv()
    {
        // -------------------------  implementation   -----------------
        var magr = r0.magnitude;
        var magv = v0.magnitude;

        // ------------------  find h n and e vectors   ----------------
        var hbar = Vector3d.Cross(r0, v0);
        var magh = hbar.magnitude;
        if (!(magh > VerySmall)) return;
        var c1 = magv * magv - mu / magr;
        var rdotv = Vector3d.Dot(r0, v0);
        var temp = 1.0 / mu;
        var ebar = new Vector3d( ((c1 * r0.x - rdotv * v0.x) * temp),
            ((c1 * r0.y - rdotv * v0.y) * temp),
            ((c1 * r0.z - rdotv * v0.z) * temp));
        eccentricity = ebar.magnitude;

        // ------------  find a e and semi-latus rectum   ----------
        p = magh * magh * temp;
    }
    
    
    public bool Evolve(int step, float dt, out Vector3 rNew, out Vector3 vNew)
    {
        double xold, a, temp;
        var c2New = 0.0;
        var c3New = 0.0;
        var xnew = 0.0;
        double dtseco = (step - step0 + dt) * FuturePhysics.DeltaTime;

        var magro = r0.magnitude;
        var magvo = v0.magnitude;
        var rdotv = Vector3d.Dot(r0, v0);

        // -------------  find sme, alpha, and a  ------------------
        var sme = ((magvo * magvo) * 0.5) - (mu / magro);
        var alpha = -sme * 2.0 / mu;
            
        if (Math.Abs(sme) > Small)
            a = -mu / (2.0 * sme);
        else
            a = double.NaN;
        
        var orbitPeriod = double.NaN;
        if (eccentricity < 1.0) {
            orbitPeriod = 2.0 * Math.PI * Math.Sqrt(a * a * a / mu);
        }
        
        // Very large times can cause precision issues, normalize if possible
        if (eccentricity < 1 && dtseco > 1E6) {
            
            dtseco %= orbitPeriod;
        }
        
        var dtsec = dtseco;

        var centerPosLast = center.futurePositionProvider.GetFuturePosition(step, dt);
        Vector3 centerVelLast = center.futureRigidBody2D.velocity[step];

        // --------------------  initialize values   -------------------
        var znew = 0.0;

        if (!(Math.Abs(dtseco) > Small))
        {
            // ----------- set vectors to incoming since 0 time --------
            rNew = r0.ToVector3();
            // Add centerPos to value we ref back
            rNew += centerPosLast;
            vNew = v0.ToVector3() + centerVelLast;
            return true;
        }
        var radialInfall = Vector3d.Cross(r0.normalized, v0.normalized).magnitude < 1E-3;
        if (radialInfall)
        {
            if (sme <= 0)
            {
                // Not debugged, but normal Kepler handles this case ok
                // EvolveRecilinearBound(dtsec, sme, ref r_new, ref v_new);
            }
            else
            {
                EvolveRecilinearUnbound(dtsec, sme, out rNew, out vNew);
                // Add centerPos to value we ref back
                rNew += centerPosLast;
                // update velocity
                vNew += centerVelLast;
                return true;
            }
        }
        // ------------   setup initial guess for x  ---------------
        // -----------------  circle and ellipse -------------------
        if (alpha >= 0)
        {
            if (Math.Abs(alpha - 1.0) > Small)
                xold = Math.Sqrt(mu) * dtsec * alpha;
            else
                // - first guess can't be too close. ie a circle, r=a
                xold = Math.Sqrt(mu) * dtsec * alpha * 0.97;
        }
        else
        {
            // --------------------  parabola  ---------------------
            if (Math.Abs(alpha) < Small)
            {
                var h = Vector3d.Cross(r0, v0);
                var pp = h.sqrMagnitude / mu;
                var s = 0.5 * (HalfPi - Math.Atan(3.0 * Math.Sqrt(mu / (pp * pp * pp)) * dtsec));
                var w = Math.Atan(Math.Pow(Math.Tan(s), (1.0 / 3.0)));
                xold = Math.Sqrt(p) * (2.0 * MathUtils.Cot(2.0 * w));
                alpha = 0.0;
            }
            else
            {
                // ------------------  hyperbola  ------------------
                temp = -2.0 * mu * dtsec /
                       (a * (rdotv + Math.Sign(dtsec) * Math.Sqrt(-mu * a) *
                           (1.0 - magro * alpha)));
                xold = Math.Sign(dtsec) * Math.Sqrt(-a) * Math.Log(temp);
            }
        } // if alpha

        var ktr = 1;
        var dtnew = -10.0;
        // conv for dtsec to x units
        var tmp = 1.0 / Math.Sqrt(mu);

        while (Math.Abs(dtnew * tmp - dtsec) >= ConvergenceThreshold && ktr < MaxIter)
        {
            var xoldsqrd = xold * xold;
            znew = xoldsqrd * alpha;

            // ------------- find c2 and c3 functions --------------
            OrbitUtils.FindC2C3(znew, out c2New, out c3New);

            // ------- use a newton iteration for new values -------
            var rval = xoldsqrd * c2New + rdotv * tmp * xold * (1.0 - znew * c3New) +
                       magro * (1.0 - znew * c2New);
            dtnew = xoldsqrd * xold * c3New + rdotv * tmp * xoldsqrd * c2New +
                    magro * xold * (1.0 - znew * c3New);

            // ------------- calculate new value for x -------------
            xnew = xold + (dtsec * Math.Sqrt(mu) - dtnew) / rval;

            // ----- check if the univ param goes negative. if so, use bissection
            if (xnew < 0.0)
                xnew = xold * 0.5;

            ktr++;
            xold = xnew;
        } // while

        if (ktr >= MaxIter)
        {
            // Mitigation: return false, calling side will fix this
            rNew = Vector3.zero;
            vNew = Vector3.zero;
            return false;
        }

        // --- find position and velocity vectors at new time --
        var xnewsqrd = xnew * xnew;
        var f = 1.0 - (xnewsqrd * c2New / magro);
        var g = dtsec - xnewsqrd * xnew * c3New / Math.Sqrt(mu);
        rNew = (f * r0 + g * v0).ToVector3();
        var magr = Math.Sqrt(rNew[0] * rNew[0] + rNew[1] * rNew[1] + rNew[2] * rNew[2]);
        var gdot = 1.0 - (xnewsqrd * c2New / magr);
        var fdot = (Math.Sqrt(mu) * xnew / (magro * magr)) * (znew * c3New - 1.0);
        temp = f * gdot - fdot * g;
        //if (Math.Abs(temp - 1.0) > 0.00001)
        //    Debug.LogWarning(string.Format("consistency check failed {0}", (temp - 1.0)));
        vNew = (fdot * r0 + gdot * v0).ToVector3();
        // Add centerPos to value we ref back
        rNew += centerPosLast;
        // update velocity
        vNew += centerVelLast;

        // if fabs
        return true;
    }
    
    
    private void EvolveRecilinearUnbound(double t, double sme, out Vector3 rNew, out Vector3 vNew)
    {
        var a = -0.5 * mu / sme;
        var v =  Math.Sqrt(mu / (a * a));
        // HACK to slow down things. Might be an error in Roy (vs "About the rectilinear Kepler motion" paper)
        v *= 0.5;
        var aPos = Math.Abs(a);
        // TODO: (opt) Can be done when r0 is set
        // NB: Added a V to scale tau so that recover correct M at t=0. (time0 was already subtracted before t was passed in)
        // Find M0 value at t=0 based on r0, v0
        // r = a[cosh(F)-1]
        var f0 = MathUtils.Acosh(r0.magnitude / aPos + 1);
        var m0 = MathUtils.Sinh(f0) - f0;
        // fix sign based on velocity align
        var dotrv = Vector3d.Dot(r0, v0);
        if (double.IsNormal(dotrv))
        {
            m0 *= Math.Sign(dotrv);
        }
        var m = v * t + m0;
        // Newton's root finder
        var i = 0;
        var u = m;
        double uNext = 0;
        while (i++ < MaxIter)
        {
            uNext = u + (m - (MathUtils.Sinh(u) - u)) / (MathUtils.Cosh(u)-1);
            if (Math.Abs(uNext - u) < ConvergenceThreshold)
                break;
            u = uNext;
        }
        if (i >= MaxIter)
        {
            Debug.LogWarning("Did not converge");
        }
        // E is from center of hyperbola, not focus
        var r = aPos * (MathUtils.Cosh(u)-1);
        rNew = (r0.normalized * r).ToVector3();
        // velocity (Roy eqn (4.106): r rdot = a^(1/2) mu^(1/2) sin(E)
        var rdot = Math.Sqrt(a * mu) * MathUtils.Sinh(u) / r;
        vNew = (r0.normalized * rdot).ToVector3();
    }
}