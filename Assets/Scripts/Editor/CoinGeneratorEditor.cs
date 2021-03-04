using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(CoinGenerator))]
public class CoinGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CoinGenerator generator = target as CoinGenerator;
        if(GUILayout.Button("Generate Coins"))
        {
            generator.GenerateCoins();
        }
        if(GUILayout.Button("Delete Coins"))
        {
            generator.CleanCurrentCoins();
        }
    }
}
