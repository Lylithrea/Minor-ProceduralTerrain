using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class MarchingCubesWindow : EditorWindow
{
    [MenuItem("Window/MarchingCubes")]
    public static void ShowWindow()
    {
        GetWindow<MarchingCubesWindow>("MarchingCubes");
    }

    string rows = "", columns = "", height = "", size = "", cutoff = "";

    private void OnGUI()
    {
        /*        GUILayout.Label("Select the points, and press calculate to see if marching cubes algorithm works.", EditorStyles.wordWrappedLabel);

                if (GUILayout.Button("Calculate"))
                {
                    GameObject marchingCube = GameObject.FindGameObjectWithTag("MarchingCube");
                    marchingCube.GetComponent<MarchingCubesTest>().CalculateTriangles();
                }*/

        if (GUILayout.Button("Generate Noise"))
        {
            GameObject noiseGenerator = GameObject.FindGameObjectWithTag("NoiseGenerator");
            //noiseGenerator.GetComponent<NoiseTest>().StartNoiseGenerator();
        }


        GUILayout.Label("Generate a perlin noise area for the marching cubes", EditorStyles.wordWrappedLabel);


        rows = EditorGUILayout.TextField("Rows: ", rows);
        columns = EditorGUILayout.TextField("Columns: ", columns);
        height = EditorGUILayout.TextField("Height: ", height);
        size = EditorGUILayout.TextField("Size: ", size);
        cutoff = EditorGUILayout.TextField("Cutoff: ", cutoff);


        if (GUILayout.Button("Generate"))
        {
            GameObject perlinNoise = GameObject.FindGameObjectWithTag("NoiseGenerator");
            NoiseTest noise = perlinNoise.GetComponent<NoiseTest>();
            noise.row = Int32.Parse(rows);
            noise.column = Int32.Parse(columns);
            noise.height = Int32.Parse(height);
            noise.size = float.Parse(size);
            noise.cutoff = float.Parse(cutoff);
            //noise.StartNoiseGenerator();
            //noise.isRunning = true;
        }

    }

}
