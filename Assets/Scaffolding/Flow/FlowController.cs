using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Scaffolding;

[Serializable]
public struct FlowItem
{
	public string EntryPoint;
	public string ButtonName;

	public FlowOption Options;
	public FlowPopupOption PopupOptions;
}

[Serializable]
public struct FlowOption
{
	public ViewType OpenAsType;
	public ViewOpenType OpenOrCloseView;
	public string TransitionType;
	public string PopupType;
	public string ExitPoint;
}

[Serializable]
public struct FlowPopupOption
{
	public string BodyText;
	public string YesText;
	public string NoText;
	public List<FlowOption> Options;
}

[Serializable]
public struct FlowSnapshot
{
	public string OpenView;
	public List<string> OpenOverlays;
	public string Transition;
}

public class FlowController : MonoBehaviour {

	private List<FlowItem> _flow;
	private string _viewName;
	private AbstractView _view;
	private string _buttonName;
	private List<FlowItem> _flowItem;
	private ViewManager _viewManager;

	private List<FlowSnapshot> _viewBreadcrumbs;

	// Use this for initialization
	void Start () {
		_viewManager = FindObjectOfType<ViewManager>();
		_viewBreadcrumbs = new List<FlowSnapshot>();
		ViewManager.ViewOpened += HandleViewOpened;
		_flow = ScaffoldingConfig.Instance.FlowInfo;
	}

	void HandleViewOpened (AbstractView changedView)
	{
		if(_viewBreadcrumbs.Count > 0)
		{
			FlowSnapshot snapshot = _viewBreadcrumbs[_viewBreadcrumbs.Count-1];
			//if the opened view is the same as the last snapshot, then load all the overlays it had open and remove it from the stack
			if(snapshot.OpenView == changedView.name)
			{
				for(int i = 0; i < snapshot.OpenOverlays.Count; ++i)
				{
					_viewManager.RequestOverlay( System.Type.GetType( snapshot.OpenOverlays[i] ) );
				}

				_viewBreadcrumbs.RemoveAt(_viewBreadcrumbs.Count-1);
			}
		}

		//if there is no back button in the view, clear out the history to stop it amassing memory.
		if(HasNoBackButtonInView(changedView))
		{
			_viewBreadcrumbs.Clear();
		}
	}

	private bool HasNoBackButtonInView(AbstractView view)
	{
		AbstractButton[] buttons = view.GetComponentsInChildren<AbstractButton>();
		foreach(AbstractButton b in buttons)
		{
			if(b.name.ToLower().Contains("back"))
			{
				return false;
			}
		}

		return true;
	}
	
	public void SetItem(AbstractView view, string buttonName)
	{
		_view = view;
		_viewName = view.name;
		_buttonName = buttonName;

		_flowItem = GetFlowItem(_viewName, _buttonName);

		TakeSnapshotOfViews();

		for(int i = 0; i< _flowItem.Count; ++i)
		{
			MoveNext(i);
		}
	}

	public void TakeSnapshotOfViews()
	{
		FlowSnapshot snap = new FlowSnapshot();
		if(_viewManager.CurrentScreen != null)
		{
			snap.OpenView = _viewManager.CurrentScreen.GetType().FullName;
			List<string> overlays = new List<string>();
			foreach(KeyValuePair<Type, AbstractView> kvp in _viewManager.GetCurrentOverlays())
			{
				//Ignore the special case overlays from this list.
				if(!(kvp.Value is AbstractTransition) && !(kvp.Value is AbstractModalPopup))
				{
					overlays.Add(kvp.Key.FullName);
				}
			}

			snap.Transition = _flowItem[0].Options.TransitionType;

			snap.OpenOverlays = overlays;
			_viewBreadcrumbs.Add(snap);
		}
	}

	public void RestoreSnapshot(AbstractView view, string buttonName)
	{
		_view = view;
		_viewName = view.name;
		_buttonName = buttonName;

		//if there is no snapshots in the history, make a best guess.
		if(_viewBreadcrumbs.Count == 0)
		{
			SetFlowItemToBestGuess(_viewName);
		}
		else
		{
			FlowSnapshot snapshot = _viewBreadcrumbs[_viewBreadcrumbs.Count-1];
			_flowItem = GetFlowItemWithEntryPoint(snapshot.OpenView, _viewName );

			//if there is no match in the history, make a best guess.
			if(string.IsNullOrEmpty( _flowItem[0].EntryPoint ))
			{
				SetFlowItemToBestGuess(_viewName);
			}
			else
			{
				FlowItem item = _flowItem[0];
				item.Options.ExitPoint = item.EntryPoint;
				item.Options.TransitionType = snapshot.Transition;
				_flowItem[0] = item;
			}

		}
		if(string.IsNullOrEmpty( _flowItem[0].EntryPoint ))
		{
			throw new System.FormatException("Couldn't find any view to take the view to, so bailing out!");
		}
		else
		{
			MoveNext(0);
		}
	}

