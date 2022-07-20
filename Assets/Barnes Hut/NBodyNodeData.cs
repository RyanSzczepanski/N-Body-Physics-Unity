using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[System.Serializable]
public struct NBodyNodeData
{
    public bool hasPlanet;
    public OrbitalBody orbitalBody;
    public Vector3D centerOfMass;
    public double mass;
}