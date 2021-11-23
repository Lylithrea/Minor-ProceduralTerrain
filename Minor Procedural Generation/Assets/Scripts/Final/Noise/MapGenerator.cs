using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    public int seed;
    public bool autoUpdate = false;
    public float radius = 1;
    public float length = 10;

    public Texture2D texture;
    public Material mat;

    public void GenerateMap()
    {
        float[,] noiseMap = PlaneWorms.GenerateNoiseMap(length, radius,mapWidth, mapHeight, noiseScale,seed);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.DrawNoiseMap(noiseMap);
    }


    
}
