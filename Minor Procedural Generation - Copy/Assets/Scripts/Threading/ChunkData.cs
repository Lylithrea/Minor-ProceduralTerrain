using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class ChunkData : MonoBehaviour
{
    public int row = 25, column = 25, height = 5;
    public float size = 1;

    //public int r = 3, c = 3, h = 3;

    public ComputeShader compute;
    


    [Range(0, 1)]
    public float cutoff;
    public float groundLevel;
    public float layerThickness = 5;

    [Range(0.43f, 0.47f)]
    public float noiseScale = 0.5f;
    public int repeat = 9;

    public Vector3 startingValue = new Vector3(0, 0, 0);

    Dictionary<Vector3, float> vertPos = new Dictionary<Vector3, float>();

    public MCA marchingCube;


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

    public Vector3[] vertPos2;
    public float[] noisePos;

    float[] test;


    ComputeBuffer noiseBuffer;
    ComputeBuffer verticesBuffer;
    ComputeBuffer comp;

    public void Awake()
    {
        int chunkSize = row * column * height;
        compute.SetFloat("chunkSize", chunkSize);
        compute.SetFloat("row", row);
        compute.SetFloat("height", height);
        compute.SetFloat("column", column);
        compute.SetFloat("size", size);
        compute.SetFloat("noiseScale", noiseScale);
        compute.SetFloat("repeat", repeat);

        Vector3 middlePoint = new Vector3((row + (row % 2)) / 2, (height + (height % 2)) / 2, (column + (column % 2)) / 2);
        middlePoint *= -1;

        compute.SetVector("middlePoint", middlePoint);
        compute.SetVector("startingValue", startingValue);

        vertPos2 = new Vector3[chunkSize];
        verticesBuffer = new ComputeBuffer(chunkSize, sizeof(float) * 3);

        verticesBuffer.SetData(vertPos2);
        compute.SetBuffer(0, "vertPos", verticesBuffer);


        noisePos = new float[chunkSize];
        noiseBuffer = new ComputeBuffer(chunkSize, sizeof(float));

        noiseBuffer.SetData(noisePos);
        compute.SetBuffer(0, "noisePos", noiseBuffer);



        test = new float[chunkSize];
        var comp = new ComputeBuffer(chunkSize, sizeof(float));      //start compute
        comp.SetData(test);
        compute.SetBuffer(0, "test", comp);
    }

    public void TestChunk()
    {
        /*        float[] test = new float[4];
                var comp = new ComputeBuffer(4, sizeof(float));      //start compute
                comp.SetData(test);
                compute.SetBuffer(0, "test", comp);
                compute.SetFloat(0, 1);

                Vector3[] vertPos = new Vector3[8];
                var verticesBuffer = new ComputeBuffer(8, sizeof(float) * 3);
                verticesBuffer.SetData(vertPos);
                compute.SetBuffer(0, "vertPos", verticesBuffer);

                compute.Dispatch(0, 4, 1, 1);
                comp.GetData(test);       //get data
                comp.Release(); //release from memory*/



        int chunkSize = row * column * height;




        compute.SetFloat("chunkSize", chunkSize);
        compute.SetFloat("row", row);
        compute.SetFloat("height", height);
        compute.SetFloat("column", column);
        compute.SetFloat("size", size);
        compute.SetFloat("noiseScale", noiseScale);
        compute.SetFloat("repeat", repeat);

        Vector3 middlePoint = new Vector3((row + (row % 2)) / 2, (height + (height % 2)) / 2, (column + (column % 2)) / 2);
        middlePoint *= -1;

        compute.SetVector("middlePoint", middlePoint);
        compute.SetVector("startingValue", startingValue);
        


        vertPos2 = new Vector3[chunkSize];
        var verticesBuffer = new ComputeBuffer(chunkSize, sizeof(float) * 3);

        verticesBuffer.SetData(vertPos2);
        compute.SetBuffer(0, "vertPos", verticesBuffer);


        noisePos = new float[chunkSize];
        var noiseBuffer = new ComputeBuffer(chunkSize, sizeof(float));

        noiseBuffer.SetData(noisePos);
        compute.SetBuffer(0, "noisePos", noiseBuffer);



        float[] test = new float[chunkSize];
        var comp = new ComputeBuffer(chunkSize, sizeof(float));      //start compute
        comp.SetData(test);
        compute.SetBuffer(0, "test", comp);




        compute.Dispatch(0, 1024, 1, 1);

        //dont do get data directly after it dispatched, it needs time to process, but what?

        comp.GetData(test);       //get data
        comp.Release(); //release from memory*/


        verticesBuffer.GetData(vertPos2);       //get data

        noiseBuffer.GetData(noisePos);       //get data

        verticesBuffer.Release(); //release from memory
        noiseBuffer.Release();


        for (int i =0; i < vertPos2.Length; i += 8)
        {
            vertPos.Clear();
            
            vertPos.Add(vertPos2[i], noisePos[i]);
            vertPos.Add(vertPos2[i+1], noisePos[i+1]);
            vertPos.Add(vertPos2[i+2], noisePos[i+2]);
            vertPos.Add(vertPos2[i+3], noisePos[i+3]);
            vertPos.Add(vertPos2[i+4], noisePos[i+4]);
            vertPos.Add(vertPos2[i+5], noisePos[i+5]);
            vertPos.Add(vertPos2[i+6], noisePos[i+6]);
            vertPos.Add(vertPos2[i+7], noisePos[i+7]);

            marchingCube.ComputeMesh(vertPos);
        }
        marchingCube.GenerateMesh();
        marchingCube.SetMesh();

        /*
                for(int i =0; i < r*c*h; i++) //triple foreach loop condensed into 1 for loop
                {
                    int x = i % r;
                    int y = Mathf.FloorToInt((i / h) % h);
                    int z = Mathf.FloorToInt(i / (h * r));
                }*/

    }


    static ChunkData()
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
        return t * t * t * (t * (t * 6 - 15) + 10);         // 6t^5 - 15t^4 + 10t^3
    }


    public void StartNoiseGenerator()
    {
        //startingValue = starting;
        Debug.Log("starting");
        //marchingCube = this.transform.GetComponent<MCA>();
        marchingCube.startingValue = startingValue;
        marchingCube.cutoff = cutoff;
        marchingCube.groundLevel = groundLevel;
        marchingCube.layerThickness = layerThickness;

        Vector3 middlePoint = new Vector3((row + (row % 2)) / 2, (height + (height % 2)) / 2, (column + (column % 2)) / 2);
        middlePoint *= -1;

        int chunkSize = row * column * height;

        //this go into compute shader
        //deliver the chunk size, and the size
        //return an vector3 array of all the positions

        for (int i = 0; i < chunkSize; i++) //triple foreach loop condensed into 1 for loop
        {
            float r = middlePoint.x + (i % row);
            float h = middlePoint.y + (Mathf.FloorToInt((i / height) % height));
            float c = middlePoint.z + (Mathf.FloorToInt(i / (height * row)));

            Dictionary<Vector3, float> vertPos = new Dictionary<Vector3, float>();
            Vector3 newPos3 = new Vector3(r * size, h * size, c * size);
            Vector3 newPos2 = new Vector3((r + 1) * size, h * size, c * size);
            Vector3 newPos0 = new Vector3(r * size, (h + 1) * size, c * size);
            Vector3 newPos7 = new Vector3(r * size, h * size, (c + 1) * size);
            Vector3 newPos1 = new Vector3((r + 1) * size, (h + 1) * size, c * size);
            Vector3 newPos6 = new Vector3((r + 1) * size, h * size, (c + 1) * size);
            Vector3 newPos4 = new Vector3(r * size, (h + 1) * size, (c + 1) * size);
            Vector3 newPos5 = new Vector3((r + 1) * size, (h + 1) * size, (c + 1) * size);

            vertPos.Add(newPos0, perlin((newPos0.x + startingValue.x) * noiseScale, (newPos0.y + startingValue.y) * noiseScale, (newPos0.z + startingValue.z) * noiseScale));
            vertPos.Add(newPos1, perlin((newPos1.x + startingValue.x) * noiseScale, (newPos1.y + startingValue.y) * noiseScale, (newPos1.z + startingValue.z) * noiseScale));
            vertPos.Add(newPos2, perlin((newPos2.x + startingValue.x) * noiseScale, (newPos2.y + startingValue.y) * noiseScale, (newPos2.z + startingValue.z) * noiseScale));
            vertPos.Add(newPos3, perlin((newPos3.x + startingValue.x) * noiseScale, (newPos3.y + startingValue.y) * noiseScale, (newPos3.z + startingValue.z) * noiseScale));
            vertPos.Add(newPos4, perlin((newPos4.x + startingValue.x) * noiseScale, (newPos4.y + startingValue.y) * noiseScale, (newPos4.z + startingValue.z) * noiseScale));
            vertPos.Add(newPos5, perlin((newPos5.x + startingValue.x) * noiseScale, (newPos5.y + startingValue.y) * noiseScale, (newPos5.z + startingValue.z) * noiseScale));
            vertPos.Add(newPos6, perlin((newPos6.x + startingValue.x) * noiseScale, (newPos6.y + startingValue.y) * noiseScale, (newPos6.z + startingValue.z) * noiseScale));
            vertPos.Add(newPos7, perlin((newPos7.x + startingValue.x) * noiseScale, (newPos7.y + startingValue.y) * noiseScale, (newPos7.z + startingValue.z) * noiseScale));

            //marchingCube.ComputeMesh(vertPos);

        }


/*
        for (int r = (int)middlePoint.x; r < row / 2; r++)
        {
            for (int c = (int)middlePoint.z; c < column / 2; c++)
            {
                for (int h = (int)middlePoint.y; h < height / 2; h++)
                {
                    Dictionary<Vector3, float> vertPos = new Dictionary<Vector3, float>();
                    Vector3 newPos3 = new Vector3(r * size, h * size, c * size);
                    Vector3 newPos2 = new Vector3((r + 1) * size, h * size, c * size);
                    Vector3 newPos0 = new Vector3(r * size, (h + 1) * size, c * size);
                    Vector3 newPos7 = new Vector3(r * size, h * size, (c + 1) * size);
                    Vector3 newPos1 = new Vector3((r + 1) * size, (h + 1) * size, c * size);
                    Vector3 newPos6 = new Vector3((r + 1) * size, h * size, (c + 1) * size);
                    Vector3 newPos4 = new Vector3(r * size, (h + 1) * size, (c + 1) * size);
                    Vector3 newPos5 = new Vector3((r + 1) * size, (h + 1) * size, (c + 1) * size);

                    vertPos.Add(newPos0, perlin((newPos0.x + startingValue.x) * noiseScale, (newPos0.y + startingValue.y) * noiseScale, (newPos0.z + startingValue.z) * noiseScale));
                    vertPos.Add(newPos1, perlin((newPos1.x + startingValue.x) * noiseScale, (newPos1.y + startingValue.y) * noiseScale, (newPos1.z + startingValue.z) * noiseScale));
                    vertPos.Add(newPos2, perlin((newPos2.x + startingValue.x) * noiseScale, (newPos2.y + startingValue.y) * noiseScale, (newPos2.z + startingValue.z) * noiseScale));
                    vertPos.Add(newPos3, perlin((newPos3.x + startingValue.x) * noiseScale, (newPos3.y + startingValue.y) * noiseScale, (newPos3.z + startingValue.z) * noiseScale));
                    vertPos.Add(newPos4, perlin((newPos4.x + startingValue.x) * noiseScale, (newPos4.y + startingValue.y) * noiseScale, (newPos4.z + startingValue.z) * noiseScale));
                    vertPos.Add(newPos5, perlin((newPos5.x + startingValue.x) * noiseScale, (newPos5.y + startingValue.y) * noiseScale, (newPos5.z + startingValue.z) * noiseScale));
                    vertPos.Add(newPos6, perlin((newPos6.x + startingValue.x) * noiseScale, (newPos6.y + startingValue.y) * noiseScale, (newPos6.z + startingValue.z) * noiseScale));
                    vertPos.Add(newPos7, perlin((newPos7.x + startingValue.x) * noiseScale, (newPos7.y + startingValue.y) * noiseScale, (newPos7.z + startingValue.z) * noiseScale));

                    marchingCube.ComputeMesh(vertPos);

                }
            }
        }*/
        //marchingCube.GenerateMesh();
    }

}


