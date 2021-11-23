using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(CubeWorms))]
public class CubePerlinGenerator : Editor
{
    public override void OnInspectorGUI()
    {
        CubeWorms mapGen = (CubeWorms)target;


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
