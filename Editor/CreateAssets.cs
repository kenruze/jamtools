using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class CreateAssets
{
    [MenuItem("Assets/Create/Singleton Script")]
    public static void CreateSpecialScript()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create Singleton", "Singleton", "cs", "Please select file name to save:");
        if (!string.IsNullOrEmpty(path))
        {
            Debug.Log("write file " + path);
            System.IO.FileInfo file = new System.IO.FileInfo(path);
            AssetDatabase.Refresh ();
            file.Directory.Create(); // If the directory already exists, this method does nothing.
            string className = file.Name;
            className = className.Substring(0,className.IndexOf("."));
            Debug.Log(className);
            File.WriteAllText(file.FullName, SingletonText(className));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh ();
            EditorUtility.FocusProjectWindow();
        }
    }

    [MenuItem("Assets/Create/Data Asset Type")]
    public static void CreateDataAssetType()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if(path == "")
        {
            path = "Assets";
        }
        path = EditorUtility.SaveFilePanel("Create Data Asset Type", path, "DataAssetType", "cs");
        if (!string.IsNullOrEmpty(path))
        {
            Debug.Log("write file " + path);
            System.IO.FileInfo file = new System.IO.FileInfo(path);
            AssetDatabase.Refresh ();
            file.Directory.Create(); // If the directory already exists, this method does nothing.
            string className = file.Name;
            className = className.Substring(0,className.IndexOf("."));
            Debug.Log(className);
            File.WriteAllText(file.FullName, DataAssetTypeText(className, className+"Data"));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh ();
            EditorUtility.FocusProjectWindow();
        }
    }

//    [MenuItem("Assets/Create/AnimatorController")]
//    public static void CreateAnimatorController()
//    {
//        UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath ("Assets/animator.controller");
//    }

    [MenuItem("Assets/Create/EmptyAsset")]
    public static void CreateEmptyAsset()
    {
        //try to assign scriptable object type
//        MonoScript script = null;
//        Type scriptableType = null;
//        if (Selection.activeObject != null && Selection.activeObject is MonoScript)
//        {   
//            script = Selection.activeObject as MonoScript;
//
//            Debug.Log("selection is type " + script.GetClass());
//            if(script.GetClass() is ScriptableObject)
//            {
//                scriptableType = script.GetClass();
//                CustomAssetUtility.CreateAsset<scriptableType>();//can't use template with varriable
//            }
//        }
        CustomAssetUtility.CreateAsset<ScriptableObject>();
//        if (Selection.activeObject != null)
//        {
//            Debug.Log(Selection.activeObject.GetType());
//            var scriptable = Selection.activeObject as ScriptableObject;
//        }
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
    bool showHidden= false;

    public override void OnInspectorGUI()
    {
		var target"+className+@" = target as " + className + @";
		serializedObject.Update ();
		GUI.changed = false;
        bool hidableFields = false;

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
            if (field.GetCustomAttributes(typeof(HideInCustomInspectorAttribute), false).Length > 0)
            {
                hidableFields = true;
                if(!showHidden)
                    skip = true;
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
        if (hidableFields)
        {
            showHidden = GUILayout.Toggle(showHidden, ""Show Hidden Fields"");
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

    public static string SingletonText(string className)
    {
        return@"
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using JamTools;
//using InControl;
//using UnityEngine.UI;

public class "+className+@" : MonoBehaviour
{
    static "+className+@" instance;
    //
    public static "+className+@" Get
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<"+className+@">();
                print(""searched for "+className+@", OK in assembly reload"");
                //should be able to create from resource name
                //if (s_instance == null)
                //{
                //    GameObject prefab = Resources.Load<GameObject>("""+className+@""");
                //    s_instance = Instantiate<GameObject>(prefab).GetComponent<"+className+@">();
                //}
            }
            return instance;
        }
    }
    void Awake()
    {
        instance = this;
    }
}";
    }

    public static string DataAssetTypeText(string className, string subclassName)
    {
    return@"using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class "+className+@" : ScriptableObject
{
    public "+subclassName+@" data;
}

[System.Serializable]
public class "+subclassName+@"
{
    //fill public data here
}";
    }
}
