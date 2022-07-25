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

    public Octree octree;

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

        if (numberOfBodies > 0)
        {
            CreateNBodies(numberOfBodies);
        }

        octree = new Octree();
        octree.Init(orbitalBodies.Length);
    }

    private void FixedUpdate()
    {
        UpdateSystem(optimization, Time.fixedDeltaTime);
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

    private void UpdateSystem(Optimization optimizationMethod, float time)
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
                    orbitalBody.ApplyForces(lossyTimeScale * time);
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
                time = lossyTimeScale * time,
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
                    orbitalBody.ApplyForces(lossyTimeScale * time);
                    orbitalBodies[j] = orbitalBody;
                }
            }
        }

        //Optimazation Parallel Force Calculations & Burst - Good balance for high body counts if using time scale but can be beat by just burst at low body counts?
        else if (optimizationMethod == Optimization.BurstAndParallelForceCalculations)
        {
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(lossLessTimeScale, Allocator.TempJob);
            NativeArray<JobHandle> calculateAllForcesJobHandles = new NativeArray<JobHandle>(lossLessTimeScale, Allocator.Temp);

            var calculateAllForcesJob = new CalculateAllForcesJob()
            {
                orbitalBodies = orbitalBodies,
            };

            var job = new ApplyForcesJob()
            {
                time = lossyTimeScale * time,
                orbitalBodies = orbitalBodies,
            };

            for (int i = 0; i < lossLessTimeScale; i++)
            {
                if (i == 0)
                {
                    calculateAllForcesJobHandles[i] = calculateAllForcesJob.Schedule(orbitalBodies.Length, 1);
                }
                else
                {
                    calculateAllForcesJobHandles[i] = calculateAllForcesJob.Schedule(orbitalBodies.Length, 1, jobHandles[i - 1]);
                }
                jobHandles[i] = job.Schedule(calculateAllForcesJobHandles[i]);
            }
            JobHandle.CompleteAll(jobHandles);
            jobHandles.Dispose();
            //octree.GenerateTree(orbitalBodies);
        }
        else if (optimizationMethod == Optimization.BarnesHut)
        {
            JobHandle generateTreeHandle;
            JobHandle applyForceHandle;
            JobHandle calculateForcesJobHandle;

            BarnesHutCalculateAllForcesJob barnesHutCalculateForcesJob = new BarnesHutCalculateAllForcesJob()
            {
                orbitalBodies = orbitalBodies,
                theta = 1f,
            };

            ApplyForcesJob applyForcesJob = new ApplyForcesJob()
            {
                time = lossyTimeScale * time,
                orbitalBodies = orbitalBodies,
            };

            //JobHandle calculateForceHandle = barnesHutCalculateAllForcesJob.Schedule(orbitalBodies.Length, 1);

            for (int i = 0; i < lossLessTimeScale; i++)
            {
                octree.GenerateTree(orbitalBodies);
                barnesHutCalculateForcesJob.nodes = octree.nodes;
                calculateForcesJobHandle = barnesHutCalculateForcesJob.Schedule(orbitalBodies.Length, 1);
                applyForceHandle = applyForcesJob.Schedule(calculateForcesJobHandle);
                applyForceHandle.Complete();
            }
        }
    }
}

//JOB
[BurstCompile]
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
[BurstCompile]
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
[BurstCompile]
public struct ApplyForcesJob : IJob
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

//JOB
//[BurstCompile]
public struct BarnesHutCalculateAllForcesJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<OrbitalBody> orbitalBodies;
    [NativeDisableParallelForRestriction]
    public NativeArray<Node<NBodyNodeData>> nodes;
    public double theta;
    //public int iteration;

    public void Execute(int index)
    {
        //iteration = 0;
        var orbitalBody = orbitalBodies[index];

        Vector3D force;
        force = CalculateForces(orbitalBody, nodes[0], index);
        orbitalBody.nextForceVector = force;;
        orbitalBodies[index] = orbitalBody;
        //Debug.Log(iteration);
    }


    private Vector3D CalculateForces(OrbitalBody orbitalBody, Node<NBodyNodeData> node, int index)
    {
        Vector3D force = Vector3D.zero;

        if (node.data.mass == 0) { /*Debug.Log("Mass = 0");*/ return Vector3D.zero; }
        if (node.data.centerOfMass == orbitalBody.orbitalData.position) { /*Debug.Log($"{orbitalBody.planetaryData.mass} {node.index}");*/ return Vector3D.zero; }

        if ((node.spacialData.radius * 2) / Vector3D.Distance(orbitalBody.orbitalData.position, node.data.centerOfMass) < theta || node.endNode)
        {
            //if (index == 5)
            //{
            //    Debug.DrawLine((orbitalBody.orbitalData.position / 5e+09).ToVector3(), (node.data.centerOfMass / 5e+09).ToVector3(), Color.green, 0f);
            //    DebugRenderer.DebugDrawCube(node.data.centerOfMass / 5e+09, node.data.mass / nodes[0].data.mass + .01f, Color.magenta, 0f);
            //    DebugRenderer.DebugDrawCube(node.spacialData.center / 5e+09, node.spacialData.radius / 5e+09, Color.blue, 0f);
            //}
            //iteration++;
            
            return NBodyPhysics.CalculateForceOfGravity(orbitalBody.orbitalData.position, orbitalBody.planetaryData.mass, node.data.centerOfMass, node.data.mass);
        }
        else
        {
            //if (index == 5)
            //{
            //    Debug.DrawLine((orbitalBody.orbitalData.position / 5e+09).ToVector3(), (node.data.centerOfMass / 5e+09).ToVector3(), Color.gray, 0f);
            //    DebugRenderer.DebugDrawCube(node.data.centerOfMass / 5e+09, .1f, Color.red, 0f);
            //    DebugRenderer.DebugDrawCube(node.spacialData.center / 5e+09, node.spacialData.radius / 5e+09, Color.yellow, 0f);
            //    Debug.Log("Fail  " + (node.spacialData.radius * 2) / Vector3D.Distance(orbitalBody.orbitalData.position, node.data.centerOfMass));
            //    Debug.Log(orbitalBody.orbitalData.position);


            //    Debug.DrawLine(Vector3.zero, Vector3.one, Color.yellow, 0f);
            //}
            //Debug.Log("Split " + index);
            for (int i = 0; i < 8; i++)
            {
                force += CalculateForces(orbitalBody, nodes[node.nodeChildren.GetChildIndex(i)], index);
            }
            //Debug.Log("FoundForce " + index);
            return force;
        }
    }
}

public enum Optimization
{
    None,
    Burst,
    ParallelForceCalculations,
    BurstAndParallelForceCalculations,
    BarnesHut,
}