using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Vector3D 
{
    public double x, y, z;

    static public Vector3D zero
    {
        get
        {
            return new Vector3D(0, 0, 0);
        }
    }

    public static Vector3D operator + (Vector3D lhs, Vector3D rhs)
    {
        return new Vector3D(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
    }

    public static Vector3D operator - (Vector3D lhs, Vector3D rhs)
    {
        return new Vector3D(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
    }

    public static Vector3D operator * (Vector3D lhs, double rhs)
    {
        return new Vector3D(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs);
    }

    public static Vector3D operator * (double lhs, Vector3D rhs)
    {
        return new Vector3D(lhs * rhs.x, lhs * rhs.y, lhs * rhs.z);
    }

    public static Vector3D operator / (Vector3D lhs, double rhs)
    {
        return new Vector3D(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs);
    }

    public static bool operator == (Vector3D lhs, Vector3D rhs)
    {
        return (lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z);
    }

    public static bool operator != (Vector3D lhs, Vector3D rhs)
    {
        return !(lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z);
    }

    public double magnitude
    {
        get
        {
            return System.Math.Sqrt(x * x + y * y + z * z);
        } 
    }

    public Vector3D normilized
    {
        get
        {
            return this / magnitude;
        }
    }

    public static double Distance(Vector3D a, Vector3D b)
    {
        return (a - b).magnitude;
    }

    public override string ToString()
    {
        return $"X:{x}, Y:{y}, Z:{z}";
    }
    public Vector3 ToVector3()
    {
        return new Vector3((float)x, (float)y, (float)z);
    }

    public Vector3D(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3D(Vector3 V)
    {
        this.x = V.x;
        this.y = V.y;
        this.z = V.z;
    }
}
