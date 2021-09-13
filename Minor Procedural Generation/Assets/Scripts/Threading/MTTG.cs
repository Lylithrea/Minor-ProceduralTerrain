using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
public class MTTG : MonoBehaviour
{
    public GameObject player;

    public Vector3 currentPosition = new Vector3(0, 0, 0);

    public int row = 20, column = 20, height = 20;
    public float size = 20;

    [Range(0, 1)]
    public float cutoff = 0.6f;
    public float groundLevel = 1500;
    public float layerThickness = 150;

    [Range(0.43f, 0.47f)]
    public float noiseScale = 0.5f;
    public int repeat = 9;

    public int chunkRadius = 2;

    public object _locker = new object();


    public GameObject chunk;
    Dictionary<Vector3, GameObject> allChunks = new Dictionary<Vector3, GameObject>();


    Queue<GameObject> tasks = new Queue<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        //calculate the vertice points for a chunk
        InitializeChunks();
    }

    // Update is called once per frame
    void Update()
    {
        if (player.transform.position.x > currentPosition.x + (row * size) / 2)
        {
            Vector3 newPosition = new Vector3(currentPosition.x + row * size, currentPosition.y, currentPosition.z);
            CreateNewChunk(newPosition);
        }
        if (player.transform.position.x < currentPosition.x - (row * size) / 2)
        {
            Vector3 newPosition = new Vector3(currentPosition.x - row * size, currentPosition.y, currentPosition.z);
            CreateNewChunk(newPosition);
        }


        if (player.transform.position.y > currentPosition.y + (height * size) / 2)
        {
            Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y + height * size, currentPosition.z);
            CreateNewChunk(newPosition);
        }
        if (player.transform.position.y < currentPosition.y - (height * size) / 2)
        {
            Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y - height * size, currentPosition.z);
            CreateNewChunk(newPosition);
        }


        if (player.transform.position.z > currentPosition.z + (column * size) / 2)
        {
            Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z + column * size);
            CreateNewChunk(newPosition);
        }
        if (player.transform.position.z < currentPosition.z - (column * size) / 2)
        {
            Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z - column * size);
            CreateNewChunk(newPosition);
        }
    }

    void CreateNewChunk(Vector3 newPosition)
    {
        Vector3 delta = newPosition - currentPosition; //gets the direction we moved in
        //get the opposite direct range
        //remove the square we dont need anymore
        //generate the new square
        /*if (allChunks.Count > 0)
        {
            if (delta.x > 0 || delta.x < 0)
            {
                for (int i = 0; i < chunkRadius + chunkRadius - 1; i++)
                {
                    for (int j = 0; j < chunkRadius + chunkRadius - 1; j++)
                    {
                        Destroy(allChunks[new Vector3(currentPosition.x - delta.x, (currentPosition.y - (chunkRadius - 1) * height * size) + j * height * size, (currentPosition.z - (chunkRadius - 1) * column * size) + i * column * size)]);
                        allChunks.Remove(new Vector3(currentPosition.x - delta.x, (currentPosition.y - (chunkRadius - 1) * height * size) + j * height * size, (currentPosition.z - (chunkRadius - 1) * column * size) + i * column * size));
                    }
                }
            }
            if (delta.y > 0 || delta.y < 0)
            {
                for (int i = 0; i < chunkRadius + chunkRadius - 1; i++)
                {
                    for (int j = 0; j < chunkRadius + chunkRadius - 1; j++)
                    {
                        Destroy(allChunks[new Vector3((currentPosition.x - (chunkRadius - 1) * row * size) + j * row * size, currentPosition.y - delta.y, (currentPosition.z - (chunkRadius - 1) * column * size) + i * column * size)]);
                        allChunks.Remove(new Vector3((currentPosition.x - (chunkRadius - 1) * row * size) + j * row * size, currentPosition.y - delta.y, (currentPosition.z - (chunkRadius - 1) * column * size) + i * column * size));
                    }
                }
            }
            if (delta.z > 0 || delta.z < 0)
            {
                for (int i = 0; i < chunkRadius + chunkRadius - 1; i++)
                {
                    for (int j = 0; j < chunkRadius + chunkRadius - 1; j++)
                    {
                        Destroy(allChunks[new Vector3((currentPosition.x - (chunkRadius - 1) * row * size) + i * row * size, (currentPosition.y - (chunkRadius - 1) * height * size) + j * height * size, currentPosition.z - delta.z)]);
                        allChunks.Remove(new Vector3((currentPosition.x - (chunkRadius - 1) * row * size) + i * row * size, (currentPosition.y - (chunkRadius - 1) * height * size) + j * height * size, currentPosition.z - delta.z));
                    }
                }
            }
        }*/

        currentPosition = newPosition;



        if (delta.x > 0 || delta.x < 0)
        {
            for (int i = 0; i < chunkRadius + chunkRadius - 1; i++)
            {
                for (int j = 0; j < chunkRadius + chunkRadius - 1; j++)
                {
                    GameObject newChunk = Instantiate(chunk);
                    newChunk.transform.position = new Vector3(currentPosition.x + delta.x, (currentPosition.y - (chunkRadius - 1) * height * size) + j * height * size, (currentPosition.z - (chunkRadius - 1) * column * size) + i * column * size);
                    newChunk.GetComponent<ChunkData>().startingValue = newChunk.transform.position;
                    /*                    Thread test = new Thread(newChunk.GetComponent<ChunkData>().StartNoiseGenerator);
                                        test.Start();
                                        test.Join();
                                        newChunk.GetComponent<MCA>().SetMesh();*/
                    newChunk.GetComponent<ChunkData>().TestChunk();
                    //allChunks.Add(newChunk.transform.position, newChunk);
                }
            }
        }

        if (delta.y > 0 || delta.y < 0)
        {
            for (int i = 0; i < chunkRadius + chunkRadius - 1; i++)
            {
                for (int j = 0; j < chunkRadius + chunkRadius - 1; j++)
                {
                    GameObject newChunk = Instantiate(chunk);
                    newChunk.transform.position = new Vector3((currentPosition.x - (chunkRadius - 1) * row * size) + j * row * size, currentPosition.y + delta.y, (currentPosition.z - (chunkRadius - 1) * column * size) + i * column * size);
                    newChunk.GetComponent<ChunkData>().startingValue = newChunk.transform.position;
                    /*                    Thread test = new Thread(newChunk.GetComponent<ChunkData>().StartNoiseGenerator);
                                        test.Start();
                                        test.Join();
                                        newChunk.GetComponent<MCA>().SetMesh();*/
                    newChunk.GetComponent<ChunkData>().TestChunk();
                    //allChunks.Add(newChunk.transform.position, newChunk);
                }
            }
        }

        if (delta.z > 0 || delta.z < 0)
        {
            for (int i = 0; i < chunkRadius + chunkRadius - 1; i++)
            {
                for (int j = 0; j < chunkRadius + chunkRadius - 1; j++)
                {
                    GameObject newChunk = Instantiate(chunk);
                    newChunk.transform.position = new Vector3((currentPosition.x - (chunkRadius - 1) * row * size) + i * row * size, (currentPosition.y - (chunkRadius - 1) * height * size) + j * height * size, currentPosition.z + delta.z);
                    newChunk.GetComponent<ChunkData>().startingValue = newChunk.transform.position;
/*                                        Thread test = new Thread(newChunk.GetComponent<ChunkData>().TestChunk);
                                        test.Start();
                                        test.Join();*/
                                        //newChunk.GetComponent<MCA>().SetMesh();
                    newChunk.GetComponent<ChunkData>().TestChunk();
                    //allChunks.Add(newChunk.transform.position, newChunk);
                }
            }
        }


    }

    Vector3 newPos = new Vector3(0,0,0);
    void InitializeChunks()
    {
        int radius = (chunkRadius + chunkRadius - 1);
        int chunkRadiusSize = radius * radius * radius;

        for (int h = 0; h < chunkRadiusSize; h++)
        {
            float i =  (h % radius);
            float j =  (Mathf.FloorToInt((h/ radius) % radius));
            float k = (Mathf.FloorToInt(h/ (radius * radius)));


            GameObject newChunk = Instantiate(chunk);
            newPos.x = currentPosition.x - (chunkRadius - 1) * row * size + i * row * size;
            newPos.y = currentPosition.y - (chunkRadius - 1) * height * size + j * height * size;
            newPos.z = currentPosition.z - (chunkRadius - 1) * column * size + k * column * size;
            newChunk.transform.position = newPos;
            
            newChunk.GetComponent<ChunkData>().startingValue = newPos;
            newChunk.GetComponent<MCA>().startingValue = newPos;


/*            Thread test = new Thread(newChunk.GetComponent<ChunkData>().TestChunk);
            test.Start();
            test.Join();*/

            newChunk.GetComponent<ChunkData>().TestChunk();
        }
    }

