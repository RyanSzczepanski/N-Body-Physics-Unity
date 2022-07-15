using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class NBodySimulation : MonoBehaviour
{
    public bool useJobs;

    public OrbitalBody[] orbitalBodies;
    public float drawScale;
    public float lossLessTimeScale;
    public float lossyTimeScale;


    private void Awake()
    {
        Application.targetFrameRate = -1;
        foreach (OrbitalBody orbitalBody in orbitalBodies)
        {
            orbitalBody.Initialize();
        }
    }

    private void Start()
    {

    }

    private void Update()
    {
        for (int i = 0; i < lossLessTimeScale; i++)
            UpdateSystem();

        foreach (OrbitalBody orbitalBody in orbitalBodies)
            orbitalBody.Draw(drawScale);
    }

    private void UpdateSystem()
    {
        foreach (OrbitalBody orbitalBody in orbitalBodies)
            orbitalBody.CalculateForces(orbitalBodies, useJobs);

        foreach (OrbitalBody orbitalBody in orbitalBodies)
            orbitalBody.ApplyForces(lossyTimeScale * Time.deltaTime);

    }
}