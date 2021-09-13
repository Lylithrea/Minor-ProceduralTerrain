using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
public class TerrainGenerator : MonoBehaviour
{
    public GameObject player;

    public float size;
    public float row, column, height;
    public Vector3 currentPosition = new Vector3(0,0,0);

    public int chunkRadius = 2;

    NoiseTest noise;

    public GameObject chunk;
    Dictionary<Vector3, GameObject> allChunks = new Dictionary<Vector3, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
/*        size = GameObject.FindGameObjectWithTag("NoiseGenerator").GetComponent<NoiseTest>().size;
        row = GameObject.FindGameObjectWithTag("NoiseGenerator").GetComponent<NoiseTest>().row;
        column = GameObject.FindGameObjectWithTag("NoiseGenerator").GetComponent<NoiseTest>().column;
        height = GameObject.FindGameObjectWithTag("NoiseGenerator").GetComponent<NoiseTest>().height;*/
        //noise = GameObject.FindGameObjectWithTag("NoiseGenerator").GetComponent<NoiseTest>();
        InitializeChunks();


    }

    // Update is called once per frame
    void Update()
    {
        if(player.transform.position.x > currentPosition.x + (row * size) / 2)
        {
            Vector3 newPosition = new Vector3(currentPosition.x + row * size, currentPosition.y, currentPosition.z);
            CreateNewChunk(newPosition);
        }        
        if(player.transform.position.x < currentPosition.x - (row * size) / 2)
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
/*        if (allChunks.Count > 0)
        {
            Destroy(allChunks[new Vector3(currentPosition.x, currentPosition.y, currentPosition.z)]);
        }*/

        Vector3 delta = newPosition - currentPosition; //gets the direction we moved in
        //get the opposite direct range
        //remove the square we dont need anymore
        //generate the new square
        if (allChunks.Count > 0)
        {
            if (delta.x > 0 || delta.x < 0)
            {
                for (int i = 0; i < chunkRadius + chunkRadius - 1; i++)
                {
                    for (int j = 0; j < chunkRadius + chunkRadius - 1; j++)
                    {
                        Destroy(allChunks[new Vector3(currentPosition.x - delta.x, (currentPosition.y - (chunkRadius - 1 ) * height * size) + j * height * size, (currentPosition.z - (chunkRadius - 1) * column * size) + i * column * size)]);
                        allChunks.Remove(new Vector3(currentPosition.x - delta.x, (currentPosition.y - (chunkRadius - 1) * height *  size) + j * height * size, (currentPosition.z - (chunkRadius - 1) * column * size) + i * column * size));
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
        }

        /*       allChunks.Remove(new Vector3(currentPosition.x, currentPosition.y, currentPosition.z));*/


        currentPosition = newPosition;



        if (delta.x > 0 || delta.x < 0)
        {
            for (int i = 0; i < chunkRadius + chunkRadius - 1; i++)
            {
                for (int j = 0; j < chunkRadius + chunkRadius - 1; j++)
                {
                    GameObject newChunk = Instantiate(chunk);
                    newChunk.transform.position = new Vector3(currentPosition.x + delta.x, (currentPosition.y - (chunkRadius - 1) * height * size) + j * height * size, (currentPosition.z - (chunkRadius - 1) * column * size) + i * column * size);
                    newChunk.GetComponent<NoiseTest>().startingValue = newChunk.transform.position;
                    newChunk.GetComponent<NoiseTest>().StartNoiseGenerator();
                    allChunks.Add(newChunk.transform.position, newChunk);
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
                    newChunk.GetComponent<NoiseTest>().startingValue = newChunk.transform.position;
                    newChunk.GetComponent<NoiseTest>().StartNoiseGenerator();
                    allChunks.Add(newChunk.transform.position, newChunk);
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
                    newChunk.GetComponent<NoiseTest>().startingValue = newChunk.transform.position;
                    newChunk.GetComponent<NoiseTest>().StartNoiseGenerator();
                    allChunks.Add(newChunk.transform.position, newChunk);
                }
            }
        }


    }


    void InitializeChunks()
    {
        for (int i = 0; i < chunkRadius + chunkRadius - 1; i++)
        {
            for (int j = 0; j < chunkRadius + chunkRadius - 1; j++)
            {
                for (int k = 0; k < chunkRadius + chunkRadius - 1; k++)
                {
                    GameObject newChunk = Instantiate(chunk);
                    newChunk.transform.position = new Vector3((currentPosition.x - (chunkRadius - 1) * row * size) + i * row * size, (currentPosition.y - (chunkRadius - 1) * height * size) + j * height * size, (currentPosition.z - (chunkRadius - 1) * column * size) + k * column * size);
                    newChunk.GetComponent<NoiseTest>().startingValue = newChunk.transform.position;
                    newChunk.GetComponent<NoiseTest>().StartNoiseGenerator();
                    allChunks.Add(newChunk.transform.position, newChunk);
                }
            }
        }
    }

}
