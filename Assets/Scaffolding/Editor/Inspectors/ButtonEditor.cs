using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using Scaffolding;

namespace Scaffolding.Editor
{
    [CustomEditor(typeof(AbstractButton),true)]
    public class ButtonEditor : UnityEditor.Editor
    {
        private AbstractButton _target;
        private List<Type> components;
        private List<string> componentNames;
        private List<string> methodNames;
        private List<MethodInfo> methods;
        private List<GameObject> sceneObjects;
        private List<AbstractView> _views;
        private List<string> _viewNames;
        private int _selectedScriptIndex;
        private int _selectedMethodIndex;
        private bool _openAsOverlay;
        private bool _firstLoad;

        public override void OnInspectorGUI()
        {
			_target = (AbstractButton)target;

            #if UNITY_4_2         
            Undo.RegisterUndo(_target,"Button Editor");           
            #elif UNITY_4_3           
            Undo.RecordObject(_target, "Button Editor");             
            #endif

            GetButtonInputCamera();

            DecideButtonAction();

			if(_target is ScaffoldingButton)
			{
            	AddScriptCallbacks();
			}

            SceneView.RepaintAll();

            EditorUtility.SetDirty(target);
        }

        private void OpenView()
        {
            //Choose view to open.
            EditorGUILayout.HelpBox("THE VIEW CALLED", MessageType.None);
            LoadAllViews();
            _target.targetViewIndex = EditorGUILayout.Popup(_target.targetViewIndex, _viewNames.ToArray());
            _target.targetViewIndex = ScaffoldingUtilitiesEditor.CheckIfMenuItemChanged(_target.targetViewLength, _target.targetViewIndex, _viewNames, _target.targetView);
            _target.targetViewLength = _viewNames.Count;
            _target.targetView = _views[_target.targetViewIndex].name;

            //choose how its opened.
            EditorGUILayout.HelpBox("OPEN AS SCREEN OR OVERLAY?", MessageType.None);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Screen:");
            _target.openAsScreen = EditorGUILayout.Toggle(_target.openAsScreen);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Overlay:");
            _openAsOverlay = EditorGUILayout.Toggle(!_target.openAsScreen);
            if (_openAsOverlay)
                _target.openAsScreen = false;
            else
                _target.openAsScreen = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();

            //if its a screen, theres some extra params.
            if (_target.openAsScreen)
            {
                EditorGUILayout.LabelField("Open a loading overlay?");
                _target.openLoadingOverlay = EditorGUILayout.Toggle(_target.openLoadingOverlay);
                if (_target.openLoadingOverlay)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    _viewNames.Remove(_target.targetView);
                    _target.loadingOverlayIndex = EditorGUILayout.Popup(_target.loadingOverlayIndex, _viewNames.ToArray());
                    _target.loadingOverlayIndex = ScaffoldingUtilitiesEditor.CheckIfMenuItemChanged(_target.loadingOverlayLength, _target.loadingOverlayIndex, _viewNames, _target.loadingOverlay);
                    _target.loadingOverlayLength = _viewNames.Count;
                    _target.loadingOverlay = _views[_target.loadingOverlayIndex].name;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
                else
                {
                    _target.loadingOverlay = "";
                }
            }
            else
            {
                EditorGUILayout.LabelField("Disable inputs on the current view?");
                _target.disableInputsOnOverlay = EditorGUILayout.Toggle(_target.disableInputsOnOverlay);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void CloseView()
        {
            EditorGUILayout.HelpBox("THE OVERLAY CALLED", MessageType.None);
            LoadAllViews();
            _target.targetViewIndex = EditorGUILayout.Popup(_target.targetViewIndex, _viewNames.ToArray());
            _target.targetViewIndex = ScaffoldingUtilitiesEditor.CheckIfMenuItemChanged(_target.targetViewLength, _target.targetViewIndex, _viewNames, _target.targetView);
            _target.targetView = _views[_target.targetViewIndex].name;
        }

        private void TellClass(int i)
        {
            //find all scripts in scene
            //                        _target.selectedScriptIndex = 1;
            OpenScriptFinder(); 
            _target.selectedScriptIndex[i] = EditorGUILayout.Popup(_target.selectedScriptIndex[i], componentNames.ToArray());
            SelectScript(components[_target.selectedScriptIndex[i]]);
            FindPublicMethodsInScript(components[_target.selectedScriptIndex[i]]);
            _target.selectedScriptIndex[i] = ScaffoldingUtilitiesEditor.CheckIfMenuItemChanged(_target.selectedScriptLength[i], _target.selectedScriptIndex[i], componentNames, _target.selectedScript[i]);
            _target.selectedScript[i] = componentNames[_target.selectedScriptIndex[i]];
            _target.selectedScriptLength[i] = componentNames.Count;


            //and then find all its methods
            EditorGUILayout.HelpBox("TO RUN THE FUNCTION", MessageType.None);
            _target.selectedMethodIndex[i] = EditorGUILayout.Popup(_target.selectedMethodIndex[i], methodNames.ToArray());
            _target.selectedMethodIndex[i] = ScaffoldingUtilitiesEditor.CheckIfMenuItemChanged(_target.selectedMethodLength[i], _target.selectedMethodIndex[i], methodNames, _target.selectedMethod[i]);
            _target.selectedMethod[i] = methods[_target.selectedMethodIndex[i]].Name;
            _target.selectedMethodLength[i] = methodNames.Count;
        }

        private void PlayAnimation()
        {
            _target.animationClip = (AnimationClip)EditorGUILayout.ObjectField(_target.animationClip, typeof(AnimationClip), true);
        }

        private void GetButtonInputCamera()
        {
            EditorGUILayout.HelpBox("THIS BUTTON GETS ITS INPUT FROM", MessageType.None);
            Camera[] cameras = GameObject.FindObjectsOfType(typeof(Camera)) as Camera[];
            List<string> cameraNames = new List<string>();

            int i = 0, l = cameras.Length;
            if (!Application.isPlaying)
            {
                for (; i < l; ++i)
                {
                    cameraNames.Add(cameras[i].name);
                }

                if (cameras.Length > 0)
                {
                    _target.inputCameraIndex = EditorGUILayout.Popup(_target.inputCameraIndex, cameraNames.ToArray());
                    if (_target.inputCamera != null)
                    {
                        _target.inputCameraIndex = ScaffoldingUtilitiesEditor.CheckIfMenuItemChanged(_target.inputCameraLength, _target.inputCameraIndex, cameraNames, _target.inputCamera);
                        _target.inputCameraLength = cameraNames.Count;
                    }
                    _target.inputCamera = cameras[_target.inputCameraIndex].name;
                }
            }
        }

        private void DecideButtonAction()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox("WHEN I CLICK THIS BUTTON, I WANT IT TO", MessageType.None);
            _target.buttonActionType = (ScaffoldingButton.ButtonActionType)EditorGUILayout.EnumPopup(_target.buttonActionType);

            switch (_target.buttonActionType)
            {
            //open a view
                case ScaffoldingButton.ButtonActionType.Open:
                    OpenView();
                    break;
            //close an overlay
                case ScaffoldingButton.ButtonActionType.Close:
                    CloseView();
                    break;
            }
            EditorGUILayout.EndVertical();
        }

        private void AddScriptCallbacks()
        {
            if (_target.selectedScript == null)
            {
                _target.selectedScript = new List<String>();
                _target.selectedScriptIndex = new List<int>();
                _target.selectedMethod = new List<string>();
                _target.selectedMethodIndex = new List<int>();
                _target.selectedScriptLength = new List<int>();
                _target.selectedMethodLength = new List<int>();
            }

            GUILayout.Label("BUT I WANT TO TELL A SCRIPT WHAT TO DO!", EditorStyles.boldLabel);
            int i = 0;
            int l = _target.selectedScript.Count;

            for (; i < l; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("I WANT TO TELL THE SCRIPT", MessageType.None);
                if (GUILayout.Button("-"))
                {
                    _target.selectedScript.RemoveAt(i);
                    _target.selectedScriptIndex.RemoveAt(i);
                    _target.selectedMethod.RemoveAt(i);
                    _target.selectedMethodIndex.RemoveAt(i);
                    _target.selectedMethodLength.RemoveAt(i);
                    _target.selectedScriptLength.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
                TellClass(i);
                EditorGUILayout.Separator();
            }
            string name = "OK! LETS ADD THAT!";
            if (_target.selectedScript.Count > 0)
                name = "ADD ANOTHER ONE";

            if (GUILayout.Button(name))
            {
                _target.selectedScript.Add("");
                _target.selectedScriptIndex.Add(0);
                _target.selectedMethod.Add(null);
                _target.selectedMethodIndex.Add(0);
                _target.selectedMethodLength.Add(0);
                _target.selectedScriptLength.Add(0);
            }
        }

        public void LoadAllViews()
        {
            _views = new List<AbstractView>();
            _viewNames = new List<string>();
            UnityEngine.Object[] views = Resources.LoadAll("Views");
            foreach (UnityEngine.Object o in views)
            {
                AbstractView v = (o as GameObject).GetComponent<AbstractView>();
                if (v != null)
                {
                    _views.Add(v);
                    _viewNames.Add(v.name);
                }
            }
        }

        private void OpenScriptFinder()
        {
            Assembly _assembly = Assembly.Load("Assembly-CSharp");

            components = new List<Type>();
            componentNames = new List<string>();
            //ading

            foreach (Type type in _assembly.GetTypes())
            {
                if (type.IsClass)
                {
                    if (type.FullName.Contains("MonoBehaviour"))
                    {
                        components.Add(type);
                        componentNames.Add(type.Name);
                    }
                    else
                    {
                        if (type.BaseType != typeof(System.Object) && !type.FullName.Contains("System") && !type.FullName.Contains("Abstract") && !type.FullName.Contains("+") && type.Namespace != "Scaffolding")
                        {
                            Type t = type;        
                            if (!t.FullName.Contains("System"))
                            {
                                components.Add(t);
                                componentNames.Add(t.Name);
                            }
                        }
                    }
                }
            }
        }

        private void SelectScript(Type type)
        {
            sceneObjects = new List<GameObject>();

            foreach (UnityEngine.Object _obj in GameObject.FindObjectsOfType(type))
            {
                sceneObjects.Add(GameObject.Find(_obj.name));
            }
        }

        private void FindPublicMethodsInScript(Type type)
        {
					
            methodNames = new List<string>();
            methods = new List<MethodInfo>();
            foreach (MethodInfo m in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (m.GetParameters().Length == 0 && (m.DeclaringType.Equals(type) || m.DeclaringType.Equals(type.BaseType)) && m.ReturnType.Equals(typeof(void)))
                {
                    methodNames.Add(m.Name);
                    methods.Add(m);
                }
            }
        }
    }
}
