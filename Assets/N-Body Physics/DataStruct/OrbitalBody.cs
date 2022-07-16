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

    //Calculates all the forces that this body experiences from every other body
    public void CalculateForcesJob(NativeArray<OrbitalBody> orbitalBodies)
    {
        NativeArray<Vector3D> forceOut = new NativeArray<Vector3D>(1, Allocator.TempJob);

        CalculateForcesJob calculateForceJob = new CalculateForcesJob()
        {
            output = forceOut,
            orbitalBodies = orbitalBodies,
            originBodyIndex = index,
        };
        JobHandle jobHandle = calculateForceJob.Schedule(orbitalBodies.Length, 1);
        jobHandle.Complete();

        nextForceVector = calculateForceJob.output[0];

        forceOut.Dispose();
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


//JOB
//Calculates all the forces that this body experiences from every other body
[BurstCompile(CompileSynchronously = false)]
public struct CalculateForcesJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3D> output;
    [NativeDisableParallelForRestriction]
    public NativeArray<OrbitalBody> orbitalBodies;
    public int originBodyIndex;

    public void Execute(int index)
    {
        //Gaurd Clause: dont calcualte for of an object on its self
        if (originBodyIndex == index) { return; }
        output[0] += NBodyPhysics.CalculateForceOfGravity(orbitalBodies[originBodyIndex].orbitalData.position, orbitalBodies[originBodyIndex].planetaryData.mass, orbitalBodies[index].orbitalData.position, orbitalBodies[index].planetaryData.mass);
    }
}