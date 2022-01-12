using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarchCubes;
using Noise;
using System;

public class Chunk : ChunkHelper
{

    [HideInInspector]
    public Mesh mesh;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    public Generation generator;
    public Dictionary<Vector3, float> values = new Dictionary<Vector3, float>();

    public List<Vector3> treeList = new List<Vector3>();
    public List<GameObject> treeObjects = new List<GameObject>();
    private bool isDoneSpawningTrees = false;


    public void Setup()
    {
        generator.noiseShader.SetVector("startingValue", this.gameObject.transform.position);
        generator.marchingCubeShader.SetVector("startingValue", this.gameObject.transform.position);

        Array.Clear(triangles, 0, triangles.Length);

        //SetTriangles();
        triangles = MarchingCube.marchingCubesGenerator(Perlin.noiseGenerator(1), 1);
        treeList = new List<Vector3>(Noise.Perlin.getTrees());

        meshRenderer = setupMeshRenderer();
        meshFilter = setupMeshFilter();
        meshCollider = setupMeshCollider();

        SetMesh();

        StartCoroutine(SpawnTrees());
        //Debug.Log("Spawning trees! : " + treeList.Count);
    }

    IEnumerator SpawnTrees()
    {
        foreach(Vector3 tree in treeList)
        {
            GameObject treeObject = Instantiate(generator.treeModel);
            Vector3 newPos = tree;
            newPos.x += UnityEngine.Random.Range(-0.75f , 0.75f );
            newPos.z += UnityEngine.Random.Range(-0.75f , 0.75f );
            treeObject.transform.position = newPos;
            yield return null;
        }
    }

    //IEnumerator

    private async void SetTriangles()
    {
        var result = await System.Threading.Tasks.Task.Run(() =>
        Perlin.noiseGenerator(1));
        var result2 = await System.Threading.Tasks.Task.Run(() =>
        MarchingCube.marchingCubesGenerator(result, 1));
        triangles = result2;
    }



    public void updatePosition(Vector3 newPosition)
    {
        if (!generator.allChunks.ContainsKey(newPosition))
        {
            //generate a new chunk
            generator.allChunks.Remove(this.gameObject.transform.position);

            this.gameObject.name = "Chunk (" + newPosition.x + "," + newPosition.y + "," + newPosition.z + ")";

            Vector3 chunkPosition = newPosition * (generator.pointsPerAxis - 1) * generator.size;
            this.gameObject.transform.position = chunkPosition;
            Perlin.noiseShader.SetVector("startingValue", chunkPosition);
            MarchingCube.marchingCubeShader.SetVector("startingValue", chunkPosition);

            //generate noise <- compute shader
            //generate marching cubes <- compute shader

            //setValues(Perlin.noiseGenerator(1));
            triangles = MarchingCube.marchingCubesGenerator(Perlin.noiseGenerator(1), 1);

            //set mesh <- main thread
            SetMesh();
            this.gameObject.isStatic = true;
            StaticBatchingUtility.Combine(this.gameObject);
            generator.allChunks.Add(newPosition, this.gameObject);
        }
    }

    public void updateMarchingCubes()
    {
        //generator.triangles = MarchingCube.marchingCubesGenerator(Perlin.noiseGenerator(1), 1);
        updateMesh();
    }

    private void setValues(Vector4[] val)
    {
        values.Clear();
        foreach(Vector4 value in val)
        {
            values[new Vector3(value.x, value.y, value.z)] = value.z;
        }
    }

    private Vector4[] getValues()
    {
        Vector4[] points = new Vector4[values.Count];
        int count = 0;
        foreach(KeyValuePair<Vector3, float> val in values)
        {
            points[count] = new Vector4(val.Key.x, val.Key.y, val.Key.z, val.Value);
            count++;
        }
        return points;
    }


    public void updateVertexDensity(int density)
    {
        generator.noiseShader.SetVector("startingValue", transform.position);
        generator.marchingCubeShader.SetVector("startingValue", transform.position);

        //generate noise <- compute shader
        //generate marching cubes <- compute shader
        triangles = MarchingCube.marchingCubesGenerator(Perlin.noiseGenerator(density), density);

        //set mesh <- main thread
        SetMesh();
    }


    public void chunkSetup()
    {
        generator.noiseShader.SetVector("startingValue", gameObject.transform.position);
        generator.marchingCubeShader.SetVector("startingValue", gameObject.transform.position);
    }


    public void SetMesh()
    {
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        //meshFilter.sharedMesh = mesh;

        mesh.vertices = createVertices();
        mesh.triangles = createTriangles(mesh.vertices.Length);

        mesh.RecalculateNormals();

        if (mesh == null)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            meshFilter.sharedMesh = mesh;
        }

        if (meshCollider.sharedMesh == null)
        {
            meshCollider.sharedMesh = mesh;
        }
        // force update
        meshCollider.enabled = false;
        meshCollider.enabled = true;

        meshRenderer.material = generator.terrainMaterial;
    }

    private void updateMesh()
    {

        //meshFilter.sharedMesh = mesh;
        mesh.Clear();

        mesh.vertices = createVertices();
        mesh.triangles = createTriangles(mesh.vertices.Length);

        mesh.RecalculateNormals();


        if (mesh == null)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            meshFilter.sharedMesh = mesh;
        }

        if (meshCollider.sharedMesh == null)
        {
            meshCollider.sharedMesh = mesh;
        }
        // force update
        meshCollider.enabled = false;
        meshCollider.enabled = true;

        meshRenderer.material = generator.terrainMaterial;
    }


}
