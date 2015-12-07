using UnityEngine;
using System.Collections;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

[System.Serializable]
[Node (false, "Float/Calculation", false)]
public class CalcNode : Node 
{
	public enum CalcType { Add, Substract, Multiply, Divide }
	public CalcType type = CalcType.Add;

	public const string ID = "calcNode";
	public override string GetID { get { return ID; } }

	public float Input1Val = 1f;
	public float Input2Val = 1f;

	public override Node Create (Vector2 pos, string name) 
	{
		CalcNode node = CreateInstance <CalcNode> ();
		
		node.name = name;
		node.rect = new Rect (pos.x, pos.y, 200, 100);
		
		node.CreateInput ("Input 1", "Float");
		node.CreateInput ("Input 2", "Float");
		
		node.CreateOutput ("Output 1", "Float");

		return node;
	}

	public override void NodeGUI () 
	{
		GUILayout.BeginHorizontal ();
		GUILayout.BeginVertical ();

		for (int j = 0; j < Inputs[0].connections.Count; j++) 
		{
			if (Inputs [0].connections[j] != null)
				GUILayout.Label (Inputs [0].name);
			else
				Input1Val = RTEditorGUI.FloatField (GUIContent.none, Input1Val);
		}
		InputKnob (0);
		// --
		for (int j = 0; j < Inputs[0].connections.Count; j++) 
		{
			if (Inputs [1].connections[j] != null)
				GUILayout.Label (Inputs [1].name);
			else
				Input2Val = RTEditorGUI.FloatField (GUIContent.none, Input2Val);
		}
		InputKnob (1);

		GUILayout.EndVertical ();
		GUILayout.BeginVertical ();

		Outputs [0].DisplayLayout ();

		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();

#if UNITY_EDITOR
		type = (CalcType)UnityEditor.EditorGUILayout.EnumPopup (new GUIContent ("Calculation Type", "The type of calculation performed on Input 1 and Input 2"), type);
#else
		GUILayout.Label (new GUIContent ("Calculation Type: " + type.ToString (), "The type of calculation performed on Input 1 and Input 2"));
#endif

		if (GUI.changed)
			NodeEditor.RecalculateFrom (this);
	}

	public override bool Calculate () 
	{
		for (int j = 0; j < Inputs[0].connections.Count; j++) 
		{
			if (Inputs[0].connections[j] != null)
				Input1Val = Inputs[0].connections[j].GetValue<float> ();
			if (Inputs[1].connections[j] != null)
				Input2Val = Inputs[1].connections[j].GetValue<float> ();
		}
		switch (type) 
		{
		case CalcType.Add:
			Outputs[0].SetValue<float> (Input1Val + Input2Val);
			break;
		case CalcType.Substract:
			Outputs[0].SetValue<float> (Input1Val - Input2Val);
			break;
		case CalcType.Multiply:
			Outputs[0].SetValue<float> (Input1Val * Input2Val);
			break;
		case CalcType.Divide:
			Outputs[0].SetValue<float> (Input1Val / Input2Val);
			break;
		}

		return true;
	}
}
