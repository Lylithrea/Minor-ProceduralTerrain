using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainEditor : MonoBehaviour
{
    public Generation generator;
    public int radius = 5;
    public GameObject sphere;
    GameObject sphereObject;
    // Update is called once per frame

    private void Start()
    {
        sphereObject = Instantiate(sphere);
        sphereObject.transform.localScale *= radius;
        sphereObject.SetActive(false);
    }


    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                sphereObject.SetActive(true);
                sphereObject.transform.position = hit.point;
                //Debug.Log("Hit at: " + hit.point);
                Vector3 position = new Vector3();
                position.x = Mathf.Floor(hit.point.x / ((generator.pointsPerAxis -1)* generator.size));
                position.y = Mathf.Floor(hit.point.y / ((generator.pointsPerAxis - 1) * generator.size));
                position.z = Mathf.Floor(hit.point.z / ((generator.pointsPerAxis - 1) * generator.size));
                if (generator.allChunks.ContainsKey(position))
                {
                    Debug.Log("Found chunk! with ID: (" + position.x + "," + position.y + "," + position.z + ")");
                    GameObject chunk = generator.allChunks[position];
                    Chunk chunkData = chunk.GetComponent<Chunk>();
                    //setNewValues(chunkData, hit.point, position);
                    chunkData.updateMarchingCubes();
                }
            }
        }
        else
        {
            sphereObject.SetActive(false);
        }
    }

    private void setNewValues(Chunk chunkData, Vector3 position, Vector3 currentChunk)
    {
        Vector3 newPos = position - (currentChunk * (generator.pointsPerAxis - 1) * generator.size);
        newPos.x = Mathf.FloorToInt(newPos.x);
        newPos.y = Mathf.FloorToInt(newPos.y);
        newPos.z = Mathf.FloorToInt(newPos.z);

        Debug.Log(newPos);

        int r = Mathf.CeilToInt(radius / 2);
        r = 2;
        for(int i = r * -1; i <= r + 1; i++)
        {
            for (int j = r * -1; j <= r + 1; j++)
            {
                for (int k = r * -1; k <= r + 1; k++)
                {
                    //chunkData.values[new Vector3(newPos.x, newPos.y , newPos.z)] = 0;

                }
            }
        }
    }


}