/*    void InitializeChunks()
    {
        for (int i = 0; i < chunkRadius + chunkRadius - 1; i++)
        {
            for (int j = 0; j < chunkRadius + chunkRadius - 1; j++)
            {
                for (int k = 0; k < chunkRadius + chunkRadius - 1; k++)
                {
                    GameObject newChunk = Instantiate(chunk);
                    newChunk.transform.position = new Vector3((currentPosition.x - (chunkRadius - 1) * row * size) + i * row * size, (currentPosition.y - (chunkRadius - 1) * height * size) + j * height * size, (currentPosition.z - (chunkRadius - 1) * column * size) + k * column * size);
                    newChunk.GetComponent<ChunkData>().startingValue = newChunk.transform.position;
                    newChunk.GetComponent<MCA>().startingValue = newChunk.transform.position;
                    //newChunk.GetComponent<ChunkData>().StartNoiseGenerator();
                    //Thread test = new Thread(newChunk.GetComponent<ChunkData>().StartNoiseGenerator);
                    newChunk.GetComponent<ChunkData>().TestChunk();
                    //test.Start();
                    //test.Join();
                    //newChunk.GetComponent<MCA>().SetMesh();        //cant do on threads, so needs to go on main thread
                    allChunks.Add(newChunk.transform.position, newChunk);
                }
            }
        }
    }*/

}
