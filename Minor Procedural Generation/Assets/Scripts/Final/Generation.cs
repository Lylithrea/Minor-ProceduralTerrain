using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Noise;
using MarchCubes;
using System.Threading;


public class Generation : MonoBehaviour
{
    public GameObject player;
    private Vector3 currentPosition;

    [Header("Noise Settings")]
    public ComputeShader noiseShader;
    //[Range(0.43f, 0.47f)]
    public float scale;
    public float groundLevelHeight = 250;
    public float frequency = 25;

    [Header("Generation Settings")]
    public int radius;
    public int groundLevel;
    public int layerThickness;
    public float cutoff;

    [Header("Chunk Settings")]
    public ComputeShader marchingCubeShader;
    public int pointsPerAxis = 10;
    public int size;
    [Range(0,2)]
    public int levelOfDetail;
    public int levelOfDetailRadius;

    Vector3[] chunkVertexPositions;

    //Contains all the currently loaded chunks
    private Dictionary<Vector3, GameObject> allChunks = new Dictionary<Vector3, GameObject>();
    private List<Vector3> currentPlayerChunks = new List<Vector3>();

    public Material terrainMaterial;
    public Vector3 currentChunk;

    private Queue<Vector3> chunkQueue = new Queue<Vector3>();
    private Queue<Vector3> destroyChunkQueue = new Queue<Vector3>();
    private Queue<GameObject> reusableChunkQueue = new Queue<GameObject>();
    private Queue<Chunks> updateQueue = new Queue<Chunks>();


    public Triangle[] triangles = new Triangle[1];

    //should be equal to computeshader numthreads
    int numThreads = 8;
    private bool spawningChunksRunning = false;
    private bool destroyingChunksRunning = false;

    public Texture groundTexture;

    struct Chunks
    {
        public GameObject chunk;
        public int points;
    }

    void Start()
    {

        //gain the positions of points within a chunk, this is always the same
        //we will use the chunks position to gain the correct values in world space
        GainChunkPositions();

        //set all the values within shaders that do not need to be updated every frame or when spawning a chunk.
        setupValues();



        //spawn the chunks where we start.
        InitializeStartingChunks();

        
    }


    private void Update()
    {
        //SpawnChunksBasedOnPlayerMovement();
        testDynamicChunks();

   /*     if (!destroyingChunksRunning)
        {
            checkDestroyingChunks();
        }*/
    }

    Vector3 playerPos = new Vector3(0,0,0);
    private void testDynamicChunks()
    {
        playerPos.x = Mathf.Floor(player.transform.position.x / ((pointsPerAxis - 1) * size));
        playerPos.y = Mathf.Floor(player.transform.position.y / ((pointsPerAxis - 1) * size));
        playerPos.z = Mathf.Floor(player.transform.position.z / ((pointsPerAxis -1 ) * size));
        //so it does not update every update loop
        if (playerPos != currentChunk)
        {
            currentChunk = playerPos;
            UpdateChunks();
        }
    }

    private void UpdateChunks()
    {
        int minX = (int)currentChunk.x - radius + 1;
        int maxX = (int)currentChunk.x + radius;

        int minY = (int)currentChunk.y - radius + 1;
        int maxY = (int)currentChunk.y + radius;

        int minZ = (int)currentChunk.z - radius + 1;
        int maxZ = (int)currentChunk.z + radius;

        chunkQueue.Clear();
        currentPlayerChunks.Clear();

        for ( int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                for (int z = minZ; z < maxZ; z++)
                {
                    if(!allChunks.ContainsKey(new Vector3(x, y, z)))
                    {
                        chunkQueue.Enqueue(new Vector3(x, y, z));
                        currentPlayerChunks.Add(new Vector3(x, y, z));
                    }
                }
            }
        }


