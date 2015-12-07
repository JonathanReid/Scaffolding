using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using Scaffolding;

namespace Scaffolding.Editor
{
    [CustomEditor(typeof(PinchThisInput))]
    public class PinchThisItemEditor : UnityEditor.Editor
    {
        private PinchThisInput _target;

        public override void OnInspectorGUI()
        {
            _target = (PinchThisInput)target;
            #if UNITY_4_2         
            Undo.RegisterUndo(_target,"PinchThisItemEditor");           
            #elif UNITY_4_3           
            Undo.RecordObject(_target, "PinchThisItemEditor");             
            #endif

            EditorGUILayout.HelpBox("THIS ITEM GETS ITS INPUT FROM", MessageType.None);
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
                    if (_target.inputCameraLength != cameraNames.Count && _target.inputCamera != null)
                    {
                        _target.inputCameraIndex = _target.inputCamera == null ? 0 : cameraNames.IndexOf(_target.inputCamera);
                    }
                    _target.inputCameraLength = cameraNames.Count;
                    _target.inputCameraIndex = Mathf.Clamp(_target.inputCameraIndex, 0, cameraNames.Count);
                    _target.inputCamera = cameras[_target.inputCameraIndex].name;
                }
            }

            EditorGUILayout.HelpBox("AND ITS SCALE IS CLAMPED TO:", MessageType.None);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Smallest scale:");
            _target.smallestScale = EditorGUILayout.FloatField(_target.smallestScale);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Largest scale:");
            _target.largestScale = EditorGUILayout.FloatField(_target.largestScale);
            EditorGUILayout.EndHorizontal();
            EditorUtility.SetDirty(_target);
        }
    }
}
