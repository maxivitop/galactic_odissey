using System;
using UnityEngine;

[RequireComponent(typeof(FutureRigidBody2D))]
public class EllipticalOrbit : FutureBehaviour, IFuturePositionProvider
{
    public GameObject center;
    public Vector2 initialVelocity;

    private IFuturePositionProvider centerPositionProvider;
    private FutureRigidBody2D rigidBody;
    private FutureTransform futureTransform;
    public int step0;
    public double eccentricity;
    public Vector3 ecc_vec;
    private double orbit_period;
    private double mu;
    //! semi-parameter that defines orbit size in GE internal units. 
    //! For a universal orbit p is needed. See SetMajorAxis() if a is needed.
    public double p = 10.0;
    public double a;

    public Vector3 r0;
    public Vector3 v0;
    private const double small = 1E-6;
    private const double verySmall = 1E-9; // 1E-13;
    private const double halfpi = Math.PI * 0.5;

    
    private void Start()
    {
        centerPositionProvider = IFuturePositionProvider.SelectFuturePositionProvider(center);
        futureTransform = GetComponent<FutureTransform>();
        rigidBody = GetComponent<FutureRigidBody2D>();
        r0 =  transform.position - center.transform.position;
        v0 = initialVelocity;
        mu = FuturePhysics.G * (rigidBody.initialMass + center.GetComponent<FutureRigidBody2D>().initialMass);
        InitializeFromRv(r0, v0);
    }

    public override void VirtualStep(int step)
    {
        futureTransform.GetState(step).position = GetFuturePosition(step, 0f);
    }

    public Vector3 GetFuturePosition(int step, float dt)
    {
        Evolve(step, dt, out var position, out var velocity);
        return position;
    }

