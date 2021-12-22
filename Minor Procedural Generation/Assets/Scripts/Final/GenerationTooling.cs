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




    /// <summary>
    /// Sets all constant values for the chunk
    /// </summary>
    public void setupValues()
    {
        noiseShader.SetFloat("chunkSize", pointsPerAxis * pointsPerAxis * pointsPerAxis);
        noiseShader.SetInt("pointsPerAxis", pointsPerAxis);
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


        Perlin.CreateBuffers();
        MarchingCube.CreateBuffers();
    }




}
