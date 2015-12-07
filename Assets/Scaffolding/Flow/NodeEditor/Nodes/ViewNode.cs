using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using Scaffolding;

[System.Serializable]
[Node (false, "Views/ViewNode", false)]
public class ViewNode : Node 
{
	public List<Scaffolding.ViewType> viewType;
	
	public const string ID = "viewNode";
	public override string GetID { get { return ID; } }
	
	public string Input1Val = "";
	public int ButtonCount = -1;
	
	public override Node Create (Vector2 pos, string name) 
	{
		ViewNode node = CreateInstance <ViewNode> ();

		viewType = new List<Scaffolding.ViewType>();
		List<string> buttons = ScaffoldingExtensions.GetButtonsInView(name);
		ButtonCount = buttons.Count;

		node.name = name;
		node.rect = new Rect (pos.x, pos.y, 200, 30 + (ButtonCount * 35));
		
		node.CreateInput ("Target view", "View");

		for(int i = 0; i < ButtonCount; i++)
		{
			node.CreateOutput (buttons[i], "View");
			viewType.Add(Scaffolding.ViewType.View);
		}

		return node;
	}
	
	public override void NodeGUI () 
	{
		if(ButtonCount == -1)
		{
			ButtonCount = ScaffoldingExtensions.GetButtonsInView(name).Count;
			viewType = new List<Scaffolding.ViewType>();
			for(int i = 0; i < ButtonCount; ++i)
			{
				viewType.Add(Scaffolding.ViewType.View);
			}
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
		// --
		
		GUILayout.EndVertical ();
		GUILayout.BeginVertical ();

		for(int i = 0; i < ButtonCount; ++i)
		{
			GUILayout.BeginVertical ();
			Outputs [i].DisplayLayout ();

			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace();
			viewType[i] = (Scaffolding.ViewType)UnityEditor.EditorGUILayout.EnumPopup(viewType[i],GUILayout.Width(55));
			GUILayout.EndHorizontal ();
			GUILayout.EndVertical ();
		}
		
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

