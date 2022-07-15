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

    public OrbitalBody(OrbitalData orbitalData, PlanetaryData planetaryData, int index)
    {
        this.index = index;
        this.orbitalData = orbitalData;
        this.planetaryData = planetaryData;
        nextForceVector = Vector3D.zero;
    }

    public void CalculateForces(OrbitalBody[] orbitalBodies, bool useJobs)
    {
        if (useJobs)
        {
            NativeArray<Vector3D> orbitalBodyPositions = new NativeArray<Vector3D>(orbitalBodies.Length, Allocator.Temp);
            NativeArray<double> orbitalBodyMasses = new NativeArray<double>(orbitalBodies.Length, Allocator.Temp);
            NativeArray<Vector3D> forceOut = new NativeArray<Vector3D>(1, Allocator.Temp);
            for (int i = 0; i < orbitalBodies.Length; i++)
            {
                orbitalBodyPositions[i] = orbitalBodies[i].orbitalData.position;
                orbitalBodyMasses[i] = orbitalBodies[i].planetaryData.mass;
            }

            CalculateForcesJob calculateForceJob = new CalculateForcesJob()
            {
                output = forceOut,
                orbitalBodyPositions = orbitalBodyPositions,
                orbitalBodyMasses = orbitalBodyMasses,
                originBodyIndex = index,
            };

            JobHandle dependecy = new JobHandle();
            JobHandle scheduleDependency = calculateForceJob.Schedule(orbitalBodies.Length, dependecy);
            JobHandle scheduleParallelJob = calculateForceJob.ScheduleParallel(orbitalBodies.Length, 64, scheduleDependency);

            scheduleParallelJob.Complete();

            nextForceVector = calculateForceJob.output[0];

            orbitalBodyPositions.Dispose();
            orbitalBodyMasses.Dispose();
            forceOut.Dispose();

            //Debug.Log(planetaryData.name + ": " + nextForceVector);
        }
        else
        {
            Vector3D force = Vector3D.zero;
            Vector3D rotation = Vector3D.zero;
            for (int i = 0; i < orbitalBodies.Length; i++)
            {
                if (i == index) { continue; }
                //force += NBodyPhysics.CalculateForce(orbitalData.position, planetaryData.mass, orbitalBodies[i].orbitalData.position, orbitalBodies[i].planetaryData.mass);
                force = CalculateForce(orbitalBodies[i]);
            }
            //Debug.Log(force);
            nextForceVector = force;
            //Debug.Log(nextForceVector);
            //nextRotationVector = force;
        }
    }

    public void CalculateForces(OrbitalBody[] orbitalBodies)
    {
        Vector3D force = Vector3D.zero;
        Vector3D rotation = Vector3D.zero;
        for (int i = 0; i < orbitalBodies.Length; i++)
        {
            if (i == index) { continue; }
            //force += NBodyPhysics.CalculateForce(orbitalData.position, planetaryData.mass, orbitalBodies[i].orbitalData.position, orbitalBodies[i].planetaryData.mass);
            force = CalculateForce(orbitalBodies[i]);
        }
        //Debug.Log(force);
        nextForceVector = force;
        //Debug.Log(nextForceVector);
        //nextRotationVector = force;
    }

    public void CalculateForces(NativeArray<OrbitalBody> orbitalBodies)
    {
        Vector3D force = Vector3D.zero;
        Vector3D rotation = Vector3D.zero;
        for (int i = 0; i < orbitalBodies.Length; i++)
        {
            if (i == index) { continue; }
            force += CalculateForce(orbitalBodies[i]);
        }
        //Debug.Log(force);
        nextForceVector = force;
        //Debug.Log(nextForceVector);
        //nextRotationVector = force;
    }


    public Vector3D CalculateForce(OrbitalBody orbitalBody)
    {
        Vector3D direction = (orbitalBody.orbitalData.position - orbitalData.position).normilized;
        double force = NBodySimulation.G * (planetaryData.mass * orbitalBody.planetaryData.mass / (Vector3D.Distance(orbitalData.position, orbitalBody.orbitalData.position) * Vector3D.Distance(orbitalData.position, orbitalBody.orbitalData.position)));
        return force * direction;
    }

    //public void ApplyForces()
    //{
    //    double acceleration = nextForceVector.magnitude / planetaryData.mass;
    //    orbitalData.velocity += acceleration * Time.deltaTime * nextForceVector.normilized;
    //    orbitalData.position += orbitalData.velocity * Time.deltaTime;
    //}

    public void ApplyForces(float time)
    {
        double acceleration = nextForceVector.magnitude / planetaryData.mass;
        orbitalData.velocity += acceleration * time * nextForceVector.normilized;
        orbitalData.position += orbitalData.velocity * time;
    }

    //public void Draw(float drawScale)
    //{
    //    body.position = position.ToVector3() / drawScale;
    //}
}

[System.Serializable]
public struct OrbitalData
{
    public Vector3D position;
    public Vector3D velocity;

    public double orbitalRadius;
    public double orbitalVelocity;
    public double inclination;
    public float eccentricity;

    public OrbitalData(OrbitalBodySO so)
    {
        position = so.GetAbsolutePositionVector();
        velocity = so.GetAbsoluteVelocityVector();

        orbitalRadius = so.orbitalRadius;
        orbitalVelocity = so.orbitalVelocity;
        inclination = so.inclination;
        eccentricity = so.eccentricity;
    }
}

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
