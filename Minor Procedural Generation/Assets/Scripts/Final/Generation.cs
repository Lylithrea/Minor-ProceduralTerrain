using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Generation : MonoBehaviour
{
    public GameObject player;
    private Vector3 currentPosition;

    [Header("Noise Settings")]
    public ComputeShader noiseShader;
    [Range(0.43f, 0.47f)]
    public float scale;

    [Header("Generation Settings")]
    public int radius;
    public int groundLevel;
    public int layerThickness;
    public float cutoff;

    [Header("Chunk Settings")]
    public ComputeShader marchingCubeShader;
    public int row;
    public int column;
    public int height;
    public int pointsPerAxis = 10;
    public int size;

    private List<Vector3[]> chunkPositions = new List<Vector3[]>();
    Vector3[] chunkVertexPositions;


    private Dictionary<Vector3, GameObject> allChunks = new Dictionary<Vector3, GameObject>();

    public GameObject sphere;
    public Material mat1;
    public Material mat2;
    public Material mat3;
    public Material mat4;
    public Material mat5;
    public Material triangleMat;


    void Start()
    {
        //1) update variables in shaders/scripts
        //2) calculate the positions of chunk points
        //3) spawn starting chunks



        //gain the positions of points within a chunk, this is always the same
        //we will use the chunks position to gain the correct values in world space
        GainChunkPositions();

        setupShaders();

        InitializeStartingChunks();
    }

    private void Update()
    {

        //check if players position has changed
        //check if the new chunks already exist
        //generate/destroy chunks based on position

        if (player.transform.position.x > currentPosition.x + (pointsPerAxis + 6) * size / 2) 
        {
            Vector3 newPosition = new Vector3(currentPosition.x + (pointsPerAxis - 2) * size, currentPosition.y, currentPosition.z);
            CreateChunk(newPosition);
            DestroyChunk(new Vector3(currentPosition.x - (pointsPerAxis - 2) * size, currentPosition.y, currentPosition.z));
        }
        if (player.transform.position.x < currentPosition.x - (pointsPerAxis - 6) * size / 2)
        {
            Vector3 newPosition = new Vector3(currentPosition.x - (pointsPerAxis - 2) * size, currentPosition.y, currentPosition.z);
            CreateChunk(newPosition);
            DestroyChunk(new Vector3(currentPosition.x + (pointsPerAxis - 2) * size, currentPosition.y, currentPosition.z));
        }


        if (player.transform.position.y > currentPosition.y + (pointsPerAxis + 6) * size / 2)
        {
            Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y + (pointsPerAxis - 2) * size, currentPosition.z);
            CreateChunk(newPosition);
            DestroyChunk(new Vector3(currentPosition.x, currentPosition.y - (pointsPerAxis - 2) * size, currentPosition.z));
        }
        if (player.transform.position.y < currentPosition.y - (pointsPerAxis - 6) * size / 2)
        {
            Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y - (pointsPerAxis - 2) * size, currentPosition.z);
            CreateChunk(newPosition);
            DestroyChunk(new Vector3(currentPosition.x, currentPosition.y + (pointsPerAxis - 2) * size, currentPosition.z));
        }


        if (player.transform.position.z > currentPosition.z + (pointsPerAxis + 6) * size / 2)
        {
            Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z + (pointsPerAxis - 2) * size);
            CreateChunk(newPosition);
            DestroyChunk(new Vector3(currentPosition.x, currentPosition.y, currentPosition.z - (pointsPerAxis - 2) * size));
        }
        if (player.transform.position.z < currentPosition.z - (pointsPerAxis - 6) * size / 2)
        {
            Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z - (pointsPerAxis - 2) * size);
            CreateChunk(newPosition);
            DestroyChunk(new Vector3(currentPosition.x, currentPosition.y, currentPosition.z + (pointsPerAxis - 2) * size));
        }
    }


    private void setupShaders()
    {
        noiseShader.SetFloat("chunkSize", pointsPerAxis * pointsPerAxis * pointsPerAxis);
        noiseShader.SetFloat("row", row);
        noiseShader.SetFloat("height", height);
        noiseShader.SetFloat("column", column);
        noiseShader.SetFloat("size", size);
        noiseShader.SetFloat("noiseScale", scale);
        noiseShader.SetFloat("repeat", 9);
        noiseShader.SetInt("pointsPerAxis", pointsPerAxis);

        marchingCubeShader.SetInt("pointsPerAxis", pointsPerAxis);
        marchingCubeShader.SetFloat("cutoff", cutoff);
    }

    private void InitializeStartingChunks()
    {
        //square radius
        int chunkSize = (radius + radius - 1);
        int chunkRadius = chunkSize * chunkSize * chunkSize;
        currentPosition = new Vector3(0 - (pointsPerAxis -2 )* size / 2, 0 - (pointsPerAxis - 2) * size / 2, 0 - (pointsPerAxis - 2) * size / 2);

/*        for(int i = 0; i < chunkRadius; i++)
        {
            float r = i % chunkSize;
            float h = Mathf.FloorToInt((i / chunkSize) % chunkSize);
            float c = Mathf.FloorToInt(i / (chunkSize * chunkSize));

            CreateChunk(new Vector3(r * pointsPerAxis * size , h * pointsPerAxis * size, c * pointsPerAxis * size));
        }*/
        CreateChunk(currentPosition);
    }


    private void GainChunkPositions()
    {
        //sets all the chunk positions since its always the same in each chunk
        int chunkSize = row * column * height;

        chunkVertexPositions = new Vector3[chunkSize];
        for (int i = 0; i < chunkSize; i++) //triple foreach loop condensed into 1 for loop
        {
            float r = i % row;
            float h = Mathf.FloorToInt((i / height) % height);
            float c = Mathf.FloorToInt(i / (height * row));
            chunkVertexPositions[i] = new Vector3(r * size, h * size, c * size);
        }

    }

    public void DestroyChunk(Vector3 position)
    {
        if(allChunks.ContainsKey(position))
        {
            Destroy(allChunks[position]);
            allChunks.Remove(position);
        }
    }

    public void CreateChunk(Vector3 startingPos)
    {
        //get vertex positions <- variable

        //generate marching cubes <- compute shader
        //set mesh <- main thread
        currentPosition = startingPos;

        GameObject chunk = new GameObject();
        chunk.transform.position = startingPos;
        chunk.AddComponent<Chunk>();
        noiseShader.SetVector("startingValue", startingPos);
        //generate noise <- compute shader
        //generate marching cubes <- compute shader
        Triangle[] triangles = noiseGenerator();

        SetMesh(chunk, triangles);
        allChunks.Add(startingPos, chunk);
    }

    void SetMesh(GameObject chunk, Triangle[] triangles)
    {
        MeshRenderer meshRenderer = chunk.GetComponent<MeshRenderer>();
        if(meshRenderer == null)
        {
            chunk.AddComponent<MeshRenderer>();
            meshRenderer = chunk.GetComponent<MeshRenderer>();
        }

        meshRenderer.material = mat3;

        MeshFilter meshFilter = chunk.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            chunk.AddComponent<MeshFilter>();
            meshFilter = chunk.GetComponent<MeshFilter>();
        }

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[triangles.Length * 3];
        for(int i = 0; i < triangles.Length; i++)
        {
            vertices[i * 3 + 0] = triangles[i].VertexA;
            vertices[i * 3 + 1] = triangles[i].VertexB;
            vertices[i * 3+ 2] = triangles[i].VertexC;
        }

        int[] newTriangles = new int[vertices.Length];
        for (int i = 0; i < newTriangles.Length; i++)
        {
            newTriangles[i] = i;
        }

        meshFilter.mesh = mesh;

        mesh.vertices = vertices;
        mesh.triangles = newTriangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        Array.Clear(vertices, 0, vertices.Length);
        Array.Clear(newTriangles, 0, newTriangles.Length);

    }

    struct cube
    {
        public Vector3 position;
        public float noise;
    };

    private void noiseGeneratorOld()
    {
        /*        cube[] cubes = new cube[chunkVertexPositions.Length];

        noiseCubes = new ComputeBuffer(chunkVertexPositions.Length, sizeof(float) * 4);
        noiseShader.SetBuffer(0, "cubes", noiseCubes);

        noiseShader.SetVector("startingValue", startingPos);

        vertexPositionBuffer = new ComputeBuffer(chunkVertexPositions.Length, sizeof(float) * 3);

        vertexPositionBuffer.SetData(chunkVertexPositions);
        noiseShader.SetBuffer(0, "vertexPositions", vertexPositionBuffer);
*/

        //ComputeBuffer results = new ComputeBuffer(testResults, sizeof(float) * 3, ComputeBufferType.Append);
        //Vector3[] test = new Vector3[testResults];


        //Debug.Log("Test result at position " + i + " : (" + test[i].x + "," + test[i].y + "," + test[i].z + ") with noise value : " + test[i].w );

        //Debug.Log("Test results amount : " + test.Length);

        //noiseCubes.GetData(cubes);
        /*        vertexPositionBuffer.Release();
                noiseCubes.Release();*/

    }


    private void testNoiseGenerator()
    {

        //should be equal to computeshader numthreads
        int numThreads = 8;

        float threadsPerAxis = (float)pointsPerAxis / (float)numThreads;

        int dispatchAmount = Mathf.CeilToInt(threadsPerAxis);

        //generate the size of the list for all the points
        int testResults = Mathf.CeilToInt(threadsPerAxis * threadsPerAxis * threadsPerAxis * numThreads * numThreads * numThreads);


        ComputeBuffer results = new ComputeBuffer(testResults, sizeof(float) * 4, ComputeBufferType.Append);

        //reset the counter value because else it starts where it left off previous run
        results.SetCounterValue(0);
        noiseShader.SetBuffer(0, "vertexPerlin", results);

        //how often the shader will get dispatched in each direction
        //(if dispatchamount and threads are 8, it will mean that the code will totally be run 8x8x8x2x2x2.)
        noiseShader.Dispatch(0, dispatchAmount, dispatchAmount, dispatchAmount);

        Vector4[] test = new Vector4[testResults];

        results.GetData(test);
        results.Release();

        for (int i = 0; i < test.Length; i++)
        {

            GameObject newSphere = Instantiate(sphere);
            newSphere.transform.position = new Vector3(test[i].x, test[i].y, test[i].z);
            if (test[i].w < -0.6f)
            {
                newSphere.GetComponent<MeshRenderer>().material = mat1;
            }
            else if (test[i].w < -0.2f)
            {
                newSphere.GetComponent<MeshRenderer>().material = mat2;
            }
            else if (test[i].w < 0.2f)
            {
                newSphere.GetComponent<MeshRenderer>().material = mat3;
            }
            else if (test[i].w < 0.6f)
            {
                newSphere.GetComponent<MeshRenderer>().material = mat4;
            }
            else
            {
                newSphere.GetComponent<MeshRenderer>().material = mat5;
            }
        }
    }

    private Triangle[] noiseGenerator()
    {

        //should be equal to computeshader numthreads
        int numThreads = 8;

        float threadsPerAxis = (float)pointsPerAxis / (float)numThreads;

        int dispatchAmount = Mathf.CeilToInt(threadsPerAxis);

        //generate the size of the list for all the points
        int vertexPerlinResults = pointsPerAxis * pointsPerAxis * pointsPerAxis;
        ComputeBuffer vertexPerlinBuffer = new ComputeBuffer(vertexPerlinResults, sizeof(float) * 4);

        //reset the counter value because else it starts where it left off previous run
        vertexPerlinBuffer.SetCounterValue(0);
        noiseShader.SetBuffer(0, "vertexPerlin", vertexPerlinBuffer);

        //how often the shader will get dispatched in each direction
        //(if dispatchamount and threads are 8, it will mean that the code will totally be run 8x8x8x2x2x2.)
        noiseShader.Dispatch(0, dispatchAmount, dispatchAmount, dispatchAmount);

        Vector4[] vertexPerlin = new Vector4[vertexPerlinResults];

        vertexPerlinBuffer.GetData(vertexPerlin);
        vertexPerlinBuffer.Release();

/*        for(int i = 0; i < vertexPerlin.Length; i++)
        {

            GameObject newSphere = Instantiate(sphere);
            newSphere.transform.position = new Vector3(vertexPerlin[i].x, vertexPerlin[i].y, vertexPerlin[i].z);
            if(vertexPerlin[i].w < -0.6f)
            {
                newSphere.GetComponent<MeshRenderer>().material = mat1;
            }
            else if(vertexPerlin[i].w < -0.2f)
            {
                newSphere.GetComponent<MeshRenderer>().material = mat2;
            }
            else if(vertexPerlin[i].w < 0.2f)
            {
                newSphere.GetComponent<MeshRenderer>().material = mat3;
            }
            else if(vertexPerlin[i].w < 0.6f)
            {
                newSphere.GetComponent<MeshRenderer>().material = mat4;
            }
            else
            {
                newSphere.GetComponent<MeshRenderer>().material = mat5;
            }
        }*/

        return marchingCubesGenerator(vertexPerlin);
    }


    struct Triangle
    {
        public Vector3 VertexA;
        public Vector3 VertexB;
        public Vector3 VertexC;
    };

    

    private Triangle[] marchingCubesGenerator(Vector4[] vertexPerlin)
    {
        int numVoxelsPerAxis = pointsPerAxis - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCounter = numVoxels * 5;

        ComputeBuffer triangleBuffer = new ComputeBuffer(maxTriangleCounter , sizeof(float)*3*3, ComputeBufferType.Append);
        triangleBuffer.SetCounterValue(0);
        marchingCubeShader.SetBuffer(0, "triangles", triangleBuffer);

        ComputeBuffer vertexBuffer = new ComputeBuffer(vertexPerlin.Length ,sizeof(float) * 4);
        vertexBuffer.SetCounterValue(0);
        vertexBuffer.SetData(vertexPerlin);
        Array.Clear(vertexPerlin, 0, vertexPerlin.Length);
        marchingCubeShader.SetBuffer(0, "vertexPerlin", vertexBuffer);

        int numThreads = 8;

        float threadsPerAxis = (float)numVoxelsPerAxis / (float)numThreads;

        int dispatchAmount = Mathf.CeilToInt(threadsPerAxis);

        marchingCubeShader.Dispatch(0,dispatchAmount,dispatchAmount,dispatchAmount);


        Triangle[] triangles = new Triangle[maxTriangleCounter];
        triangleBuffer.GetData(triangles);


        triangleBuffer.Release();
        vertexBuffer.Release();


        return triangles;
    }

}
