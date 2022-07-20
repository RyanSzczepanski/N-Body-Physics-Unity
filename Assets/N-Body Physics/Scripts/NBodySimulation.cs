using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;

public class NBodySimulation : MonoBehaviour
{
    public int numberOfBodies;

    public Optimization optimization;

    public GameObject Prefab;

    public OrbitalBodySO[] orbitalBodiesSO;
    public GameObject[] orbitalBodyObjects;
    private NativeArray<OrbitalBody> orbitalBodies;

    public float drawScale;
    public int lossLessTimeScale;
    public int lossyTimeScale;

    private void Awake()
    {
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

        if (numberOfBodies <= 0) { return; }
            CreateNBodies(numberOfBodies);
    }

    private void Update()
    {
        UpdateSystem(optimization);
        for (int i = 0; i < orbitalBodies.Length; i++)
            DrawBody(i);
    }

    private void EndSimulation()
    {
        orbitalBodies.Dispose();
    }

    private void DrawBody(int i)
    {
        orbitalBodyObjects[i].transform.position = (orbitalBodies[i].orbitalData.position / drawScale).ToVector3();
    }

    private void CreateNBodies(int n)
    {
        float dist = 10000000000;
        orbitalBodies.Dispose();
        orbitalBodies = new NativeArray<OrbitalBody>(n, Allocator.Persistent);
        orbitalBodyObjects = new GameObject[n];
        for (int i = 0; i < n; i++)
        {
            orbitalBodies[i] = new OrbitalBody(orbitalBodiesSO[0], i)
            {
                orbitalData = new OrbitalData(orbitalBodiesSO[0])
                {
                    //Cube
                    position = new Vector3D(Random.Range(-dist, dist), Random.Range(-dist, dist), Random.Range(-dist, dist)),
                    //Sphere
                    //position = new Vector3D(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normilized * Random.Range(-dist, dist),
                    velocity = Vector3D.zero,
                },
            };
            orbitalBodyObjects[i] = Instantiate(Prefab);
            orbitalBodyObjects[i].name = $"Planet {i}";
        }
    }

    private void UpdateSystem(Optimization optimizationMethod)
    {
        //Optimazation None - Not good for anything
        if (optimizationMethod == Optimization.None)
        {
            for (int i = 0; i < lossLessTimeScale; i++)
            {
                for (int j = 0; j < orbitalBodies.Length; j++)
                {
                    var orbitalBody = orbitalBodies[j];
                    orbitalBody.CalculateForces(orbitalBodies);
                    orbitalBodies[j] = orbitalBody;
                }

                for (int j = 0; j < orbitalBodies.Length; j++)
                {
                    var orbitalBody = orbitalBodies[j];
                    orbitalBody.ApplyForces(lossyTimeScale * Time.deltaTime);
                    orbitalBodies[j] = orbitalBody;
                }
            }
        }

        //Optimazation Burst - Good for high time scales
        else if (optimizationMethod == Optimization.Burst)
        {
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(lossLessTimeScale, Allocator.TempJob);
            var job = new UpdateSystemBurstJob()
            {
                time = lossyTimeScale * Time.deltaTime,
                orbitalBodies = orbitalBodies,
            };
            for (int i = 0; i < lossLessTimeScale; i++)
            {
                if (i == 0)
                {
                    jobHandles[i] = job.Schedule(new JobHandle());
                }
                else
                {
                    jobHandles[i] = job.Schedule(jobHandles[i - 1]);
                }
            }
            JobHandle.CompleteAll(jobHandles);
            jobHandles.Dispose();
        }

        //Optimazation Parallel Force Calculations - Good for high body counts
        else if (optimizationMethod == Optimization.ParallelForceCalculations)
        {
            var job = new CalculateAllForcesJob()
            {
                orbitalBodies = orbitalBodies,
            };
            for (int i = 0; i < lossLessTimeScale; i++)
            {
                JobHandle jobHandle = job.Schedule(orbitalBodies.Length, 1);
                jobHandle.Complete();

                for (int j = 0; j < orbitalBodies.Length; j++)
                {
                    var orbitalBody = orbitalBodies[j];
                    orbitalBody.ApplyForces(lossyTimeScale * Time.deltaTime);
                    orbitalBodies[j] = orbitalBody;
                }
            }
        }

        //Optimazation Parallel Force Calculations & Burst - Good balance for high body counts if using time scale but can be beat by just burst at low body counts?
        else if (optimizationMethod == Optimization.BurstAndParallelForceCalculations)
        {
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(lossLessTimeScale, Allocator.TempJob);
            NativeArray<JobHandle> calculateAllForcesJJobHandles = new NativeArray<JobHandle>(lossLessTimeScale, Allocator.Temp);

            var calculateAllForcesJob = new CalculateAllForcesJob()
            {
                orbitalBodies = orbitalBodies,
            };

            var job = new ApplyFrocesJob()
            {
                time = lossyTimeScale * Time.deltaTime,
                orbitalBodies = orbitalBodies,
            };

            for (int i = 0; i < lossLessTimeScale; i++)
            {
                if (i == 0)
                {
                    calculateAllForcesJJobHandles[i] = calculateAllForcesJob.Schedule(orbitalBodies.Length, 1);
                }
                else
                {
                    calculateAllForcesJJobHandles[i] = calculateAllForcesJob.Schedule(orbitalBodies.Length, 1, jobHandles[i - 1]);
                }
                jobHandles[i] = job.Schedule(calculateAllForcesJJobHandles[i]);
            }
            JobHandle.CompleteAll(jobHandles);
            jobHandles.Dispose();
        }
    }
}

//JOB
[BurstCompile(CompileSynchronously = false)]
public struct UpdateSystemBurstJob : IJob
{
    [NativeDisableParallelForRestriction]
    public NativeArray<OrbitalBody> orbitalBodies;
    public float time;
    public void Execute()
    {
        UpdateSystem();
    }

    public void UpdateSystem()
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

//JOB
[BurstCompile(CompileSynchronously = false)]
public struct CalculateAllForcesJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<OrbitalBody> orbitalBodies;
    public void Execute(int index)
    {
        var orbitalBody = orbitalBodies[index];
        orbitalBody.CalculateForces(orbitalBodies);
        orbitalBodies[index] = orbitalBody;
    }
}

//JOB
[BurstCompile(CompileSynchronously = false)]
public struct ApplyFrocesJob : IJob
{
    [NativeDisableParallelForRestriction]
    public NativeArray<OrbitalBody> orbitalBodies;
    public float time;
    public void Execute()
    {
        for (int i = 0; i < orbitalBodies.Length; i++)
        {
            var orbitalBody = orbitalBodies[i];
            orbitalBody.ApplyForces(time);
            orbitalBodies[i] = orbitalBody;
        }
    }
}

public enum Optimization
{
    None,
    Burst,
    ParallelForceCalculations,
    BurstAndParallelForceCalculations
}