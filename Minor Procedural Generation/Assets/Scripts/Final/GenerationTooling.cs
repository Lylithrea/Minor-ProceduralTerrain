using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Noise;
using MarchCubes;

public class GenerationTooling : MonoBehaviour
{
    public GameObject player;
    private Vector3 currentPosition;

    [Header("Noise Settings")]
    public ComputeShader noiseShader;
    //[Range(0.43f, 0.47f)]
    public float scale;
    public float groundLevelHeight = 250;
    public int groundLevel;
    [Range(0,1f)] [Tooltip("Influences how much value the next octave brings")]
    public float persistence = 0.65f;
    [Range(0, 1f)] [Tooltip("Influences the frequency/noise of the next octave")]
    public float lacunarity = 0.25f;
    [Range(1, 8)][Tooltip("Influences how many layers of noise go on top of each other")]
    public int octaves = 4;

    [Header("Generation Settings")]
    public int radius;
    public float cutoff;

    [Header("Chunk Settings")]
    public ComputeShader marchingCubeShader;
    public int pointsPerAxis = 10;
    public int size;
    [Range(0, 2)]
    public int levelOfDetail;
    public int levelOfDetailRadius;

    Vector3[] chunkVertexPositions;

    //Contains all the currently loaded chunks
    public Dictionary<Vector3, GameObject> allChunks = new Dictionary<Vector3, GameObject>();
    public List<Vector3> currentPlayerChunks = new List<Vector3>();

    public Material terrainMaterial;
    public Vector3 currentChunk;

    public Queue<Vector3> chunkQueue = new Queue<Vector3>();
    public Queue<Vector3> destroyChunkQueue = new Queue<Vector3>();
    public Queue<GameObject> reusableChunkQueue = new Queue<GameObject>();
    public Queue<Chunks> updateQueue = new Queue<Chunks>();



    //should be equal to computeshader numthreads
    int numThreads = 8;
    public bool spawningChunksRunning = false;
    public bool destroyingChunksRunning = false;

    public Texture groundTexture;

    public struct Chunks
    {
        public GameObject chunk;
        public int points;
    }

    public Triangle[] triangles = new Triangle[1];


    /// <summary>
    /// Calculates all vertex points within a chunk at the start, since every vertex position is the same within chunk.
    /// </summary>
    public void GainChunkPositions()
    {
        //sets all the chunk positions since its always the same in each chunk
        int chunkSize = pointsPerAxis * pointsPerAxis * pointsPerAxis;

        chunkVertexPositions = new Vector3[chunkSize];
        for (int i = 0; i < chunkSize; i++) //triple foreach loop condensed into 1 for loop
        {
            float r = i % pointsPerAxis;
            float h = Mathf.FloorToInt((i / pointsPerAxis) % pointsPerAxis);
            float c = Mathf.FloorToInt(i / (pointsPerAxis * pointsPerAxis));
            chunkVertexPositions[i] = new Vector3(r * size, h * size, c * size);
        }

    }



    /// <summary>
    /// Sets all constant values for the chunk
    /// </summary>
    public void setupValues()
    {
        noiseShader.SetFloat("chunkSize", pointsPerAxis * pointsPerAxis * pointsPerAxis);
        noiseShader.SetFloat("size", size);
        noiseShader.SetFloat("noiseScale", scale);
        noiseShader.SetFloat("repeat", 9);
        noiseShader.SetInt("pointsPerAxis", pointsPerAxis);
        noiseShader.SetFloat("groundHeight", groundLevel);
        noiseShader.SetFloat("mountainHeight", groundLevelHeight);
        noiseShader.SetFloat("persistence", persistence);
        noiseShader.SetFloat("lacunarity", lacunarity);
        noiseShader.SetInt("octaves", octaves);

        marchingCubeShader.SetInt("pointsPerAxis", pointsPerAxis);
        marchingCubeShader.SetFloat("cutoff", cutoff);

        Perlin.noiseShader = noiseShader;
        Perlin.pointsPerAxis = pointsPerAxis;
        Perlin.numThreads = numThreads;
        Perlin.size = size;

        MarchingCube.pointsPerAxis = pointsPerAxis;
        MarchingCube.numThreads = numThreads;
        MarchingCube.marchingCubeShader = marchingCubeShader;

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
