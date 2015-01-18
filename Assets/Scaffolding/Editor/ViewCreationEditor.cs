using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using UnityEditor;
using System.IO;
using UnityEditorInternal;
using Scaffolding;
using System.Collections.Generic;


#if UNITY_4_6 || UNITY_5_0
using UnityEngine.UI;
#endif

namespace Scaffolding.Editor
{
    public class ViewCreationEditor : EditorWindow
    {
        private static ViewCreationEditor _window;
        private static bool _wait;
        private static float _time;
        private static string _fileName;
		private static string _modelFileName;
        private static GameObject _createdViewObject;
		private static GameObject _createdModelObject;
        private static bool _firstTimeCreated;
		private static bool _createModel;
		//for allowing users to select which file they want their views to extend from, default is AbstractView.
		private static List<string> _extendableClassNames;
		private static int _selectedView = -1;

		private static List<string> _extendableModelClassNames;
		private static int _selectedModel = -1;

		private static ScaffoldingConfig _scaffoldingConfig;

        [MenuItem("GameObject/Create Other/Scaffolding/New View", false, 12900)]
        static void CreateNewView()
        {
            _firstTimeCreated = false;
            _fileName = "";
			_modelFileName = "";
            _window = GetWindow<ViewCreationEditor>(true, "Scaffolding - Create new view");
            _window.maxSize = new Vector2(500, 200);
            _window.minSize = new Vector2(500, 200);
        }

		#if UNITY_4_6 || UNITY_5_0
		private static Canvas CreateCanvas(GameObject c)
		{
			Canvas canvas = c.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;

			c.AddComponent<CanvasScaler>();
			c.AddComponent<GraphicRaycaster>();

			return canvas;
		}

		[MenuItem("GameObject/UI/Scaffolding/Button", false, 12900)]
		static void CreateNewButton()
		{
			Canvas canvas = GameObject.FindObjectOfType<Canvas>();
			if(canvas == null)
			{
				GameObject c = new GameObject();
				canvas = CreateCanvas(c);
			}

			GameObject go = new GameObject();
			go.name = "Button Item";
			go.transform.SetParent(canvas.transform);
			go.AddComponent<ScaffoldingUGUIButton>();
			go.AddComponent<Image>();
			go.AddComponent<Button>();
			RectTransform tr = go.GetComponent<RectTransform>();
			tr.anchoredPosition = new Vector2(0,0); 

			GameObject label = new GameObject();
			label.name = "Label";
			Text text = label.AddComponent<Text>();
			text.text = "Button";
			text.alignment = TextAnchor.MiddleCenter;
			text.color = Color.black;
			tr = text.GetComponent<RectTransform>();
			label.transform.SetParent(canvas.transform);
			tr.localPosition = new Vector3(0,0,0);
			label.transform.SetParent(go.transform);

			Selection.activeGameObject = go;
		}
#else
			
			[MenuItem("GameObject/Create Other/Scaffolding/Button", false, 12900)]
        static void CreateNewButton()
        {
            GameObject go = Instantiate(Resources.Load("Prefabs/_Button_Sliced_Prefab")) as GameObject;
            go.name = "Button Item";
            ScaffoldingButton b = go.GetComponent<ScaffoldingButton>();
            b.created = false;

            if (Selection.activeGameObject != null)
            {
                go.transform.parent = Selection.activeGameObject.transform;
            }
            Selection.activeObject = go;
        }
#endif

		private static void CreateConfig()
		{
			_scaffoldingConfig = ScaffoldingConfig.Instance;
		}

        void OnGUI()
        {
			CreateConfig();

			GUILayout.BeginHorizontal(GUI.skin.FindStyle("Box"));
            GUILayout.Label("Create new view!", EditorStyles.boldLabel);
			GUILayout.EndHorizontal();
            if (!GameObject.FindObjectOfType(typeof(ViewManager)))
            {
                _firstTimeCreated = true;
                GameObject go = new GameObject();
                go.name = "ViewManager";
                go.AddComponent<ViewManager>();
            }

            if (_firstTimeCreated)
            {
                GUILayout.Label("Hey! It looks like you dont have a ViewManager set up! I've created on for you!", EditorStyles.boldLabel);
            }

            _fileName = EditorGUILayout.TextField("View Name:", _fileName);
			_modelFileName = _fileName + "Model";


			GetAllAbstractViewBasedClasses();

			GUILayout.Label("Choose which class this view should extend (AbstractView is default)");
			_selectedView = EditorGUILayout.Popup(_selectedView,_extendableClassNames.ToArray());

			_createModel = EditorGUILayout.Toggle("Create model for view: ", _createModel);
			GetAllAbstractModelBasedClasses();
			if(_createModel)
			{
				GUILayout.Label("Choose which class this model should extend (AbstractModel is default)");
				_selectedModel = EditorGUILayout.Popup(_selectedModel,_extendableModelClassNames.ToArray());
			}

            GUI.enabled = _fileName.Length > 0;

			EditorGUILayout.Separator();

            if (GUILayout.Button("Create!"))
            {
                CreateNewFileOfType();
            }
            GUI.enabled = true;
        }

