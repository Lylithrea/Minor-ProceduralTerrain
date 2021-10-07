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
    public int pointsPerAxis = 10;
    public int size;

    Vector3[] chunkVertexPositions;

    //Contains all the currently loaded chunks
    private Dictionary<Vector3, GameObject> allChunks = new Dictionary<Vector3, GameObject>();

    public Material terrainMaterial;
    private Vector3 currentChunk;


    Triangle[] triangles = new Triangle[1];

    //should be equal to computeshader numthreads
    int numThreads = 8;

    void Start()
    {
        //gain the positions of points within a chunk, this is always the same
        //we will use the chunks position to gain the correct values in world space
        GainChunkPositions();

        //set all the values within shaders that do not need to be updated every frame or when spawning a chunk.
        setupShaders();

        //spawn the chunks where we start.
        InitializeStartingChunks();
    }

    private void Update()
    {
        SpawnChunksBasedOnPlayerMovement();

    }

    private void SpawnChunksBasedOnPlayerMovement()
    {
        //check if players position has changed
        //check if the new chunks already exist
        //generate/destroy chunks based on position

        if (Mathf.Floor(player.transform.position.x / ((pointsPerAxis - 1) * size)) > currentChunk.x )
        {
            currentChunk.x += 1;
            Vector3 startPos = new Vector3(currentChunk.x + radius - 1, currentChunk.y - radius + 1, currentChunk.z - radius + 1);
            Vector3 endPos = new Vector3(currentChunk.x + radius - 1, currentChunk.y + radius - 1, currentChunk.z + radius - 1);
            CreateMultipleChunks(startPos, endPos);
            startPos.x = currentChunk.x - radius ;
            endPos.x = currentChunk.x - radius;
            DestroyMultipleChunks(startPos, endPos);
        }
        if (Mathf.Floor(player.transform.position.x / ((pointsPerAxis - 1) * size)) < currentChunk.x)
        {
            currentChunk.x -= 1;
            Vector3 startPos = new Vector3(currentChunk.x - radius + 1, currentChunk.y - radius + 1, currentChunk.z - radius + 1);
            Vector3 endPos = new Vector3(currentChunk.x - radius + 1, currentChunk.y + radius - 1, currentChunk.z + radius - 1);
            CreateMultipleChunks(startPos, endPos);
            startPos.x = currentChunk.x + radius;
            endPos.x = currentChunk.x + radius;
            DestroyMultipleChunks(startPos, endPos);
        }


        if (Mathf.Floor(player.transform.position.y / ((pointsPerAxis - 1) * size)) > currentChunk.y)
        {
            currentChunk.y += 1;
            Vector3 startPos = new Vector3(currentChunk.x - radius + 1, currentChunk.y + radius - 1, currentChunk.z - radius + 1);
            Vector3 endPos = new Vector3(currentChunk.x + radius - 1, currentChunk.y + radius - 1, currentChunk.z + radius - 1);
            CreateMultipleChunks(startPos, endPos);
            startPos.y = currentChunk.y - radius;
            endPos.y = currentChunk.y - radius;
            DestroyMultipleChunks(startPos, endPos);
        }
        if (Mathf.Floor(player.transform.position.y / ((pointsPerAxis - 1) * size)) < currentChunk.y)
        {
            currentChunk.y -= 1;
            Vector3 startPos = new Vector3(currentChunk.x - radius + 1, currentChunk.y - radius + 1, currentChunk.z - radius + 1);
            Vector3 endPos = new Vector3(currentChunk.x + radius - 1, currentChunk.y - radius + 1, currentChunk.z + radius - 1);
            CreateMultipleChunks(startPos, endPos);
            startPos.y = currentChunk.y + radius;
            endPos.y = currentChunk.y + radius;
            DestroyMultipleChunks(startPos, endPos);
        }

        if (Mathf.Floor(player.transform.position.z / ((pointsPerAxis - 1) * size)) > currentChunk.z)
        {
            currentChunk.z += 1;
            Vector3 startPos = new Vector3(currentChunk.x - radius + 1, currentChunk.y - radius + 1, currentChunk.z + radius - 1);
            Vector3 endPos = new Vector3(currentChunk.x + radius - 1, currentChunk.y + radius - 1, currentChunk.z + radius - 1);
            CreateMultipleChunks(startPos, endPos);
            startPos.z = currentChunk.z - radius;
            endPos.z = currentChunk.z - radius;
            DestroyMultipleChunks(startPos, endPos);
        }


        if (Mathf.Floor(player.transform.position.z / ((pointsPerAxis - 1) * size)) < currentChunk.z)
        {
            currentChunk.z -= 1;
            Vector3 startPos = new Vector3(currentChunk.x - radius + 1, currentChunk.y - radius + 1, currentChunk.z - radius + 1);
            Vector3 endPos = new Vector3(currentChunk.x + radius - 1, currentChunk.y + radius - 1, currentChunk.z - radius + 1);
            CreateMultipleChunks(startPos, endPos);
            startPos.z = currentChunk.z + radius;
            endPos.z = currentChunk.z + radius;
            DestroyMultipleChunks(startPos, endPos);
        }


    }

    public void CreateMultipleChunks(Vector3 startPos, Vector3 endPos)
    {
        for(float i = startPos.x; i <= endPos.x; i++)
        {
            for (float j = startPos.y; j <= endPos.y; j++)
            {
                for (float k = startPos.z; k <= endPos.z; k++)
                {
                    CreateChunk(new Vector3(i * (pointsPerAxis - 1) * size, j * (pointsPerAxis - 1) * size, k * (pointsPerAxis - 1) * size));
                }
            }
        }
    }

    public void DestroyMultipleChunks(Vector3 startPos, Vector3 endPos)
    {
        for (float i = startPos.x; i <= endPos.x; i++)
        {
            for (float j = startPos.y; j <= endPos.y; j++)
            {
                for (float k = startPos.z; k <= endPos.z; k++)
                {
                    DestroyChunk(new Vector3(i * (pointsPerAxis - 1) * size, j * (pointsPerAxis - 1) * size, k * (pointsPerAxis - 1) * size));
                }
            }
        }
    }


    /// <summary>
    /// Destroys chunk on given position, the chunks needs to be contained in the dictionairy 'allChunks', where it will get removed.
    /// </summary>
    /// <param name="position">The position on which the chunk needs to be destroyed.</param>
    public void DestroyChunk(Vector3 position)
    {
/*        position.x -= Mathf.Floor(position.x / (pointsPerAxis * size));
        position.y -= Mathf.Floor(position.y / (pointsPerAxis * size));
        position.z -= Mathf.Floor(position.z / (pointsPerAxis * size));*/

        if (allChunks.ContainsKey(position))
        {
            Destroy(allChunks[position]);
            allChunks.Remove(position);
        }
    }

    /// <summary>
    /// Creates a new chunk with mesh on given position.
    /// </summary>
    /// <param name="startingPos">The middle point of the chunk position in world space.</param>
    public void CreateChunk(Vector3 startingPos)
    {
/*        startingPos.x -= Mathf.Floor(startingPos.x /  (pointsPerAxis * size));
        startingPos.y -= Mathf.Floor(startingPos.y / (pointsPerAxis * size));
        startingPos.z -= Mathf.Floor(startingPos.z / (pointsPerAxis * size));*/

        if (!allChunks.ContainsKey(startingPos))
        {
            //generate a new chunk
            currentPosition = startingPos;
            GameObject chunk = new GameObject();

            chunk.transform.position = startingPos;
            chunk.AddComponent<Chunk>();
            noiseShader.SetVector("startingValue", startingPos);
            marchingCubeShader.SetVector("startingValue", startingPos);

            //generate noise <- compute shader
            //generate marching cubes <- compute shader
            Array.Clear(triangles, 0, triangles.Length);
            triangles = noiseGenerator();

            //set mesh <- main thread
            SetMesh(chunk);
            allChunks.Add(startingPos, chunk);
        }
    }

    /// <summary>
    /// Sets the mesh of given chunk, together with its triangles.
    /// </summary>
    /// <param name="chunk">The chunk which needs to be set. </param>
    void SetMesh(GameObject chunk)
    {
        MeshRenderer meshRenderer = setupMeshRenderer(chunk);
        MeshFilter meshFilter = setupMeshFilter(chunk);

        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        mesh.vertices = createVertices();
        mesh.triangles = createTriangles(createVertices().Length);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();


        meshRenderer.material = terrainMaterial;
    }


    /// <summary>
    /// Generates a noise and with the noise it will generate with the marching cube algorithm triangles
    /// </summary>
    /// <returns>An array of triangles used for meshes</returns>
    private Triangle[] noiseGenerator()
    {
        //in the future this might be updated dynamicly because of vertices points per chunk
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

        //get the values
        Vector4[] vertexPerlin = new Vector4[vertexPerlinResults];
        vertexPerlinBuffer.GetData(vertexPerlin);


        //release the buffers
        vertexPerlinBuffer.Release();

        //with the value we got, run the marching cube generator and return that.
        return marchingCubesGenerator(vertexPerlin);
    }

    /// <summary>
    /// Using a marching cube algorithm it will generate triangles based on positions of vertices and their appropiate values.
    /// </summary>
    /// <param name="vertexPerlin">An array which exists out of a position together with a value which decides if its terrain or not.</param>
    /// <returns></returns>
    private Triangle[] marchingCubesGenerator(Vector4[] vertexPerlin)
    {
        //in the future this might be updated dynamicly because of vertices points per chunk
        int numVoxelsPerAxis = pointsPerAxis - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCounter = numVoxels * 5;

        //create a buffer for the triangles
        ComputeBuffer triangleBuffer = new ComputeBuffer(maxTriangleCounter , sizeof(float)*3*3, ComputeBufferType.Append);
        triangleBuffer.SetCounterValue(0);
        marchingCubeShader.SetBuffer(0, "triangles", triangleBuffer);

        //creates a buffer for the input of the values
        ComputeBuffer vertexBuffer = new ComputeBuffer(vertexPerlin.Length ,sizeof(float) * 4);
        vertexBuffer.SetCounterValue(0);
        vertexBuffer.SetData(vertexPerlin);
        marchingCubeShader.SetBuffer(0, "vertexPerlin", vertexBuffer);

        //calculates how often the compute shader needs to be dispatched.
        float threadsPerAxis = (float)numVoxelsPerAxis / (float)numThreads;
        int dispatchAmount = Mathf.CeilToInt(threadsPerAxis);

        //dispatch the shader
        marchingCubeShader.Dispatch(0,dispatchAmount,dispatchAmount,dispatchAmount);

        //if we dont copy the count over, it means that if the size in the previous chunk was bigger, it wont overwrite the last values
        //this means we take over data from the previous chunk to the new chunk
        ComputeBuffer triangleCounter = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        ComputeBuffer.CopyCount(triangleBuffer, triangleCounter, 0);
        int[] triangleCountArray = { 0 };
        triangleCounter.GetData(triangleCountArray);
        int triangleAmount = triangleCountArray[0];

        //get the triangle data
        Triangle[] triangles = new Triangle[triangleAmount];
        triangleBuffer.GetData(triangles);

        //release all the buffers
        triangleCounter.Release();
        triangleBuffer.Release();
        vertexBuffer.Release();

        //return the triangles we created.
        return triangles;
    }


    /// <summary>
    /// Sets all constant values for the chunk
    /// </summary>
    private void setupShaders()
    {
        noiseShader.SetFloat("chunkSize", pointsPerAxis * pointsPerAxis * pointsPerAxis);
        noiseShader.SetFloat("size", size);
        noiseShader.SetFloat("noiseScale", scale);
        noiseShader.SetFloat("repeat", 9);
        noiseShader.SetInt("pointsPerAxis", pointsPerAxis);

        marchingCubeShader.SetInt("pointsPerAxis", pointsPerAxis);
        marchingCubeShader.SetFloat("cutoff", cutoff);
        marchingCubeShader.SetFloat("groundLevel", groundLevel);
        marchingCubeShader.SetFloat("layerThickness", layerThickness);
    }

    /// <summary>
    /// Generates the starting chunks when starting the world
    /// </summary>
    private void InitializeStartingChunks()
    {
        //square radius
        int chunkSize = (radius + radius - 1);
        int chunkRadius = chunkSize * chunkSize * chunkSize;

        //spawn chunks based on players position
        //the position of a chunk is at the 0,0,0 position of the chunk (thus not the middle of the chunk)
        Vector3 chunkpos = new Vector3();
        chunkpos.x = Mathf.Floor(player.transform.position.x / (pointsPerAxis * size));
        chunkpos.y = Mathf.Floor(player.transform.position.y / (pointsPerAxis * size));
        chunkpos.z = Mathf.Floor(player.transform.position.z / (pointsPerAxis * size));
        currentChunk = chunkpos;

        for (int i = 0; i < chunkRadius; i++)
        {
            float r = i % chunkSize;
            float h = Mathf.FloorToInt((i / chunkSize) % chunkSize);
            float c = Mathf.FloorToInt(i / (chunkSize * chunkSize));


            Vector3 newpos = new Vector3();
            newpos.x = (chunkpos.x - radius + 1 + r) * size * (pointsPerAxis - 1);
            newpos.y = (chunkpos.y - radius + 1 + h) * size * (pointsPerAxis - 1);
            newpos.z = (chunkpos.z - radius + 1 + c) * size * (pointsPerAxis - 1);

            CreateChunk(newpos);
        }

    }

    /// <summary>
    /// Calculates all vertex points within a chunk at the start, since every vertex position is the same within chunk.
    /// </summary>
    private void GainChunkPositions()
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
    /// Sets up mesh renderer of given gameobject. Either gets the mesh renderer or creates one.
    /// </summary>
    /// <param name="chunk">Given gameobject which needs to contain a mesh renderer.</param>
    /// <returns></returns>
    private MeshRenderer setupMeshRenderer(GameObject chunk)
    {
        MeshRenderer meshRenderer = chunk.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            chunk.AddComponent<MeshRenderer>();
            meshRenderer = chunk.GetComponent<MeshRenderer>();
        }
        return meshRenderer;
    }

    /// <summary>
    /// Sets up mesh filter of given gameobject. Either gets the mesh filter or creates one.
    /// </summary>
    /// <param name="chunk">Given gameobject which needs to contain a mesh filter.</param>
    /// <returns></returns>
    private MeshFilter setupMeshFilter(GameObject chunk)
    {
        MeshFilter meshFilter = chunk.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            chunk.AddComponent<MeshFilter>();
            meshFilter = chunk.GetComponent<MeshFilter>();
        }
        return meshFilter;
    }

    /// <summary>
    /// Creates a Vector3 array from a triangles array, so it can be used for a mesh.
    /// </summary>
    /// <returns>A vector3 array with vertex positions in correct order for triangles.</returns>
    private Vector3[] createVertices()
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
    private int[] createTriangles(int amount)
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
    struct Triangle
    {
        public Vector3 VertexA;
        public Vector3 VertexB;
        public Vector3 VertexC;
    };


}
