using UnityEngine;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine.Audio;
using System.Linq;
using System;
using System.Collections.Generic;

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

	private static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
	{
		return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
	}

	public static void GetMethodsFromAudioClass()
	{
		Assembly mscorlib = typeof(AudioImporter).Assembly;
		string dump = "";
		foreach (Type type in mscorlib.GetTypes())
		{
			if(type.Name.Contains("Audio"))
			{
				dump += type.FullName + "\n";
			}
		}

//		Debug.Log(dump);

		Type audioUtilClass = mscorlib.GetType("UnityEditor.AudioMixerItem");
		foreach (MethodInfo info in audioUtilClass.GetMethods())//(BindingFlags.NonPublic | BindingFlags.Instance))
		{
			{
				Debug.Log(info.Name);
				ParameterInfo[] parms = info.GetParameters();
				foreach(ParameterInfo pinfo in parms)
				{
					Debug.Log("--- info " + pinfo.Name + " " + pinfo.ParameterType);
				}
			}
		}

//		Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
//		Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioMixerWindow");
//		MethodInfo method = audioUtilClass.GetMethod("Create");
//		UnityEditor.TreeViewItem item = new TreeViewItem();
//		method.Invoke(null,new object[]{item});

//		Assembly unityEditorAssembly = typeof(AudioMixer).Assembly;
//		Type audioUtilClass = unityEditorAssembly.GetType("UnityEngine.Audio");
//		MethodInfo[] methods = audioUtilClass.GetMethods();
//		foreach(MethodInfo info in methods)
//		{
//			Debug.Log(info.Name);
//			ParameterInfo[] parms = info.GetParameters();
//			foreach(ParameterInfo pinfo in parms)
//			{
//				Debug.Log("--- info " + pinfo.Name);
//			}
//		}
	}

}
