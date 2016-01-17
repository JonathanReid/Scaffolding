﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using Scaffolding;
using System.IO;
using System.Text;
#if UNITY_5_3
using UnityEditor.SceneManagement;
#endif


namespace Scaffolding.Editor
{
    public class ViewLibraryWindow : EditorWindow
    {
        private static ViewLibraryWindow _window;

        private List<string> _viewNames;
		private List<string> _fullViewNames;
        private List<AbstractView> _abstractViews;

		private List<string> _transitionNames;
		private List<string> _fulltransitionNames;
		private List<AbstractTransition> _abstractTransitions;

        private Vector2 _scrollPos;
		private Texture2D _backgroundTexture;
		private string searchString = "";
		private int _backgroundImageHeight = 0;
		private string _renamingView = "";
		private string _tempViewName = "";
		private int _startingViewIndex;
		private int _viewLength;
		private ViewType _viewType;
		private ScaffoldingConfig _scaffoldingConfig;
		private bool _applicationPlaying;
		private bool _scenesFoldout = true;
		private bool _transitionsFoldout = true;
		private bool _viewsFoldout = true;

        [MenuItem("Tools/Scaffolding/Views/Open View Library")]
        static void OpenViewLibrary()
        {
            _window = (ViewLibraryWindow)EditorWindow.GetWindow(typeof(ViewLibraryWindow));
			_window.titleContent = new GUIContent("View Library");
		}

		private void CreateBackgroundTexture()
		{
			_backgroundTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
			float val = 0;
			if(EditorGUIUtility.isProSkin)
			{
				val = 45f/255f;
			}
			else
			{
				val = 162f/255f;
			}
			_backgroundTexture.SetPixel(0, 0, new Color(val,val,val));
			_backgroundTexture.Apply();
		}

		private void OnEnable()
		{
			CreateConfig();
		}

		private void CancelSearch()
		{
			searchString = "";
			GUI.FocusControl(null);
		}

		private void SaveAndCloseAll()
		{
			int i = 0, l = _viewNames.Count;
			for (; i < l; ++i)
			{
				System.Type t = _abstractViews[i].GetType();
				AbstractView sceneObject = GameObject.FindObjectOfType(t) as AbstractView;
				if (sceneObject != null)
				{
					//when you close a view, save the prefab!
					SaveAndClose(sceneObject.gameObject);
				}
			}
		}

		private void CreateNewView()
		{
			EditorApplication.ExecuteMenuItem("GameObject/Create Other/Scaffolding/New View");
		}

		private void CreateConfig()
		{
			_scaffoldingConfig = ScaffoldingConfig.Instance;
		}

		void Update()
		{
			// This is necessary to make the framerate normal for the editor window.
			
			if(!Application.isPlaying && _applicationPlaying)
			{
				Repaint();
				CreateConfig();
				_scaffoldingConfig.UpdateScaffoldingPath();
			}
			
			if(Application.isPlaying && !_applicationPlaying)
			{
				Repaint();
				CreateConfig();
				_scaffoldingConfig.UpdateScaffoldingPath();
			}
		}

