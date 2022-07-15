using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct NBodyPhysics
{
    public static OrbitalData ApplyForces(OrbitalBody orbitalBody, float time)
    {
        double acceleration = orbitalBody.nextForceVector.magnitude / orbitalBody.planetaryData.mass;
        orbitalBody.orbitalData.velocity += acceleration * time * orbitalBody.nextForceVector.normilized;
        orbitalBody.orbitalData.position += orbitalBody.orbitalData.velocity * time;
        return orbitalBody.orbitalData;
    }

    public static void ApplyForces(ref OrbitalBody orbitalBody, float time)
    {
        double acceleration = orbitalBody.nextForceVector.magnitude / orbitalBody.planetaryData.mass;
        orbitalBody.orbitalData.velocity += acceleration * time * orbitalBody.nextForceVector.normilized;
        orbitalBody.orbitalData.position += orbitalBody.orbitalData.velocity * time;
    }

    public static void CalculateForces(int originBodyIndex, ref OrbitalBody[] orbitalBodies)
    {
        Vector3D force = Vector3D.zero;
        Vector3D rotation = Vector3D.zero;
        for (int i = 0; i < orbitalBodies.Length; i++)
        {
            if (i == originBodyIndex) { continue; }
            force += NBodyPhysics.CalculateForce(orbitalBodies[originBodyIndex].orbitalData.position, orbitalBodies[originBodyIndex].planetaryData.mass, orbitalBodies[i].orbitalData.position, orbitalBodies[i].planetaryData.mass);

        }
        orbitalBodies[originBodyIndex].nextForceVector = force;
    }

    public static Vector3D CalculateForce(Vector3D originBodyPos, double originBodyMass, Vector3D actingBodyPos, double actingBodyMass)
    {
        Vector3D direction = (actingBodyPos - originBodyPos).normilized;
        double force = NBodySimulation.G * (originBodyMass * actingBodyMass / (Vector3D.Distance(originBodyPos, actingBodyPos) * Vector3D.Distance(originBodyPos, actingBodyPos)));
        return force * direction;
    }
}


public struct CalculateForcesJob : IJobFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3D> output;
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3D> orbitalBodyPositions;
    [NativeDisableParallelForRestriction]
    public NativeArray<double> orbitalBodyMasses;
    public int originBodyIndex;
    public void Execute(int index)
    {
        if (originBodyIndex == index) { return; }
        Vector3D direction = (orbitalBodyPositions[index] - orbitalBodyPositions[originBodyIndex]).normilized;
        double force = NBodySimulation.G * (orbitalBodyMasses[originBodyIndex] * orbitalBodyMasses[index] /
            (Vector3D.Distance(orbitalBodyPositions[originBodyIndex], orbitalBodyPositions[index]) * Vector3D.Distance(orbitalBodyPositions[originBodyIndex], orbitalBodyPositions[index])));
        output[0] += force * direction;
    }
}
