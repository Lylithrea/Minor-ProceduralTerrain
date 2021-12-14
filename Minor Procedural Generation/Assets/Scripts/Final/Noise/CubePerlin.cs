﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CubePerlin : MonoBehaviour
{

    public int mapWidth;
    public int mapHeight;
    public int mapLenght;
    public float noiseScale;
    public int seed;
    public bool autoUpdate = false;
    [Range(0f,1f)]
    public float cutOffValue;
    public bool cutOff = false;
    private Dictionary<Vector3, float> perlinPoints = new Dictionary<Vector3, float>();

    public void GenerateCube()
    {
        perlinPoints.Clear();
        Init();
    }

    public void Init()
    {
        for(int x =0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int z = 0; z < mapLenght; z++)
                {
                    perlinPoints.Add(new Vector3(x, y, z), selfmadeCubeNoise(x/noiseScale, y/noiseScale, z/noiseScale));
                }
            }
        }
    }

    public static double grad(int hash, double x, double y, double z)
    {
        //bit wise and
        switch (hash & 0xF)
        {
            case 0x0: return x + y;
            case 0x1: return -x + y;
            case 0x2: return x - y;
            case 0x3: return -x - y;
            case 0x4: return x + z;
            case 0x5: return -x + z;
            case 0x6: return x - z;
            case 0x7: return -x - z;
            case 0x8: return y + z;
            case 0x9: return -y + z;
            case 0xA: return y - z;
            case 0xB: return -y - z;
            case 0xC: return y + x;
            case 0xD: return -y + z;
            case 0xE: return y - x;
            case 0xF: return -y - z;
            default: return 0; // never happens
        }
    }

    public static float interpolate(float value)
    {
        return value * value * value * (value * (value * 6 - 15) + 10);
        //return (6 * Mathf.Pow(value, 5) - 15 * Mathf.Pow(value, 4) + 10 * Mathf.Pow(value, 3));
    }

    public static double lerp(double a, double b, double x)
    {
        return a + x * (b - a);
    }

    private float selfmadeCubeNoise(float x, float y, float z)
    {
        float result = 0;

        //points in perlin space
        float pointX = x - Mathf.Floor(x);
        float pointY = y - Mathf.Floor(y);
        float pointZ = z - Mathf.Floor(z);


        // Calculate the "unit cube" that the point asked will be located in
        int xi = (int)x & 255;
        int yi = (int)y & 255;
        int zi = (int)z & 255;

        //get semi-random number from the permutation table (255 numbers, incl)
        //we have 8 corners, thus 8 times
        int cornerA = px[px[px[xi] + yi] + zi];
        int cornerB = px[px[px[xi] + yi + 1] + zi];
        int cornerC = px[px[px[xi] + yi] + zi + 1];
        int cornerD = px[px[px[xi] + yi + 1] + zi + 1];

        int cornerE = px[px[px[xi + 1] + yi] + zi];
        int cornerF = px[px[px[xi + 1] + yi + 1] + zi];
        int cornerG = px[px[px[xi + 1] + yi] + zi + 1];
        int cornerH = px[px[px[xi + 1] + yi + 1] + zi + 1];

        //based on those numbers we get a direction, in this case 8 different directions are possible
        double cornerAdir = grad(cornerA, pointX, pointY, pointZ);
        double cornerBdir = grad(cornerB, pointX, pointY -1, pointZ);
        double cornerCdir = grad(cornerC, pointX, pointY, pointZ -1 );
        double cornerDdir = grad(cornerD, pointX, pointY -1, pointZ -1);

        double cornerEdir = grad(cornerE, pointX - 1, pointY, pointZ);
        double cornerFdir = grad(cornerF, pointX - 1, pointY -1, pointZ);
        double cornerGdir = grad(cornerG, pointX - 1, pointY, pointZ -1);
        double cornerHdir = grad(cornerH, pointX - 1, pointY -1, pointZ -1 );

        //Debug.Log("For cornerA we get : X: " + pointX + " Y: " + pointY + " Z: " + pointZ + ", which results in : " + cornerAdir);
        
        //combine the directions we got, 2 at the time, first on the x, then on y, then on z
        //x:
        double resultA = lerp(cornerAdir, cornerEdir, interpolate(pointX));
        double resultB = lerp(cornerBdir, cornerFdir, interpolate(pointX));
        double resultC = lerp(cornerCdir, cornerGdir, interpolate(pointX));
        double resultD = lerp(cornerDdir, cornerHdir, interpolate(pointX));

        double resultE = lerp(resultA, resultB, interpolate(pointY));
        double resultF = lerp(resultC, resultD, interpolate(pointY));

        double resultG = lerp(resultE, resultF, interpolate(pointZ));


        result = ((((float)resultG + 1) / 2) - 0.25f) * 2;
        //Debug.Log("Result : " + result);
        //Debug.Log("Result : " + result);

        //result = (float)resultG;

        return result;
    }





































    [ExecuteInEditMode]
    private void OnDrawGizmos()
    {

        foreach(KeyValuePair<Vector3, float> pair in perlinPoints)
        {
            if (cutOff)
            {
                if (((pair.Value + 1) / 2) > cutOffValue)
                {
                    Gizmos.color = Color.Lerp(Color.black, Color.white, pair.Value);
                    Gizmos.DrawSphere(pair.Key, 0.25f);
                }
            }
            else
            {
                Gizmos.color = Color.Lerp(Color.black, Color.white,pair.Value);
                Gizmos.DrawSphere(pair.Key, 0.25f);
            }
        }

    }


    private static readonly int[] permutation = { 151,160,137,91,90,15,                 // Hash lookup table as defined by Ken Perlin.  This is a randomly
    131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,    // arranged array of all numbers from 0-255 inclusive.
    190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
    88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
    77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
    102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
    135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
    5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
    223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
    129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
    251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
    49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
    138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
    };
    public static readonly int[] px;                                                    // Doubled permutation to avoid overflow

    static CubePerlin()
    {
        px = new int[512];
        for (int x = 0; x < 512; x++)
        {
            px[x] = permutation[x % 256];
        }
    }
}