using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Noise;
using MarchCubes;
using System.Threading;


public class Generation : GenerationTooling
{
    List<Vector3> tempPos = new List<Vector3>();


    void Start()
    {
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
        chunkQueue.Clear();
        currentPlayerChunks.Clear();


        int angleIncrease = 25;
        float angle = 0;

        for (float r = 0; r < radius; r += 2 * Mathf.PI * 0.0025f)
        {

            //convert to cartesian coordinate system
            float carX2 = r * Mathf.Cos((angle * Mathf.PI) / 180);
            float carY2 = r * Mathf.Sin((angle * Mathf.PI) / 180);

            //convert to chunk size
            carX2 *= pointsPerAxis * size;
            carY2 *= pointsPerAxis * size;

            //convert the points to chunk positions (square spacing)
            carX2 = Mathf.Floor(carX2 / (pointsPerAxis * size));
            carY2 = Mathf.Floor(carY2 / (pointsPerAxis * size));

            for(int i = radius*-1; i < radius; i++)
            {
                Vector3 newPos = new Vector3(currentChunk.x + carX2, currentChunk.y + i, currentChunk.z + carY2);
                //makes sure we dont add extra chunks that are not needed in the queue
                if (!allChunks.ContainsKey(newPos) && !currentPlayerChunks.Contains(newPos))
                {
                    chunkQueue.Enqueue(newPos);
                    currentPlayerChunks.Add(newPos);
                }
            }
            //increase the angle, by an decreasing amount, so the spacing somewhat stays the same
            angle += angleIncrease / (1 + r);
        }
        if (!spawningChunksRunning)
        {
            SpawnChunksTest();
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

    private async void SpawnChunksTest()
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
            await System.Threading.Tasks.Task.Yield();
        }
        StartCoroutine(UpdateChunkMeshes());
        spawningChunksRunning = false;
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
        chunkQueue.Clear();
        currentPlayerChunks.Clear();

        int angleIncrease = 25;
        float angle = 0;

        for (float r = 0; r < radius; r += 2 * Mathf.PI * 0.0025f)
        {

            //convert to cartesian coordinate system
            float carX2 = r * Mathf.Cos((angle * Mathf.PI) / 180);
            float carY2 = r * Mathf.Sin((angle * Mathf.PI) / 180);

            //convert to chunk size
            carX2 *= pointsPerAxis * size;
            carY2 *= pointsPerAxis * size;

            //convert the points to chunk positions (square spacing)
            carX2 = Mathf.Floor(carX2 / (pointsPerAxis * size));
            carY2 = Mathf.Floor(carY2 / (pointsPerAxis * size));

            for (int i = radius * -1; i < radius; i++)
            {
                Vector3 newPos = new Vector3(currentChunk.x + carX2, currentChunk.y + i, currentChunk.z + carY2);
                //makes sure we dont add extra chunks that are not needed in the queue
                if (!allChunks.ContainsKey(newPos) && !currentPlayerChunks.Contains(newPos))
                {
                    chunkQueue.Enqueue(newPos);
                    currentPlayerChunks.Add(newPos);
                }
            }
            //increase the angle, by an decreasing amount, so the spacing somewhat stays the same
            angle += angleIncrease / (1 + r);
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
