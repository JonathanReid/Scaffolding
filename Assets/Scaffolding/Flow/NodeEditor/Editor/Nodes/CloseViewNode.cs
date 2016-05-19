#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using NodeEditorFramework;

[System.Serializable]
[Node (false, "Views/Close Overlay", false)]
public class CloseViewNode : Node
{
    public const string ID = "closeViewNode";
    public override string GetID
    {
        get
        {
            return ID;
        }
    }

    [HideInInspector]
    public bool assigned = false;
    public float value = 0;

    public override Node Create (Vector2 pos, string name = "")
    {
        // This function has to be registered in Node_Editor.ContextCallback
        CloseViewNode node = CreateInstance <CloseViewNode> ();

        node.name = "Close Overlay";
        node.rect = new Rect (pos.x, pos.y, 200, 100);

        node.CreateInput ("Target view", "View");
        node.CreateOutput ("Then Open:", "View");

        return node;
    }

    public void Refresh()
    {
        Debug.Log("Refreh");
    }

    public override void DrawNode ()
    {
        GUI.color = Color.magenta;
        base.DrawNode ();
        GUI.color = Color.white;
    }

    public override void NodeGUI ()
    {
        MinifiableNode = true;
        GUILayout.BeginHorizontal ();
        GUILayout.BeginVertical ();

        GUILayout.Label("Close Overlay");
        InputKnob (0);

//      for(int i = 0; i < Inputs[0].connections.Count; ++i)
//      {
//          if(Inputs[0].connections[i] != null)
//          {
//              GUILayout.Label (Inputs[0].connections[i].body.name);
//          }
//      }

        GUILayout.EndVertical ();
        GUILayout.BeginVertical ();


        GUILayout.BeginVertical ();
        GUILayout.BeginHorizontal ();
        Outputs [0].DisplayLayout ();
        GUILayout.EndHorizontal ();
        GUILayout.EndVertical ();

        GUILayout.EndVertical ();
        GUILayout.EndHorizontal ();
    }

    public override bool Calculate ()
    {

        return true;
    }
}
#endif