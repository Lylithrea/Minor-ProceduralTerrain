using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlaneWorms
{
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
    private static readonly int[] px;                                                    // Doubled permutation to avoid overflow

    static PlaneWorms()
    {
        px = new int[512];
        for (int x = 0; x < 512; x++)
        {
            px[x] = permutation[x % 256];
        }
    }

    public static float[,] GenerateNoiseMap(float length, float radius, int mapWidth, int mapHeight, float scale, int seed)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float sampleX = x / scale;
                float sampleY = y / scale;

                //noiseMap[(int)sampleX, (int)sampleY] = 0;
                //float perlinValue = generateWorms(sampleX, sampleY);

                int xi = (int)sampleX & 255;
                int yi = (int)sampleY & 255;

                int somethingX = (int)(x + y) % 25;
                int somethingY = px[px[(int)xi] + (int)yi];
                int randomX = px[xi];
                int randomY = px[yi];
                if (randomX == randomY && randomX >= 254)
                {
                    //Debug.Log("Something!");
                    //noiseMap[(int)x, (int)y] = 1;
                    generateWorms(x, y, length, radius, mapWidth, mapHeight, scale, noiseMap);
                }

                //noiseMap[x, y] = 0;
            }
        }

        

        return noiseMap;
    }

    private static void generateWorms(float x, float y, float length, float radius, int mapWidth, int mapHeight, float scale, float[,] noiseMap)
    {
/*        int middleX = mapWidth / 2;
        int middleY = mapHeight / 2;
        noiseMap[middleX, middleY] = 1;*/


        int currentX = Mathf.RoundToInt(x);
        int currentY = Mathf.RoundToInt(y);

        //Debug.Log("Current position: " + currentX + " , " + currentY);

        //loop through 1 row of perlin noise
        for(int i = 0; i < length; i++)
        {
            float value = selfmadeNoise(y + i / scale, x);

            

            //change value to scale from -180 to 180 instead of 0 to 1
            float valueDegree = value * 720 - 360;
            int offsetX = Mathf.RoundToInt(Mathf.Cos(valueDegree));
            int offsetY = Mathf.RoundToInt(Mathf.Sin(valueDegree));

            //since cos(180) and cos(-180) are both -1, we want to multiply it by -1 whenever its lower than 0 so its going in all direcitons
            if(valueDegree < 0)
            {
                offsetX *= -1;
                offsetY *= -1;
            }

            int directionAX = offsetY * -1;
            int directionAY = offsetX;

            int directionBX = offsetY;
            int directionBY = offsetX * -1;

            currentX += offsetX;
            currentY += offsetY;



            for (int j = 0; j < radius; j++)
            {
                float perlinValue = 1;
                if (currentX + directionAX * j < mapWidth && currentY + directionAY * j < mapHeight && currentX + directionAX * j >= 0 && currentY + directionAY * j >= 0)
                {
                    int currentPixelX = currentX + directionAX * j;
                    int currentPixelY = currentY + directionAY * j;
                    //float weight = (currentPixelX + currentPixelY) / (currentX + currentY);
                    float weight = 1 - j / radius;
                    noiseMap[currentPixelX, currentPixelY] += perlinValue * weight;
                }

                if (currentX + directionBX * j < mapWidth && currentY + directionBY * j < mapHeight && currentX + directionBX * j >= 0 && currentY + directionBY * j >= 0)
                {
                    //noiseMap[currentX + directionBX * j, currentY + directionBY * j] = perlinValue;
                    int currentPixelX = currentX + directionBX * j;
                    int currentPixelY = currentY + directionBY * j;
                    //float weight = (currentPixelX + currentPixelY) / (currentX + currentY);
                    float weight = 1 - j / radius;
                    noiseMap[currentPixelX, currentPixelY] += perlinValue * weight;
                }
 
            }
        }


    }



    public static float interpolate(float value)
    {
        return (6 * Mathf.Pow(value, 5) - 15 * Mathf.Pow(value, 4) + 10 * Mathf.Pow(value, 3));
    }

    public static float grad(int hash, float x, float y)
    {
        switch (hash & 0x3)
        {
            case 0x0: return x + y;
            case 0x1: return -x + y;
            case 0x2: return x - y;
            case 0x3: return -x - y;
            default: return 0;
        }
    }

    public static double lerp(double a, double b, double x)
    {
        return a + x * (b - a);
    }

    public static float selfmadeNoise(float x, float y)
    {
        //points in the perlin space
        float pointA = x - Mathf.Floor(x);
        float pointB = y - Mathf.Floor(y);

        // Calculate the "unit cube" that the point asked will be located in
        int xi = (int)x & 255;
        int yi = (int)y & 255;

        //get a semi-random number from a table for 255 included numbers
        int testA = px[px[xi] + yi];
        int testB = px[px[xi + 1] + yi];
        int testC = px[px[xi] + yi + 1];
        int testD = px[px[xi + 1] + yi + 1];

        //based on those random numbers generate a direction, in this case from 4 different directions
        float testAh = grad(testA, pointA, pointB);
        float testBh = grad(testB, pointA - 1, pointB);
        float testCh = grad(testC, pointA, pointB - 1);
        float testDh = grad(testD, pointA - 1, pointB - 1);

        //Combine the directions together, 2 at the time, first on the X axis
        double resultA = lerp(testAh, testBh, interpolate(pointA));
        double resultB = lerp(testCh, testDh, interpolate(pointA));

        //then combine those 2 values together on the y axis
        float endResult = (float)lerp(resultA, resultB, interpolate(pointB));

        endResult = (endResult + 1) / 2;
        return endResult;
    }

}