/*
// Each #kernel tells which function to compile; you can have many kernels
#pragma kernel CSMain

// Create a RenderTexture with enableRandomWrite flag and set it
// with cs.SetTexture
float1 chunkSize;
float1 row;
float1 height;
float1 column;
float1 size;
float1 noiseScale;
float1 repeat;

float3 middlePoint;
float3 startingValue;

RWStructuredBuffer<float3> vertPos;
RWStructuredBuffer<float1> noisePos;

static int permutation[] = { 151,160,137,91,90,15,                 // Hash lookup table as defined by Ken Perlin.  This is a randomly
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

int p[];




static float lerp(float a, float b, float x)
{
    return a + x * (b - a);
}


static float grad(int hash, float x, float y, float z)
{
    switch (hash & 0xF)
    {
        case 0x0:
            return x + y;
        case 0x1:
            return -x + y;
        case 0x2:
            return x - y;
        case 0x3:
            return -x - y;
        case 0x4:
            return x + z;
        case 0x5:
            return -x + z;
        case 0x6:
            return x - z;
        case 0x7:
            return -x - z;
        case 0x8:
            return y + z;
        case 0x9:
            return -y + z;
        case 0xA:
            return y - z;
        case 0xB:
            return -y - z;
        case 0xC:
            return y + x;
        case 0xD:
            return -y + z;
        case 0xE:
            return y - x;
        case 0xF:
            return -y - z;
        default:
            return 0; // never happens
    }
}

int inc(int num)
{
    num++;
    if (repeat > 0)
        num %= repeat;

    return num;
}

static float fade(float t)
{
    return t * t * t * (t * (t * 6 - 15) + 10); // 6t^5 - 15t^4 + 10t^3
}

float perlin(float1 x, float1 y, float1 z)
{
    if (repeat > 0)
    { // If we have any repeat on, change the coordinates to their "local" repetitions
        x = x % repeat;
        y = y % repeat;
        z = z % repeat;
    }

    int xi = (int)x & 255; // Calculate the "unit cube" that the point asked will be located in
    int yi = (int)y & 255; // The left bound is ( |_x_|,|_y_|,|_z_| ) and the right bound is that
    int zi = (int)z & 255; // plus 1.  Next we calculate the location (from 0.0 to 1.0) in that cube.
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
    x1 = lerp(grad((int)aaa, xf, yf, zf), // The gradient function calculates the dot product between a pseudorandom
                grad((int)baa, xf - 1, yf, zf), // gradient vector and the vector from the input coordinate to the 8
                u); // surrounding points in its unit cube.
    x2 = lerp(grad((int)aba, xf, yf - 1, zf), // This is all then lerped together as a sort of weighted average based on the faded (u,v,w)
                grad((int)bba, xf - 1, yf - 1, zf), // values we made earlier.
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



/*
[numthreads(8, 1, 1)]
void CSMain(uint3 id : SV_DispatchThreadID)
{
    //for (int x = 0; x < 512; x++)
    //{
    //    p[x] = permutation[x % 256];
    //}


    for (int i = 0; i < chunkSize; i++) //triple foreach loop condensed into 1 for loop
    {
        float r = middlePoint[0] + (i % row);
        float h = middlePoint[1] + (floor((i / height) % height));
        float c = middlePoint[2] + (floor(i / (height * row)));


        float3 newPos3 = float3(r * size, h * size, c * size);
        float3 newPos2 = float3((r + 1) * size, h * size, c * size);
        float3 newPos0 = float3(r * size, (h + 1) * size, c * size);
        float3 newPos7 = float3(r * size, h * size, (c + 1) * size);
        float3 newPos1 = float3((r + 1) * size, (h + 1) * size, c * size);
        float3 newPos6 = float3((r + 1) * size, h * size, (c + 1) * size);
        float3 newPos4 = float3(r * size, (h + 1) * size, (c + 1) * size);
        float3 newPos5 = float3((r + 1) * size, (h + 1) * size, (c + 1) * size);

        noisePos[0] = perlin((newPos0.x + startingValue[0]) * noiseScale, (newPos0.y + startingValue[1]) * noiseScale, (newPos0.z + startingValue[2]) * noiseScale);
        noisePos[1] = perlin((newPos1.x + startingValue[0]) * noiseScale, (newPos1.y + startingValue[1]) * noiseScale, (newPos1.z + startingValue[2]) * noiseScale);
        noisePos[2] = perlin((newPos2.x + startingValue[0]) * noiseScale, (newPos2.y + startingValue[1]) * noiseScale, (newPos2.z + startingValue[2]) * noiseScale);
        noisePos[3] = perlin((newPos3.x + startingValue[0]) * noiseScale, (newPos3.y + startingValue[1]) * noiseScale, (newPos3.z + startingValue[2]) * noiseScale);
        noisePos[4] = perlin((newPos4.x + startingValue[0]) * noiseScale, (newPos4.y + startingValue[1]) * noiseScale, (newPos4.z + startingValue[2]) * noiseScale);
        noisePos[5] = perlin((newPos5.x + startingValue[0]) * noiseScale, (newPos5.y + startingValue[1]) * noiseScale, (newPos5.z + startingValue[2]) * noiseScale);
        noisePos[6] = perlin((newPos6.x + startingValue[0]) * noiseScale, (newPos6.y + startingValue[1]) * noiseScale, (newPos6.z + startingValue[2]) * noiseScale);
        noisePos[7] = perlin((newPos7.x + startingValue[0]) * noiseScale, (newPos7.y + startingValue[1]) * noiseScale, (newPos7.z + startingValue[2]) * noiseScale);

        vertPos[0] = newPos0;
        vertPos[1] = newPos1;
        vertPos[2] = newPos2;
        vertPos[3] = newPos3;
        vertPos[4] = newPos4;
        vertPos[5] = newPos5;
        vertPos[6] = newPos6;
        vertPos[7] = newPos7;
    }

}
*/
