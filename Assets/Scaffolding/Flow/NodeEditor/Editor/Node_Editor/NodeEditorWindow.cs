using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using NodeEditorFramework;
using NodeEditorFramework.Resources;
using Scaffolding;
using NodeEditorFramework.Utilities;
#if UNITY_5_3
using UnityEditor.SceneManagement;
#endif

namespace NodeEditorFramework
{
	public class NodeEditorWindow : EditorWindow 
	{
		// Information about current instance
		private static NodeEditorWindow _editor;
		public static NodeEditorWindow editor
		{
			get
			{
				AssureHasEditor ();
				return _editor;
			}
		}
		public static void AssureHasEditor () 
		{
			if (_editor == null)
			{
				CreateEditor ();
				_editor.Repaint ();
			}
		}

		// Opened Canvas:
		public NodeCanvas mainNodeCanvas;
		public NodeEditorState mainEditorState;
		public static NodeCanvas MainNodeCanvas { get { return editor.mainNodeCanvas; } }
		public static NodeEditorState MainEditorState { get { return editor.mainEditorState; } }
		public static string openedCanvasPath;

		// GUI Settings
		public static int sideWindowWidth = 400;
		private static Texture iconTexture;

		[MenuItem("Tools/Scaffolding/Flow Editor")]
		public static void CreateEditor () 
		{
			NodeEditor.editorPath = ScaffoldingConfig.Instance.ScaffoldingPath + "/Flow/NodeEditor/";

			_editor = GetWindow<NodeEditorWindow> ();
			_editor.minSize = new Vector2 (800, 600);
			if(string.IsNullOrEmpty(ScaffoldingConfig.Instance.ScaffoldingFlowCanvasAsset))
			{
				ScaffoldingConfig.Instance.ScaffoldingFlowCanvasAsset = ScaffoldingConfig.Instance.ScaffoldingPath + "/Flow/NodeEditor/Resources/Saves/GameFlow.asset";
				_editor.LoadViewCanvas();
			}
			else
			{
				_editor.LoadNodeCanvas (ScaffoldingConfig.Instance.ScaffoldingFlowCanvasAsset);
			}
			NodeEditor.Repaint += _editor.Repaint;
			NodeEditor.initiated = false;

			ResourceManager.Init(NodeEditor.editorPath + "Resources/");
			iconTexture = ResourceManager.LoadTexture(EditorGUIUtility.isProSkin? "Textures/Icon_Dark.png" : "Textures/Icon_Light.png");
			_editor.titleContent = new GUIContent("Flow Editor", iconTexture);
		}

		/// <summary>
		/// Handle opening canvas when double-clicking asset
		/// </summary>
		[UnityEditor.Callbacks.OnOpenAsset(1)]
		public static bool AutoOpenCanvas (int instanceID, int line) 
		{
			if (Selection.activeObject != null && Selection.activeObject.GetType () == typeof(NodeCanvas))
			{
				string NodeCanvasPath = AssetDatabase.GetAssetPath (instanceID);
				NodeEditorWindow.CreateEditor ();
				EditorWindow.GetWindow<NodeEditorWindow> ().LoadNodeCanvas (NodeCanvasPath);
				return true;
			}
			return false;
		}

		public void OnEnable()
		{

		}

		public void OnDestroy () 
		{
			NodeEditor.Repaint -= _editor.Repaint;
		}

		#region GUI

		public void OnGUI () 
		{
			// Initiation
			NodeEditor.checkInit ();
			if (NodeEditor.InitiationError) 
			{
				GUILayout.Label ("Initiation failed! Check console for more information!");
				return;
			}
			AssureHasEditor ();

			if (mainNodeCanvas == null)
			{
				if(string.IsNullOrEmpty(ScaffoldingConfig.Instance.ScaffoldingFlowCanvasAsset))
				{
					ScaffoldingConfig.Instance.ScaffoldingFlowCanvasAsset = ScaffoldingConfig.Instance.ScaffoldingPath + "/Flow/NodeEditor/Resources/Saves/GameFlow.asset";
					LoadViewCanvas();
				}
				else
				{
					LoadNodeCanvas (ScaffoldingConfig.Instance.ScaffoldingFlowCanvasAsset);
				}
			}

			// Example of creating Nodes and Connections through code
//			CalcNode calcNode1 = NodeTypes.getDefaultNode ("calcNode").Create (new Rect (200, 200, 200, 100));
//			CalcNode calcNode2 = NodeTypes.getDefaultNode ("calcNode").Create (new Rect (600, 200, 200, 100));
//			Node.ApplyConnection (calcNode1.Outputs [0], calcNode2.Inputs [0]);



			// Specify the Canvas rect in the EditorState:
			mainEditorState.canvasRect = canvasWindowRect;
			// If you want to use GetRect:
//			Rect canvasRect = GUILayoutUtility.GetRect (600, 600);
//			if (Event.current.type != EventType.Layout)
//				mainEditorState.canvasRect = canvasRect;

			// Perform drawing with error-handling
			try
			{
				NodeEditor.DrawCanvas (mainNodeCanvas, mainEditorState);
			}
			catch (UnityException e)
			{ // on exceptions in drawing flush the canvas to avoid locking the ui.
				NewNodeCanvas ();
				Debug.LogError ("Unloaded Canvas due to exception in Draw!");
				Debug.LogException (e);
			}

			// Draw Side Window
			sideWindowWidth = 0;

			NodeEditorGUI.StartNodeGUI ();
			GUILayout.BeginArea (sideWindowRect);
			DrawSideWindow ();
			GUILayout.EndArea ();
			NodeEditorGUI.EndNodeGUI ();


		}

