using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generation : MonoBehaviour
{
    public GameObject player;
    private Vector3 currentPosition;

    [Header("Noise Settings")]
    public ComputeShader noiseShader;
    public float scale;

    [Header("Generation Settings")]
    public int radius;
    public int groundLevel;
    public int layerThickness;
    public int cutoff;

    [Header("Chunk Settings")]
    public ComputeShader marchingCubeShader;
    public int row;
    public int column;
    public int height;
    public int pointsPerAxis = 10;
    public int size;

    private List<Vector3[]> chunkPositions = new List<Vector3[]>();
    Vector3[] chunkVertexPositions;

    private ComputeBuffer noiseCubes;
    private ComputeBuffer vertexPositionBuffer;

    public GameObject sphere;
    public Material mat1;
    public Material mat2;
    public Material mat3;
    public Material mat4;
    public Material mat5;


    void Start()
    {
        //1) update variables in shaders/scripts
        //2) calculate the positions of chunk points
        //3) spawn starting chunks



        //gain the positions of points within a chunk, this is always the same
        //we will use the chunks position to gain the correct values in world space
        GainChunkPositions();

        setupNoiseShader();

        InitializeStartingChunks();
    }

    private void Update()
    {

        //check if players position has changed
        //check if the new chunks already exist
        //generate/destroy chunks based on position

/*        if (player.transform.position.x > currentPosition.x + (row * size) / 2)
        {
            Vector3 newPosition = new Vector3(currentPosition.x + row * size, currentPosition.y, currentPosition.z);
            CreateChunk(newPosition);
        }
        if (player.transform.position.x < currentPosition.x - (row * size) / 2)
        {
            Vector3 newPosition = new Vector3(currentPosition.x - row * size, currentPosition.y, currentPosition.z);
            CreateChunk(newPosition);
        }


        if (player.transform.position.y > currentPosition.y + (height * size) / 2)
        {
            Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y + height * size, currentPosition.z);
            CreateChunk(newPosition);
        }
        if (player.transform.position.y < currentPosition.y - (height * size) / 2)
        {
            Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y - height * size, currentPosition.z);
            CreateChunk(newPosition);
        }


        if (player.transform.position.z > currentPosition.z + (column * size) / 2)
        {
            Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z + column * size);
            CreateChunk(newPosition);
        }
        if (player.transform.position.z < currentPosition.z - (column * size) / 2)
        {
            Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z - column * size);
            CreateChunk(newPosition);
        }*/
    }


    private void setupNoiseShader()
    {
        noiseShader.SetFloat("chunkSize", row*height*column);
        noiseShader.SetFloat("row", row);
        noiseShader.SetFloat("height", height);
        noiseShader.SetFloat("column", column);
        noiseShader.SetFloat("size", size);
        noiseShader.SetFloat("noiseScale", scale);
        noiseShader.SetFloat("repeat", 9);
        noiseShader.SetInt("pointsPerAxis", pointsPerAxis);

        marchingCubeShader.SetInt("pointsPerAxis", pointsPerAxis);
        marchingCubeShader.SetFloat("cutoff", cutoff);


        //noiseBuffer = new ComputeBuffer();
    }

    private void InitializeStartingChunks()
    {
        //square radius
        int chunkSize = (radius + radius - 1);
        int chunkRadius = chunkSize * chunkSize * chunkSize;
/*
        for(int i = 0; i < chunkRadius; i++)
        {
            float r = i % chunkSize;
            float h = Mathf.FloorToInt((i / chunkSize) % chunkSize);
            float c = Mathf.FloorToInt(i / (chunkSize * chunkSize));

            CreateChunk(new Vector3(r * row * size, h * height * size, c * column * size));
        }*/
        CreateChunk(new Vector3(0,0,0));
    }


    private void GainChunkPositions()
    {
        int chunkSize = row * column * height;

        chunkVertexPositions = new Vector3[chunkSize];
        for (int i = 0; i < chunkSize; i++) //triple foreach loop condensed into 1 for loop
        {
            float r = i % row;
            float h = Mathf.FloorToInt((i / height) % height);
            float c = Mathf.FloorToInt(i / (height * row));

            /*            Vector3[] newPositions = new Vector3[8];
                        newPositions[0] = new Vector3(r * size, (h + 1) * size, c * size);
                        newPositions[1] = new Vector3((r + 1) * size, (h + 1) * size, c * size);
                        newPositions[2] = new Vector3((r + 1) * size, h * size, c * size);
                        newPositions[3] = new Vector3(r * size, h * size, c * size);
                        newPositions[4] = new Vector3(r * size, (h + 1) * size, (c + 1) * size);
                        newPositions[5] = new Vector3((r + 1) * size, (h + 1) * size, (c + 1) * size);
                        newPositions[6] = new Vector3((r + 1) * size, h * size, (c + 1) * size);
                        newPositions[7] = new Vector3(r * size, h * size, (c + 1) * size);
                        chunkPositions.Add(newPositions);*/

            chunkVertexPositions[i] = new Vector3(r * size, h * size, c * size);
        }

    }

    public void CreateChunk(Vector3 startingPos)
    {
        //get vertex positions <- variable
        //generate noise <- compute shader
        //generate marching cubes <- compute shader
        //set mesh <- main thread
        currentPosition = startingPos;

        GameObject chunk = new GameObject();
        chunk.transform.position = startingPos;
        chunk.AddComponent<Chunk>();
        noiseGenerator();
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

    private void noiseGenerator()
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

        for(int i = 0; i < vertexPerlin.Length; i++)
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
        }

        marchingCubesGenerator(vertexPerlin);
    }
    struct Triangle
    {
        Vector3 VertexA;
        Vector3 VertexB;
        Vector3 VertexC;
    };



    private void marchingCubesGenerator(Vector4[] vertexPerlin)
    {
        int numVoxelsPerAxis = pointsPerAxis - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCounter = numVoxels * 5;
        ComputeBuffer triangleBuffer = new ComputeBuffer(maxTriangleCounter , sizeof(float)*3*3, ComputeBufferType.Append);
        triangleBuffer.SetCounterValue(0);
        //marchingCubeShader.SetBuffer(0, "triangles", triangleBuffer);

        ComputeBuffer vertexBuffer = new ComputeBuffer(vertexPerlin.Length ,sizeof(float) * 4);
        marchingCubeShader.SetBuffer(0, "vertexPerlin", vertexBuffer);

        marchingCubeShader.Dispatch(0,1,1,1);

        Triangle[] triangles = new Triangle[maxTriangleCounter];

        triangleBuffer.GetData(triangles);


        triangleBuffer.Release();
        vertexBuffer.Release();

        //marchingCubeShader
    }

}
