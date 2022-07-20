using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class DebugRenderer
{
    static bool draw = false;
    static bool newBatch = true;
    static int count = 0;
    static List<GameObject> gameObjects = new List<GameObject>();

    static Mesh mesh = new Mesh();


    public static void CreateMesh()
    {
        mesh.name = "Bounds";
        Vector3[] vertices = new Vector3[8] {
            new Vector3(2, 2, 2), new Vector3(2, 2, -2), new Vector3(2, -2, 2), new Vector3(2, -2, -2),
            new Vector3(-2, 2, 2), new Vector3(-2, 2, -2), new Vector3(-2, -2, 2), new Vector3(-2, -2, -2),
        };
        mesh.vertices = vertices;

        int[] cube = new int[] {
            0, 1, 0, 2, 3, 1, 3, 2,
            4, 5, 4, 6, 7, 5, 7, 6,
            0, 4, 1, 5, 2, 6, 3, 7,
        };
        mesh.SetIndices(cube, MeshTopology.Lines, 0, true);
    }

    public static void DoDraw(bool doDraw)
    {
        draw = doDraw;
    }

    public static void StartNewBatch()
    {
        newBatch = true;
    }

    public static void DebugDrawCube(Vector3D center, double radius, Color color, float time)
    {
        //Face1
        Debug.DrawLine((center + SpacialOctreeData.GetOffsetVector(0) * radius).ToVector3(), (center + SpacialOctreeData.GetOffsetVector(1) * radius).ToVector3(), color, time);
        Debug.DrawLine((center + SpacialOctreeData.GetOffsetVector(0) * radius).ToVector3(), (center + SpacialOctreeData.GetOffsetVector(2) * radius).ToVector3(), color, time);
        Debug.DrawLine((center + SpacialOctreeData.GetOffsetVector(3) * radius).ToVector3(), (center + SpacialOctreeData.GetOffsetVector(1) * radius).ToVector3(), color, time);
        Debug.DrawLine((center + SpacialOctreeData.GetOffsetVector(3) * radius).ToVector3(), (center + SpacialOctreeData.GetOffsetVector(2) * radius).ToVector3(), color, time);
        //    //Face2
        Debug.DrawLine((center + SpacialOctreeData.GetOffsetVector(4) * radius).ToVector3(), (center + SpacialOctreeData.GetOffsetVector(5) * radius).ToVector3(), color, time);
        Debug.DrawLine((center + SpacialOctreeData.GetOffsetVector(4) * radius).ToVector3(), (center + SpacialOctreeData.GetOffsetVector(6) * radius).ToVector3(), color, time);
        Debug.DrawLine((center + SpacialOctreeData.GetOffsetVector(7) * radius).ToVector3(), (center + SpacialOctreeData.GetOffsetVector(5) * radius).ToVector3(), color, time);
        Debug.DrawLine((center + SpacialOctreeData.GetOffsetVector(7) * radius).ToVector3(), (center + SpacialOctreeData.GetOffsetVector(6) * radius).ToVector3(), color, time);
        //    //Connecting Arms
        Debug.DrawLine((center + SpacialOctreeData.GetOffsetVector(0) * radius).ToVector3(), (center + SpacialOctreeData.GetOffsetVector(4) * radius).ToVector3(), color, time);
        Debug.DrawLine((center + SpacialOctreeData.GetOffsetVector(1) * radius).ToVector3(), (center + SpacialOctreeData.GetOffsetVector(5) * radius).ToVector3(), color, time);
        Debug.DrawLine((center + SpacialOctreeData.GetOffsetVector(2) * radius).ToVector3(), (center + SpacialOctreeData.GetOffsetVector(6) * radius).ToVector3(), color, time);
        Debug.DrawLine((center + SpacialOctreeData.GetOffsetVector(3) * radius).ToVector3(), (center + SpacialOctreeData.GetOffsetVector(7) * radius).ToVector3(), color, time);
    }



    public static void DrawCube(float3 center, float radius, int batchCount, Material material)
    {
        if (!draw) { return; }
        if (newBatch)
        {
            newBatch = false;
            count = 0;
        }
        if(gameObjects.Count > batchCount)
        {
            for (int i = batchCount; i < gameObjects.Count; i++)
            {
                gameObjects[i].SetActive(false);
            }
        }      
           

        GameObject go;
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        if (count < gameObjects.Count)
        {
            if(gameObjects[count] is null)
                go = new GameObject();
            else
                go = gameObjects[count];
            if(!go.activeSelf)
                go.SetActive(true);
            meshFilter = go.GetComponent<MeshFilter>();
            meshRenderer = go.GetComponent<MeshRenderer>();
        }
        else
        {
            go = new GameObject();
            meshFilter = go.AddComponent<MeshFilter>();
            meshRenderer = go.AddComponent<MeshRenderer>();
            meshFilter.sharedMesh = mesh;
            meshRenderer.material = material;
            gameObjects.Add(go);
        }
        
        go.transform.position = center;
        go.transform.localScale = new Vector3(radius/2, radius/2, radius/2);
        count++;
    }
}
