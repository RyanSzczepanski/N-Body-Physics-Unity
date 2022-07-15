using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


[System.Serializable]
public class OrbitalBody
{
    public const double G = 0.00000000006673;

    public OrbitalBodySO orbitalBodyData;

    [SerializeField] Vector3D position;
    [SerializeField] Vector3D currentVelocityVector;
    public Vector3D nextForceVector;

    public Transform body;

    public void Initialize()
    {
        currentVelocityVector = orbitalBodyData.GetAbsoluteVelocityVector();
        position = orbitalBodyData.GetAbsolutePositionVector();

        //Debug.Log($"{orbitalBodyData.name}: {position.ToString()}, {currentVelocityVector.ToString()}");
    }

    public void CalculateForces(OrbitalBody[] orbitalBodies, bool useJobs)
    {
        Vector3D force = Vector3D.zero;
        Vector3D rotation = Vector3D.zero;
        foreach (OrbitalBody orbitalBody in orbitalBodies)
        {
            if (orbitalBody == this) { continue; }
            force += CalculateForce(orbitalBody);
        }

        nextForceVector = force;
        //nextRotationVector = force;
    }


    public Vector3D CalculateForce(OrbitalBody orbitalBody)
    {
        Vector3D direction = (orbitalBody.position - position).normilized;
        double force = G * (orbitalBodyData.mass * orbitalBody.orbitalBodyData.mass / (Vector3D.Distance(position, orbitalBody.position) * Vector3D.Distance(position, orbitalBody.position)));
        return force * direction;
    }

    public static Vector3D CalculateForce(Vector3D originBodyPos, double originBodyMass, Vector3D actingBodyPos, double actingBodyMass)
    {
        Vector3D direction = (actingBodyPos - originBodyPos).normilized;
        double force = G * (originBodyMass * actingBodyMass / (Vector3D.Distance(originBodyPos, actingBodyPos) * Vector3D.Distance(originBodyPos, actingBodyPos)));
        return force * direction;
    }

    public void ApplyForces()
    {
        double acceleration = nextForceVector.magnitude / orbitalBodyData.mass;
        currentVelocityVector = acceleration * Time.deltaTime * nextForceVector.normilized + currentVelocityVector;
        position += currentVelocityVector * Time.deltaTime;
        //body.Position = position;
    }

    public void ApplyForces(float time)
    {
        double acceleration = nextForceVector.magnitude / orbitalBodyData.mass;
        currentVelocityVector = acceleration * time * nextForceVector.normilized + currentVelocityVector;
        position += currentVelocityVector * time;
    }

    public void Draw(float drawScale)
    {
        body.position = position.ToVector3() / drawScale;
    }
}


public struct OrbitalData
{
    public Vector3D position;
    public Vector3D currentVelocityVector;
    public Vector3D nextForceVector;

    public OrbitalBodySO orbitalParent;
    public double orbitalRadius;
    public double orbitalVelocity;
    public double inclination;
    public float eccentricity;
    public OrbitalData(OrbitalBodySO so)
    {
        position = Vector3D.zero;
        currentVelocityVector = Vector3D.zero;
        nextForceVector = Vector3D.zero;
        orbitalParent = so.orbitalParent;
        orbitalRadius = so.orbitalRadius;
        orbitalVelocity = so.orbitalVelocity;
        inclination = so.inclination;
        eccentricity = so.eccentricity;
    }
}
