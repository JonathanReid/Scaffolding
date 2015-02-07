using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using Scaffolding;
using System.IO;
using System.Text;


namespace Scaffolding.Editor
{
    public class ViewBrowserWindow : EditorWindow
    {
        private static ViewBrowserWindow _window;
        private List<string> _viewNames;
        private List<AbstractView> _abstractViews;
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
		private bool _viewsFoldout = true;

        [MenuItem("Tools/Scaffolding/Open View Library")]
        static void OpenViewLibrary()
        {
            _window = (ViewBrowserWindow)EditorWindow.GetWindow(typeof(ViewBrowserWindow));
            _window.title = " View Library";
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
			}
			
			if(Application.isPlaying && !_applicationPlaying)
			{
				Repaint();
			}
		}

        void OnGUI()
        {
			_applicationPlaying = Application.isPlaying;
			CreateConfig();
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
			GUILayout.BeginHorizontal(GUI.skin.FindStyle("Box"));

			if(_scaffoldingConfig.StartingView == null)
			{
				_scaffoldingConfig.StartingView = new List<ScaffoldingStartingView>();
				_scaffoldingConfig.SetViewDataForScene(_scaffoldingConfig.GetDefaultStartingView());
			}

			string name = EditorApplication.currentScene;
			name = name.Remove(0,name.LastIndexOf("/")+1);
			int index = name.LastIndexOf(".unity");
			name = name.Remove(index,name.Length - index);

			ScaffoldingStartingView sv = _scaffoldingConfig.GetViewDataForScene(name);
			sv.StartingViewIndex = EditorGUILayout.Popup("Starting View:",sv.StartingViewIndex, _viewNames.ToArray());
			if (sv.StartingViewName != null)
			{
				sv.StartingViewIndex = ScaffoldingUtilitiesEditor.CheckIfMenuItemChanged(_viewLength, sv.StartingViewIndex, _viewNames, sv.StartingViewName);
			}
			_viewLength = _viewNames.Count;
			sv.StartingViewName = _viewNames[sv.StartingViewIndex];

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal(GUI.skin.FindStyle("Box"));

			sv.StartingViewType = (ViewType)EditorGUILayout.EnumPopup("Open as:",sv.StartingViewType);

			GUILayout.EndHorizontal();

			_scaffoldingConfig.SetViewDataForScene(sv);
			EditorUtility.SetDirty(_scaffoldingConfig);

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
								EditorApplication.SaveCurrentSceneIfUserWantsTo();
								EditorApplication.OpenScene(S.path);
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
				GameObject obj = PrefabUtility.InstantiatePrefab(Resources.Load(_scaffoldingConfig.ViewPrefabPath(viewName)+viewName)) as GameObject;
#if UNITY_4_6
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
					string oldNamePrefab = _scaffoldingConfig.FullViewPrefabPath(viewName)+viewName;
					string oldNameScript = _scaffoldingConfig.ScriptsPath() + viewName;
					string newNamePrefab = _scaffoldingConfig.FullViewPrefabPath(_tempViewName)+_tempViewName;
					string newNameScript = _scaffoldingConfig.ScriptsPath() + _tempViewName;

					AssetDatabase.MoveAsset(oldNamePrefab + ".prefab",newNamePrefab + ".prefab");
					RenameFile(oldNameScript + ".cs",viewName,_tempViewName);
					AssetDatabase.MoveAsset(oldNameScript + ".cs",newNameScript + ".cs");


					AssetDatabase.MoveAsset(oldNamePrefab + "Model.prefab",newNamePrefab + "Model.prefab");
					RenameFile(oldNameScript + "Model.cs",viewName,_tempViewName);
					AssetDatabase.MoveAsset(oldNameScript + "Model.cs",newNameScript + "Model.cs");
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

					AssetDatabase.DeleteAsset(_scaffoldingConfig.FullViewPrefabPath(viewName)+viewName + ".prefab");
					AssetDatabase.DeleteAsset(_scaffoldingConfig.ScriptsPath() + viewName + ".cs");
					AssetDatabase.DeleteAsset(_scaffoldingConfig.FullViewPrefabPath(viewName)+viewName + "Model.prefab");
					AssetDatabase.DeleteAsset(_scaffoldingConfig.ScriptsPath() + viewName + "Model.cs");
					AssetDatabase.Refresh();
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
            PrefabUtility.ReplacePrefab(g, PrefabUtility.GetPrefabParent(g), ReplacePrefabOptions.ConnectToPrefab);
        }

        private void Close(GameObject g)
        {
            UnityEditor.Editor.DestroyImmediate(g);
        }

        private void SaveAndClose(GameObject g)
        {
            PrefabUtility.ReplacePrefab(g, PrefabUtility.GetPrefabParent(g), ReplacePrefabOptions.ConnectToPrefab);
            UnityEditor.Editor.DestroyImmediate(g);
        }

        private void CreateAllViews()
        {
            UnityEngine.Object[] views = Resources.LoadAll("");//_scaffoldingConfig.ViewPrefabPath());
            _viewNames = new List<string>();
            _abstractViews = new List<AbstractView>();
            foreach (UnityEngine.Object o in views)
            {
                if (o is GameObject)
                {
                    AbstractView v = (o as GameObject).GetComponent<AbstractView>();
                    if (v != null)
                    {
                        _viewNames.Add(v.GetType().Name);
                        _abstractViews.Add(v);
                    }
                }
            }
            views = null;
        }
    }
}
