using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using Scaffolding.Transitions;
using System.Reflection;
using System;

namespace Scaffolding.Editor
{
	[CustomEditor(typeof(TransitionComponentBase), true)]
	public class TransitionComponentEditor : UnityEditor.Editor
	{
		private TransitionComponentBase _target;
		private List<string> _transitions;
		private List<string> _transitionTypes;
		private int _selectedView = -1;
		
		public override void OnInspectorGUI()
		{
			_target = (TransitionComponentBase)target;

			DrawTransitionChooser();

			if(_target != null)
			{
				if(_target.GetType().IsSubclassOf(typeof(TransitionComponentBase)))
				{
					DrawDefaultInspector();
				}
				else
				{
				}

				EditorUtility.SetDirty(_target);
			}
		}

		private void DrawTransitionChooser()
		{
			GetAllTransitions();

			_selectedView = EditorGUILayout.Popup("Transition: ", _selectedView ,_transitions.ToArray());

			if(GUI.changed)
			{
				_target.gameObject.AddComponent( System.Type.GetType( _transitionTypes[_selectedView] + ", Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null") );
				DestroyImmediate(_target.gameObject.GetComponent(_target.GetType()));
				_target = null;
			}
		}

		private void GetAllTransitions()
		{
			if(_transitions == null || _transitionTypes == null)
			{
				_transitions = new List<string>();
				_transitionTypes = new List<string>();

				Assembly _assembly = Assembly.Load("Assembly-CSharp");
				
				foreach (Type type in _assembly.GetTypes())
				{
					if (type.IsClass)
					{
						if(type == typeof(TransitionComponentBase))
						{
							_transitions.Add(type.Name);
							_transitionTypes.Add(type.FullName);
						}
						
						if (type.IsSubclassOf(typeof(TransitionComponentBase)))
						{
							_transitions.Add(type.Name);
							_transitionTypes.Add(type.FullName);
						}
						if(type == _target.GetType())
						{
							_selectedView = _transitions.Count-1;
						}
					}
				}
			}
		}
	}
}
