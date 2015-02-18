using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using UnityEditorInternal;
using Scaffolding;
using System.Collections.Generic;

namespace Scaffolding.Editor
{
    public class ScaffoldingConfigEditor : EditorWindow
    {
		private ScaffoldingConfig _scaffoldingConfig;

		private List<string> _scriptsPath;
        private List<string> _prefabsPath;
        private string _instantiatePath;
		private string _modelPath;
		private bool _enableAllGameObjects;
        private Vector2 _scrollPos;
        private string _text;

		[MenuItem("Tools/Scaffolding/Preferences")]
        static void OpenPreferences()
        {
            GetWindow<ScaffoldingConfigEditor>(true, "Scaffolding - Preferences");
        }

		private void CreateConfig()
		{
			if(_scaffoldingConfig == null)
			{
				_scaffoldingConfig = ScaffoldingConfig.Instance;
			}
			_scriptsPath = _scaffoldingConfig.ScaffoldingScriptsPath;
			if(_scriptsPath.Count == 0)
			{
				_scriptsPath.Add("Assets/Scripts/Views/");
			}
			_prefabsPath = _scaffoldingConfig.ScaffoldingResourcesPath;
			if(_prefabsPath.Count == 0)
			{
				_prefabsPath.Add("Assets/Resources/Views");
			}
			_instantiatePath = _scaffoldingConfig.ScaffoldingInstantiatePath;
			_modelPath = _scaffoldingConfig.ScaffoldingModelInstantiatePath;
			_enableAllGameObjects = _scaffoldingConfig.ScaffoldingEnableAllGameobjects;
		}

		private bool _prefabPathToggle;
		private bool _scriptPathToggle;

        void OnGUI()
        {
			CreateConfig();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.BeginHorizontal();
			_scriptPathToggle = EditorGUILayout.Foldout(_scriptPathToggle,"Used Scripts Paths:");
			EditorGUILayout.EndHorizontal();
			if(_scriptPathToggle)
			{
				for(int i = 0;i<_scriptsPath.Count;++i)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(_scriptsPath[i]);
					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Set path for generated scripts"))
			{
				string path = EditorUtility.OpenFolderPanel(
					"Pick Scaffolding Scripts path",
					"Assets",
					"");

				if(path != "")
				{
					int index = _scriptsPath.IndexOf(path);
					if(index < 0)
					{
						_scriptsPath.Add(path);
						index = _scriptsPath.Count-1;
					}
					_scriptsPath[index] = path;
					if(_scriptsPath[index].IndexOf("Assets") > -1)
					{ 
						_scriptsPath[index] = _scriptsPath[index].Remove(0,_scriptsPath[index].IndexOf("Assets"));
					}
				}
			}

            EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

//			EditorGUILayout.EndHorizontal();
			EditorGUILayout.LabelField("Current Resources Path: " + _prefabsPath );
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Set path for generated view prefabs"))
			{
				string path = EditorUtility.OpenFolderPanel(
					"Pick Scaffolding Prefabs path",
					"Assets",
					"");

				if(path != "")
				{
					_prefabsPath = path;
					if(_prefabsPath.IndexOf("Assets") > -1)
					{ 
						_prefabsPath = _prefabsPath.Remove(0,_prefabsPath.IndexOf("Assets"));
					}
				}
			}

            EditorGUILayout.EndHorizontal();

			//this is so lazy...
            if (!_prefabsPath[_prefabsPath.Count-1].Contains("Resources"))
            {
                EditorGUILayout.HelpBox("PREFABS PATH MUST BE IN A RESOURCES FOLDER!", MessageType.Error);
            }

			EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Set view GameObject parent in the scene");
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
            _instantiatePath = EditorGUILayout.TextField(_instantiatePath);
            if (_instantiatePath[_instantiatePath.Length - 1] != '/')
            {
                _instantiatePath += "/";
            }
            EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Set model GameObject parent in the scene");
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			_modelPath = EditorGUILayout.TextField(_modelPath);
			if (_modelPath[_modelPath.Length - 1] != '/')
			{
				_modelPath += "/";
			}
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Enable all children in loaded views");
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			_enableAllGameObjects = EditorGUILayout.Toggle(_enableAllGameObjects);
			EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();

			SetDataBack();
            AssetDatabase.SaveAssets();
        }

		private void SetDataBack()
		{
			_scaffoldingConfig.ScaffoldingScriptsPath = _scriptsPath;
			_scaffoldingConfig.ScaffoldingResourcesPath = _prefabsPath;
			_scaffoldingConfig.ScaffoldingInstantiatePath = _instantiatePath;
			_scaffoldingConfig.ScaffoldingModelInstantiatePath = _modelPath;
			_scaffoldingConfig.ScaffoldingEnableAllGameobjects = _enableAllGameObjects;

			EditorUtility.SetDirty(_scaffoldingConfig);
		}

        void OnDestroy()
        {
            AssetDatabase.SaveAssets();
        }
    }
}
