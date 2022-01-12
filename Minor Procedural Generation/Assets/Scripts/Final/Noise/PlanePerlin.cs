using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class  PlanePerlin
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

    static PlanePerlin()
    {
        px = new int[512];
        for (int x = 0; x < 512; x++)
        {
            px[x] = permutation[x % 256];
        }
    }

    public static float[,] GenerateNoiseMap(float low, float high, float weight2, float agressiveness, int mapWidth, int mapHeight, float scale, int seed)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        if(scale <= 0)
        {
            scale = 0.0001f;
        }

        for(int y=0; y< mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {

                float frequency = scale;
                //height
                float persistence = 0.5f;
                //frequency
                float lacunarity = 0.75f;
                float endPerlin = 0;
                float weight = 1;
                float gain = 5f;


                for (int j = 0; j < 1; j++)
                {
                    float sampleX = x / frequency;
                    float sampleY = y / frequency;

                    float perlinValue = selfmadeNoise(sampleX, sampleY);

                    endPerlin += perlinValue * weight;

                    //endPerlin = Mathf.Abs(endPerlin);
                    //weight = endPerlin * gain;

                    //endPerlin = 1 - endPerlin;
                    //endPerlin *= endPerlin;
                    weight *= persistence;
                    frequency *= lacunarity;

                    //weight = weight > 1.0f ? 1.0f : weight < 0f ? 0f : weight;

                }

                float freq = 100;
                float sampleX2 = x / freq;
                float sampleY2 = y / freq;
                float cavePerlin = 1;

                for (int n = 1; n < 6; n++)
                {
                    float factor = Mathf.Pow(2, n);
                    cavePerlin -= selfmadeNoise(sampleX2 * factor, sampleY2 * factor) / factor;
                }


                float sampleX3 = x / frequency;
                float sampleY3 = y / frequency;
                endPerlin = 1;
                for (int n = 1; n < 6; n++)
                {
                    float factor = Mathf.Pow(2, n);
                    endPerlin -= selfmadeNoise(sampleX3 * factor, sampleY3 * factor) / factor;
                }

                //endPerlin += selfmadeNoise(x / agressiveness, y / agressiveness) * weight2;
                //float newHigh2 = high;
                //newHigh2 -= selfmadeNoise(x / agressiveness, y / agressiveness) * weight2;

                if(y > mapHeight / 2)
                {
                    float delta = y - mapHeight / 2;
                    //cavePerlin += (delta / (mapHeight / 2))/5;
                }

                float caveheight = 0.45f;
                if (cavePerlin > caveheight)
                {
                    cavePerlin = 0;
                }
                else
                {
                    cavePerlin = 1;
                }

                //endPerlin = cavePerlin;

                float newHigh = high;
                float newLow = low;

                float scalar = selfmadeNoise(x / 25.5f, y / 25.5f);
                if(scalar > 0.5f)
                {
                    //scalar = scalar + (1 - scalar) / agressiveness;
                }
                else
                {
                    //scalar = scalar - scalar / agressiveness;
                }

                scalar *= agressiveness;
                scalar -= agressiveness / 2;


                if (weight2 != 0)
                {
                    newHigh += scalar * weight2;
                    newLow -= scalar * weight2;
                }

                if(endPerlin < newHigh && endPerlin > newLow)
                {
                    float value2 = endPerlin - newLow;
                    value2 = value2 * (1 / (newHigh - newLow));
                    value2 *= 2;
                    value2 -= 1;
                    if (value2 < 0)
                    {
                        value2 *= -1;
                    }
                    endPerlin = 1 - value2;
                }
                else
                {
                    endPerlin = 0;
                }
                //noiseMap[x, y] = cavePerlin + endPerlin;
                noiseMap[x, y] = endPerlin;
                //noiseMap[x, y] = selfmadeNoise(x / 50.5f, y / 50.5f);

            }
        }

        return noiseMap;
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
        float testBh = grad(testB, pointA-1, pointB);
        float testCh = grad(testC,pointA, pointB-1);
        float testDh = grad(testD, pointA-1, pointB-1);

        //Combine the directions together, 2 at the time, first on the X axis
        double resultA = lerp(testAh, testBh, interpolate(pointA));
        double resultB = lerp(testCh, testDh, interpolate(pointA));

        //then combine those 2 values together on the y axis
        float endResult= (float)lerp(resultA, resultB, interpolate(pointB));

        endResult = (endResult + 1) / 2;
        return endResult;
    }


































































    private static int[,] grad3 = {{1,1,0},{-1,1,0},{1,-1,0},{-1,-1,0},
                                                        {1,0,1},{-1,0,1},{1,0,-1},{-1,0,-1},
                                                        {0,1,1},{0,-1,1},{0,1,-1},{0,-1,-1}};
    private static int[,] grad4 = {{0,1,1,1}, {0,1,1,-1}, {0,1,-1,1}, {0,1,-1,-1},
 {0,-1,1,1}, {0,-1,1,-1}, {0,-1,-1,1}, {0,-1,-1,-1},
 {1,0,1,1}, {1,0,1,-1}, {1,0,-1,1}, {1,0,-1,-1},
 {-1,0,1,1}, {-1,0,1,-1}, {-1,0,-1,1}, {-1,0,-1,-1},
 {1,1,0,1}, {1,1,0,-1}, {1,-1,0,1}, {1,-1,0,-1},
 {-1,1,0,1}, {-1,1,0,-1}, {-1,-1,0,1}, {-1,-1,0,-1},
 {1,1,1,0}, {1,1,-1,0}, {1,-1,1,0}, {1,-1,-1,0},
 {-1,1,1,0}, {-1,1,-1,0}, {-1,-1,1,0}, {-1,-1,-1,0}};
    private static int[] p = {151,160,137,91,90,15,
 131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
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
 138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180};
    // To remove the need for index wrapping, double the permutation table length
    private static int[] perm = new int[512];


    // A lookup table to traverse the simplex around a given point in 4D.
    // Details can be found where this table is used, in the 4D noise method.
    private static int[,] simplex = {
 {0,1,2,3},{0,1,3,2},{0,0,0,0},{0,2,3,1},{0,0,0,0},{0,0,0,0},{0,0,0,0},{1,2,3,0},
 {0,2,1,3},{0,0,0,0},{0,3,1,2},{0,3,2,1},{0,0,0,0},{0,0,0,0},{0,0,0,0},{1,3,2,0},
 {0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},
 {1,2,0,3},{0,0,0,0},{1,3,0,2},{0,0,0,0},{0,0,0,0},{0,0,0,0},{2,3,0,1},{2,3,1,0},
 {1,0,2,3},{1,0,3,2},{0,0,0,0},{0,0,0,0},{0,0,0,0},{2,0,3,1},{0,0,0,0},{2,1,3,0},
 {0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},
 {2,0,1,3},{0,0,0,0},{0,0,0,0},{0,0,0,0},{3,0,1,2},{3,0,2,1},{0,0,0,0},{3,1,2,0},
 {2,1,0,3},{0,0,0,0},{0,0,0,0},{0,0,0,0},{3,1,0,2},{0,0,0,0},{3,2,0,1},{3,2,1,0}};
    // This method is a *lot* faster than using (int)Math.floor(x)
    private static int fastfloor(double x)
    {
        return x > 0 ? (int)x : (int)x - 1;
    }
    private static double dot(int[] g, double x, double y)
    {
        return g[0] * x + g[1] * y;
    }
    private static double dot(int[] g, double x, double y, double z)
    {
        return g[0] * x + g[1] * y + g[2] * z;
    }
    private static double dot(int[] g, double x, double y, double z, double w)
    {
        return g[0] * x + g[1] * y + g[2] * z + g[3] * w;
    }

    // 2D simplex noise
    public static double noise(double xin, double yin)
    {
        double n0, n1, n2; // Noise contributions from the three corners
                           // Skew the input space to determine which simplex cell we're in
        double F2 = 0.5 * (Mathf.Sqrt(3.0f) - 1.0);
        double s = (xin + yin) * F2; // Hairy factor for 2D
        int i = fastfloor(xin + s);
        int j = fastfloor(yin + s);
        double G2 = (3.0 - Mathf.Sqrt(3.0f)) / 6.0;
        double t = (i + j) * G2;
        double X0 = i - t; // Unskew the cell origin back to (x,y) space
        double Y0 = j - t;
        double x0 = xin - X0; // The x,y distances from the cell origin
        double y0 = yin - Y0;
        // For the 2D case, the simplex shape is an equilateral triangle.
        // Determine which simplex we are in.
        int i1, j1; // Offsets for second (middle) corner of simplex in (i,j) coords
        if (x0 > y0) { i1 = 1; j1 = 0; } // lower triangle, XY order: (0,0)->(1,0)->(1,1)
        else { i1 = 0; j1 = 1; } // upper triangle, YX order: (0,0)->(0,1)->(1,1)
                                 // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
                                 // a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
                                 // c = (3-sqrt(3))/6
        double x1 = x0 - i1 + G2; // Offsets for middle corner in (x,y) unskewed coords
        double y1 = y0 - j1 + G2;
        double x2 = x0 - 1.0 + 2.0 * G2; // Offsets for last corner in (x,y) unskewed coords
        double y2 = y0 - 1.0 + 2.0 * G2;
        // Work out the hashed gradient indices of the three simplex corners
        int ii = i & 255;
        int jj = j & 255;
        int gi0 = perm[ii + perm[jj]] % 12;
        int gi1 = perm[ii + i1 + perm[jj + j1]] % 12;
        int gi2 = perm[ii + 1 + perm[jj + 1]] % 12;
        // Calculate the contribution from the three corners
        double t0 = 0.5 - x0 * x0 - y0 * y0;
        if (t0 < 0) n0 = 0.0;
        else
        {
            t0 *= t0;
            int[] value = new int[2];
            value[0] = grad3[gi0, 0];
            value[1] = grad3[gi0, 1];
            n0 = t0 * t0 * dot(value, x0, y0); // (x,y) of grad3 used for 2D gradient
        }
        double t1 = 0.5 - x1 * x1 - y1 * y1;
        if (t1 < 0) n1 = 0.0;
        else
        {
            t1 *= t1;
            int[] value = new int[2];
            value[0] = grad3[gi1, 0];
            value[1] = grad3[gi1, 1];
            n1 = t1 * t1 * dot(value, x1, y1);
        }
        double t2 = 0.5 - x2 * x2 - y2 * y2;
        if (t2 < 0) n2 = 0.0;
        else
        {
            t2 *= t2;
            int[] value = new int[2];
            value[0] = grad3[gi2, 0];
            value[1] = grad3[gi2, 1];
            n2 = t2 * t2 * dot(value, x2, y2);
        }
        // Add contributions from each corner to get the final noise value.
        // The result is scaled to return values in the interval [-1,1].
        return 70.0 * (n0 + n1 + n2);
    }
}
