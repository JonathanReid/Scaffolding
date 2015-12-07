using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using Scaffolding;

[System.Serializable]
[Node (false, "Views/Transition", false)]
public class TransitionNode : Node 
{
	public const string ID = "transitionNode";
	public override string GetID { get { return ID; } }
	
	public string Input1Val = "";

	public string[] Transitions;
	public int TransitionIndex;
	
	public override Node Create (Vector2 pos, string name) 
	{
		TransitionNode node = CreateInstance <TransitionNode> ();
		
		node.name = "Transition";
		node.rect = new Rect (pos.x, pos.y, 200, 65);
		
		node.CreateInput ("Target view", "View");
		node.CreateOutput ("Transition to:", "View");

		return node;
	}
	
	public override void NodeGUI () 
	{

		if(Transitions == null || Transitions.Length == 0)
		{
			Transitions = ScaffoldingExtensions.GetAllTransitions().ToArray();

		}

		GUILayout.BeginHorizontal ();
		GUILayout.BeginVertical ();
		
		GUILayout.Label("Connected views:");
		InputKnob (0);
		
		for(int i = 0; i < Inputs[0].connections.Count; ++i)
		{
			if(Inputs[0].connections[i] != null)
			{
				GUILayout.Label (Inputs[0].connections[i].body.name);
			}
		}
		
		GUILayout.EndVertical ();
		GUILayout.BeginVertical ();

		GUILayout.BeginVertical ();
		Outputs [0].DisplayLayout ();
		GUILayout.EndVertical ();

		TransitionIndex = UnityEditor.EditorGUILayout.Popup(TransitionIndex, Transitions);
	
		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();
		
		
		if (GUI.changed)
			NodeEditor.RecalculateFrom (this);
	}
	
	public override bool Calculate () 
	{
		if (Inputs[0].connections.Count > 0 && Inputs[0].connections[0] != null)
			Input1Val = Inputs[0].connections[0].body.name;
		
		return true;
	}
}