	private void SetFlowItemToBestGuess(string viewName)
	{
		_flowItem = GetFlowItemWithEntryPoint( viewName );
		FlowItem item = _flowItem[0];
		item.Options.ExitPoint = item.EntryPoint;
		_flowItem[0] = item;
	}

	public bool HasInformationForView(string viewName, string button)
	{
		List<FlowItem> f = GetFlowItem(viewName, button);
		if(f.Count > 0)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public bool HasNextPointInFlow(int index)
	{
		if(!string.IsNullOrEmpty(_flowItem[index].Options.ExitPoint))
		{	
			return true;
		}
		else
		{
			return false;
		}
	}

	public Type GetNextPointInFlow(int index)
	{
		if(!string.IsNullOrEmpty(_flowItem[index].EntryPoint))
		{	
			return ScaffoldingExtensions.GetType(_flowItem[index].Options.ExitPoint);
		}
		else
		{
			return null;
		}
	}

	public ViewType GetOpenAsType(int index) 
	{
		if(!string.IsNullOrEmpty(_flowItem[index].EntryPoint))
		{
			return _flowItem[index].Options.OpenAsType;
		}
		else
		{
			return ViewType.View;
		}
	}


	public ViewOpenType OpenOrCloseView(int index) 
	{
		if(!string.IsNullOrEmpty(_flowItem[index].EntryPoint))
		{
			return _flowItem[index].Options.OpenOrCloseView;
		}
		else
		{
			return ViewOpenType.Open;
		}
	}

	public ViewOpenType OpenOrCloseModalPopup(int index, int id) 
	{
		if(!string.IsNullOrEmpty(_flowItem[index].EntryPoint))
		{
			return _flowItem[index].PopupOptions.Options[id].OpenOrCloseView;
		}
		else
		{
			return ViewOpenType.Open;
		}
	}

	public bool HasTransition(int index)
	{
		if(!string.IsNullOrEmpty(_flowItem[index].EntryPoint))
		{
			return !string.IsNullOrEmpty(_flowItem[index].Options.TransitionType);
		}
		else
		{
			return false;
		}
	}

	public Type GetTransition(int index)
	{
		if(!string.IsNullOrEmpty(_flowItem[index].EntryPoint))
		{
			return ScaffoldingExtensions.GetType(_flowItem[index].Options.TransitionType);
		}
		else
		{
			return null;
		}
	}

	//popups

	public bool HasPopup(int index)
	{
		if(!string.IsNullOrEmpty(_flowItem[index].EntryPoint))
		{
			return !string.IsNullOrEmpty(_flowItem[index].Options.PopupType);
		}
		else
		{
			return false;
		}
	}
	
	public Type GetPopup(int index)
	{
		if(!string.IsNullOrEmpty(_flowItem[index].EntryPoint))
		{
			return ScaffoldingExtensions.GetType(_flowItem[index].Options.PopupType);
		}
		else
		{
			return null;
		}
	}

	public bool HasPopup(int index, int id)
	{
		if(!string.IsNullOrEmpty(_flowItem[index].EntryPoint))
		{
			return !string.IsNullOrEmpty(_flowItem[index].PopupOptions.Options[id].PopupType);
		}
		else
		{
			return false;
		}
	}
	
	public Type GetPopup(int index, int id)
	{
		if(!string.IsNullOrEmpty(_flowItem[index].EntryPoint))
		{
			return ScaffoldingExtensions.GetType(_flowItem[index].PopupOptions.Options[id].PopupType);
		}
		else
		{
			return null;
		}
	}

	public Type GetPopupExitPoint(int index, int id)
	{
		if(!string.IsNullOrEmpty(_flowItem[index].EntryPoint))
		{
			return ScaffoldingExtensions.GetType(_flowItem[index].PopupOptions.Options[id].ExitPoint);
		}
		else
		{
			return null;
		}
	}
	
	public ViewType GetPopupOpenType(int index, int id) 
	{
		if(!string.IsNullOrEmpty(_flowItem[index].EntryPoint))
		{
			return _flowItem[index].PopupOptions.Options[id].OpenAsType;
		}
		else
		{
			return ViewType.View;
		}
	}
	
	public bool PopupOptionHasTransition(int index, int id)
	{
		if(!string.IsNullOrEmpty(_flowItem[index].EntryPoint))
		{
			return !string.IsNullOrEmpty(_flowItem[index].PopupOptions.Options[id].TransitionType);
		}
		else
		{
			return false;
		}
	}
	
	public Type GetPopupOptionTransition(int index, int id)
	{
		if(!string.IsNullOrEmpty(_flowItem[index].EntryPoint))
		{
			return ScaffoldingExtensions.GetType(_flowItem[index].PopupOptions.Options[id].TransitionType);
		}
		else
		{
			return null;
		}
	}

	public string GetPopupBodyText(int index)
	{
		return _flowItem[index].PopupOptions.BodyText;
	}

	public string GetPopupButtonText(int index, int id)
	{
		return id == 0 ? _flowItem[index].PopupOptions.YesText : _flowItem[index].PopupOptions.NoText;
	}

	private List<FlowItem> GetFlowItem(string viewName, string buttonName)
	{
		List<FlowItem> items = new List<FlowItem>();
		int i = 0, l = _flow.Count;
		for(;i<l;++i)
		{
			if(_flow[i].EntryPoint == viewName)
			{
				if(_flow[i].ButtonName == buttonName)
				{
					if(_flow[i].Options.OpenAsType == ViewType.Overlay)
					{
						items.Insert(0,_flow[i]);
					}
					else
					{
						items.Add(_flow[i]);
					}
				}
			}
		}

		return items;
	}

	private List<FlowItem> GetFlowItemWithEntryPoint(string entry, string exit)
	{
		List<FlowItem> items = new List<FlowItem>();
		int i = 0, l = _flow.Count;
		for(;i<l;++i)
		{
			if(_flow[i].Options.ExitPoint == exit)
			{
				if(_flow[i].EntryPoint == entry)
				{
					if(_flow[i].Options.OpenAsType == ViewType.Overlay)
					{
						items.Insert(0,_flow[i]);
					}
					else
					{
						items.Add(_flow[i]);
					}
				}
			}
		}
		
		return items;
	}

	private List<FlowItem> GetFlowItemWithEntryPoint(string exit)
	{
		List<FlowItem> items = new List<FlowItem>();
		int i = 0, l = _flow.Count;
		for(;i<l;++i)
		{
			if(_flow[i].Options.ExitPoint == exit)
			{
				if(_flow[i].Options.OpenAsType == ViewType.Overlay)
				{
					items.Insert(0,_flow[i]);
				}
				else
				{
					items.Add(_flow[i]);
				}
			}
		}
		
		return items;
	}

	#region button clicked, now lets move

	private void PopupOK()
	{
		MoveNextPopup(_popupIndex, 0);
	}

	private void PopupNo()
	{
		MoveNextPopup(_popupIndex, 1);
	}

	private int _popupIndex;

	private void MoveNext(int index)
	{
		switch (GetOpenAsType (index)) 
		{
		case ViewType.View:

			if(HasNextPointInFlow(index))
			{
				if(HasTransition(index))
				{
					_view.TransitionTo(GetNextPointInFlow(index),GetTransition(index) );
				}
				else
				{
					_view.RequestView( GetNextPointInFlow(index) );
				}
			}
			switch (OpenOrCloseView (index)) 
			{
			case ViewOpenType.Open:
				break;

			case ViewOpenType.Close:
				_view.RequestOverlayClose( _view.GetType() );
				break;
			}
			break;
		case ViewType.Overlay:

			if(HasNextPointInFlow(index))
			{
				if(HasPopup(index))
				{
					_popupIndex = index;
					_view.RequestModalPopup(GetPopup(index),PopupOK,GetPopupButtonText(index,0),PopupNo,GetPopupButtonText(index,1), GetPopupBodyText(index));
				}
				else
				{
					_view.RequestOverlay( GetNextPointInFlow(index) );
				}
			}

			switch (OpenOrCloseView (index)) 
			{
			case ViewOpenType.Open:

				break;

			case ViewOpenType.Close:
				_view.RequestOverlayClose( _view.GetType() );
				break;
			}
			break;
		}
	}

	private void MoveNextPopup(int index, int popupButtonID)
	{
		switch (GetPopupOpenType (index, popupButtonID)) 
		{
		case ViewType.View:
			switch (OpenOrCloseModalPopup (index, popupButtonID)) 
			{
			case ViewOpenType.Open:
				if(PopupOptionHasTransition(index, popupButtonID))
				{
					_view.TransitionTo(GetPopupExitPoint(index, popupButtonID),GetPopupOptionTransition(index, popupButtonID) );
				}
				else
				{
					_view.RequestView( GetPopupExitPoint(index, popupButtonID) );
				}
				break;
			case ViewOpenType.Close:
				_view.RequestOverlayClose( _view.GetType() );
				break;
			}
			break;
		case ViewType.Overlay:
			switch (OpenOrCloseModalPopup (index, popupButtonID)) 
			{
			case ViewOpenType.Open:
				if(HasPopup(index, popupButtonID))
				{
					_view.RequestModalPopup(GetPopup(index, popupButtonID),PopupOK,GetPopupButtonText(index, 0),PopupNo,GetPopupButtonText(index, 1), GetPopupBodyText(index));
				}
				else
				{
					_view.RequestOverlay( GetPopupExitPoint(index,popupButtonID) );
				}
				break;

			case ViewOpenType.Close:
				_view.RequestOverlayClose( _view.GetType() );
				break;
			}
			break;
		}
	}

	#endregion
}