        void OnGUI()
        {
			_applicationPlaying = Application.isPlaying;
            CreateAllViews();

            //toolbar GUI
            //create search bar
            GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
            GUIStyle toolbar = GUI.skin.FindStyle("ToolbarSeachTextField");
            toolbar.fixedWidth = 80;
            toolbar.stretchWidth = true;
            searchString = GUILayout.TextField(searchString, toolbar);
            
            GUIStyle cancel = GUI.skin.FindStyle("ToolbarSeachCancelButtonEmpty");
            if (searchString.Length > 0)
                cancel = GUI.skin.FindStyle("ToolbarSeachCancelButton");
                
            if (GUILayout.Button("", cancel))
            {
                CancelSearch();
            }

            toolbar.fixedWidth = 0;
            toolbar.stretchWidth = true;

            //close all open views button
            if (GUILayout.Button("Save & Close All", EditorStyles.toolbarButton))
            {
               SaveAndCloseAll();
            }

            if (GUILayout.Button("Create New", EditorStyles.toolbarButton))
            {
                CreateNewView();
            }

			if (GUILayout.Button("Settings", EditorStyles.toolbarButton))
			{
				EditorApplication.ExecuteMenuItem("Tools/Scaffolding/Preferences");
			}

			GUILayout.EndHorizontal();
			GUILayout.BeginVertical(GUI.skin.FindStyle("Box"));

			if(_scaffoldingConfig.StartingView == null)
			{
				_scaffoldingConfig.StartingView = new List<ScaffoldingStartingView>();
				_scaffoldingConfig.SetViewDataForScene(_scaffoldingConfig.GetDefaultStartingView());
			}

			string name = "";
			#if UNITY_5_3
			name = EditorSceneManager.GetActiveScene().name;
			#else
			name = EditorApplication.currentScene;
			#endif
			if(name != "")
			{
				#if !UNITY_5_3
				name = name.Remove(0,name.LastIndexOf("/")+1);
				int index = name.LastIndexOf(".unity");
				name = name.Remove(index,name.Length - index);
				#endif

				ScaffoldingStartingView sv = _scaffoldingConfig.GetViewDataForScene(name);
				sv.StartingViewIndex = EditorGUILayout.Popup("Starting View:",sv.StartingViewIndex, _viewNames.ToArray());
				if (sv.StartingViewName != null)
				{
					sv.StartingViewIndex = ScaffoldingUtilitiesEditor.CheckIfMenuItemChanged(_viewLength, sv.StartingViewIndex, _fullViewNames, sv.StartingViewName);
				}
				_viewLength = _viewNames.Count;
				if(sv.StartingViewIndex < _fullViewNames.Count)
				{
					sv.StartingViewName = _fullViewNames[sv.StartingViewIndex];
				}

				sv.StartingViewType = (ViewType)EditorGUILayout.EnumPopup("Open as:",sv.StartingViewType);

				GUILayout.EndHorizontal();

				_scaffoldingConfig.SetViewDataForScene(sv);
				EditorUtility.SetDirty(_scaffoldingConfig);
			}
			else
			{
				GUILayout.Label("You need to save the scene first!");
				GUILayout.EndHorizontal();
			}

            //Setting up the library scroll area
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
           

            GUILayout.BeginVertical();

			if(!EditorApplication.isPlaying)
			{
				if (!EditorApplication.isCompiling)
				{
						//display all buttons for views    
					_backgroundImageHeight = 0;
					int i = 0, l = _viewNames.Count;
					int j = 0;

					_scenesFoldout = EditorGUILayout.Foldout(_scenesFoldout,"Buildable Scenes");
					if(_scenesFoldout)
					{
						GUILayout.Space(5);
						_backgroundImageHeight += 23;
					
						foreach (UnityEditor.EditorBuildSettingsScene S in UnityEditor.EditorBuildSettings.scenes)
						{
							DrawBackgroundImage(j);
							EditorGUILayout.BeginHorizontal();
							string n = S.path.Substring(S.path.LastIndexOf('/')+1);
							n = n.Substring(0,n.Length-6);
							GUIStyle skin = EditorStyles.boldLabel;
							skin.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
							
							GUILayout.Label(n,skin);
							if (GUILayout.Button("Open", GUILayout.ExpandWidth(true), GUILayout.Width(218)))
							{
								#if UNITY_5_3
								EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
								EditorSceneManager.OpenScene(S.path);
								#else
								EditorApplication.SaveCurrentSceneIfUserWantsTo();
								EditorApplication.OpenScene(S.path);
								#endif
							}
							EditorGUILayout.EndHorizontal();
							GUILayout.Space(5);
							j++;
						}
					}
					else
					{
						GUILayout.Space(5);
						_backgroundImageHeight += 23;
					}

					if(_transitionNames.Count > 0)
					{
						_transitionsFoldout = EditorGUILayout.Foldout(_transitionsFoldout,"Transitions");
						if(_transitionsFoldout)
						{
							GUILayout.Space(5);
							_backgroundImageHeight += 23;
							
							i = 0;
							l = _transitionNames.Count;
							
							for (; i < l;++i)
							{
								DrawBackgroundImage(j);
								//handle individual buttons 
								EditorGUILayout.BeginHorizontal();
								System.Type t = _abstractTransitions[i].GetType();
								AbstractTransition sceneObject = GameObject.FindObjectOfType(t) as AbstractTransition;
								if (sceneObject != null)
								{
									OpenViewButtons(_transitionNames[i], sceneObject.gameObject);
								}
								else
								{
									ClosedViewButtons(_transitionNames[i]);
								}
								EditorGUILayout.EndHorizontal();
								
								GUILayout.Space(5);
								j++;
							}
						}
						else
						{
							GUILayout.Space(5);
							_backgroundImageHeight += 23;
						}
					}

					_viewsFoldout = EditorGUILayout.Foldout(_viewsFoldout,"Views");

					if(_viewsFoldout)
					{
						GUILayout.Space(5);
						_backgroundImageHeight += 23;

						i = 0;
						l = _viewNames.Count;

						for (; i < l;++i)
			            {
			                //handle searching
			                if (searchString.Length == 0 || (searchString.Length > 0 && _viewNames[i].IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0))
			                {
								DrawBackgroundImage(j);
			                    //handle individual buttons 
			                    EditorGUILayout.BeginHorizontal();
			                    System.Type t = _abstractViews[i].GetType();
			                    AbstractView sceneObject = GameObject.FindObjectOfType(t) as AbstractView;
			                    if (sceneObject != null)
			                    {
			                        OpenViewButtons(_viewNames[i], sceneObject.gameObject);
			                    }
			                    else
			                    {
			                        ClosedViewButtons(_viewNames[i]);
			                    }
			                    EditorGUILayout.EndHorizontal();

								GUILayout.Space(5);
								j++;
			                }
			            }
					}
				}
				else
				{
					EditorGUILayout.HelpBox("Updating view library...",MessageType.Info);
				}
			}
			else
			{
				EditorGUILayout.HelpBox("Best not fiddle with views while Unity is running.",MessageType.Info);
			}
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();

			EditorUtility.SetDirty(_scaffoldingConfig);
        }

