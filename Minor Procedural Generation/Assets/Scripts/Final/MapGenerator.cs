using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    public bool autoUpdate = false;

    public Texture2D texture;
    public Material mat;

    public void GenerateMap()
    {
        float[,] noiseMap = PlanePerlin.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);

        //float[,] map = texture.GetPixels();

        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.DrawNoiseMap(noiseMap);
        //Texture mainTex = mat.mainTexture;
        //Texture2D newTex = new Texture2D(mainTex.width, mainTex.height);

        //display.SetTexture((Texture2D)mat.GetTexture("surface"));

        float result = PlanePerlin.selfmadeNoise(0.6f, 0.8f, 1);
        Debug.Log("Final result : " + result + " !");

    }


    
}
