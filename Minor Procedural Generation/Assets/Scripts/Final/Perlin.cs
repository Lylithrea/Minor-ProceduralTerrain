using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Noise
{
    public class Perlin : ScriptableObject
    {
        public static ComputeShader noiseShader;
        public static int pointsPerAxis;
        public static int numThreads;
        public static float size;

        /// <summary>
        /// Generates a noise and with the noise it will generate with the marching cube algorithm triangles
        /// </summary>
        /// <returns>An array of triangles used for meshes</returns>
        public static Vector4[] noiseGenerator(int points)
        {
            //in the future this might be updated dynamicly because of vertices points per chunk
            int currentPoints = pointsPerAxis * points ;
            float threadsPerAxis = (float)currentPoints / (float)numThreads;
            int dispatchAmount = Mathf.CeilToInt(threadsPerAxis);

            noiseShader.SetInt("pointsPerAxis", currentPoints);
            noiseShader.SetFloat("size", size / points);
            noiseShader.SetFloat("chunkSize", currentPoints * currentPoints * currentPoints);

            //generate the size of the list for all the points
            int vertexPerlinResults = currentPoints * currentPoints * currentPoints;

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
            //return marchingCubesGenerator(vertexPerlin, points);
            return vertexPerlin;
        }
    }
}
