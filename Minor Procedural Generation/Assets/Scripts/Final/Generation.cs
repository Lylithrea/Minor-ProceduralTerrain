using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generation : MonoBehaviour
{
    [Header("Noise Settings")]
    public ComputeShader noiseShader;
    public int scale;

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
    public int size;

    private List<Vector3[]> chunkPositions = new List<Vector3[]>();

    void Start()
    {
        //1) update variables in shaders/scripts
        //2) calculate the positions of chunk points
        //3) spawn starting chunks


        //gain the positions of points within a chunk, this is always the same
        //we will use the chunks position to gain the correct values in world space
        GainChunkPositions();

        InitializeStartingChunks();
    }

    private void InitializeStartingChunks()
    {
        //square radius
        int chunkSize = (radius + radius - 1);
        int chunkRadius = chunkSize * chunkSize * chunkSize;

        for(int i = 0; i < chunkRadius; i++)
        {
            float r = i % chunkSize;
            float h = Mathf.FloorToInt((i / chunkSize) % chunkSize);
            float c = Mathf.FloorToInt(i / (chunkSize * chunkSize));

            CreateChunk(new Vector3(r * row * size, h * height * size, c * column * size));
        }
    }


    private void GainChunkPositions()
    {
        int chunkSize = row * column * height;
        for (int i = 0; i < chunkSize; i++) //triple foreach loop condensed into 1 for loop
        {
            float r = i % row;
            float h = Mathf.FloorToInt((i / height) % height);
            float c = Mathf.FloorToInt(i / (height * row));

            Vector3[] newPositions = new Vector3[8];
            newPositions[0] = new Vector3(r * size, (h + 1) * size, c * size);
            newPositions[1] = new Vector3((r + 1) * size, (h + 1) * size, c * size);
            newPositions[2] = new Vector3((r + 1) * size, h * size, c * size);
            newPositions[3] = new Vector3(r * size, h * size, c * size);
            newPositions[4] = new Vector3(r * size, (h + 1) * size, (c + 1) * size);
            newPositions[5] = new Vector3((r + 1) * size, (h + 1) * size, (c + 1) * size);
            newPositions[6] = new Vector3((r + 1) * size, h * size, (c + 1) * size);
            newPositions[7] = new Vector3(r * size, h * size, (c + 1) * size);
            chunkPositions.Add(newPositions);
        }
    }

    void Update()
    {
        //check if players position has changed
        //check if the new chunks already exist
        //generate/destroy chunks based on position
    }

    public void CreateChunk(Vector3 startingPos)
    {
        //get vertex positions <- variable
        //generate noise <- compute shader
        //generate marching cubes <- compute shader
        //set mesh <- main thread

        GameObject chunk = new GameObject();
        chunk.transform.position = startingPos;
        chunk.AddComponent<Chunk>();
    }

    private void noiseGenerator()
    {

    }

    private void marchingCubesGenerator()
    {

    }

}
