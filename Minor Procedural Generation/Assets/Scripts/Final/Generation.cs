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


    //update all the settings in variables and other scripts and/or shaders
    void Start()
    {
        //1) update variables in shaders/scripts
        //2) calculate the positions of chunk points
        //3) spawn starting chunks
    }

    void Update()
    {
        //check if players position has changed
    }

    public void CreateChunk()
    {
        //get vertex positions <- variable
        //generate noise <- compute shader
        //generate marching cubes <- compute shader
        //set mesh <- main thread
    }

}
