using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


[System.Serializable]
public struct OrbitalBody
{
    public int index;
    public OrbitalData orbitalData;
    public PlanetaryData planetaryData;

    public Vector3D nextForceVector;

    public OrbitalBody(OrbitalBodySO so, int index)
    {
        this.index = index;
        orbitalData = new OrbitalData(so);
        planetaryData = new PlanetaryData(so);
        nextForceVector = Vector3D.zero;
    }

    //Calculates all the forces that this body experiences from every other body
    public void CalculateForces(NativeArray<OrbitalBody> orbitalBodies)
    {
        Vector3D force = Vector3D.zero;
        for (int i = 0; i < orbitalBodies.Length; i++)
        {
            if (i == index) { continue; }
            force += NBodyPhysics.CalculateForceOfGravity(orbitalData.position, planetaryData.mass, orbitalBodies[i].orbitalData.position, orbitalBodies[i].planetaryData.mass);
        }
        nextForceVector = force;
    }

    //Applies the calculated force
    //Seperate function so you can caluclate the force that every body experiences and then update the bodies position after all forces have been calculated
    public void ApplyForces(float time)
    {
        double acceleration = nextForceVector.magnitude / planetaryData.mass;
        //Using Normilized NextForceVector to turn it into Vector from Scalar
        orbitalData.velocity += acceleration * time * nextForceVector.normilized;
        orbitalData.position += orbitalData.velocity * time;
    }
}