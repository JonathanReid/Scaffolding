using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Scaffolding.Editor
{
    [CustomEditor(typeof(SwipeInput))]
    public class SwipeInputEditor : UnityEditor.Editor
    {
        private SwipeInput _target;

        public override void OnInspectorGUI()
        {
            _target = (SwipeInput)target;

            #if UNITY_4_2         
            Undo.RegisterUndo(_target,"SwipeEditor");           
            #elif UNITY_4_3           
            Undo.RecordObject(_target, "SwipeEditor");             
            #endif

            EditorGUILayout.HelpBox("HOW FAST DO YOU NEED TO SWIPE?", MessageType.None);
            int speed = Mathf.RoundToInt((_target.velocityThreshold / 4000) * 100);
            EditorGUILayout.HelpBox("SPEED: " + speed + "%", MessageType.None);

            _target.velocityThreshold = GUILayout.HorizontalSlider(_target.velocityThreshold, 0, 4000);

            if (_target.velocityThreshold < 1500 && _target.velocityThreshold > 0)
            {
                EditorGUILayout.HelpBox("SLOW", MessageType.None);
            }
            if (_target.velocityThreshold < 3000 && _target.velocityThreshold > 1500)
            {
                EditorGUILayout.HelpBox("MEDIUM", MessageType.None);
            }
            if (_target.velocityThreshold < 4000 && _target.velocityThreshold > 3000)
            {
                EditorGUILayout.HelpBox("FAST!", MessageType.None);
            }
            EditorUtility.SetDirty(_target);
        }
    }
}