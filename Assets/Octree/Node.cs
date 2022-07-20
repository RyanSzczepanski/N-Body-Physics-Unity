using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[System.Serializable]
public struct Node<Data>
{
    public Data data;
    public SpacialOctreeData spacialData;

    public int parentNodeIndex;
    public int index;
    public NodeChildren nodeChildren;

    public bool endNode;

    public static Node<Data> CreateNewNode(Data data, SpacialOctreeData spacialData, int parentNodeIndex, int index)
    {
        return new Node<Data>()
        {
            data = data,
            spacialData = spacialData,
            parentNodeIndex = parentNodeIndex,
            index = index,
            endNode = true,

            nodeChildren = new NodeChildren()
            {
                child0 = -1,
                child1 = -1,
                child2 = -1,
                child3 = -1,
                child4 = -1,
                child5 = -1,
                child6 = -1,
                child7 = -1,
            },
        };
    }

    public int GetDepth(NativeArray<Node<Data>> nodes)
    {
        int depth = 0;
        if (parentNodeIndex == -1)
            return depth;
        else
        {
            depth++;
            depth += nodes[parentNodeIndex].GetDepth(nodes);
        }
        return depth;
    }

    public int GetMasterNodeIndex(NativeArray<Node<Data>> nodes)
    {
        if(parentNodeIndex == -1)
            return index;
        else
            nodes[parentNodeIndex].GetMasterNodeIndex(nodes);
        return -1;
    }

    public Node<Data> PopulateChild(int siblingIndex, Data data, int index)
    {
        endNode = false;
        if(nodeChildren.GetChildIndex(siblingIndex) != -1)
        {
            Debug.Log($"Child {siblingIndex} of Node Index {index} is already populated with Node Index {nodeChildren.GetChildIndex(siblingIndex)}");
        }

        nodeChildren.SetChildIndex(siblingIndex, index);

        return CreateNewNode(
            data,
            new SpacialOctreeData() {
                center = spacialData.center + SpacialOctreeData.GetOffsetVector(siblingIndex) * spacialData.radius / 2,
                radius = spacialData.radius / 2},
            this.index,
            index);
    }   
}