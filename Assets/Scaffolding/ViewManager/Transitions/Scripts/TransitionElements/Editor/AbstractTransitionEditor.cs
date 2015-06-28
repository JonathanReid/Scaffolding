using UnityEngine;
using System.Collections;
using UnityEditor;
using Scaffolding;

namespace Scaffolding.Editor
{
    [CustomEditor(typeof(AbstractTransition), true)]
    public class AbstractTransitionEditor : UnityEditor.Editor
    {
		private AbstractTransition _target;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
			_target = (AbstractTransition)target;
            #if UNITY_4_2         
            Undo.RegisterUndo(_target,"AbstractViewEditor");           
            #elif UNITY_4_3           
            Undo.RecordObject(_target, "AbstractViewEditor");             
            #endif

			_target.SetupHolders();
            EditorUtility.SetDirty(_target);
        }
    }
}
