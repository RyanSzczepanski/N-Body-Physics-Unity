using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct BarnesHut : IJob
{
    [NativeDisableParallelForRestriction]
    public NativeArray<OrbitalBody> bodies;
    [NativeDisableParallelForRestriction]
    public NativeList<Node<NBodyNodeData>> nodes;

    public void Execute()
    {
        Sort();
    }

    public void Sort()
    {
        for (int i = 0; i < bodies.Length; i++)
        {
            Recursive(bodies[i], 0);
        }
    }


    private void Recursive(OrbitalBody body, int searchNodeIndex)
    {
        Node<NBodyNodeData> currentNode = nodes[searchNodeIndex];
        //If node isnt and end node skip to children
        if (!currentNode.endNode)
        {
            //Needs to jump in and update node before decending
            currentNode.data.centerOfMass = (currentNode.data.mass * currentNode.data.centerOfMass + body.planetaryData.mass * body.orbitalData.position) / (currentNode.data.mass + body.planetaryData.mass);
            currentNode.data.mass += body.planetaryData.mass;
            nodes[searchNodeIndex] = currentNode;
            Recursive(body, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(body.orbitalData.position))/*Index Of Child Quadrent That Planet Is In*/);
            return;
        }

        //If node has a planet bump both down a layer
        if (currentNode.data.hasPlanet)
        {
            OrbitalBody existingOccupiedNode = currentNode.data.orbitalBody;
            currentNode.data.hasPlanet = false;
            
            //Generates all children
            for (int l = 0; l < 8; l++)
            {
                nodes.Add(currentNode.PopulateChild(
                    l,
                    new NBodyNodeData(),
                    nodes.Length));
            }
            //Needs to jump in and update node before decending
            currentNode.data.centerOfMass = (currentNode.data.mass * currentNode.data.centerOfMass + body.planetaryData.mass * body.orbitalData.position) / (currentNode.data.mass + body.planetaryData.mass);
            currentNode.data.mass += body.planetaryData.mass;
            nodes[searchNodeIndex] = currentNode;
            //Bumps existing planet down a node
            Recursive(existingOccupiedNode, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(existingOccupiedNode.orbitalData.position)));
            //Continues the search for a node
            Recursive(body, currentNode.nodeChildren.GetChildIndex(currentNode.spacialData.GetChildOctantsIndex(body.orbitalData.position)));
            return;
        }
        //Found node and adds planet
        currentNode.data.hasPlanet = true;
        currentNode.data.orbitalBody = body;

        currentNode.data.centerOfMass = body.orbitalData.position;
        currentNode.data.mass = body.planetaryData.mass;

        nodes[searchNodeIndex] = currentNode;
        return;
    }
}