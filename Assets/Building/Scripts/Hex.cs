using System;
using System.Collections.Generic;
using UnityEngine;

public struct Hex
{
    public static readonly Hex zero = new Hex(0, 0, 0);
    public static readonly Hex invalid = new Hex(-100, -100) { s=-100 };
    public static readonly Hex downLeft = new Hex(0, -1, +1);
    public static readonly Hex upRight = new Hex(0, +1, -1);
    public static readonly Hex left = new Hex(-1, 0, +1);
    public static readonly Hex upLeft = new Hex(-1, +1, 0);
    public static readonly Hex downRight = new Hex(1, -1, 0);
    public static readonly Hex right = new Hex(1, 0, -1);
    public static readonly float width = Mathf.Sqrt(3);
    public static readonly float height = 1.5f;

    public static readonly Hex[] neighbours = new[]
        { downLeft, upRight, left, upLeft, downRight, right };

    private int q;
    private int r;
    private int s;

    public int Q => q;
    public int R => r;
    public int S => s;

    public Hex(int q, int r, int s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
#if DEBUG
        CheckValidity();
#endif
    }

    public Hex(int q, int r) : this()
    {
        this.q = q;
        this.r = r;
        s = -q - r;
    }

    public Vector2 ToCartesian()
    {
        return new Vector2( (q + r / 2f) * width, r * height) * RenderingSettings.Instance.side;
    }
    
    public static Hex FromCartesian(Vector2 point)
    {
        point /= RenderingSettings.Instance.side;
        var q = Mathf.Sqrt(3) / 3 * point.x - 1f / 3 * point.y;
        var r = 2f / 3 * point.y;
        return Round(new Vector3(q, r, -q - r));
    }

    public static IEnumerable<Hex> Circle(Hex center, int radius)
    {
        for (var q = -radius; q <= radius; q++)
        {
            for (var r = Mathf.Max(-radius, -q - radius);
                 r <= Mathf.Min(radius, -q + radius);
                 r++)
            {
                yield return new Hex(q, r);
            }
        }
    }

    public static Hex Round(Vector3 qrs)
    {
        var hex = new Hex
        {
            q = Mathf.RoundToInt(qrs.x),
            r = Mathf.RoundToInt(qrs.y),
            s = Mathf.RoundToInt(qrs.z)
        };

        var qDiff = Mathf.Abs(hex.q - qrs.x);
        var rDiff = Mathf.Abs(hex.r - qrs.y);
        var sDiff = Mathf.Abs(hex.s - qrs.z);

        if (qDiff > rDiff && qDiff > sDiff)
        {
            hex.q = -hex.r - hex.s;
        }
        else if (rDiff > sDiff)
        {
            hex.r = -hex.q - hex.s;
        }
        else
        {
            hex.s = -hex.q - hex.r;
        }

        return hex;
    }

    public static Hex operator +(Hex lhs, Hex rhs)
    {
        return new Hex(lhs.q + rhs.q, lhs.r + rhs.r, lhs.s + rhs.s);
    }

    public static Hex operator -(Hex lhs, Hex rhs)
    {
        return new Hex(lhs.q - rhs.q, lhs.r - rhs.r, lhs.s - rhs.s);
    }

    public static Hex operator *(Hex lhs, int rhs)
    {
        return new Hex(lhs.q * rhs, lhs.r * rhs, lhs.s * rhs);
    }
    
    public static Hex operator /(Hex lhs, int rhs)
    {
        return new Hex(lhs.q / rhs, lhs.r / rhs, lhs.s / rhs);
    }

    public static Hex operator -(Hex hex)
    {
        return new Hex(-hex.q, -hex.r, -hex.s);
    }

    public bool Equals(Hex other)
    {
        return q == other.q && r == other.r;
    }

    public override bool Equals(object obj)
    {
        return obj is Hex other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(q, r);
    }

    public static bool operator ==(Hex left, Hex right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Hex left, Hex right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"Hex({q},{r},{s})";
    }

    private void CheckValidity()
    {
        if (q + r + s != 0)
        {
            Debug.LogError("Invalid hex " + this);
        }
    }
}