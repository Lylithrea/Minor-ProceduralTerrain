using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRender;

    public void DrawNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Debug.Log("Map height: " + height + " Map width: " + width);


        Texture2D texture = new Texture2D(width, height);

        Color[] colourMap = new Color[width * height];
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, (noiseMap[x, y] + 1)/2);
                Debug.Log("On point (" + x + "," + y + " has the value of : " + noiseMap[x, y]);
            }
        }

        texture.SetPixels(colourMap);
        texture.Apply();

        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(width, 1, height);
    }

    public void SetTexture(Texture2D imageTexture)
    {
        Texture2D texture = new Texture2D(imageTexture.width, imageTexture.height);

        texture = imageTexture;
        texture.Apply();

        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(imageTexture.width, 1, imageTexture.width);
    }
}
