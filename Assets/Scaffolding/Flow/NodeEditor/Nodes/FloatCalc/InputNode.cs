﻿using UnityEngine;
using System.Collections;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

[System.Serializable]
[Node (false, "Float/Input", false)]
public class InputNode : Node 
{
	public const string ID = "inputNode";
	public override string GetID { get { return ID; } }

	public float value = 1f;

	public override Node Create (Vector2 pos, string name = "") 
	{ // This function has to be registered in Node_Editor.ContextCallback
		InputNode node = CreateInstance <InputNode> ();
		
		node.name = "Input Node";
		node.rect = new Rect (pos.x, pos.y, 200, 50);;
		
		NodeOutput.Create (node, "Value", "Float");

		return node;
	}

	public override void NodeGUI () 
	{
		value = RTEditorGUI.FloatField (new GUIContent ("Value", "The input value of type float"), value);
		OutputKnob (0);

		if (GUI.changed)
			NodeEditor.RecalculateFrom (this);
	}
	
	public override bool Calculate () 
	{
		Outputs[0].SetValue<float> (value);
		return true;
	}
}