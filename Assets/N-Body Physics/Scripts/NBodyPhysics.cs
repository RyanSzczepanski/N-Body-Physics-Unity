using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct NBodyPhysics
{
    public const double G = 0.00000000006673;

    public static Vector3D CalculateForceOfGravity(Vector3D originBodyPos, double originBodyMass, Vector3D actingBodyPos, double actingBodyMass)
    {
        //Get the direction of the force 
        Vector3D direction = (actingBodyPos - originBodyPos).normilized;
        //Force Equation | F=G*M1*M2/(R^2)
        double force = G * (originBodyMass * actingBodyMass / (Vector3D.Distance(originBodyPos, actingBodyPos) * Vector3D.Distance(originBodyPos, actingBodyPos)));
        return force * direction;
    }
}
