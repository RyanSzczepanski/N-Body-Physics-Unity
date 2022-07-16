using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PlanetaryData
{
    //public string name;
    public double radius;
    public double mass;

    public PlanetaryData(OrbitalBodySO so)
    {
        //name = so.name;
        radius = so.radius;
        mass = so.mass;
    }
}