        if (!spawningChunksRunning)
        {
            StartCoroutine(SpawnChunks());
        }



    }

    void checkDestroyingChunks()
    {
        destroyChunkQueue.Clear();
        foreach (KeyValuePair<Vector3, GameObject> chunk in allChunks)
        {
            if (!currentPlayerChunks.Contains(chunk.Key))
            {
                destroyChunkQueue.Enqueue(chunk.Key);
            }
        }
        if (!destroyingChunksRunning)
        {
            StartCoroutine(DestroyChunks());
        }
    }

    IEnumerator SpawnChunks()
    {
        spawningChunksRunning = true;
        while (chunkQueue.Count > 0)
        {
            Vector3 chunkPos = chunkQueue.Dequeue();
            if (reusableChunkQueue.Count > 0)
            {
                GameObject chunk = reusableChunkQueue.Dequeue();
                UpdateChunk(chunk, chunkPos);
            }
            else
            {
                CreateChunk(chunkPos);
            }
            yield return null;
        }
        StartCoroutine(UpdateChunkMeshes());
        //checkDestroyingChunks();
/*        if (!destroyingChunksRunning) {
            StartCoroutine(DestroyChunks());
        }*/
        spawningChunksRunning = false;
    }

    IEnumerator DestroyChunks()
    {
        destroyingChunksRunning = true;
        while (destroyChunkQueue.Count > 0)
        {
            Vector3 chunkPos = destroyChunkQueue.Dequeue();
            DestroyChunk(chunkPos);
            yield return null;
        }

        destroyingChunksRunning = false;
    }

    IEnumerator UpdateChunkMeshes()
    {
        while(updateQueue.Count > 0)
        {
            Chunks chunk = updateQueue.Dequeue();
            UpdateMesh(chunk.chunk, chunk.points);
            yield return null;
        }
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
            startPos = new Vector3(currentChunk.x - levelOfDetailRadius, currentChunk.y - levelOfDetailRadius + 1, currentChunk.z - levelOfDetailRadius + 1);
            endPos = new Vector3(currentChunk.x - levelOfDetailRadius, currentChunk.y + levelOfDetailRadius - 1, currentChunk.z + levelOfDetailRadius - 1);
            UpdateMultipleMeshes(startPos, endPos);
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
            startPos = new Vector3(currentChunk.x + levelOfDetailRadius, currentChunk.y - levelOfDetailRadius + 1, currentChunk.z - levelOfDetailRadius + 1);
            endPos = new Vector3(currentChunk.x + levelOfDetailRadius, currentChunk.y + levelOfDetailRadius - 1, currentChunk.z + levelOfDetailRadius - 1);
            UpdateMultipleMeshes(startPos, endPos);
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
            startPos = new Vector3(currentChunk.x - levelOfDetailRadius + 1, currentChunk.y - levelOfDetailRadius, currentChunk.z - levelOfDetailRadius + 1);
            endPos = new Vector3(currentChunk.x + levelOfDetailRadius - 1, currentChunk.y - levelOfDetailRadius, currentChunk.z + levelOfDetailRadius - 1);
            UpdateMultipleMeshes(startPos, endPos);
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
            startPos = new Vector3(currentChunk.x - levelOfDetailRadius + 1, currentChunk.y + levelOfDetailRadius, currentChunk.z - levelOfDetailRadius + 1);
            endPos = new Vector3(currentChunk.x + levelOfDetailRadius - 1, currentChunk.y + levelOfDetailRadius, currentChunk.z + levelOfDetailRadius - 1);
            UpdateMultipleMeshes(startPos, endPos);
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
            startPos = new Vector3(currentChunk.x - levelOfDetailRadius + 1, currentChunk.y - levelOfDetailRadius + 1, currentChunk.z - levelOfDetailRadius);
            endPos = new Vector3(currentChunk.x + levelOfDetailRadius - 1, currentChunk.y + levelOfDetailRadius - 1, currentChunk.z - levelOfDetailRadius);
            UpdateMultipleMeshes(startPos, endPos);
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
            startPos = new Vector3(currentChunk.x - levelOfDetailRadius + 1, currentChunk.y - levelOfDetailRadius + 1, currentChunk.z + levelOfDetailRadius);
            endPos = new Vector3(currentChunk.x + levelOfDetailRadius - 1, currentChunk.y + levelOfDetailRadius - 1, currentChunk.z + levelOfDetailRadius);
            UpdateMultipleMeshes(startPos, endPos);
        }


    }


    public void UpdateMultipleMeshes(Vector3 startPos, Vector3 endPos)
    {
        for (float i = startPos.x; i <= endPos.x; i++)
        {
            for (float j = startPos.y; j <= endPos.y; j++)
            {
                for (float k = startPos.z; k <= endPos.z; k++)
                {
                    Chunks newchunk;
                    newchunk.chunk = allChunks[new Vector3(i, j, k)];
                    newchunk.points = levelOfDetail + 1;
                    updateQueue.Enqueue(newchunk);
                    //UpdateMesh(allChunks[new Vector3(i,j,k)], 1);
                }
            }
        }
    }


    public void CreateMultipleChunks(Vector3 startPos, Vector3 endPos)
    {
        //update vertex count
        if (levelOfDetailRadius > 0)
        {
            int levelOfDetailSize = (levelOfDetailRadius + levelOfDetailRadius - 1);
            int chunkChecks = levelOfDetailSize * levelOfDetailSize * levelOfDetailSize;
            for (int i = 0; i < chunkChecks; i++)
            {
                float r = i % levelOfDetailSize;
                float h = Mathf.FloorToInt((i / levelOfDetailSize) % levelOfDetailSize);
                float c = Mathf.FloorToInt(i / (levelOfDetailSize * levelOfDetailSize));

                Vector3 checkChunkPos = currentChunk;
                checkChunkPos.x = checkChunkPos.x - levelOfDetailRadius + 1 + r;
                checkChunkPos.y = checkChunkPos.y - levelOfDetailRadius + 1 + h;
                checkChunkPos.z = checkChunkPos.z - levelOfDetailRadius + 1 + c;

                //Debug.Log("Chunk pos: " + checkChunkPos);

                if (allChunks.ContainsKey(checkChunkPos))
                {
                    Chunks newchunk;
                    newchunk.chunk = allChunks[checkChunkPos];
                    newchunk.points = levelOfDetail + 1;
                    updateQueue.Enqueue(newchunk);
                    //UpdateMesh(allChunks[checkChunkPos], levelOfDetail + 1);
                }
            }

        }


        for(float i = startPos.x; i <= endPos.x; i++)
        {
            for (float j = startPos.y; j <= endPos.y; j++)
            {
                for (float k = startPos.z; k <= endPos.z; k++)
                {
                    //CreateChunk(new Vector3(i , j , k ));
                    chunkQueue.Enqueue(new Vector3(i, j, k));
                }
            }
        }
        if (!spawningChunksRunning)
        {
            StartCoroutine(SpawnChunks());
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
                    //DestroyChunk(new Vector3(i, j , k));
                    destroyChunkQueue.Enqueue(new Vector3(i, j, k));
                    //reusableChunkQueue.Enqueue(allChunks[new Vector3(i, j, k)]);
                }
            }
        }
        if (!destroyingChunksRunning)
        {
            StartCoroutine(DestroyChunks());
        }
    }


    /// <summary>
    /// Destroys chunk on given position, the chunks needs to be contained in the dictionairy 'allChunks', where it will get removed.
    /// </summary>
    /// <param name="position">The position on which the chunk needs to be destroyed.</param>
    public void DestroyChunk(Vector3 position)
    {
        if (allChunks.ContainsKey(position))
        {
            //Destroy(allChunks[position]);
            reusableChunkQueue.Enqueue(allChunks[position]);
            allChunks.Remove(position);
        }
    }

    /// <summary>
    /// Creates a new chunk with mesh on given position.
    /// </summary>
    /// <param name="startingChunk">The middle point of the chunk position in world space.</param>
    public void CreateChunk(Vector3 startingChunk)
    {
        if (!allChunks.ContainsKey(startingChunk))
        {
            //generate a new chunk

            GameObject chunk = new GameObject();
            chunk.name = "Chunk (" + startingChunk.x + "," + startingChunk.y + "," + startingChunk.z + ")";

            Vector3 chunkPosition = startingChunk * (pointsPerAxis - 1) * size;
            chunk.transform.position = chunkPosition;
            chunk.AddComponent<Chunk>();
            chunk.GetComponent<Chunk>().generator = this;

            noiseShader.SetVector("startingValue", chunkPosition);
            marchingCubeShader.SetVector("startingValue", chunkPosition);

            //generate noise <- compute shader
            //generate marching cubes <- compute shader
            Array.Clear(triangles, 0, triangles.Length);
            if (currentChunk == startingChunk)
            {
                triangles = MarchingCube.marchingCubesGenerator(Perlin.noiseGenerator(levelOfDetail + 1), levelOfDetail + 1);
            }
            else
            {
                triangles = MarchingCube.marchingCubesGenerator(Perlin.noiseGenerator(1), 1);
            }



            //set mesh <- main thread
            chunk.GetComponent<Chunk>().SetMesh();
            allChunks.Add(startingChunk, chunk);
        }
    }


    public void CheckLevelOfDetail(int levelOfDetail, Vector3 startingChunk)
    {
        if(levelOfDetail == 1)
        {
            if (currentChunk == startingChunk)
            {
                triangles = MarchingCube.marchingCubesGenerator(Perlin.noiseGenerator(levelOfDetail + 1), levelOfDetail + 1);
                allChunks[startingChunk].GetComponent<Chunk>().SetMesh();
            }
            else
            {
                triangles = MarchingCube.marchingCubesGenerator(Perlin.noiseGenerator(1), 1);
            }
        }
    }

    public void UpdateChunk(GameObject chunk, Vector3 startingChunk)
    {
        if (!allChunks.ContainsKey(startingChunk))
        {
            //generate a new chunk
            //allChunks.Remove(chunk.transform.position);

            chunk.name = "Chunk (" + startingChunk.x + "," + startingChunk.y + "," + startingChunk.z + ")";

            Vector3 chunkPosition = startingChunk * (pointsPerAxis - 1) * size;
            chunk.transform.position = chunkPosition;
            noiseShader.SetVector("startingValue", chunkPosition);
            marchingCubeShader.SetVector("startingValue", chunkPosition);

            //generate noise <- compute shader
            //generate marching cubes <- compute shader
            Array.Clear(triangles, 0, triangles.Length);
            if (currentChunk == startingChunk)
            {
                triangles = MarchingCube.marchingCubesGenerator(Perlin.noiseGenerator(levelOfDetail + 1), levelOfDetail + 1);
            }
            else
            {
                triangles = MarchingCube.marchingCubesGenerator(Perlin.noiseGenerator(1), 1);
            }



            //set mesh <- main thread
            chunk.GetComponent<Chunk>().SetMesh();
            allChunks.Add(startingChunk, chunk);
        }
    }

    public void UpdateMesh(GameObject chunk, int points)
    {
        chunk.GetComponent<Chunk>().updateVertexDensity(points);
        //Debug.Log("Updating mesh");
/*            noiseShader.SetVector("startingValue", chunk.transform.position);
            marchingCubeShader.SetVector("startingValue", chunk.transform.position);

            //generate noise <- compute shader
            //generate marching cubes <- compute shader
            //Array.Clear(triangles, 0, triangles.Length);
            triangles = MarchingCube.marchingCubesGenerator(Perlin.noiseGenerator(points), points);

            //set mesh <- main thread
            SetMesh(chunk);*/
    }




    /// <summary>
    /// Sets all constant values for the chunk
    /// </summary>
    private void setupValues()
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
        marchingCubeShader.SetTexture(0, "groundTex", groundTexture);
        marchingCubeShader.SetFloat("height", groundLevelHeight);
        marchingCubeShader.SetFloat("frequency", frequency);

        Perlin.noiseShader = noiseShader;
        Perlin.pointsPerAxis = pointsPerAxis;
        Perlin.numThreads = numThreads;
        Perlin.size = size;

        MarchingCube.pointsPerAxis = pointsPerAxis;
        MarchingCube.numThreads = numThreads;
        MarchingCube.marchingCubeShader = marchingCubeShader;

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
            newpos.x = (chunkpos.x - radius + 1 + r);
            newpos.y = (chunkpos.y - radius + 1 + h);
            newpos.z = (chunkpos.z - radius + 1 + c);

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
