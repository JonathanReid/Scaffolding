using UnityEngine;
using System.Collections;
using NodeEditorFramework;

[System.Serializable]
[Node (false, "Views/Close Overlay", false)]
public class CloseViewNode : Node 
{
	public const string ID = "closeViewNode";
	public override string GetID { get { return ID; } }
	
	[HideInInspector]
	public bool assigned = false;
	public float value = 0;
	
	public override Node Create (Vector2 pos, string name = "") 
	{ // This function has to be registered in Node_Editor.ContextCallback
		CloseViewNode node = CreateInstance <CloseViewNode> ();
		
		node.name = "Close Overlay";
		node.rect = new Rect (pos.x, pos.y, 200, 100);
		
		node.CreateInput ("Target view", "View");
		node.CreateOutput ("Transition to:", "View");
		
		return node;
	}
	
	public override void NodeGUI () 
	{
		GUILayout.BeginHorizontal ();
		GUILayout.BeginVertical ();

		GUILayout.Label("Close Overlays:");
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

		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();
	}
	
	public override bool Calculate () 
	{

		return true;
	}
}
