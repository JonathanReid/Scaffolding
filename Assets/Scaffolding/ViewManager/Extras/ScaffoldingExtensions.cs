using UnityEngine;
using System.Collections;
using System;

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
			o = go.AddComponent(type.Name);
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