		private bool _wasUsingProSkin;
		private void DrawBackgroundImage(int i)
		{
			if(_backgroundTexture == null || EditorGUIUtility.isProSkin != _wasUsingProSkin)
			{
				CreateBackgroundTexture();
				_wasUsingProSkin = EditorGUIUtility.isProSkin;
			}

			if(i%2 == 0)
			{
				GUI.DrawTexture(new Rect(0, _backgroundImageHeight, maxSize.x, 26), _backgroundTexture, ScaleMode.StretchToFill);
			}
			_backgroundImageHeight+= 26;
		}
		
		private void OpenViewButtons(string viewName, GameObject go)
        {
			GUIStyle skin = EditorStyles.boldLabel;
			skin.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

			GUILayout.Label(viewName,skin);
            if (GUILayout.Button("Close", GUILayout.ExpandWidth(true), GUILayout.Width(40)))
            {
                Close(go);
            }
            if (GUILayout.Button("Save", GUILayout.ExpandWidth(true), GUILayout.Width(40)))
            {
                Save(go);
            }
            if (GUILayout.Button("Save & Close", GUILayout.ExpandWidth(true), GUILayout.Width(80)))
            {
                SaveAndClose(go);
            }
        }

		private bool IsViewBeingEdited(string viewName)
		{
			if(_renamingView == viewName)
			{
				return true;
			}
			return false;
		}

