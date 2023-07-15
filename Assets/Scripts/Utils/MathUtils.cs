using System;

public class MathUtils
{
    /// <summary>
    /// Cotangent function. 
    /// 
    /// Adapted from Vallado's astmath CPP functions. 
    /// 
    /// </summary>
    public static double Cot(double x) {
        double temp = Math.Tan(x);
        if (Math.Abs(temp) < 1E-8) {
            return double.NaN;
        } else {
            return 1.0 / temp;
        }
    }

    public static double Acosh(double x) {
        return Math.Log(x + Math.Sqrt(x * x - 1));
    }

    public static double Asinh(double x) {
        return Math.Log(x + Math.Sqrt(x * x + 1));
    }

    public static double Sinh(double x) {
        return 0.5*(Math.Exp(x) - Math.Exp(-x));
    }

    public static double Cosh(double x) {
        return 0.5 * (Math.Exp(x) + Math.Exp(-x));
    }
}