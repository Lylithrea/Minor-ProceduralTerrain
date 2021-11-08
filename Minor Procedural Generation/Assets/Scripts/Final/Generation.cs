using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Noise;
using MarchCubes;
using System.Threading;


public class Generation : GenerationTooling
{

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
        testDynamicChunks();
    }

    Vector3 playerPos = new Vector3(0,0,0);
    //check movement of player to spawn new chunks
    private void testDynamicChunks()
    {
        playerPos.x = Mathf.Floor(player.transform.position.x / ((pointsPerAxis - 1) * size));
        playerPos.y = Mathf.Floor(player.transform.position.y / ((pointsPerAxis - 1) * size));
        playerPos.z = Mathf.Floor(player.transform.position.z / ((pointsPerAxis -1 ) * size));
        //so it does not update every update loop
        if (playerPos != currentChunk)
        {
            //still need to add updating of the vertex density
            currentChunk = playerPos;
            UpdateChunks();
        }
    }

    //update the chunks based on player position
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

    IEnumerator SpawnChunks()
    {
        spawningChunksRunning = true;
        while (chunkQueue.Count > 0)
        {
            Vector3 chunkPos = chunkQueue.Dequeue();
            if (reusableChunkQueue.Count > 0)
            {
                GameObject chunk = reusableChunkQueue.Dequeue();
                chunk.GetComponent<Chunk>().updatePosition(chunkPos);
            }
            else
            {
                CreateChunk(chunkPos);
            }
            yield return null;
        }
        StartCoroutine(UpdateChunkMeshes());
        spawningChunksRunning = false;
    }



    public void UpdateMultipleMeshes(Vector3 startPos, Vector3 endPos, int detail)
    {
        for (float i = startPos.x; i <= endPos.x; i++)
        {
            for (float j = startPos.y; j <= endPos.y; j++)
            {
                for (float k = startPos.z; k <= endPos.z; k++)
                {
                    Chunks newchunk;
                    newchunk.chunk = allChunks[new Vector3(i, j, k)];
                    newchunk.points = detail;
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
        while (updateQueue.Count > 0)
        {
            Chunks chunk = updateQueue.Dequeue();
            chunk.chunk.GetComponent<Chunk>().updateVertexDensity(chunk.points);
            yield return null;
        }
    }


}
