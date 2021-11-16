using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(CubePerlin))]
public class CubePerlinGenerator : Editor
{
    public override void OnInspectorGUI()
    {
        CubePerlin mapGen = (CubePerlin)target;

        //checks if any values has changed
        if (DrawDefaultInspector())
        {
            if (mapGen.autoUpdate)
            {
                mapGen.GenerateCube();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            mapGen.GenerateCube();
        }
    }
}
