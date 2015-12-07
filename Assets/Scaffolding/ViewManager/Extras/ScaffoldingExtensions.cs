using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Scaffolding
{
public static class ScaffoldingExtensions
{
    public static object GetReference(this GameObject gameObejct, Type type)
    {
        object o = GameObject.FindObjectOfType(type);
        
        if (o == null)
        {
            GameObject go = new GameObject();
            go.name = type.Name;
#if UNITY_5_0
            o = go.AddComponent(type);
#else
			o = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(go, "Assets/Scaffolding/Extras/ScaffoldingExtensions.cs (20,8)", type.Name);
#endif
        }
        
        return o;
    }

	public static void EnableAllChildren(this GameObject gameObject)
	{
			RecursiveEnable(gameObject.transform);
	}

		private static void RecursiveEnable(Transform t)
		{
			t.gameObject.SetActive(true);
			foreach(Transform tr in t)
			{
				RecursiveEnable(tr);
			}
		}

		public static List<string> GetAllViews()
		{
			List<string> viewList = new List<string>();
			
			foreach(string prefabPath in ScaffoldingConfig.Instance.ScaffoldingResourcesPath)
			{
				UnityEngine.Object[] views = Resources.LoadAll(ConvertPathToResourcePath(prefabPath));
				
				foreach (UnityEngine.Object o in views)
				{
					if (o is GameObject)
					{
						AbstractView v = (o as GameObject).GetComponent<AbstractView>();
						if (v != null)
						{
							if(v is AbstractTransition)
							{
								
							}
							else if(v is AbstractModalPopup)
							{

							}
							else
							{
								if(!viewList.Contains(v.GetType().FullName))
								{
									viewList.Add(v.GetType().FullName);
								}
							}
						}
					}
				}
				views = null;
			}
			return viewList;
		}

		public static List<string> GetAllTransitions()
		{
			List<string> transitionList = new List<string>();
			
			foreach(string prefabPath in ScaffoldingConfig.Instance.ScaffoldingResourcesPath)
			{
				UnityEngine.Object[] views = Resources.LoadAll(ConvertPathToResourcePath(prefabPath));
				
				foreach (UnityEngine.Object o in views)
				{
					if (o is GameObject)
					{
						AbstractView v = (o as GameObject).GetComponent<AbstractView>();
						if (v != null)
						{
							if(v is AbstractTransition)
							{
								if(!transitionList.Contains(v.GetType().FullName))
								{
									transitionList.Add(v.GetType().FullName);
								}
							}
						}
					}
				}
				views = null;
			}
			return transitionList;
		}

		public static List<string> GetAllPopups()
		{
			List<string> popupList = new List<string>();
			
			foreach(string prefabPath in ScaffoldingConfig.Instance.ScaffoldingResourcesPath)
			{
				UnityEngine.Object[] views = Resources.LoadAll(ConvertPathToResourcePath(prefabPath));
				
				foreach (UnityEngine.Object o in views)
				{
					if (o is GameObject)
					{
						AbstractView v = (o as GameObject).GetComponent<AbstractView>();
						if (v != null)
						{
							if(v is AbstractModalPopup)
							{
								if(!popupList.Contains(v.GetType().FullName))
								{
									popupList.Add(v.GetType().FullName);
								}
							}
						}
					}
				}
				views = null;
			}
			return popupList;
		}


		public static Type GetType(string className)
		{
			string typeName = className + ", Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
			//need to include the assembly as get type doesnt work in html5
			Type t = System.Type.GetType(typeName);
			if (t != null) return t;
			foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
			{
				t = a.GetType(typeName);
				if (t != null)
					return t;
			}
			return t;
		}
		
		public static List<string> GetButtonsInView(string viewName)
		{
			List<string> butts = new List<string>();
			foreach(string prefabPath in ScaffoldingConfig.Instance.ScaffoldingResourcesPath)
			{
				UnityEngine.Object view = Resources.Load(ConvertPathToResourcePath(prefabPath+viewName));
				if(view is GameObject)
				{
					GameObject go = GameObject.Instantiate(view) as GameObject;
					
					AbstractButton[] buttons = go.GetComponentsInChildren<AbstractButton>();
					foreach(AbstractButton button in buttons)
					{
						butts.Add(button.name);
					}
					
					MonoBehaviour.DestroyImmediate(go);
				}
			}
			
			return butts;
		}
		
		public static string ConvertPathToResourcePath(string path)
		{
			return path.Remove(0,path.IndexOf("Resources/")+10);
		}

    public static bool IsRetina(this GameObject gameObject)
    {
        #if IOS
		if (iPhone.generation == iPhoneGeneration.iPhone4)
		{
			return true;
		}
		if (iPhone.generation == iPhoneGeneration.iPhone4S)
		{
			return true;
		}
		if (iPhone.generation == iPhoneGeneration.iPhone5)
		{
			return true;
		}
		if (iPhone.generation == iPhoneGeneration.iPhone5C)
		{
			return true;
		}
		if (iPhone.generation == iPhoneGeneration.iPhone5S)
		{
			return true;
		}
		if (iPhone.generation == iPhoneGeneration.iPad3Gen)
		{
			return true;
		}
		if (iPhone.generation == iPhoneGeneration.iPad4Gen)
		{
			return true;
		}
		if (iPhone.generation == iPhoneGeneration.iPadMini1Gen)
		{
			return true;
		}
		if (iPhone.generation == iPhoneGeneration.iPadUnknown)
		{
			return true;
		}
        #endif
        return false;
    }
}
}
