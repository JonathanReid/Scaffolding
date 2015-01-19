using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;

namespace Scaffolding {

	public enum ViewType
	{
		View,
		Overlay,
	}

	public class ScaffoldingConfig : ScriptableObject{
	 
		public const string VIEW_NAME = "[VIEW_NAME]";
		public const string VIEW_TYPE = "[VIEW_TYPE]";
		public const string MODEL_NAME = "[MODEL_NAME]";
		public const string MODEL_TYPE = "[MODEL_TYPE]";

	    public string ScaffoldingResourcesPath = "Assets/Resources/Views/";
	    public string ScaffoldingScriptsPath = "Assets/Scripts/Views/";
	    public string ScaffoldingInstantiatePath = "Views/";
		public string ScaffoldingModelInstantiatePath = "Model/";
	    public string ScaffoldingPath = "Assets/";
	    public bool ScaffoldingEnableAllGameobjects = true;
		public string StartingView;
		public ViewType StartingViewType;

		private static string _scaffoldingPath;
		public static ScaffoldingConfig _instance;
		public static ScaffoldingConfig Instance
		{
			get
			{
				if(_instance == null)
				{
					_instance = CreateInstance();
				}
				_instance.UpdateScaffoldingPath();
				return _instance;
			}
		}

		private void UpdateScaffoldingPath()
		{
			RecursivelyFindFolderPath("Assets");
			_instance.ScaffoldingPath = _scaffoldingPath;
		}

		private static void RecursivelyFindFolderPath(string dir)
		{
			string[] paths = Directory.GetDirectories(dir);
			foreach(string s in paths)
			{
				if(s.Contains("Scaffolding"))
				{
					_scaffoldingPath = s;
					break;
				}
				else
				{
					RecursivelyFindFolderPath(s);
				}
			}
		}

		public static ScaffoldingConfig CreateInstance()
		{
			string path = "";
			ScaffoldingConfig sc = null;

#if UNITY_EDITOR
			RecursivelyFindFolderPath("Assets");
			path = _scaffoldingPath+"/Resources/SCConfig.asset";

			if(AssetDatabase.LoadAssetAtPath(path,typeof(ScaffoldingConfig)) == null)
			{
				sc = ScriptableObject.CreateInstance<ScaffoldingConfig> ();
				
				string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path);
				
				AssetDatabase.CreateAsset (sc, assetPathAndName);
				
				AssetDatabase.SaveAssets ();
				AssetDatabase.Refresh();
				EditorUtility.FocusProjectWindow ();
			}
			
			sc = AssetDatabase.LoadAssetAtPath(path,typeof(ScaffoldingConfig)) as ScaffoldingConfig;
			sc.ScaffoldingPath = _scaffoldingPath;
#else
			path = "SCConfig.asset";
			sc = Resources.Load<ScaffoldingConfig>(path);
#endif

			return sc;
		}

		public GameObject DetermineParentGameObjectPath()
		{
			GameObject g = null;
			string[] p = ScaffoldingInstantiatePath.Split(new char[1]{ '/' }, 10);
			int count = 0;
			
			foreach (string n in p)
			{
				if (n.Equals(""))
					continue;
				
				GameObject path = null;
				if (count == 0)
					path = GameObject.Find(n);
				else
				{
					Transform t = g.transform.FindChild(n);
					if (t != null)
						path = t.gameObject;
				}
				
				if (path == null)
				{
					path = new GameObject();
					path.name = n;
				}
				if (g != null)
					path.transform.parent = g.transform;
				g = path;
				
				count++;
			}
			return g;
		}

		public GameObject DetermineParentModelGameObjectPath()
		{
			GameObject g = null;
			string[] p = ScaffoldingModelInstantiatePath.Split(new char[1]{ '/' }, 10);
			int count = 0;
			
			foreach (string n in p)
			{
				if (n.Equals(""))
					continue;
				
				GameObject path = null;
				if (count == 0)
					path = GameObject.Find(n);
				else
				{
					Transform t = g.transform.FindChild(n);
					if (t != null)
						path = t.gameObject;
				}
				
				if (path == null)
				{
					path = new GameObject();
					path.name = n;
				}
				if (g != null)
					path.transform.parent = g.transform;
				g = path;
				
				count++;
			}
			return g;
		}

		public string ViewPrefabPath()
		{
			string path = ScaffoldingResourcesPath;
			int index = path.IndexOf("Resources/");
			if(index > -1)
			{
				path = path.Remove(0, index + 10);
			}
			return path+"/";
		}

		public string FullViewPrefabPath()
		{
			string path = ScaffoldingResourcesPath;
			if (path[path.Length - 1] != '/')
				path += "/";

			return path;
		}

		public string ScriptsPath()
		{
			string path = ScaffoldingScriptsPath;
			if (path[path.Length - 1] != '/')
				path += "/";
			
			return path;
		}
	}
}
