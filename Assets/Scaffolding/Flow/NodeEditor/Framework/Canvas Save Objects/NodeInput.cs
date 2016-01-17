﻿#if UNITY_EDITOR
using System;
using UnityEngine;
using NodeEditorFramework;
using System.Collections.Generic;

namespace NodeEditorFramework
{
	public class NodeInput : NodeKnob
	{
		public override NodeSide defaultSide { get { return NodeSide.Left; } }

		public List<NodeOutput> connections = new List<NodeOutput>();

		// Value
		[System.NonSerialized]
		private object value = null;
		private System.Type valueType;
		
		/// <summary>
		/// Creates a new NodeOutput in NodeBody of specified type
		/// </summary>
		public static NodeInput Create (Node NodeBody, string InputName, string InputType) 
		{
			NodeInput input = CreateInstance <NodeInput> ();
			input.body = NodeBody;
			input.name = InputName;
			input.SetType (InputType);
			NodeBody.Inputs.Add (input);
			return input;
		}
		
		public override void SetType (string Type) 
		{
			type = Type;
			TypeData typeData = ConnectionTypes.GetTypeData (type);
			texturePath = typeData.declaration.OutputKnob_TexPath;
			knobTexture = typeData.OutputKnob;
		}
		
		#region Value
		
		public bool IsValueNull { get { return value == null; } }
		
		/// <summary>
		/// Gets the output value.
		/// </summary>
		/// <returns>Value, if null default(T) (-> For reference types, null. For value types, default value)</returns>
		public T GetValue<T> ()
		{
			if (valueType == null)
				valueType = ConnectionTypes.GetType(type);
			if (valueType == typeof(T))
			{
				if (value == null)
					value = getDefault<T> ();
				return (T)value;
			}
			UnityEngine.Debug.LogError ("Trying to GetValue<" + typeof(T).FullName + "> for input Type: " + valueType.FullName);
			return getDefault<T> ();
		}
		
		/// <summary>
		/// Sets the value.
		/// </summary>
		public void SetValue<T> (T Value)
		{
			if (valueType == null)
				valueType = ConnectionTypes.GetType(type);
			if (valueType == typeof(T))
				value = Value;
			else
				UnityEngine.Debug.LogError("Trying to SetValue<" + typeof(T).FullName + "> for input Type: " + valueType.FullName);
		}
		
		/// <summary>
		/// Resets the value to null.
		/// </summary>
		public void ResetValue () 
		{
			value = null;
		}
		
		/// <summary>
		/// Returns for value types the default value; for reference types:, the default constructor if existant, else null
		/// </summary>
		public static T getDefault<T> ()
		{
			T var;
			if (typeof(T).GetConstructor (Type.EmptyTypes) != null)
			{ // Try to create using an empty constructor if existant
				var = Activator.CreateInstance<T> ();
			}
			else
			{ // Else try to get default. Returns null only on reference types
				var = default(T);
			}
			return var;
		}
		#endregion
	}
}
#endif