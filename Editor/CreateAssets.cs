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
		File.WriteAllText(file.FullName, InspectorTextSupportEnumList(selectedFileName));
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
        var target"+className+@" = target as "+className+@";
        GUI.changed = false;
 
        DrawDefaultInspector();
        if(GUILayout.Button(""Button""))
        {
        }
       if (GUI.changed)
            EditorUtility.SetDirty(target"+className+@");

    }
}";
    }

	public static string InspectorTextSupportEnumList(string className)
	{
		return@"
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;

[CustomEditor(typeof(" + className + @"))]
[CanEditMultipleObjects]
public class "+className+@"Inspector : Editor
{
    public override void OnInspectorGUI()
    {
		var target"+className+@" = target as " + className + @";
		serializedObject.Update ();
		GUI.changed = false;

		var type = typeof(" + className + @");
		var flagsForAllFields = BindingFlags.Public |
			//BindingFlags.NonPublic |
			BindingFlags.Instance |
			BindingFlags.FlattenHierarchy;

		//draw enumlists in a special way
		foreach (var field in type.GetFields(flagsForAllFields))
		{
			bool skip = false;
			foreach (var attribute in field.GetCustomAttributes(typeof(EnumListAttribute), false))
			{
				skip = true;
				var enumListAttribute = attribute as EnumListAttribute;
				EnumListGUI(serializedObject, field.Name, enumListAttribute.Enum);
				break;
			}
			if(!skip)
				EditorGUILayout.PropertyField (serializedObject.FindProperty (field.Name),true);
		}

		//draw buttons for functions with the ContextMenu attribute
		foreach (var method in type.GetMethods ())
		{
			foreach (var attribute in method.GetCustomAttributes(typeof(ContextMenu),false))
			{
				if(GUILayout.Button(((ContextMenu)attribute).menuItem))
				{
					method.Invoke (serializedObject.targetObject, null);
				}
			}
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty (target"+className+@");
			serializedObject.ApplyModifiedProperties ();
			serializedObject.Update ();
		}
    }

	void EnumListGUI(SerializedObject obj, string name, System.Type enumType)
	{
		EditorGUILayout.LabelField (""[""+name+""]"");
		int oldIndent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 1;
		int size = obj.FindProperty(name + "".Array.size"").intValue;
		//ensure the enumlist has the right number of entries
		int newSize = System.Enum.GetValues(enumType).Length;
		if (newSize != size)
			obj.FindProperty(name + "".Array.size"").intValue = newSize;
		for (int i=0;i<newSize;i++)
		{
			var prop = obj.FindProperty(string.Format(""{0}.Array.data[{1}]"", name, i));
			EditorGUILayout.PropertyField(prop);
		}
		EditorGUI.indentLevel = oldIndent;
	}
}";
	}
}