		private void GetAllAbstractViewBasedClasses()
		{
			_extendableClassNames = new List<string>();
			Assembly _assembly = Assembly.Load("Assembly-CSharp");

			foreach (Type type in _assembly.GetTypes())
			{
				if (type.IsClass)
				{
					if(type == typeof(AbstractView))
					{
						_extendableClassNames.Add(type.Name);
						if(_selectedView == -1 && type == typeof(AbstractView))
						{
							_selectedView = _extendableClassNames.Count-1;
						}
					}
					if (type.BaseType == typeof(AbstractView))
					{
						_extendableClassNames.Add(type.Name);
					}
				}
			}
		}

		private void GetAllAbstractModelBasedClasses()
		{
			_extendableModelClassNames = new List<string>();
			Assembly _assembly = Assembly.Load("Assembly-CSharp");
			
			foreach (Type type in _assembly.GetTypes())
			{
				if (type.IsClass)
				{
					if(type == typeof(AbstractModel))
					{
						_extendableModelClassNames.Add(type.Name);
						if(_selectedModel == -1 && type == typeof(AbstractModel))
						{
							_selectedModel = _extendableModelClassNames.Count-1;
						}
					}
					if (type.BaseType == typeof(AbstractModel))
					{
						_extendableModelClassNames.Add(type.Name);
					}
				}
			}
		}

        public static void CreateNewFileOfType()
        {   
            CreateScript();

            CreateAssets();
            AssetDatabase.Refresh();
            InternalEditorUtility.AddScriptComponentUnchecked(_createdViewObject, AssetDatabase.LoadAssetAtPath(TargetPath(), typeof(MonoScript)) as MonoScript);
			if(_createModel)
			{
				InternalEditorUtility.AddScriptComponentUnchecked(_createdModelObject, AssetDatabase.LoadAssetAtPath(TargetModelPath(), typeof(MonoScript)) as MonoScript);
			}
            _createdViewObject.AddComponent<Animation>();
#if UNITY_4_6 || UNITY_5_0
			CreateCanvas(_createdViewObject);
#endif
            if (_window != null)
                _window.Close();
        }

        void OnDestroy()
        {
            if (_firstTimeCreated)
            {
                Selection.activeObject = GameObject.FindObjectOfType(typeof(ViewManager));
            }
        }

        private static string TargetPath()
        {
			return _scaffoldingConfig.ScriptsPath() + _fileName + ".cs";
        }

		private static string TargetModelPath()
		{
			return _scaffoldingConfig.ScriptsPath() + _modelFileName + ".cs";
		}

        private static void CreateScript()
        {
			if (!Directory.Exists(_scaffoldingConfig.ScriptsPath()))
				Directory.CreateDirectory(_scaffoldingConfig.ScriptsPath());

            var writer = new StreamWriter(TargetPath());
            writer.Write(GetViewClass());
            writer.Close();
            writer.Dispose();

			if(_createModel)
			{
				writer = new StreamWriter(TargetModelPath());
				writer.Write(GetModelClass());
				writer.Close();
				writer.Dispose();
			}

            AssetDatabase.Refresh();
        }

        private static void CreateAssets()
        {
			if (!Directory.Exists(_scaffoldingConfig.FullViewPrefabPath()))
				Directory.CreateDirectory(_scaffoldingConfig.FullViewPrefabPath());

			UnityEngine.Object obj = PrefabUtility.CreateEmptyPrefab(_scaffoldingConfig.FullViewPrefabPath() + _fileName + ".prefab");
            GameObject go = new GameObject();
            go.name = _fileName;
            _createdViewObject = PrefabUtility.ReplacePrefab(go, obj, ReplacePrefabOptions.ConnectToPrefab);
			DestroyImmediate(go);

			if(_createModel)
			{
				obj = PrefabUtility.CreateEmptyPrefab(_scaffoldingConfig.FullViewPrefabPath() + _modelFileName + ".prefab");
				go = new GameObject();
				go.name = _modelFileName;

				GameObject modelHolder = _scaffoldingConfig.DetermineParentModelGameObjectPath();
				if(modelHolder == null)
				{
					modelHolder = new GameObject();
					modelHolder.name = "Model";
				}

				go.transform.parent = modelHolder.transform;
				_createdModelObject = PrefabUtility.ReplacePrefab(go, obj, ReplacePrefabOptions.ConnectToPrefab);
			}
        }

        private static string GetViewClass()
        {
            string type = _extendableClassNames[_selectedView];

			TextAsset t = Resources.Load<TextAsset>("ViewTemplate");

			string text = t.text;

			text = text.Replace(ScaffoldingConfig.VIEW_NAME,_fileName);
			text = text.Replace(ScaffoldingConfig.VIEW_TYPE,type);

            return text;
        }

		private static string GetModelClass()
		{
			string type = _extendableModelClassNames[_selectedModel];

			TextAsset t = Resources.Load<TextAsset>("ModelTemplate");

			string text = t.text;
			
			text = text.Replace(ScaffoldingConfig.MODEL_NAME,_modelFileName);
			text = text.Replace(ScaffoldingConfig.MODEL_TYPE,type);

			return text;
		}
    }
}
