using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Orbital Body", fileName = "New Orbital Body")]
public class OrbitalBodySO : ScriptableObject
{
    public string planetName;
    public double mass;
    public double radius;

    public OrbitalBodySO orbitalParent;

    public double orbitalRadius;
    public double orbitalVelocity;
    public double inclination;
    public float eccentricity;
    double inclinationRads
    {
        get { return inclination * System.Math.PI / 180; }
    }

    public Vector3D GetAbsoluteVelocityVector()
    {
        if(orbitalParent != null)
        {
            Vector3D velocityVector = new Vector3D(0, System.Math.Sin(inclinationRads) * orbitalVelocity, System.Math.Cos(inclinationRads) * orbitalVelocity);
            return velocityVector + orbitalParent.GetAbsoluteVelocityVector();
        }
        return Vector3D.zero;
    }

    public Vector3D GetAbsolutePositionVector()
    {
        if (orbitalParent != null)
        {
            Vector3D cartisianPosition = new Vector3D(System.Math.Cos(inclinationRads) * orbitalRadius, System.Math.Sin(inclinationRads) * orbitalRadius, 0);
            return cartisianPosition + orbitalParent.GetAbsolutePositionVector();
        }
        return Vector3D.zero;
    }

    public bool DoesOrbitAround(OrbitalBodySO targetBody)
    {
        if(orbitalParent == null)
        {
            return false;
        }
        else if(orbitalParent == targetBody)
        {
            return true;
        }
        else
        {
            return orbitalParent.DoesOrbitAround(targetBody);
        }
    }

    public int OrbitalParentLength()
    {
        int i = 0;
        if (orbitalParent is null) { return 0; }
        else { i++; }
        i += orbitalParent.OrbitalParentLength();
        return i;
    }

    public static int CompareOrbitalBody(OrbitalBodySO x, OrbitalBodySO y)
    {
        if(x.orbitalParent is null)
        {
            if(y.orbitalParent is null)
            {
                return 0;
            }
            else if(y.DoesOrbitAround(x))
            {
                return -1;
            }
            else if(x.DoesOrbitAround(y))
            {
                return 1;
            }
            else
            {
                //Can break out but is unreachable scenario with only one solar system
                return 0;
            }
        }
        else
        {
            if (y.orbitalParent is null)
            {
                return 1;
            }
            else if (y.DoesOrbitAround(x))
            {
                return -1;
            }
            else if (x.DoesOrbitAround(y))
            {
                return 1;
            }
            else if (x.orbitalParent == y.orbitalParent)
            {
                //Double moon case or planets around a sun
                //Needs to break out farther
                if (x.orbitalRadius < y.orbitalRadius)
                {
                    return -1;
                }
                else if (x.orbitalRadius > y.orbitalRadius)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                //Comparing planet to other planets moon
                Debug.Log($"{x.OrbitalParentLength()} | {y.OrbitalParentLength()}");
                if (x.OrbitalParentLength() > y.OrbitalParentLength())
                {
                    //X is a moon and y is a planet
                    return CompareOrbitalBody(x.orbitalParent, y);
                }
                else if (x.OrbitalParentLength() < y.OrbitalParentLength())
                {
                    //Y is a moon and x is planet
                    return CompareOrbitalBody(x, y.orbitalParent);
                }
                else
                {
                    //X and Y are both moons of different planetse
                    return CompareOrbitalBody(x.orbitalParent, y.orbitalParent);
                }
            }
        }
        return 0;
    }
}
