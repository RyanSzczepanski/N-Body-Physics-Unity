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
    int index;
    public OrbitalData orbitalData;
    public PlanetaryData planetaryData;

    [SerializeField] private Vector3D nextForceVector;

    public OrbitalBody(OrbitalBodySO so, int index)
    {
        this.index = index;
        orbitalData = new OrbitalData(so);
        planetaryData = new PlanetaryData(so);
        nextForceVector = Vector3D.zero;
    }

    public void CalculateForces(OrbitalBody[] orbitalBodies, bool useJobs)
    {
        Vector3D force = Vector3D.zero;
        Vector3D rotation = Vector3D.zero;
        for (int i = 0; i < orbitalBodies.Length; i++)
        {
            if (i == index) { continue; }
            force += NBodyPhysics.CalculateForce(orbitalData.position, planetaryData.mass, orbitalBodies[i].orbitalData.position, orbitalBodies[i].planetaryData.mass);
        }
        //Debug.Log(force);
        nextForceVector = force;
        //nextRotationVector = force;
    }


    //public Vector3D CalculateForce(OrbitalBody orbitalBody)
    //{
    //    Vector3D direction = (orbitalBody.position - position).normilized;
    //    double force = NBodySimulation.G * (orbitalBodyData.mass * orbitalBody.orbitalBodyData.mass / (Vector3D.Distance(position, orbitalBody.position) * Vector3D.Distance(position, orbitalBody.position)));
    //    return force * direction;
    //}

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
    public string name;
    public double radius;
    public double mass;

    public PlanetaryData(OrbitalBodySO so)
    {
        name = so.name;
        radius = so.radius;
        mass = so.mass;
    }
}
