using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class CreateAssets
{
    [MenuItem("Assets/Create/EmptyAsset")]
    public static void CreateEmptyAsset()
    {
        CustomAssetUtility.CreateAsset<ScriptableObject>();
    }

    [MenuItem("Assets/Create/Inspector")]
    public static void CreateInspector()
    {
        CreateCustomInspector();
    }

    public static void CreateCustomInspector(string fileClassName = "BehaviourTypeInspector")
    {
        string selectedFileName = "BehaviourType";

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if(path == "")
        {
            path = "Assets";
        }
        else if(Path.GetExtension(path) != "")
        {
            Debug.Log(path);
            selectedFileName = Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject));
            Debug.Log(selectedFileName);
            path = path.Replace(selectedFileName, "");
            Debug.Log(path);
            selectedFileName = selectedFileName.Substring(0, selectedFileName.IndexOf("."));
            Debug.Log(selectedFileName);
            fileClassName = selectedFileName + "Inspector";
            Debug.Log(fileClassName);

        }
		path += (path.Contains ("Editor") ? "/" : "/Editor/") + fileClassName + ".cs";
        //string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + fileClassName + ".cs");
		Debug.Log("write file " + path);
		System.IO.FileInfo file = new System.IO.FileInfo(path);
		AssetDatabase.Refresh ();
		file.Directory.Create(); // If the directory already exists, this method does nothing.
		File.WriteAllText(file.FullName, InspectorText(selectedFileName));
        AssetDatabase.SaveAssets();
		AssetDatabase.Refresh ();
        EditorUtility.FocusProjectWindow();
    }

    public static string InspectorText(string className)
    {
        return @"using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(" + className + @"))]
public class "+className+@"Inspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if(GUILayout.Button(""Button""))
        {
        }
    }
}";
    }
}
