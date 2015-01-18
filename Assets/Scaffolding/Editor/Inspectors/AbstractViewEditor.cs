using UnityEngine;
using System.Collections;
using UnityEditor;
using Scaffolding;

namespace Scaffolding.Editor
{
    [CustomEditor(typeof(AbstractView), true)]
    public class AbstractViewEditor : UnityEditor.Editor
    {
        private AbstractView _target;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            _target = (AbstractView)target;
            #if UNITY_4_2         
            Undo.RegisterUndo(_target,"AbstractViewEditor");           
            #elif UNITY_4_3           
            Undo.RecordObject(_target, "AbstractViewEditor");             
            #endif

            GUILayout.Label("VIEW", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("I WANT THIS VIEW TO", MessageType.None);
            _target.showingType = (AbstractView.ShowingTypes)EditorGUILayout.EnumPopup(_target.showingType);

            GUILayout.Label("In Transition");
            _target.inTransition = (AnimationClip)EditorGUILayout.ObjectField(_target.inTransition, typeof(AnimationClip), true);
            GUILayout.Label("Out Transition");
            _target.outTransition = (AnimationClip)EditorGUILayout.ObjectField(_target.outTransition, typeof(AnimationClip), true);

            if (GUILayout.Button("Save View"))
            {
                PrefabUtility.ReplacePrefab(_target.gameObject, PrefabUtility.GetPrefabParent(_target.gameObject), ReplacePrefabOptions.ConnectToPrefab);
            }

            if (GUILayout.Button("Move to library"))
            {
                PrefabUtility.ReplacePrefab(_target.gameObject, PrefabUtility.GetPrefabParent(_target.gameObject), ReplacePrefabOptions.ConnectToPrefab);
                UnityEditor.Editor.DestroyImmediate(_target.gameObject);
            }
            EditorUtility.SetDirty(_target);
        }
    }
}
