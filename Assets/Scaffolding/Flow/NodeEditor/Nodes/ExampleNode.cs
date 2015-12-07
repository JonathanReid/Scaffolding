using UnityEngine;
using NodeEditorFramework;

[Node (false, "Example Node", true)]
public class ExampleNode : Node 
{
	public const string ID = "exampleNode";
	public override string GetID { get { return ID; } }
	
	public override Node Create (Vector2 pos, string name = "") 
	{
		ExampleNode node = CreateInstance<ExampleNode> ();
		
		node.rect = new Rect (pos.x, pos.y, 150, 60);
		node.name = "Example Node";
		
		node.CreateInput ("Value", "Float");
		node.CreateOutput ("Output val", "Float");
		node.CreateOutput ("Output val", "Float");
		node.CreateOutput ("Output val", "Float");
		node.CreateOutput ("Output val", "Float");
		node.CreateOutput ("Output val", "Float");
		
		return node;
	}
	
	public override void NodeGUI () 
	{
		GUILayout.Label ("This is a custom Node!");

		GUILayout.BeginHorizontal ();
		GUILayout.BeginVertical ();

		Inputs [0].DisplayLayout ();

		GUILayout.EndVertical ();
		GUILayout.BeginVertical ();
		
		Outputs [0].DisplayLayout ();
		Outputs [1].DisplayLayout ();
		Outputs [2].DisplayLayout ();
		Outputs [3].DisplayLayout ();
		
		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();
		
	}
	
	public override bool Calculate () 
	{
		if (!allInputsReady ())
			return false;
		for (int j = 0; j < Inputs[0].connections.Count; j++) 
		{
			Outputs[0].SetValue<float> (Inputs[0].connections[j].GetValue<float> () * 5);
		}
		return true;
	}
}
