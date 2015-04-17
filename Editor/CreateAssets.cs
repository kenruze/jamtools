using UnityEngine;
using UnityEditor;
using System;

public class CreateAssets
{
    [MenuItem("Assets/Create/EmptyAsset")]
    public static void CreateEmptyAsset()
    {
        CustomAssetUtility.CreateAsset<ScriptableObject>();
    }

}
