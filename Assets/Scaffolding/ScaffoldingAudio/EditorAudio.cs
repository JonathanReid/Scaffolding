using UnityEngine;
using System.Collections;
using System.Reflection;
using UnityEditor;
using System;

public class EditorAudio {

	public static void PlayClip(AudioClip clip) {
		Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
		Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
		MethodInfo method = audioUtilClass.GetMethod(
			"PlayClip",
			BindingFlags.Static | BindingFlags.Public,
			null,
			new System.Type[] {
			typeof(AudioClip)
		},
		null
		);
		method.Invoke(
			null,
			new object[] {
			clip
		}
		);
	}
	
	public static void StopClips()
	{
		Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
		Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
		MethodInfo method = audioUtilClass.GetMethod("StopAllClips");
		method.Invoke(null,new object[0]);
	}
	
	public static void GetMethodsFromAudioClass()
	{
		Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
		Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
		MethodInfo[] methods = audioUtilClass.GetMethods();
		foreach(MethodInfo info in methods)
		{
			Debug.Log(info.Name);
			ParameterInfo[] parms = info.GetParameters();
			foreach(ParameterInfo pinfo in parms)
			{
				Debug.Log("--- info " + pinfo.Name);
			}
		}
	}

}
