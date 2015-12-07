using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using Scaffolding;

[System.Serializable]
[Node (false, "Views/StartNode", false)]
public class StartNode : Node 
{
	public List<Scaffolding.ViewType> viewType;
	
	public const string ID = "startNode";
	public override string GetID { get { return ID; } }
	
	public string Input1Val = "";
	
	public override Node Create (Vector2 pos, string name) 
	{
		StartNode node = CreateInstance <StartNode> ();

		viewType = new List<Scaffolding.ViewType>();

		node.name = "Starting View";
		node.rect = new Rect (pos.x, pos.y, 200, 65);

		node.CreateOutput ("Starting View", "View");

		return node;
	}
	
	public override void NodeGUI () 
	{
		if(viewType == null || viewType.Count == 0)
		{
			viewType = new List<Scaffolding.ViewType>();
			viewType.Add(Scaffolding.ViewType.View);
		}

		GUILayout.BeginHorizontal ();

		GUILayout.BeginVertical ();


		GUILayout.BeginVertical ();
		Outputs [0].DisplayLayout ();

		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
		viewType[0] = (Scaffolding.ViewType)UnityEditor.EditorGUILayout.EnumPopup(viewType[0],GUILayout.Width(55));
		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();

		
		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();

		
		if (GUI.changed)
			NodeEditor.RecalculateFrom (this);
	}
	
	public override bool Calculate () 
	{

		return true;
	}
}