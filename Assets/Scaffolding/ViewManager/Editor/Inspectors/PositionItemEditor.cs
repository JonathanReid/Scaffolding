using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Scaffolding;

namespace Scaffolding.Editor
{
    [CustomEditor(typeof(PositionItem))]
    public class PositionItemEditor : UnityEditor.Editor
    {
        private PositionItem _target;
        private Camera[] _allCameras;
        private List<string> _allCameraNames;

        public override void OnInspectorGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("I WANT THIS TRANSFORM TO BE ANCHORED TO THE:", MessageType.None);
                _target = (PositionItem)target;
                #if UNITY_4_2         
                Undo.RegisterUndo(_target,"PositionItemEditor");           
                #elif UNITY_4_3           
                Undo.RecordObject(_target, "PositionItemEditor");             
                #endif

                PositionItem.ScreenPosition pos = _target.screenPosition;
                _target.screenPosition = (PositionItem.ScreenPosition)EditorGUILayout.EnumPopup(_target.screenPosition);

                if (_target.screenPosition != pos)
                {
                    _target.SetPosition();
                }

                if (_target.screenPosition != PositionItem.ScreenPosition.None)
                {
                    //edge
                    EditorGUILayout.HelpBox("AND I WANT IT TO SIT THIS FAR FROM THE EDGE:", MessageType.None);
                    Vector2 edge = _target.edgeBuffer;
                    _target.edgeBuffer = EditorGUILayout.Vector2Field("Pixels from edge", _target.edgeBuffer);
                    _target.edgeBuffer.x = Mathf.RoundToInt(_target.edgeBuffer.x);
                    _target.edgeBuffer.y = Mathf.RoundToInt(_target.edgeBuffer.y);
                    if (edge != _target.edgeBuffer)
                    {
                        _target.SetPosition();  
                    }

                    //cameras
                    _allCameras = GameObject.FindObjectsOfType(typeof(Camera)) as Camera[];
                    _allCameraNames = new List<string>();

                    int i = 0, l = _allCameras.Length;
                    for (; i < l; ++i)
                    {
                        _allCameraNames.Add(_allCameras[i].name);
                    }

                    if (_allCameras.Length > 0)
                    {
                        EditorGUILayout.HelpBox("AND I WANT TO ALIGN IT TO THE EDGE OF THIS CAMERA:", MessageType.None);
                        int index = _target.renderingCameraIndex;
                        _target.renderingCameraIndex = EditorGUILayout.Popup(_target.renderingCameraIndex, _allCameraNames.ToArray());
                        if (_target.renderingCameraLength != _allCameraNames.Count && _target.renderingCamera != null)
                        {
                            _target.renderingCameraIndex = _target.renderingCamera == null ? 0 : _allCameraNames.IndexOf(_target.renderingCamera);
                        }
                        Camera c = _allCameras[_target.renderingCameraIndex];
                        if (c.orthographic)
                        {
                            _target.renderingCamera = _allCameras[_target.renderingCameraIndex].name;
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("POSITION ITEM ONLY WORKS WITH ORTHOGRAPHIC CAMERAS!", MessageType.Error);
                            EditorGUILayout.LabelField("sorry!");
                        }

                        if (_target.renderingCameraIndex != index)
                        {
                            _target.SetPosition();  
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("YOU DONT HAVE ANY CAMERAS IN THE SCENE!", MessageType.Error);
                    }

                    _target.renderingCameraLength = _allCameraNames.Count;

                    EditorUtility.SetDirty(_target);
                }
                else
                {
                    EditorGUILayout.HelpBox("THERE IS NO ANCHOR POSITION DEFINED, I'LL MOVE FREELY!", MessageType.Error);
                    EditorGUILayout.LabelField("Perhaps you dont really need me?");
                    if (GUILayout.Button("You're right! I don't need you at all!"))
                    {
                        DestroyImmediate(_target);
                    }
                }

            }
        }
    }
}
