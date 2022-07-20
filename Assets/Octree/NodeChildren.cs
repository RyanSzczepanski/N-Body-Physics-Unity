using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NodeChildren
{
    public int child0;
    public int child1;
    public int child2;
    public int child3;
    public int child4;
    public int child5;
    public int child6;
    public int child7;

    public int GetChildIndex(int child)
    {
        switch (child)
        {
            case 0: return child0;
            case 1: return child1;
            case 2: return child2;
            case 3: return child3;
            case 4: return child4;
            case 5: return child5;
            case 6: return child6;
            case 7: return child7;
            default: return -1;
        }
    }

    public void SetChildIndex(int child, int value)
    {
        switch (child)
        {
            case 0: child0 = value; break;
            case 1: child1 = value; break;
            case 2: child2 = value; break;
            case 3: child3 = value; break;
            case 4: child4 = value; break;
            case 5: child5 = value; break;
            case 6: child6 = value; break;
            case 7: child7 = value; break;
        }
    }
}

public enum OctreeChild
{
    RightTopBack = 0,     //000
    RightTopFront = 1,    //001
    RightBottomBack = 2,  //010
    RightBottomFront = 3, //011
    LeftTopBack = 4,      //100
    LeftTopFront = 5,     //101
    LeftBottomBack = 6,   //110
    LeftBottomFront = 7,  //111
}