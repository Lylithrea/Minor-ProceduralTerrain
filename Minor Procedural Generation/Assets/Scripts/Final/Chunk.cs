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
    public List<Vector3> flowerList = new List<Vector3>();
    public List<GameObject> treeObjects = new List<GameObject>();
    public List<GameObject> flowerObjects = new List<GameObject>();
    private bool isDoneSpawningTrees = false;


    public void Setup()
    {
        generator.noiseShader.SetVector("startingValue", this.gameObject.transform.position);
        generator.marchingCubeShader.SetVector("startingValue", this.gameObject.transform.position);

        Array.Clear(triangles, 0, triangles.Length);

        //SetTriangles();
        triangles = MarchingCube.marchingCubesGenerator(Perlin.noiseGenerator(1), 1);
        treeList = new List<Vector3>(Noise.Perlin.getTrees());
        flowerList = new List<Vector3>(Noise.Perlin.getFlowers());

        meshRenderer = setupMeshRenderer();
        meshFilter = setupMeshFilter();
        meshCollider = setupMeshCollider();

        SetMesh();

        StartCoroutine(SpawnTrees());

        //Debug.Log("Spawning trees! : " + treeList.Count);
    }

    IEnumerator SpawnFlowers()
    {
        /*        foreach (Vector3 flower in flowerList)
                {
                    GameObject flow = Instantiate(generator.flower);
                    Vector3 newPos = flower;
                    newPos.y += 1;
                    flow.transform.position = newPos;
                    yield return null;
                }*/
        int size = generator.pointsPerAxis;
        int scale = generator.size;

        for(int i = 0; i< size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                float noise = generator.heightmapNoise((this.transform.position.x + i *  scale) / 20.5f, (this.transform.position.z + j *scale) / 20.5f);
                if(noise > 0.75f)
                {
                    Vector3 rayPos = new Vector3(this.transform.position.x + i * scale, 1000, this.transform.position.z + j * scale);

                    RaycastHit hit;
                    if (Physics.Raycast(rayPos, transform.TransformDirection(Vector3.down), out hit, 1000))
                    {
                        //newPos = hit.transform.position;
                        if (hit.transform.tag == "Tree")
                        {
                            break;
                        }
                        rayPos.y = rayPos.y - hit.distance + 0.25f;
                    }
                    if (rayPos.y < generator.groundLevel + generator.mountainHeight * 0.15f)
                    {
                        GameObject flower = Instantiate(generator.flower);
                        rayPos.x += UnityEngine.Random.Range(-scale * 1f, scale *1f);
                        rayPos.z += UnityEngine.Random.Range(-scale * 1f, scale *1f);
                        flower.transform.position = rayPos;
                        float randomScale = UnityEngine.Random.Range(1f, 1.5f);
                        flower.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
                    }

                }


                yield return null;
            }
        }



    }

    IEnumerator SpawnTrees()
    {
        foreach(Vector3 tree in treeList)
        {

            Vector3 newPos = tree;
            newPos.x += UnityEngine.Random.Range(-0.75f , 0.75f );
            newPos.z += UnityEngine.Random.Range(-0.75f , 0.75f );

            Vector3 rayPos = newPos;
            rayPos.y += 10;

            RaycastHit hit;
            if (Physics.Raycast(rayPos, transform.TransformDirection(Vector3.down), out hit, 12))
            {
                //newPos = hit.transform.position;
                if (hit.transform.tag == "Tree")
                {
                    break;
                }
                newPos.y = rayPos.y - hit.distance;
            }
            float min = generator.groundLevel;
            float delta = generator.mountainHeight;
            float lowMiddle = min + delta * 0.15f;
            float highMiddle = min + delta * 0.35f;

/*            Debug.Log("Pos Y: " + newPos.y);
            Debug.Log("Low middle: " + lowMiddle);
            Debug.Log("High middle: " + highMiddle);*/

            GameObject treeObject;


            if (newPos.y < lowMiddle)
            {
                treeObject = Instantiate(generator.lowTreeModels[UnityEngine.Random.Range(0, generator.lowTreeModels.Count)]);
                //Debug.Log("Spawning low tree!");
            }
            else if(newPos.y < highMiddle)
            {
                treeObject = Instantiate(generator.middleTreeModels[UnityEngine.Random.Range(0, generator.middleTreeModels.Count)]);
                //Debug.Log("Spawning middle tree!");
            }
            else
            {
                treeObject = Instantiate(generator.highTreeModels[UnityEngine.Random.Range(0, generator.highTreeModels.Count)]);
                //Debug.Log("Spawning high tree!");
            }

            //GameObject treeObject = Instantiate(treeModel);

            treeObject.transform.position = newPos;
            float randomScale = UnityEngine.Random.Range(1f, 1.2f);
            treeObject.transform.localScale = new Vector3(randomScale, randomScale, randomScale);

            Renderer mesh = treeObject.GetComponentInChildren<MeshRenderer>();
            MaterialPropertyBlock mat = new MaterialPropertyBlock();
            mesh.GetPropertyBlock(mat);
            Color col = new Color(UnityEngine.Random.Range(0.15f, 0.3f), UnityEngine.Random.Range(0.45f, 0.65f), UnityEngine.Random.Range(0.25f, 0.40f));
            //mat.SetColor("_BaseColor", col);
            mesh.SetPropertyBlock(mat);

            yield return null;
        }
        //StartCoroutine(SpawnFlowers());
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
