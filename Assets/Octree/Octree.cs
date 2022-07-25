using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct Octree
{
    public NativeList<Node<NBodyNodeData>> nodes;
    //public NativeArray<int> nodesWithPlanets;
    public NativeList<Node<NBodyNodeData>> newNodes;

    public Node<NBodyNodeData>[] debugArray;
    public void Init(int size)
    {
        nodes = new NativeList<Node<NBodyNodeData>>(0, Allocator.Persistent);
        //nodesWithPlanets = new NativeArray<int>(size, Allocator.Persistent);
    }

    private void PreWork()
    {
        newNodes = new NativeList<Node<NBodyNodeData>>(0, Allocator.TempJob);
        newNodes.Add(Node<NBodyNodeData>.CreateNewNode(
            new NBodyNodeData(),
            new SpacialOctreeData()
            {
                center = Vector3D.zero,
                radius = 7500000000000,
            },
            -1,
            0));
    }

    public void GenerateTree(NativeArray<OrbitalBody> data)
    {
        PreWork();

        BarnesHut barnesHut = new BarnesHut()
        {
            bodies = data,
            nodes = newNodes,
        };

        JobHandle jobHandle = barnesHut.Schedule();

        jobHandle.Complete();
        nodes.CopyFrom(barnesHut.nodes);

        newNodes.Dispose();
    }

    public JobHandle GenerateTreeJob(NativeArray<OrbitalBody> data, JobHandle dependecy)
    {
        PreWork();

        BarnesHut barnesHut = new BarnesHut()
        {
            bodies = data,
            nodes = nodes,
        };

        JobHandle jobHandle = barnesHut.Schedule(dependecy);
        return jobHandle;
    }

    public void Draw(int drawDepth)
    {
        if (nodes.Length <= 0) { return; }
        if (drawDepth != -1)
        {
            Node<NBodyNodeData>[] nodesToDraw = GetAllNodesAtDepth(0, drawDepth);
            foreach (Node<NBodyNodeData> node in nodesToDraw)
            {
                DebugRenderer.DebugDrawCube(node.spacialData.center / 5e+09, node.spacialData.radius / 5e+09, new Color(1, 1 - node.GetDepth(nodes) / 15f, 0), 0f);
            }
        }
        else
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].endNode)
                {
                    DebugRenderer.DebugDrawCube(nodes[i].spacialData.center / 5e+09, nodes[i].spacialData.radius / 5e+09, new Color(1, 1 - nodes[i].GetDepth(nodes) / 15f, 0), 0f);
                }
            }
        }
    }

    private Node<NBodyNodeData>[] GetAllNodesAtDepth(int parentIndex, int depth)
    {
        if (nodes[parentIndex].GetDepth(nodes) == depth)
        {
            return new Node<NBodyNodeData>[1] { nodes[parentIndex] };
        }
        else if (nodes[parentIndex].GetDepth(nodes) < depth)
        {
            if (nodes[parentIndex].endNode) { return new Node<NBodyNodeData>[0]; }

            if (nodes[parentIndex].GetDepth(nodes) == depth - 1)
            {
                Node<NBodyNodeData>[] childNodes = new Node<NBodyNodeData>[8];
                for (int i = 0; i < 8; i++)
                {
                    childNodes[i] = nodes[nodes[parentIndex].nodeChildren.GetChildIndex(i)];
                }
                return childNodes;
            }
            else
            {
                List<Node<NBodyNodeData>> nodeses = new List<Node<NBodyNodeData>>();
                for (int i = 0; i < 8; i++)
                {
                    nodeses.AddRange(GetAllNodesAtDepth(nodes[parentIndex].nodeChildren.GetChildIndex(i), depth));
                }
                return nodeses.ToArray();
            }
        }
        return new Node<NBodyNodeData>[0];
    }
}