    //RVtoCOE
    public void InitializeFromRv(Vector3 r, Vector3 v)
    {
        double  magr, magv, sme, rdotv, temp, c1, magh;

        // -------------------------  implementation   -----------------
        magr = r.magnitude;
        magv = v.magnitude;

        // ------------------  find h n and e vectors   ----------------
        Vector3 hbar = Vector3.Cross(r, v);
        magh = hbar.magnitude;
        if (!(magh > verySmall)) return;
        c1 = magv * magv - mu / magr;
        rdotv = Vector3.Dot(r, v);
        temp = 1.0 / mu;
        Vector3 ebar = new Vector3( (float) ((c1 * r.x - rdotv * v.x) * temp),
            (float) ((c1 * r.y - rdotv * v.y) * temp),
            (float) ((c1 * r.z - rdotv * v.z) * temp));
        ecc_vec = ebar;
        eccentricity = ebar.magnitude;

        // ------------  find a e and semi-latus rectum   ----------
        sme = (magv * magv * 0.5) - (mu / magr);
        // was check vs small, but really care about > 0
        if (Math.Abs(sme) > verySmall)
            a = -mu / (2.0 * sme);
        else
            a = double.NaN;
        p = magh * magh * temp;

        orbit_period = double.NaN;
        if (eccentricity < 1.0) {
            orbit_period = 2.0 * Math.PI * Math.Sqrt(a * a * a / mu);
        }
    }
    
    
    public void Evolve(int step, float dt, out Vector3 r_new, out Vector3 v_new)
    {
        int ktr, numiter;
        double f, g, fdot, gdot, rval, xold, xoldsqrd, xnewsqrd, znew, pp, dtnew, rdotv, a, dtsec, alpha, sme, s, w, temp, magro, magvo, magr;
        double c2new = 0.0;
        double c3new = 0.0;
        double xnew = 0.0;
        double dtseco = (step - step0 + dt) * FuturePhysics.DeltaTime;
        r_new = Vector3.zero; v_new = Vector3.zero;

        // Very large times can cause precision issues, normalize if possible
        if (eccentricity < 1 && dtseco > 1E6) {
            dtseco %= orbit_period;
        }
        
        dtsec = dtseco;

        Vector3 centerPosLast = centerPositionProvider.GetFuturePosition(step, dt);
        Vector3 centerVelLast = Vector3.zero; // TODO
        
        // -------------------------  implementation   -----------------
        // set constants and intermediate printouts
        numiter = 100;

        // --------------------  initialize values   -------------------
        znew = 0.0;

        if (Math.Abs(dtseco) > small) {
            // <TODO>: (performance) put this where r0, v0 are changed and re-use it. 
            magro = r0.magnitude;
            magvo = v0.magnitude;
            rdotv = Vector3.Dot(r0, v0);
            // </TODO>

            // -------------  find sme, alpha, and a  ------------------
            sme = ((magvo * magvo) * 0.5) - (mu / magro);
            alpha = -sme * 2.0 / mu;
            
            if (Math.Abs(sme) > small)
                a = -mu / (2.0 * sme);
            else
                a = double.NaN;

            bool radialInfall = Vector3.Cross(r0.normalized, v0.normalized).magnitude < 1E-3;
            if (radialInfall) {
                if (sme <= 0) {
                    // Not debugged, but normal Kepler handles this case ok
                    // EvolveRecilinearBound(dtsec, sme, ref r_new, ref v_new);
                } else {
                    EvolveRecilinearUnbound(dtsec, sme, ref r_new, ref v_new);
                    // Add centerPos to value we ref back
                    r_new += centerPosLast;
                    // update velocity
                    v_new += centerVelLast;
                    return;
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
                if (Math.Abs(alpha - 1.0) > small)
                    xold = Math.Sqrt(mu) * dtsec * alpha;
                else
                    // - first guess can't be too close. ie a circle, r=a
                    xold = Math.Sqrt(mu) * dtsec * alpha * 0.97;
            } else {
                // --------------------  parabola  ---------------------
                if (Math.Abs(alpha) < small) {
                    Vector3 h = Vector3.Cross(r0, v0);
                    pp = h.sqrMagnitude / mu;
                    s = 0.5 * (halfpi - Math.Atan(3.0 * Math.Sqrt(mu / (pp * pp * pp)) * dtsec));
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
            double tmp = 1.0 / Math.Sqrt(mu);

            while ((Math.Abs(dtnew * tmp - dtsec) >= small) && (ktr < numiter)) {
                xoldsqrd = xold * xold;
                znew = xoldsqrd * alpha;

                // ------------- find c2 and c3 functions --------------
                OrbitUtils.FindC2C3(znew, out c2new, out c3new);

                // ------- use a newton iteration for new values -------
                rval = xoldsqrd * c2new + rdotv * tmp * xold * (1.0 - znew * c3new) +
                    magro * (1.0 - znew * c2new);
                dtnew = xoldsqrd * xold * c3new + rdotv * tmp * xoldsqrd * c2new +
                    magro * xold * (1.0 - znew * c3new);

                // ------------- calculate new value for x -------------
                xnew = xold + (dtsec * Math.Sqrt(mu) - dtnew) / rval;

                // ----- check if the univ param goes negative. if so, use bissection
                if (xnew < 0.0)
                    xnew = xold * 0.5;

                ktr++;
                xold = xnew;
            }  // while

            if (ktr >= numiter) {
                Debug.LogWarning(string.Format("{0} not converged in {1} iterations. dtnew={2} tmp={3} dto={4} expr={5}",
                    "gameObject.name", numiter, dtnew, tmp, dtsec, Math.Abs(dtnew * tmp - dtsec)));
                Debug.LogFormat("ecc={0} p={1}", eccentricity, p);
                // Mitigation: use last known position
                Debug.LogError("Didn't fix ge.GetPositionDouble(nbody, ref r_new)");
                // ge.GetPositionDouble(nbody, ref r_new);
            } else {
                // --- find position and velocity vectors at new time --
                xnewsqrd = xnew * xnew;
                f = 1.0 - (xnewsqrd * c2new / magro);
                g = dtsec - xnewsqrd * xnew * c3new / Math.Sqrt(mu);
                r_new = (float)f * r0 + (float)g * v0; // TODO(double)
                magr = Math.Sqrt(r_new[0] * r_new[0] + r_new[1] * r_new[1] + r_new[2] * r_new[2]);
                gdot = 1.0 - (xnewsqrd * c2new / magr);
                fdot = (Math.Sqrt(mu) * xnew / (magro * magr)) * (znew * c3new - 1.0);
                temp = f * gdot - fdot * g;
                //if (Math.Abs(temp - 1.0) > 0.00001)
                //    Debug.LogWarning(string.Format("consistency check failed {0}", (temp - 1.0)));
                v_new = (float) fdot * r0 + (float) gdot * v0; // TODO(double)
                // Add centerPos to value we ref back
                r_new += centerPosLast;
                // update velocity
                v_new += centerVelLast;
            }
        } // if fabs
        else {
            // ----------- set vectors to incoming since 0 time --------
            r_new = r0;
            // Add centerPos to value we ref back
            r_new += centerPosLast;
            v_new = v0 + centerVelLast;
        }
    }
    
    
    private void EvolveRecilinearUnbound(double t, double sme, ref Vector3 r_new, ref Vector3 v_new)
    {
        double eta = sme;
        double a = -0.5 * mu / eta;
        double V =  Math.Sqrt(mu / (a * a));
        // HACK to slow down things. Might be an error in Roy (vs "About the rectilinear Kepler motion" paper)
        V *= 0.5;
        double a_pos = Math.Abs(a);
        // TODO: (opt) Can be done when r0 is set
        // NB: Added a V to scale tau so that recover correct M at t=0. (time0 was already subtracted before t was passed in)
        // Find M0 value at t=0 based on r0, v0
        // r = a[cosh(F)-1]
        double F0 = MathUtils.Acosh(r0.magnitude / a_pos + 1);
        double M0 = MathUtils.Sinh(F0) - F0;
        // fix sign based on velocity align
        M0 *= Math.Sign(Vector3.Dot(r0, v0));
        double M = V * t + M0;
        // Newton's root finder
        int i = 0;
        double u = M;
        double u_next = 0;
        while (i++ < 1000)
        {
            u_next = u + (M - (MathUtils.Sinh(u) - u)) / (MathUtils.Cosh(u)-1);
            if (Math.Abs(u_next - u) < 1E-6)
                break;
            u = u_next;
        }
        if (i >= 100)
        {
            Debug.LogWarning("Did not converge");
        }
        // E is from center of hyperbola, not focus
        double r = a_pos * (MathUtils.Cosh(u)-1);
        r_new = r0.normalized * (float)r; //TODO(double)
        // velocity (Roy eqn (4.106): r rdot = a^(1/2) mu^(1/2) sin(E)
        double rdot = Math.Sqrt(a * mu) * MathUtils.Sinh(u) / r;
        v_new = r0.normalized * (float)rdot; //TODO(double)
    }


    public int GetPriority()
    {
        return 1010;
    }
}