		public void OnLostFocus() {
			SaveNodeCanvas(ScaffoldingConfig.Instance.ScaffoldingFlowCanvasAsset);
		}

		public void DrawSideWindow () 
		{

			GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
			if (GUILayout.Button (new GUIContent ("Save Flow As", "Saves the canvas as a new Canvas Asset File in the Assets Folder"),EditorStyles.toolbarButton)) 
			{
				SaveNodeCanvas (EditorUtility.SaveFilePanelInProject ("Save Node Canvas", "Node Canvas", "asset", "Saving to a file is only needed once.", ResourceManager.resourcePath + "Saves/"));
			}
			if (GUILayout.Button (new GUIContent ("Load Flow", "Loads the canvas from a Canvas Asset File in the Assets Folder"),EditorStyles.toolbarButton)) 
			{
				string path = EditorUtility.OpenFilePanel ("Load Node Canvas", ResourceManager.resourcePath + "Saves/", "asset");
				if (!path.Contains (Application.dataPath)) 
				{
					if (path != String.Empty)
						ShowNotification (new GUIContent ("You should select an asset inside your project folder!"));
					return;
				}
				path = path.Replace (Application.dataPath, "Assets");
				LoadNodeCanvas (path);
			}
			if(GUILayout.Button(new GUIContent("Reset", "Loads a canvas from the view library"),EditorStyles.toolbarButton))
			{
				LoadViewCanvas ();
			}

			GUILayout.EndHorizontal();
		}
		
		public Rect sideWindowRect 
		{
			get { return new Rect (0, 0, position.width, 30); }
		}
		public Rect canvasWindowRect 
		{
			get { return new Rect (0, 0, position.width - sideWindowWidth, position.height); }
		}

		#endregion

		#region Save/Load
		
		/// <summary>
		/// Saves the mainNodeCanvas and it's associated mainEditorState as an asset at path
		/// </summary>
		public void SaveNodeCanvas (string path) 
		{
			NodeEditor.SaveNodeCanvas (mainNodeCanvas, path, mainEditorState);
			Repaint ();
			SaveViewFlow ();
		}
		
		/// <summary>
		/// Loads the mainNodeCanvas and it's associated mainEditorState from an asset at path
		/// </summary>
		public void LoadNodeCanvas (string path) 
		{
			// Load the NodeCanvas
			NodeCanvas nodeCanvas = NodeEditor.LoadNodeCanvas (path);
			if (nodeCanvas == null)
				return;
			mainNodeCanvas = nodeCanvas;
			
			// Load the associated MainEditorState
			List<NodeEditorState> editorStates = NodeEditor.LoadEditorStates (path);
			mainEditorState = editorStates.Find (x => x.name == "MainEditorState");
			if (mainEditorState == null)
				mainEditorState = CreateInstance<NodeEditorState> ();
			
			// Set some editor properties
			openedCanvasPath = path;

			NodeEditor.RecalculateAll (mainNodeCanvas);
			Repaint ();
		}

		public void LoadViewCanvas()
		{
			NewNodeCanvas();

			List<string> views = ScaffoldingExtensions.GetAllViews();

			int x = -300;
			int y = -200;

			StartNode startNode = (StartNode)NodeTypes.getDefaultNode("startNode").Create(new Vector2(x,y),"Starting View");
			mainNodeCanvas.nodes.Add(startNode);

			y += 150;

			for(int i = 0; i < views.Count; ++i)
			{
				ViewNode viewNode = (ViewNode)NodeTypes.getDefaultNode ("viewNode").Create (new Vector2 (x, y),views[i]);

				x += 250;
				if(i % 3 == 0 && i > 0)
				{
					y += 150;
					x = -300;
				}
				mainNodeCanvas.nodes.Add(viewNode);
			}

		}

