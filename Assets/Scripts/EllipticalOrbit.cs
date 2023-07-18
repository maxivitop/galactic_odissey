using System;
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
    private const double VerySmall = 1E-9; // 1E-13;
    private const double HalfPi = Math.PI * 0.5;

    public EllipticalOrbit(GravitySource center, double myMass, int step0, Vector3d r0, Vector3d v0)
    {
        this.center = center;
        this.step0 = step0;
        this.r0 = r0;
        this.v0 = v0;
        mu = FuturePhysics.G * (myMass + center.futureRigidBody2D.initialMass);
        InitializeFromRv();
    }

    //RVtoCOE
    private void InitializeFromRv()
    {
        double  magr, magv, rdotv, temp, c1, magh;

        // -------------------------  implementation   -----------------
        magr = r0.magnitude;
        magv = v0.magnitude;

        // ------------------  find h n and e vectors   ----------------
        var hbar = Vector3d.Cross(r0, v0);
        magh = hbar.magnitude;
        if (!(magh > VerySmall)) return;
        c1 = magv * magv - mu / magr;
        rdotv = Vector3d.Dot(r0, v0);
        temp = 1.0 / mu;
        var ebar = new Vector3d( ((c1 * r0.x - rdotv * v0.x) * temp),
            ((c1 * r0.y - rdotv * v0.y) * temp),
            ((c1 * r0.z - rdotv * v0.z) * temp));
        eccentricity = ebar.magnitude;

        // ------------  find a e and semi-latus rectum   ----------
        p = magh * magh * temp;
    }
    
    
    public bool Evolve(int step, double dt, out Vector3d rNew, out Vector3d vNew)
    {
        int ktr, numiter;
        double f, g, fdot, gdot, rval, xold, xoldsqrd, xnewsqrd, znew, pp, dtnew, rdotv, a, dtsec, alpha, sme, s, w, temp, magro, magvo, magr;
        var c2New = 0.0;
        var c3New = 0.0;
        var xnew = 0.0;
        var dtseco = (step - step0 + dt) * FuturePhysics.DeltaTime;

        magro = r0.magnitude;
        magvo = v0.magnitude;
        rdotv = Vector3d.Dot(r0, v0);

        // -------------  find sme, alpha, and a  ------------------
        sme = ((magvo * magvo) * 0.5) - (mu / magro);
        alpha = -sme * 2.0 / mu;
            
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
        
        dtsec = dtseco;

        var centerPosLast = center.futurePositionProvider.GetFuturePosition(step, dt);
        Vector3d centerVelLast = center.futureRigidBody2D.GetState(step).velocity;
        
        // -------------------------  implementation   -----------------
        // set constants and intermediate printouts
        numiter = 100;

        // --------------------  initialize values   -------------------
        znew = 0.0;

        if (Math.Abs(dtseco) > Small) {
            var radialInfall = Vector3d.Cross(r0.normalized, v0.normalized).magnitude < 1E-3;
            if (radialInfall) {
                if (sme <= 0) {
                    // Not debugged, but normal Kepler handles this case ok
                    // EvolveRecilinearBound(dtsec, sme, ref r_new, ref v_new);
                } else {
                    EvolveRecilinearUnbound(dtsec, sme, out rNew, out vNew);
                    // Add centerPos to value we ref back
                    rNew += centerPosLast;
                    // update velocity
                    vNew += centerVelLast;
                    return true;
                }
            }

            // This check breaks the case where SI units are used for the Solar System 
            // (not the recommended choice of units, but more likely than an exactly parabolic Kepler mode)
            // Likewise for check of alpha below. 
            //if (Math.Abs(alpha) < small)   // parabola
            //    alpha = 0.0;

            // ------------   setup initial guess for x  ---------------
            // -----------------  circle and ellipse -------------------
            // if (alpha >= small) {
            if (alpha >= 0) {
                if (Math.Abs(alpha - 1.0) > Small)
                    xold = Math.Sqrt(mu) * dtsec * alpha;
                else
                    // - first guess can't be too close. ie a circle, r=a
                    xold = Math.Sqrt(mu) * dtsec * alpha * 0.97;
            } else {
                // --------------------  parabola  ---------------------
                if (Math.Abs(alpha) < Small) {
                    var h = Vector3d.Cross(r0, v0);
                    pp = h.sqrMagnitude / mu;
                    s = 0.5 * (HalfPi - Math.Atan(3.0 * Math.Sqrt(mu / (pp * pp * pp)) * dtsec));
                    w = Math.Atan(Math.Pow(Math.Tan(s), (1.0 / 3.0)));
                    xold = Math.Sqrt(p) * (2.0 * MathUtils.Cot(2.0 * w));
                    alpha = 0.0;
                } else {
                    // ------------------  hyperbola  ------------------
                    temp = -2.0 * mu * dtsec /
                        (a * (rdotv + Math.Sign(dtsec) * Math.Sqrt(-mu * a) * (1.0 - magro * alpha)));
                    xold = Math.Sign(dtsec) * Math.Sqrt(-a) * Math.Log(temp);
                }
            } // if alpha

            ktr = 1;
            dtnew = -10.0;
            // conv for dtsec to x units
            var tmp = 1.0 / Math.Sqrt(mu);

            while (Math.Abs(dtnew * tmp - dtsec) >= Small && ktr < numiter) {
                xoldsqrd = xold * xold;
                znew = xoldsqrd * alpha;

                // ------------- find c2 and c3 functions --------------
                OrbitUtils.FindC2C3(znew, out c2New, out c3New);

                // ------- use a newton iteration for new values -------
                rval = xoldsqrd * c2New + rdotv * tmp * xold * (1.0 - znew * c3New) +
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
            }  // while

            if (ktr >= numiter) {
                // Mitigation: return false, calling side will fix this
                rNew = Vector3d.zero;
                vNew = Vector3d.zero;
                return false;
            } 
            // --- find position and velocity vectors at new time --
            xnewsqrd = xnew * xnew;
            f = 1.0 - (xnewsqrd * c2New / magro);
            g = dtsec - xnewsqrd * xnew * c3New / Math.Sqrt(mu);
            rNew = f * r0 + g * v0;
            magr = Math.Sqrt(rNew[0] * rNew[0] + rNew[1] * rNew[1] + rNew[2] * rNew[2]);
            gdot = 1.0 - (xnewsqrd * c2New / magr);
            fdot = (Math.Sqrt(mu) * xnew / (magro * magr)) * (znew * c3New - 1.0);
            temp = f * gdot - fdot * g;
            //if (Math.Abs(temp - 1.0) > 0.00001)
            //    Debug.LogWarning(string.Format("consistency check failed {0}", (temp - 1.0)));
            vNew = fdot * r0 + gdot * v0;
            // Add centerPos to value we ref back
            rNew += centerPosLast;
            // update velocity
            vNew += centerVelLast;
        
        } // if fabs
        else {
            // ----------- set vectors to incoming since 0 time --------
            rNew = r0;
            // Add centerPos to value we ref back
            rNew += centerPosLast;
            vNew = v0 + centerVelLast;
        }

        return true;
    }
    
    
    private void EvolveRecilinearUnbound(double t, double sme, out Vector3d rNew, out Vector3d vNew)
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
        while (i++ < 1000)
        {
            uNext = u + (m - (MathUtils.Sinh(u) - u)) / (MathUtils.Cosh(u)-1);
            if (Math.Abs(uNext - u) < 1E-6)
                break;
            u = uNext;
        }
        if (i >= 100)
        {
            Debug.LogWarning("Did not converge");
        }
        // E is from center of hyperbola, not focus
        var r = aPos * (MathUtils.Cosh(u)-1);
        rNew = r0.normalized * r;
        // velocity (Roy eqn (4.106): r rdot = a^(1/2) mu^(1/2) sin(E)
        var rdot = Math.Sqrt(a * mu) * MathUtils.Sinh(u) / r;
        vNew = r0.normalized * rdot;
    }
}