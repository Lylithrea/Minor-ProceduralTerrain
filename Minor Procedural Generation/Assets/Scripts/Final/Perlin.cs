using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;

namespace Noise
{
    public class Perlin : ScriptableObject
    {
        public static ComputeShader noiseShader;
        public static int pointsPerAxis;
        public static int numThreads = 8;
        public static float size;
        static ComputeBuffer vertexPerlinBuffer;
        static ComputeBuffer treeBuffer;
        static ComputeBuffer treeCounter;
        static Vector4[] vertexPerlin;
        static int vertexPerlinResults;
        public static bool isReady = false;
        private static List<Vector3> treeList = new List<Vector3>();

        public static void ReleaseBuffers()
        {
            vertexPerlinBuffer.Release();
        }


        public static void CreateBuffers()
        {
            vertexPerlinResults = pointsPerAxis * pointsPerAxis * pointsPerAxis;
            vertexPerlin = new Vector4[vertexPerlinResults];
            vertexPerlinBuffer = new ComputeBuffer(vertexPerlinResults, sizeof(float) * 4);
            noiseShader.SetBuffer(0, "vertexPerlin", vertexPerlinBuffer);


            treeCounter = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
            treeBuffer = new ComputeBuffer(vertexPerlinResults, sizeof(float) * 3, ComputeBufferType.Append);
            noiseShader.SetBuffer(0, "treePositions", treeBuffer);
        }

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

            //reset the counter value because else it starts where it left off previous run
            vertexPerlinBuffer.SetCounterValue(0);
            treeBuffer.SetCounterValue(0);

            //how often the shader will get dispatched in each direction
            //(if dispatchamount and threads are 8, it will mean that the code will totally be run 8x8x8x2x2x2.)
            noiseShader.Dispatch(0, dispatchAmount, dispatchAmount, dispatchAmount);

            //get the values


            //AsyncGPUReadbackRequest request =  UnityEngine.Rendering.AsyncGPUReadback.Request(vertexPerlinBuffer);

            /*            AsyncGPUReadback.Request(vertexPerlinBuffer, (req) =>
                        {
                            req.GetData();
                            isReady = true;
                        });*/



            ComputeBuffer.CopyCount(treeBuffer, treeCounter, 0);
            int[] treeCountArray = { 0 };
            treeCounter.GetData(treeCountArray);
            int treeAmount = treeCountArray[0];
            Vector3[] trees = new Vector3[treeAmount];
            treeBuffer.GetData(trees);
            //Debug.Log("Got trees! : " + treeAmount);
            setTrees(trees);
            vertexPerlinBuffer.GetData(vertexPerlin);



            return vertexPerlin;
        }

        public static void setTrees(Vector3[] trees)
        {
            treeList.Clear();
            foreach(Vector3 tree in trees)
            {
                treeList.Add(tree);
            }
            //Debug.Log("Adding trees! : " + trees.Length);
        }

        public static List<Vector3> getTrees()
        {
            return treeList;
        }

        public static IEnumerator AsyncExtract()
        {
            while (true)
            {
                // extract
                var request = AsyncGPUReadback.Request(vertexPerlinBuffer);

                yield return new WaitUntil(() => request.done);
                var vertex = request.GetData<Vector4>();

                int i = 0;
                foreach(var vert in vertex)
                {
                    vertexPerlin[i] = vert;
                    i++;
                }
            }
        }

    }


}
