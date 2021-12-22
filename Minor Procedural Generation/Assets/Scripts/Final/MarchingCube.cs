using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchCubes
{
    public class MarchingCube : ScriptableObject
    {
        public static int pointsPerAxis;
        public static int numThreads = 8;
        public static ComputeShader marchingCubeShader;
        static ComputeBuffer triangleBuffer;
        static ComputeBuffer vertexBuffer;
        static ComputeBuffer triangleCounter;

        public static void ReleaseBuffers()
        {
            triangleCounter.Release();
            triangleBuffer.Release();
            vertexBuffer.Release();
        }

        public static void CreateBuffers()
        {
            int currentPoints = pointsPerAxis;

            int numVoxelsPerAxis = currentPoints - 1;
            int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
            int maxTriangleCounter = numVoxels * 5;

            int vertexPerlinCount = pointsPerAxis * pointsPerAxis * pointsPerAxis;

            triangleBuffer = new ComputeBuffer(maxTriangleCounter, sizeof(float) * 3 * 3, ComputeBufferType.Append);
            vertexBuffer = new ComputeBuffer(vertexPerlinCount, sizeof(float) * 4);
            triangleCounter = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

            marchingCubeShader.SetBuffer(0, "triangles", triangleBuffer);
            marchingCubeShader.SetBuffer(0, "vertexPerlin", vertexBuffer);
        }

        /// <summary>
        /// Using a marching cube algorithm it will generate triangles based on positions of vertices and their appropiate values.
        /// </summary>
        /// <param name="vertexPerlin">An array which exists out of a position together with a value which decides if its terrain or not.</param>
        /// <returns></returns>
        public static Chunk.Triangle[] marchingCubesGenerator(Vector4[] vertexPerlin, int points)
        {
            //in the future this might be updated dynamicly because of vertices points per chunk

            //create a buffer for the triangles

            triangleBuffer.SetCounterValue(0);


            //creates a buffer for the input of the values

            vertexBuffer.SetCounterValue(0);
            vertexBuffer.SetData(vertexPerlin);


            int currentPoints = pointsPerAxis;

            int numVoxelsPerAxis = currentPoints - 1;

            //calculates how often the compute shader needs to be dispatched.
            float threadsPerAxis = (float)numVoxelsPerAxis / (float)numThreads;
            int dispatchAmount = Mathf.CeilToInt(threadsPerAxis);

            //dispatch the shader
            marchingCubeShader.Dispatch(0, dispatchAmount, dispatchAmount, dispatchAmount);

            //if we dont copy the count over, it means that if the size in the previous chunk was bigger, it wont overwrite the last values
            //this means we take over data from the previous chunk to the new chunk
            ComputeBuffer.CopyCount(triangleBuffer, triangleCounter, 0);
            int[] triangleCountArray = { 0 };
            triangleCounter.GetData(triangleCountArray);
            int triangleAmount = triangleCountArray[0];

            //get the triangle data
            Chunk.Triangle[] triangles = new Chunk.Triangle[triangleAmount];
            triangleBuffer.GetData(triangles);

            //return the triangles we created.
            return triangles;
        }
    }
}
