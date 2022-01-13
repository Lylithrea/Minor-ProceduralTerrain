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
    public int groundLevel;
    public GameObject chunkPrefab;

    public GameObject flower;
    public List<GameObject> highTreeModels;
    public List<GameObject> middleTreeModels;
    public List<GameObject> lowTreeModels;

    [Header("HeightMap Settings")]
    public float mountainHeight = 250;
    [Range(0, 1f)]    [Tooltip("Influences how much value the next octave brings")]
    public float persistence = 0.65f;
    [Range(0, 1f)]    [Tooltip("Influences the frequency/noise of the next octave")]
    public float lacunarity = 0.25f;
    [Range(1, 8)]    [Tooltip("Influences how many layers of noise go on top of each other")]
    public int octaves = 4;
    public float heightMapScale = 150;
    [Range(0, 10)]
    public float mountainExp = 4;


    [Header("Cave Settings")]
    [Range(0f, 1f)]
    public float caveCutoff = 0.65f;
    public float caveScale = 150;
    public float minBigCaves = 0;
    public float maxBigCaves = 250;
    public float deltaBigCaves = 75;
    [Range(0f, 1f)]
    public float strengthBigCaves = 0.1f;



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

        //heightMapSettings
        noiseShader.SetFloat("mountainHeight", mountainHeight);
        noiseShader.SetFloat("persistence", persistence);
        noiseShader.SetFloat("lacunarity", lacunarity);
        noiseShader.SetInt("octaves", octaves);
        noiseShader.SetFloat("heightMapScale", heightMapScale);
        noiseShader.SetFloat("exp", mountainExp);

        //caves settings
        noiseShader.SetFloat("caveCutoff", caveCutoff);
        noiseShader.SetFloat("caveScale", caveScale);
        noiseShader.SetFloat("caveMin", minBigCaves);
        noiseShader.SetFloat("caveMax", maxBigCaves);
        noiseShader.SetFloat("caveDelta", deltaBigCaves);
        noiseShader.SetFloat("caveStrength", strengthBigCaves);


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

    public float heightmapNoise(float x, float y)
    {
        //points in the perlin space
        float pointA = x - Mathf.FloorToInt(x);
        float pointB = y - Mathf.FloorToInt(y);

        // Calculate the "unit cube" that the point asked will be located in
        int xi = (int)x & 255;
        int yi = (int)y & 255;

        //get a semi-random number from a table for 255 included numbers
        int testA = p[p[xi] + yi];
        int testB = p[p[xi + 1] + yi];
        int testC = p[p[xi] + yi + 1];
        int testD = p[p[xi + 1] + yi + 1];

        //based on those random numbers generate a direction, in this case from 4 different directions
        float testAh = gradplane(testA, pointA, pointB);
        float testBh = gradplane(testB, pointA - 1, pointB);
        float testCh = gradplane(testC, pointA, pointB - 1);
        float testDh = gradplane(testD, pointA - 1, pointB - 1);


        //Combine the directions together, 2 at the time, first on the X axis
        double resultA = lerp(testAh, testBh, interpolate(pointA));
        double resultB = lerp(testCh, testDh, interpolate(pointA));

        //then combine those 2 values together on the y axis
        float endResult = (float)lerp((float)resultA, (float)resultB, interpolate(pointB));
        endResult = (endResult + 1) / 2;
        return endResult;
    }

    float interpolate(float value)
    {
        return value * value * value * (value * (value * 6 - 15) + 10);
    }


    static float gradplane(int hash, float x, float y)
    {
        switch (hash & 0x3)
        {
            case 0x0:
                return x + y;
            case 0x1:
                return -x + y;
            case 0x2:
                return x - y;
            case 0x3:
                return -x - y;
            default:
                return 0; // never happens
        }
    }


    static int[] p =
    {
    151, 160, 137, 91, 90, 15, // Hash lookup table as defined by Ken Perlin.  This is a randomly
    131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, // arranged array of all numbers from 0-255 inclusive.
    190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33,
    88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166,
    77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244,
    102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196,
    135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123,
    5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42,
    223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
    129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228,
    251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107,
    49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254,
    138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180,
    151, 160, 137, 91, 90, 15, // Hash lookup table as defined by Ken Perlin.  This is a randomly
    131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, // arranged array of all numbers from 0-255 inclusive.
    190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33,
    88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166,
    77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244,
    102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196,
    135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123,
    5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42,
    223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
    129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228,
    251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107,
    49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254,
    138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
};

    static float lerp(float a, float b, float x)
    {
        return a + x * (b - a);
    }




}
