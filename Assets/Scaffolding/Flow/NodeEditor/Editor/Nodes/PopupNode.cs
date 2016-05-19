#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using Scaffolding;

[System.Serializable]
[Node (false, "Views/Popup", false)]
public class PopupNode : Node
{
    public List<Scaffolding.ViewType> viewType;

    public const string ID = "popupNode";
    public override string GetID
    {
        get
        {
            return ID;
        }
    }

    public string Input1Val = "";

    public string[] Popups;
    public int PopupIndex;
    public List<string> Buttons = new List<string>();
    public int t = -1;
    public string Body = "BodyText";
    public string YesText = "OK";
    public string NoText = "Cancel";

    public override Node Create (Vector2 pos, string name)
    {
        PopupNode node = CreateInstance <PopupNode> ();
        viewType = new List<Scaffolding.ViewType>();

        node.name = "Modal Popup";
        node.rect = new Rect (pos.x, pos.y, 200, 200);

        node.CreateInput ("Target view", "View");
        node.CreateOutput ("Yes:", "View");
        node.CreateOutput ("No:", "View");

        return node;
    }

    public override void NodeGUI ()
    {
        MinifiableNode = true;

        if(Popups == null || Popups.Length == 0)
        {
            Popups = ScaffoldingExtensions.GetAllPopups().ToArray();
        }

        if(Popups.Length == 0)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("No popups found in project!\nPlease create a popup that extends\nAbstractModalPopup!");
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical ();
            Inputs[0].Hide();
            Outputs [0].Hide ();
            Outputs [1].Hide ();

            GUILayout.EndVertical ();
        }
        else
        {

            if(viewType == null || viewType.Count == 0)
            {
                viewType = new List<Scaffolding.ViewType>();
                viewType.Add(Scaffolding.ViewType.View);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Popup type:");
            PopupIndex = UnityEditor.EditorGUILayout.Popup(PopupIndex, Popups);

            if (t != PopupIndex && Popups.Length > 0)
            {
                Buttons = ScaffoldingExtensions.GetButtonsInView(Popups[PopupIndex]);
                t = PopupIndex;

                viewType = new List<Scaffolding.ViewType>();

                for(int i = 0; i < Buttons.Count; ++i)
                {
                    viewType.Add(Scaffolding.ViewType.View);
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal ();
            GUILayout.BeginVertical ();

            GUILayout.Label("Input");
            InputKnob (0);

            //      for(int i = 0; i < Inputs[0].connections.Count; ++i)
            //      {
            //          if(Inputs[0].connections[i] != null)
            //          {
            //              GUILayout.Label (Inputs[0].connections[i].body.name);
            //          }
            //      }


            GUILayout.Label("Body Text:");
            Body = GUILayout.TextArea(Body, GUILayout.Width(100));
            GUILayout.Label("Yes Text:");
            YesText = GUILayout.TextArea(YesText);
            GUILayout.Label("No Text:");
            NoText = GUILayout.TextArea(NoText);

            GUILayout.EndVertical ();
            GUILayout.BeginVertical ();


            for(int i = 0; i < Buttons.Count; ++i)
            {
                Outputs [i].DisplayLayout ();

                GUILayout.BeginHorizontal ();
                GUILayout.FlexibleSpace();
                viewType[i] = (Scaffolding.ViewType)UnityEditor.EditorGUILayout.EnumPopup(viewType[i], GUILayout.Width(55));

                GUILayout.EndHorizontal ();
            }



            GUILayout.EndVertical ();


            GUILayout.EndHorizontal ();
        }

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