using System;

public class OrbitUtils
{
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