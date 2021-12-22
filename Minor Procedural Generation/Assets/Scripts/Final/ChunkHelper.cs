using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkHelper : MonoBehaviour
{
    public Triangle[] triangles = new Triangle[1];



    /// <summary>
    /// Sets up mesh renderer of given gameobject. Either gets the mesh renderer or creates one.
    /// </summary>
    /// <param name="chunk">Given gameobject which needs to contain a mesh renderer.</param>
    /// <returns></returns>
    public MeshCollider setupMeshCollider()
    {
        if (gameObject.GetComponent<MeshCollider>() == null)
        {
            gameObject.AddComponent<MeshCollider>();
        }
        return gameObject.GetComponent<MeshCollider>();
    }

    /// <summary>
    /// Sets up mesh renderer of given gameobject. Either gets the mesh renderer or creates one.
    /// </summary>
    /// <param name="chunk">Given gameobject which needs to contain a mesh renderer.</param>
    /// <returns></returns>
    public MeshRenderer setupMeshRenderer()
    {
        if (gameObject.GetComponent<MeshRenderer>() == null)
        {
            gameObject.AddComponent<MeshRenderer>();
        }
        return gameObject.GetComponent<MeshRenderer>();
    }

    /// <summary>
    /// Sets up mesh filter of given gameobject. Either gets the mesh filter or creates one.
    /// </summary>
    /// <param name="chunk">Given gameobject which needs to contain a mesh filter.</param>
    /// <returns></returns>
    public MeshFilter setupMeshFilter()
    {
        if (gameObject.GetComponent<MeshFilter>() == null)
        {
            gameObject.AddComponent<MeshFilter>();
        }
        return gameObject.GetComponent<MeshFilter>();
    }


    /// <summary>
    /// Creates a Vector3 array from a triangles array, so it can be used for a mesh.
    /// </summary>
    /// <returns>A vector3 array with vertex positions in correct order for triangles.</returns>
    public Vector3[] createVertices()
    {
        Vector3[] vertices = new Vector3[triangles.Length * 3];
        for (int i = 0; i < triangles.Length; i++)
        {
            vertices[i * 3 + 0] = triangles[i].VertexA;
            vertices[i * 3 + 1] = triangles[i].VertexB;
            vertices[i * 3 + 2] = triangles[i].VertexC;
        }
        return vertices;
    }

    /// <summary>
    /// Generates a int array going from 0 to triangle amount. This list is orderer since all vertices are already in correct order in the triangle array.
    /// </summary>
    /// <param name="amount">The amount of triangles are put into the mesh.</param>
    /// <returns>An int array going from 0 to the amount.</returns>
    public int[] createTriangles(int amount)
    {
        int[] newTriangles = new int[amount];
        for (int i = 0; i < newTriangles.Length; i++)
        {
            newTriangles[i] = i;
        }
        return newTriangles;
    }

    /// <summary>
    /// Struct for the triangles, since compute shaders run a synchronious we need to give back a list of triangles based of 3 positions.
    /// </summary>
    public struct Triangle
    {
        public Vector3 VertexA;
        public Vector3 VertexB;
        public Vector3 VertexC;
    };
}