		private void SaveViewFlow()
		{
			ScaffoldingConfig.Instance.FlowInfo = new List<FlowItem>();

			for(int i = 0; i < mainNodeCanvas.nodes.Count; ++i)
			{
				if(mainNodeCanvas.nodes[i].GetType() == typeof(StartNode))
				{
					Node node = mainNodeCanvas.nodes[i].Outputs[0].connections[0].body;
					string name = "";
					#if UNITY_5_3
					name = EditorSceneManager.GetActiveScene().name;
					#else
					name = EditorApplication.currentScene;
					#endif
					if(name != "")
					{
						name = name.Remove(0,name.LastIndexOf("/")+1);
						int index = name.LastIndexOf(".unity");
						name = name.Remove(index,name.Length - index);
						
						ScaffoldingStartingView sv = ScaffoldingConfig.Instance.GetViewDataForScene(name);
						sv.StartingViewIndex = ScaffoldingExtensions.GetAllViews().IndexOf(node.name)+1;
						sv.StartingViewName = node.name;
						sv.StartingViewType = (mainNodeCanvas.nodes[i] as StartNode).viewType[0];

						ScaffoldingConfig.Instance.SetViewDataForScene(sv);
					}
					MapPathOfView(node);
				}
			}

		}

		private FlowOption CreateFlowOption(Node nextNode, int id)
		{
			PopupNode popupNode = (nextNode as PopupNode);
			Node n = nextNode.Outputs[id].connections[0].body;

			FlowOption option = new FlowOption();
			option.OpenAsType = popupNode.viewType[id];
			option.OpenOrCloseView = (n is CloseViewNode) ? ViewOpenType.Close : ViewOpenType.Open;
			option.ExitPoint = n.name;
			if(n is TransitionNode)
			{
				option.TransitionType = (n as TransitionNode).Transitions[(n as TransitionNode).TransitionIndex];
				option.ExitPoint = n.Outputs[0].connections[0].body.name;
			}

			return option;
		}

		private void MapPathOfView(Node n)
		{
			for(int i = 0; i < n.Outputs.Count; ++i)
			{
				if(n.Outputs[i].connections.Count > 0)
				{
					FlowItem item = new FlowItem();
					Node nextNode = n.Outputs[i].connections[0].body;

					FlowOption option = new FlowOption();

					if(nextNode is PopupNode)
					{
						Node yesNode = nextNode.Outputs[0].connections[0].body;

						PopupNode popupNode = (nextNode as PopupNode);

						FlowPopupOption popupOption = new FlowPopupOption();
						popupOption.BodyText = popupNode.Body;
						popupOption.YesText = popupNode.YesText;
						popupOption.NoText = popupNode.NoText;
						option.PopupType = popupNode.Popups[popupNode.PopupIndex];

						popupOption.Options = new List<FlowOption>();
						popupOption.Options.Add(CreateFlowOption(nextNode, 0));
						popupOption.Options.Add(CreateFlowOption(nextNode, 1));

						item.PopupOptions = popupOption;

						nextNode = yesNode;
					}

					item.ButtonName = n.Outputs[i].name;
					item.EntryPoint = n.name;

					//create flow option for the base flow movement

					if(nextNode is TransitionNode)
					{
						//adding in the transition type and changing the next node to skip over the transition
						//certain nodes just facilitate a flow move and shouldnt be couldnt as a step.
						option.TransitionType = (nextNode as TransitionNode).Transitions[(nextNode as TransitionNode).TransitionIndex];
						nextNode = nextNode.Outputs[0].connections[0].body;
					}

					//setting up the movement choices, whether its a view or overlay etc.
					option.OpenAsType = (n as ViewNode).viewType[i];

					option.ExitPoint = nextNode.name;

					if(nextNode is CloseViewNode)
					{
						option.OpenOrCloseView = (nextNode is CloseViewNode) ? ViewOpenType.Close : ViewOpenType.Open;
						if(nextNode.Outputs[0].connections.Count > 0)
						{
							nextNode = nextNode.Outputs[0].connections[0].body;
							option.ExitPoint = nextNode.name;
						}
						else
						{
							option.ExitPoint = "";
						}
					}

					item.Options = option;
					//if theres an identical flow item, bail outta here!
					if(IsFlowInfoUnique(item))
					{
						ScaffoldingConfig.Instance.FlowInfo.Add(item);
						MapPathOfView(nextNode);
					}
					else
					{

					}
				}
			}
		}

		private bool IsFlowInfoUnique(FlowItem item)
		{
			for(int i = 0; i < ScaffoldingConfig.Instance.FlowInfo.Count; ++i)
			{
				FlowItem compare = ScaffoldingConfig.Instance.FlowInfo[i];
				if(item.ButtonName == compare.ButtonName &&
				   item.EntryPoint == compare.EntryPoint &&
				   item.Options.ExitPoint == compare.Options.ExitPoint)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Creates and opens a new empty node canvas
		/// </summary>
		public void NewNodeCanvas () 
		{
			Debug.Log("NEW");
			// New NodeCanvas
			mainNodeCanvas = CreateInstance<NodeCanvas> ();
			mainNodeCanvas.name = "New Canvas";
			// New NodeEditorState
			mainEditorState = CreateInstance<NodeEditorState> ();
			mainEditorState.canvas = mainNodeCanvas;
			mainEditorState.name = "MainEditorState";
			// Set some properties
			openedCanvasPath = "";
		}
		
		#endregion
	}
}