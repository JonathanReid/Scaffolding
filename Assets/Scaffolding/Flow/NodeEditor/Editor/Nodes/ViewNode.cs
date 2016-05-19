#if UNITY_EDITOR
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
    public Scaffolding.ViewType viewType;

    public const string ID = "viewNode";
    public override string GetID
    {
        get
        {
            return ID;
        }
    }

    public string Input1Val = "";
    public int ButtonCount = -1;

    public override Node Create (Vector2 pos, string name)
    {
        ViewNode node = CreateInstance <ViewNode> ();

        viewType = Scaffolding.ViewType.View;
        List<string> buttons = ScaffoldingExtensions.GetButtonsInView(name);
        ButtonCount = buttons.Count;

        node.name = name;
        node.rect = new Rect (pos.x, pos.y, 200, 70 + (Mathf.Clamp((ButtonCount - 1), 0, 100) * 20));

        node.CreateInput ("Target view", "View");

        for(int i = 0; i < ButtonCount; i++)
        {
            node.CreateOutput (buttons[i], "View");
        }

        return node;
    }

    public void Refresh()
    {
        int buttonCount = ButtonCount;

        List<string> buttons = ScaffoldingExtensions.GetButtonsInView(name);
        ButtonCount = buttons.Count;

        for(int i = buttonCount; i < ButtonCount; ++i)
        {
            CreateOutput (buttons[i], "View");
        }

        rect = new Rect (rect.x, rect.y, 200, 70 + (Mathf.Clamp((ButtonCount - 1), 0, 100) * 20));
    }

    public override void DrawNode ()
    {

        base.DrawNode ();

    }

    public override void NodeGUI ()
    {
        if(ButtonCount == -1)
        {
            ButtonCount = ScaffoldingExtensions.GetButtonsInView(name).Count;
        }

        GUILayout.BeginHorizontal ();

        GUILayout.BeginVertical ();
        GUILayout.Label("Open as");
        InputKnob (0);
//      GUILayout.Label("Request");
        viewType = (Scaffolding.ViewType)UnityEditor.EditorGUILayout.EnumPopup(viewType, GUILayout.Width(55));
        GUILayout.EndVertical ();
//
//      for(int i = 0; i < Inputs[0].connections.Count; ++i)
//      {
//          if(Inputs[0].connections[i] != null)
//          {
//              GUILayout.Label (Inputs[0].connections[i].body.name);
//          }
//      }

        GUILayout.BeginVertical ();

        for(int i = 0; i < ButtonCount; ++i)
        {

            GUILayout.BeginHorizontal ();
            GUILayout.Label("Button:");
            Outputs [i].DisplayLayout ();
            GUILayout.EndHorizontal ();


            GUILayout.Space(10);
        }

        GUILayout.EndVertical ();
        GUILayout.EndHorizontal ();


        if (GUI.changed)
        {
            NodeEditor.RecalculateFrom (this);
        }
    }

    public override bool Calculate ()
    {
        if (Inputs[0].connections.Count > 0 && Inputs[0].connections[0] != null)
        {
            Input1Val = Inputs[0].connections[0].body.name;
        }

        return true;
    }
}

#endif