        private void ClosedViewButtons(string viewName)
        {

			if(_renamingView != "" && _renamingView != viewName)
			{
				GUI.enabled = false;
			}
			else
			{
				GUI.enabled = true;
			}

			if(_renamingView == viewName)
			{
				_tempViewName = GUILayout.TextArea(_tempViewName);
			}
			else
			{
				GUIStyle skin = EditorStyles.boldLabel;
				skin.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
				
				GUILayout.Label(viewName,skin);
			}

			if(IsViewBeingEdited(viewName))
			{
				GUI.enabled = false;
			}
			if (GUILayout.Button("Open", GUILayout.Width(100)))
            {
				GameObject obj = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>(_scaffoldingConfig.ViewPrefabPath(viewName)+viewName)) as GameObject;
				#if UNITY_4_6 || UNITY_5
				obj.transform.SetParent(_scaffoldingConfig.DetermineParentGameObjectPath().transform);
#else
				obj.transform.parent = _scaffoldingConfig.DetermineParentGameObjectPath().transform;
#endif
                Selection.activeObject = obj;
            }
			GUI.enabled = true;
			if(IsViewBeingEdited(viewName))
			{
				if (GUILayout.Button("Done", GUILayout.Width(60)))
				{
					_renamingView = "";
					string oldNamePrefab = "";
					string oldNameScript = "";
					var guids = AssetDatabase.FindAssets(viewName);
					foreach(var guid in guids)
					{
						string s = AssetDatabase.GUIDToAssetPath(guid);
						if(s.Contains(".cs"))
						{
							oldNameScript = s;
						}
						else if(s.Contains(".prefab"))
						{
							oldNamePrefab = s;
						}
					}

					string newNamePrefab = oldNamePrefab.Replace(viewName,_tempViewName);
					string newNameScript = oldNameScript.Replace(viewName,_tempViewName);

					Debug.Log(newNamePrefab);

					AssetDatabase.MoveAsset(oldNamePrefab,newNamePrefab);
					RenameFile(oldNameScript,viewName,_tempViewName);
					AssetDatabase.MoveAsset(oldNameScript,newNameScript );


					AssetDatabase.MoveAsset(oldNamePrefab + "Model",newNamePrefab + "Model");
					RenameFile(oldNameScript + "Model",viewName,_tempViewName);
					AssetDatabase.MoveAsset(oldNameScript + "Model",newNameScript + "Model");
					AssetDatabase.Refresh();
					CreateAllViews();
				}
			}
			else
			{
				if(_renamingView != "" && _renamingView != viewName)
				{
					GUI.enabled = false;
				}
				if (GUILayout.Button("Rename", GUILayout.Width(60)))
				{
					_renamingView = viewName;
					_tempViewName = viewName;
				}
			}

			if(_renamingView != "" && _renamingView != viewName || IsViewBeingEdited(viewName))
			{
				GUI.enabled = false;
			}
			if (GUILayout.Button("Delete", GUILayout.Width(50)))
            {
                if (EditorUtility.DisplayDialog("Delete " + viewName + " from your project?", "Deleting this view will permenantly remove it from your project, are you sure?", "Yes", "No"))
                {
					string name = viewName + "Model";
					GameObject go = GameObject.Find(name);
					DestroyImmediate(go);

					DeleteFile(viewName,".prefab");
					DeleteFile(viewName,".cs");
					DeleteFile(viewName,"Model.prefab");
					DeleteFile(viewName,"Model.cs");

					AssetDatabase.Refresh();
                }
            }
        }

		private void DeleteFile(string name,string extension)
		{
			var guids = AssetDatabase.FindAssets(name);
			foreach(var guid in guids)
			{
				string s = AssetDatabase.GUIDToAssetPath(guid);
				if(s.Contains(extension))
				{
					AssetDatabase.DeleteAsset(s);
				}
			}
		}

