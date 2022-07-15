using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;

public class NBodySimulation : MonoBehaviour
{
    public const double G = 0.00000000006673;

    public int n;

    public bool useBurstCompiler;
    //public bool useBurst;

    public GameObject Prefab;

    public OrbitalBodySO[] orbitalBodiesSO;
    private NativeArray<OrbitalBody> orbitalBodies;
    public GameObject[] orbitalBodyObjects;
    public float drawScale;
    public int lossLessTimeScale;
    public int lossyTimeScale;
    JobHandle jobHandle;

    private void Awake()
    {
        Application.targetFrameRate = -1;
        orbitalBodies = new NativeArray<OrbitalBody>(orbitalBodiesSO.Length, Allocator.Persistent);
        for (int i = 0; i < orbitalBodiesSO.Length; i++)
        {
            orbitalBodies[i] = new OrbitalBody()
            {
                orbitalData = new OrbitalData(orbitalBodiesSO[i]),
                planetaryData = new PlanetaryData(orbitalBodiesSO[i]),
                index = i,
            };
        }
        if (n == 0) { return; }
        CreateNBodies(n);
    }

    private void Start()
    {

    }

    private void Update()
    {
        if (useBurstCompiler)
        {
            //Doesnt Work In Parallel
            var job = new UpdateSystem()
            {
                time = lossyTimeScale * Time.deltaTime,
                orbitalBodies = orbitalBodies,
            };
            JobHandle jobHandle = new JobHandle();
            jobHandle = job.ScheduleParallel(lossLessTimeScale, lossLessTimeScale, new JobHandle());
            jobHandle.Complete();
        }
        else
        {
            for (int i = 0; i < lossLessTimeScale; i++)
                UpdateSystem();
        }

        for (int i = 0; i < orbitalBodies.Length; i++)
            DrawBody(i);
    }

    private void EndSimulation()
    {
        orbitalBodies.Dispose();
    }

    private void UpdateSystem()
    {
        for (int i = 0; i < orbitalBodies.Length; i++)
        {
            var orbitalBody = orbitalBodies[i];
            orbitalBody.CalculateForces(orbitalBodies);
            orbitalBodies[i] = orbitalBody;
        }
            

        for (int i = 0; i < orbitalBodies.Length; i++)
        {
            var orbitalBody = orbitalBodies[i];
            orbitalBody.ApplyForces(lossyTimeScale * Time.deltaTime);
            orbitalBodies[i] = orbitalBody;
        }
    }

    private void DrawBody(int i)
    {
        orbitalBodyObjects[i].transform.position = (orbitalBodies[i].orbitalData.position / drawScale).ToVector3();
    }

    private void CreateNBodies(int n)
    {
        float dist = 10000000000;
        orbitalBodies = new NativeArray<OrbitalBody>(n, Allocator.Persistent);
        orbitalBodyObjects = new GameObject[n];
        for (int i = 0; i < n; i++)
        {
            orbitalBodies[i] = new OrbitalBody(orbitalBodiesSO[0], i)
            {
                orbitalData = new OrbitalData(orbitalBodiesSO[0])
                {
                    position = new Vector3D(Random.Range(-dist, dist), Random.Range(-dist, dist), Random.Range(-dist, dist)),
                    velocity = Vector3D.zero,
                },
            };
            orbitalBodyObjects[i] = Instantiate(Prefab);
            orbitalBodyObjects[i].name = $"Planet {i}";
        }
    }
}

[BurstCompile(CompileSynchronously = true)]
public struct UpdateSystem : IJobFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<OrbitalBody> orbitalBodies;
    public float time;
    public void Execute(int Index)
    {
        for (int i = 0; i < orbitalBodies.Length; i++)
        {
            var orbitalBody = orbitalBodies[i];
            orbitalBody.CalculateForces(orbitalBodies);
            orbitalBodies[i] = orbitalBody;
        }
            
        
        for (int i = 0; i < orbitalBodies.Length; i++)
        {
            var orbitalBody = orbitalBodies[i];
            orbitalBody.ApplyForces(time);
            orbitalBodies[i] = orbitalBody;
        }        
    }

    public void UpdateSystemFrame()
    {
        for (int i = 0; i < orbitalBodies.Length; i++)
        {
            var orbitalBody = orbitalBodies[i];
            orbitalBody.CalculateForces(orbitalBodies);
            orbitalBodies[i] = orbitalBody;
        }


        for (int i = 0; i < orbitalBodies.Length; i++)
        {
            var orbitalBody = orbitalBodies[i];
            orbitalBody.ApplyForces(time);
            orbitalBodies[i] = orbitalBody;
        }
    }
}