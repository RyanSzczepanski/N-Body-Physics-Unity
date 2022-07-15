using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NBodyPhysics : MonoBehaviour
{
    public static Vector3D CalculateForce(Vector3D originBodyPos, double originBodyMass, Vector3D actingBodyPos, double actingBodyMass)
    {
        Vector3D direction = (actingBodyPos - originBodyPos).normilized;
        double force = NBodySimulation.G * (originBodyMass * actingBodyMass / (Vector3D.Distance(originBodyPos, actingBodyPos) * Vector3D.Distance(originBodyPos, actingBodyPos)));
        return force * direction;
    }
}
