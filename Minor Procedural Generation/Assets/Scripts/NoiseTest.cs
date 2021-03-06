using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseTest : MonoBehaviour
{
    public int row = 25, column = 25, height = 5;
    public float size = 1;
    public float noiseScale = 0.5f;
    public float cutoff;

    public Dictionary<Vector3, float> noisePos = new Dictionary<Vector3, float>();
    List<Vector2> points = new List<Vector2>();
    List<Vector3> positions = new List<Vector3>();
    Dictionary<Vector3, float> vertPos = new Dictionary<Vector3, float>();
    
    public int repeat = 2;

/*    private void Update()
    {
        noiseScale += 0.001f;
        StartNoiseGenerator();
    }*/

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

    private static readonly int[] p;                                                    // Doubled permutation to avoid overflow

    static NoiseTest()
    {
        p = new int[512];
        for (int x = 0; x < 512; x++)
        {
            p[x] = permutation[x % 256];
        }
    }

    public float perlin(float x, float y, float z)
    {
        if (repeat > 0)
        {                                    // If we have any repeat on, change the coordinates to their "local" repetitions
            x = x % repeat;
            y = y % repeat;
            z = z % repeat;
        }

        int xi = (int)x & 255;                              // Calculate the "unit cube" that the point asked will be located in
        int yi = (int)y & 255;                              // The left bound is ( |_x_|,|_y_|,|_z_| ) and the right bound is that
        int zi = (int)z & 255;                              // plus 1.  Next we calculate the location (from 0.0 to 1.0) in that cube.
        float xf = x - (int)x;
        float yf = y - (int)y;
        float zf = z - (int)z;

        float u = fade(xf);
        float v = fade(yf);
        float w = fade(zf);

        float aaa, aba, aab, abb, baa, bba, bab, bbb;
        aaa = p[p[p[xi] + yi] + zi];
        aba = p[p[p[xi] + inc(yi)] + zi];
        aab = p[p[p[xi] + yi] + inc(zi)];
        abb = p[p[p[xi] + inc(yi)] + inc(zi)];
        baa = p[p[p[inc(xi)] + yi] + zi];
        bba = p[p[p[inc(xi)] + inc(yi)] + zi];
        bab = p[p[p[inc(xi)] + yi] + inc(zi)];
        bbb = p[p[p[inc(xi)] + inc(yi)] + inc(zi)];

        float x1, x2, y1, y2;
        x1 = lerp(grad((int)aaa, xf, yf, zf),           // The gradient function calculates the dot product between a pseudorandom
                    grad((int)baa, xf - 1, yf, zf),             // gradient vector and the vector from the input coordinate to the 8
                    u);                                     // surrounding points in its unit cube.
        x2 = lerp(grad((int)aba, xf, yf - 1, zf),           // This is all then lerped together as a sort of weighted average based on the faded (u,v,w)
                    grad((int)bba, xf - 1, yf - 1, zf),             // values we made earlier.
                      u);
        y1 = lerp(x1, x2, v);

        x1 = lerp(grad((int)aab, xf, yf, zf - 1),
                    grad((int)bab, xf - 1, yf, zf - 1),
                    u);
        x2 = lerp(grad((int)abb, xf, yf - 1, zf - 1),
                      grad((int)bbb, xf - 1, yf - 1, zf - 1),
                      u);
        y2 = lerp(x1, x2, v);

        return (lerp(y1, y2, w) + 1) / 2;


    }

    public static float lerp(float a, float b, float x)
    {
        return a + x * (b - a);
    }

    // Source: http://riven8192.blogspot.com/2010/08/calculate-perlinnoise-twice-as-fast.html
    public static float grad(int hash, float x, float y, float z)
    {
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


    public int inc(int num)
    {
        num++;
        if (repeat > 0) num %= repeat;

        return num;
    }


    public static float fade(float t)
    {
        // Fade function as defined by Ken Perlin.  This eases coordinate values
        // so that they will ease towards integral values.  This ends up smoothing
        // the final output.
        return t * t * t * (t * (t * 6 - 15) + 10);         // 6t^5 - 15t^4 + 10t^3
    }
    
    public void StartNoiseGenerator()
    {
        positions.Clear();
        GameObject.FindGameObjectWithTag("MarchingCube").GetComponent<MarchingCubesTest>().cutoff = cutoff;
        noisePos.Clear();
        for(int r = 0; r < row - 1; r++)
        {
            for (int c = 0; c < column -1; c++)
            {
                for (int h = 0; h < height -1; h++)
                {
                    //float noiseIndex = Perlin3D(r * noiseScale, h * noiseScale, c * noiseScale);
                    //Debug.Log(noiseIndex);

                    //noisePos.Add(new Vector3(r * size, h * size, c * size), noiseIndex);

                    Dictionary<Vector3, float> vertPos = new Dictionary<Vector3, float>();
                    Vector3 newPos3 = new Vector3(r * size, h * size, c * size);
                    Vector3 newPos2 = new Vector3((r + 1) * size, h * size, c * size);
                    Vector3 newPos0 = new Vector3(r * size, (h + 1) * size, c * size);
                    Vector3 newPos7 = new Vector3(r * size, h * size, (c + 1) * size);
                    Vector3 newPos1 = new Vector3((r + 1) * size, (h + 1) * size, c * size);
                    Vector3 newPos6 = new Vector3((r + 1) * size, h * size, (c + 1) * size);
                    Vector3 newPos4 = new Vector3(r * size, (h + 1) * size, (c + 1) * size);
                    Vector3 newPos5 = new Vector3((r + 1) * size, (h + 1) * size, (c + 1) * size);
/*                    vertPos.Add(newPos0, Perlin3D(newPos0.x * noiseScale, newPos0.y * noiseScale, newPos0.z * noiseScale));
                    vertPos.Add(newPos1, Perlin3D(newPos1.x * noiseScale, newPos1.y * noiseScale, newPos1.z * noiseScale));
                    vertPos.Add(newPos2, Perlin3D(newPos2.x * noiseScale, newPos2.y * noiseScale, newPos2.z * noiseScale));
                    vertPos.Add(newPos3, Perlin3D(newPos3.x * noiseScale, newPos3.y * noiseScale, newPos3.z * noiseScale));
                    vertPos.Add(newPos4, Perlin3D(newPos4.x * noiseScale, newPos4.y * noiseScale, newPos4.z * noiseScale));
                    vertPos.Add(newPos5, Perlin3D(newPos5.x * noiseScale, newPos5.y * noiseScale, newPos5.z * noiseScale));
                    vertPos.Add(newPos6, Perlin3D(newPos6.x * noiseScale, newPos6.y * noiseScale, newPos6.z * noiseScale));
                    vertPos.Add(newPos7, Perlin3D(newPos7.x * noiseScale, newPos7.y * noiseScale, newPos7.z * noiseScale));*/
                    
                    vertPos.Add(newPos0, perlin(newPos0.x * noiseScale, newPos0.y * noiseScale, newPos0.z * noiseScale));
                    vertPos.Add(newPos1, perlin(newPos1.x * noiseScale, newPos1.y * noiseScale, newPos1.z * noiseScale));
                    vertPos.Add(newPos2, perlin(newPos2.x * noiseScale, newPos2.y * noiseScale, newPos2.z * noiseScale));
                    vertPos.Add(newPos3, perlin(newPos3.x * noiseScale, newPos3.y * noiseScale, newPos3.z * noiseScale));
                    vertPos.Add(newPos4, perlin(newPos4.x * noiseScale, newPos4.y * noiseScale, newPos4.z * noiseScale));
                    vertPos.Add(newPos5, perlin(newPos5.x * noiseScale, newPos5.y * noiseScale, newPos5.z * noiseScale));
                    vertPos.Add(newPos6, perlin(newPos6.x * noiseScale, newPos6.y * noiseScale, newPos6.z * noiseScale));
                    vertPos.Add(newPos7, perlin(newPos7.x * noiseScale, newPos7.y * noiseScale, newPos7.z * noiseScale));

                    positions.Add(newPos0);
                    positions.Add(newPos1);
                    positions.Add(newPos2);
                    positions.Add(newPos3);
                    positions.Add(newPos4);
                    positions.Add(newPos5);
                    positions.Add(newPos6);
                    positions.Add(newPos7);

                    GameObject.FindGameObjectWithTag("MarchingCube").GetComponent<MarchingCubesTest>().ComputeMesh(vertPos);
                    
                }
            }
        }
        GameObject.FindGameObjectWithTag("MarchingCube").GetComponent<MarchingCubesTest>().GenerateMesh();
    }



    public static float Perlin3D(float x, float y, float z)
    {
        float ab = Mathf.PerlinNoise(x,y);
        float bc = Mathf.PerlinNoise(y,z);
        float ac = Mathf.PerlinNoise(x,z);

        float ba = Mathf.PerlinNoise(y,x);
        float cb = Mathf.PerlinNoise(z,y);
        float ca = Mathf.PerlinNoise(z,x);

        float abc = ab + bc + ac + ba + cb + ca;
        return abc / 6f;
    }


    private void OnDrawGizmos()
    {
        /*        foreach(KeyValuePair<Vector3, float> pos in noisePos)
                {
                    Gizmos.color = new Color(pos.Value, pos.Value, pos.Value, 1);
                    Gizmos.DrawSphere(pos.Key, 0.1f);
                }*/

        foreach (Vector3 pos in positions)
        {
            float sample = Perlin3D(pos.x, pos.y, pos.z);
            Gizmos.color = new Color(sample, sample, sample, 1);
            Gizmos.DrawSphere(pos, 0.25f);
        }

/*            foreach (Vector2 noise in points)
        {
            Gizmos.color = Color.red;
            Vector3 pos = new Vector3(noise.x, 0, noise.y);
            Gizmos.DrawSphere(pos, 0.05f);
        }*/
    }

}