		private void RenameFile(string path, string oldname, string newname)
		{
			if(File.Exists(path))
			{
				var reader = new StreamReader(path,Encoding.UTF8);
				string file = reader.ReadToEnd();
				file = file.Replace(oldname,newname);
				reader.Close();
				reader.Dispose();
				var writer = new StreamWriter(path);
				writer.Write(file);
				writer.Close();
				writer.Dispose();
			}
		}

        private void Save(GameObject g)
        {
			string viewName = g.name;
			RecursivelyFindAndReplacePrefabs(g.transform);
			PrefabUtility.ReplacePrefab(g, PrefabUtility.GetPrefabParent(g), ReplacePrefabOptions.ReplaceNameBased);
			UnityEditor.Editor.DestroyImmediate(g);

			GameObject obj = PrefabUtility.InstantiatePrefab(Resources.Load(_scaffoldingConfig.ViewPrefabPath(viewName)+viewName)) as GameObject;
			#if UNITY_4_6 || UNITY_5
			obj.transform.SetParent(_scaffoldingConfig.DetermineParentGameObjectPath().transform);
			#else
			obj.transform.parent = _scaffoldingConfig.DetermineParentGameObjectPath().transform;
			#endif
			Selection.activeObject = obj;
		}
		
		private void Close(GameObject g)
        {
            UnityEditor.Editor.DestroyImmediate(g);
        }

		private void SaveAndClose(GameObject g)
		{
			RecursivelyFindAndReplacePrefabs(g.transform);
			
			string path = AssetDatabase.GetAssetPath (PrefabUtility.GetPrefabParent(g));
			PrefabUtility.CreatePrefab(path,g,ReplacePrefabOptions.ReplaceNameBased);
			
			//			PrefabUtility.ReplacePrefab(obj, g, ReplacePrefabOptions.ReplaceNameBased);
			UnityEditor.Editor.DestroyImmediate(g);
		}
		
		private void RecursivelyFindAndReplacePrefabs(Transform t)
		{
			AbstractSkinnableView bv = t.GetComponentInChildren<AbstractSkinnableView>(); 
			if(bv != null)
			{
				GameObject g = bv.gameObject;
				string path = AssetDatabase.GetAssetPath (PrefabUtility.GetPrefabParent(g));
				PrefabUtility.CreatePrefab(path,g,ReplacePrefabOptions.ReplaceNameBased);
//				PrefabUtility.ReplacePrefab(g, PrefabUtility.GetPrefabParent(g), ReplacePrefabOptions.ReplaceNameBased);
				UnityEditor.Editor.DestroyImmediate(g);
			}
		}

        private void CreateAllViews()
        {
			_viewNames = new List<string>();
			_fullViewNames = new List<string>();
			_abstractViews = new List<AbstractView>();
			_transitionNames = new List<string>();
			_fulltransitionNames = new List<string>();
			_abstractTransitions = new List<AbstractTransition>();

			foreach(string prefabPath in _scaffoldingConfig.ScaffoldingResourcesPath)
			{
				UnityEngine.Object[] views = Resources.LoadAll(ScaffoldingExtensions.ConvertPathToResourcePath(prefabPath));

				foreach (UnityEngine.Object o in views)
				{
					if (o is GameObject)
					{
						AbstractView v = (o as GameObject).GetComponent<AbstractView>();
						if (v != null)
						{
							if(v is AbstractTransition)
							{
								if(!_transitionNames.Contains(v.GetType().Name))
							   	{
									_fulltransitionNames.Add(v.GetType().FullName);
									_transitionNames.Add(v.GetType().Name);
									_abstractTransitions.Add(v as AbstractTransition);
								}
							}
							else
							{
								if(!_viewNames.Contains(v.GetType().Name))
								{
									_fullViewNames.Add(v.GetType().FullName);
									_viewNames.Add(v.GetType().Name);
									_abstractViews.Add(v);
								}
							}
						}
					}
				}
				views = null;
			}


        }
    }
}