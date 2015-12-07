using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class ScriptableObjectCreator<T> : ScriptableObject where T : ScriptableObject {

	private static string ObjectPath;
	private static string ObjectFolderPath;
	private static string ObjectName;
	private static string ObjectParentFolder;

	private static T _reference;
	public static T Get(string name, string folderParent)
	{
		ObjectName = name;
		ObjectParentFolder = folderParent.Remove(folderParent.IndexOf("Resources")-1,folderParent.Length - (folderParent.IndexOf("Resources")-1));
		if(_reference == null)
		{
			if(!Application.isPlaying)
			{
				_reference = CreateObject();
				UpdateObjectFolderPath();
			}
			else
			{
				_reference = LoadInstance();
			}
		}

		return _reference;
	}

	public static T LoadInstance()
	{
		string path = ObjectPath;
		path = path.Remove(0, path.IndexOf("Resources") + 10);
		return Resources.Load<T>(path);
	}
	
	public static T CreateObject()
	{
		string path = "";
		T sc = null;

		RecursivelyFindFolderPath("Assets");
		RecursivelyFindAsset("Assets");
		path = ObjectPath;
		
		if(ObjectPath != null && !ObjectPath.Contains("Resources"))
		{
			Debug.LogWarning("The config file needs to be in a resources folder!");
		}
		
		if(path == null)
		{
			path = ObjectFolderPath + "/Resources/"+ObjectName;
		}

		if(AssetDatabase.LoadAssetAtPath(path,typeof(T)) == null)
		{
			sc = ScriptableObject.CreateInstance<T> ();
			
			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path);
			
			AssetDatabase.CreateAsset (sc, assetPathAndName);
			
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow ();
		}
		
		sc = AssetDatabase.LoadAssetAtPath(path,typeof(T)) as T;
		
		return sc;
	}
	
	public static void UpdateObjectFolderPath()
	{
		RecursivelyFindFolderPath("Assets");
		RecursivelyFindAsset("Assets");
	}

	private static void RecursivelyFindFolderPath(string dir)
	{
		string[] paths = Directory.GetDirectories(dir);
		foreach(string s in paths)
		{
			if(s.Contains(ObjectParentFolder))
			{
				ObjectFolderPath = s;
				break;
			}
			else
			{
				RecursivelyFindFolderPath(s);
			}
		}
	}
	
	private static void RecursivelyFindAsset(string dir)
	{
		string[] paths = Directory.GetDirectories(dir);
		foreach(string s in paths)
		{
			string[] files = Directory.GetFiles(s);
			foreach(string f in files)
			{
				if(f.Contains(ObjectName))
				{
					ObjectPath = f;
					break;
				}
			}
			RecursivelyFindAsset(s);
		}
	}
}
