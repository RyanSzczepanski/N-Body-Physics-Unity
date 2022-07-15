using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class NBodySimulation : MonoBehaviour
{
    public const double G = 0.00000000006673;

    public bool useJobs;
    public OrbitalBodySO[] orbitalBodiesSO;
    [SerializeField] private OrbitalBody[] orbitalBodies;
    public GameObject[] orbitalBodyObjects;
    public float drawScale;
    public float lossLessTimeScale;
    public float lossyTimeScale;


    private void Awake()
    {
        Application.targetFrameRate = -1;
        orbitalBodies = new OrbitalBody[orbitalBodiesSO.Length];
        for (int i = 0; i < orbitalBodiesSO.Length; i++)
        {
            orbitalBodies[i] = new OrbitalBody(orbitalBodiesSO[i], i);
        }
    }

    private void Start()
    {

    }

    private void Update()
    {
        for (int i = 0; i < lossLessTimeScale; i++)
            UpdateSystem();

        for (int i = 0; i < orbitalBodies.Length; i++)
            DrawBody(i);
    }

    private void UpdateSystem()
    {
        for (int i = 0; i < orbitalBodies.Length; i++)
            orbitalBodies[i].CalculateForces(orbitalBodies, useJobs);
        //foreach (OrbitalBody orbitalBody in orbitalBodies)
        //    orbitalBody.CalculateForces(orbitalBodies, useJobs);

        for (int i = 0; i < orbitalBodies.Length; i++)
            orbitalBodies[i].ApplyForces(lossyTimeScale * Time.deltaTime);

    }

    private void DrawBody(int i)
    {
        orbitalBodyObjects[i].transform.position = (orbitalBodies[i].orbitalData.position / drawScale).ToVector3();
    }
}