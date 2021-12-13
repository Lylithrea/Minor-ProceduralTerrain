﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarchCubes;
using Noise;

public class Chunk : MonoBehaviour
{

    [HideInInspector]
    public Mesh mesh;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    public Generation generator;

    public void DestroyOrDisable()
    {
        if (Application.isPlaying)
        {
            mesh.Clear();
            gameObject.SetActive(false);
        }
        else
        {
            DestroyImmediate(gameObject, false);
        }
    }

    // Add components/get references in case lost (references can be lost when working in the editor)
    public void SetUp(Material mat, bool generateCollider)
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        if (meshCollider == null && generateCollider)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        if (meshCollider != null && !generateCollider)
        {
            DestroyImmediate(meshCollider);
        }

        mesh = meshFilter.sharedMesh;
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            meshFilter.sharedMesh = mesh;
        }

        if (generateCollider)
        {
            if (meshCollider.sharedMesh == null)
            {
                meshCollider.sharedMesh = mesh;
            }
            // force update
            meshCollider.enabled = false;
            meshCollider.enabled = true;
        }

        meshRenderer.material = mat;
    }

    public void updatePosition(Vector3 newPosition)
    {
        if (!generator.allChunks.ContainsKey(newPosition))
        {
            //generate a new chunk
            generator.allChunks.Remove(this.gameObject.transform.position);

            this.gameObject.name = "Chunk (" + newPosition.x + "," + newPosition.y + "," + newPosition.z + ")";

            Vector3 chunkPosition = newPosition * (generator.pointsPerAxis - 1) * generator.size;
            this.gameObject.transform.position = chunkPosition;
            Perlin.noiseShader.SetVector("startingValue", chunkPosition);
            MarchingCube.marchingCubeShader.SetVector("startingValue", chunkPosition);

            //generate noise <- compute shader
            //generate marching cubes <- compute shader
            //Array.Clear(generator.triangles, 0, generator.triangles.Length);
            if (generator.currentChunk == newPosition)
            {
                generator.triangles = MarchingCube.marchingCubesGenerator(Perlin.noiseGenerator(1), 1);
            }
            else
            {
                generator.triangles = MarchingCube.marchingCubesGenerator(Perlin.noiseGenerator(1), 1);
            }



            //set mesh <- main thread
            SetMesh();
            generator.allChunks.Add(newPosition, this.gameObject);
        }
    }


    public void updateVertexDensity(int density)
    {
        generator.noiseShader.SetVector("startingValue", transform.position);
        generator.marchingCubeShader.SetVector("startingValue", transform.position);

        //generate noise <- compute shader
        //generate marching cubes <- compute shader
        generator.triangles = MarchingCube.marchingCubesGenerator(Perlin.noiseGenerator(density), density);

        //set mesh <- main thread
        SetMesh();
    }


    public void chunkSetup()
    {
        generator.noiseShader.SetVector("startingValue", gameObject.transform.position);
        generator.marchingCubeShader.SetVector("startingValue", gameObject.transform.position);
    }


    public void SetMesh()
    {
        MeshRenderer meshRenderer = setupMeshRenderer();
        MeshFilter meshFilter = setupMeshFilter();

        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        mesh.vertices = generator.createVertices();
        mesh.triangles = generator.createTriangles(mesh.vertices.Length);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();


        meshRenderer.material = generator.terrainMaterial;
    }

    /// <summary>
    /// Sets up mesh renderer of given gameobject. Either gets the mesh renderer or creates one.
    /// </summary>
    /// <param name="chunk">Given gameobject which needs to contain a mesh renderer.</param>
    /// <returns></returns>
    private MeshRenderer setupMeshRenderer()
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
    private MeshFilter setupMeshFilter()
    {
        if (gameObject.GetComponent<MeshFilter>() == null)
        {
            gameObject.AddComponent<MeshFilter>();
        }
        return gameObject.GetComponent<MeshFilter>();
    }
}
