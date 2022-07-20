using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct SpacialOctreeData
{
    public Vector3D center;
    public float radius;

    public static Vector3D GetOffsetVector(int ChildIndex)
    {
        Vector3D offset = Vector3D.zero;
        switch (ChildIndex)
        {
            case (int)OctreeChild.RightTopBack:
                offset = new Vector3D(1, 1, 1);
                break;
            case (int)OctreeChild.RightTopFront:
                offset = new Vector3D(1, 1, -1);
                break;
            case (int)OctreeChild.RightBottomBack:
                offset = new Vector3D(1, -1, 1);
                break;
            case (int)OctreeChild.RightBottomFront:
                offset = new Vector3D(1, -1, -1);
                break;
            case (int)OctreeChild.LeftTopBack:
                offset = new Vector3D(-1, 1, 1);
                break;
            case (int)OctreeChild.LeftTopFront:
                offset = new Vector3D(-1, 1, -1);
                break;
            case (int)OctreeChild.LeftBottomBack:
                offset = new Vector3D(-1, -1, 1);
                break;
            case (int)OctreeChild.LeftBottomFront:
                offset = new Vector3D(-1, -1, -1);
                break;
        }
        return offset;
    }

    public int GetChildOctantsIndex(Vector3D position)
    {
        Vector3D localPosition = position - center;
        int output = 0;
        if (localPosition.x < 0)
            output = output | 1 << 2;
        if (localPosition.y < 0)
            output = output | 1 << 1;
        if (localPosition.z < 0)
            output = output | 1 << 0;
        return output;
    }

    public bool ContainsPosition(Vector3D pos)
    {
        return (pos.x < center.x + radius && pos.x > center.x - radius &&
                pos.y < center.y + radius && pos.y > center.y - radius &&
                pos.z < center.z + radius && pos.z > center.z - radius);
    }
}