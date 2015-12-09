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
	private FlowItem _flowItem;
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

		MoveNext();
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

			snap.Transition = _flowItem.Options.TransitionType;

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
			if(string.IsNullOrEmpty( _flowItem.EntryPoint ))
			{
				SetFlowItemToBestGuess(_viewName);
			}
			else
			{
				_flowItem.Options.ExitPoint = _flowItem.EntryPoint;
				_flowItem.Options.TransitionType = snapshot.Transition;
			}

		}
		if(string.IsNullOrEmpty( _flowItem.EntryPoint ))
		{
			throw new System.FormatException("Couldn't find any view to take the view to, so bailing out!");
		}
		else
		{
			MoveNext();
		}
	}

	private void SetFlowItemToBestGuess(string viewName)
	{
		_flowItem = GetFlowItemWithEntryPoint( viewName );
		_flowItem.Options.ExitPoint = _flowItem.EntryPoint;
	}

	public bool HasInformationForView(string viewName, string button)
	{
		FlowItem f = GetFlowItem(viewName, button);
		if(!string.IsNullOrEmpty(f.EntryPoint))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public bool HasNextPointInFlow()
	{
		if(!string.IsNullOrEmpty(_flowItem.Options.ExitPoint))
		{	
			return true;
		}
		else
		{
			return false;
		}
	}

	public Type GetNextPointInFlow()
	{
		if(!string.IsNullOrEmpty(_flowItem.EntryPoint))
		{	
			return ScaffoldingExtensions.GetType(_flowItem.Options.ExitPoint);
		}
		else
		{
			return null;
		}
	}

	public ViewType GetOpenAsType() 
	{
		if(!string.IsNullOrEmpty(_flowItem.EntryPoint))
		{
			return _flowItem.Options.OpenAsType;
		}
		else
		{
			return ViewType.View;
		}
	}


	public ViewOpenType OpenOrCloseView() 
	{
		if(!string.IsNullOrEmpty(_flowItem.EntryPoint))
		{
			return _flowItem.Options.OpenOrCloseView;
		}
		else
		{
			return ViewOpenType.Open;
		}
	}

	public ViewOpenType OpenOrCloseModalPopup(int id) 
	{
		if(!string.IsNullOrEmpty(_flowItem.EntryPoint))
		{
			return _flowItem.PopupOptions.Options[id].OpenOrCloseView;
		}
		else
		{
			return ViewOpenType.Open;
		}
	}

	public bool HasTransition()
	{
		if(!string.IsNullOrEmpty(_flowItem.EntryPoint))
		{
			return !string.IsNullOrEmpty(_flowItem.Options.TransitionType);
		}
		else
		{
			return false;
		}
	}

	public Type GetTransition()
	{
		if(!string.IsNullOrEmpty(_flowItem.EntryPoint))
		{
			return ScaffoldingExtensions.GetType(_flowItem.Options.TransitionType);
		}
		else
		{
			return null;
		}
	}

	//popups

	public bool HasPopup()
	{
		if(!string.IsNullOrEmpty(_flowItem.EntryPoint))
		{
			return !string.IsNullOrEmpty(_flowItem.Options.PopupType);
		}
		else
		{
			return false;
		}
	}
	
	public Type GetPopup()
	{
		if(!string.IsNullOrEmpty(_flowItem.EntryPoint))
		{
			return ScaffoldingExtensions.GetType(_flowItem.Options.PopupType);
		}
		else
		{
			return null;
		}
	}

	public bool HasPopup(int id)
	{
		if(!string.IsNullOrEmpty(_flowItem.EntryPoint))
		{
			return !string.IsNullOrEmpty(_flowItem.PopupOptions.Options[id].PopupType);
		}
		else
		{
			return false;
		}
	}
	
	public Type GetPopup(int id)
	{
		if(!string.IsNullOrEmpty(_flowItem.EntryPoint))
		{
			return ScaffoldingExtensions.GetType(_flowItem.PopupOptions.Options[id].PopupType);
		}
		else
		{
			return null;
		}
	}

	public Type GetPopupExitPoint(int id)
	{
		if(!string.IsNullOrEmpty(_flowItem.EntryPoint))
		{
			return ScaffoldingExtensions.GetType(_flowItem.PopupOptions.Options[id].ExitPoint);
		}
		else
		{
			return null;
		}
	}
	
	public ViewType GetPopupOpenType(int id) 
	{
		if(!string.IsNullOrEmpty(_flowItem.EntryPoint))
		{
			return _flowItem.PopupOptions.Options[id].OpenAsType;
		}
		else
		{
			return ViewType.View;
		}
	}
	
	public bool PopupOptionHasTransition(int id)
	{
		if(!string.IsNullOrEmpty(_flowItem.EntryPoint))
		{
			return !string.IsNullOrEmpty(_flowItem.PopupOptions.Options[id].TransitionType);
		}
		else
		{
			return false;
		}
	}
	
	public Type GetPopupOptionTransition(int id)
	{
		if(!string.IsNullOrEmpty(_flowItem.EntryPoint))
		{
			return ScaffoldingExtensions.GetType(_flowItem.PopupOptions.Options[id].TransitionType);
		}
		else
		{
			return null;
		}
	}

	public string GetPopupBodyText()
	{
		return _flowItem.PopupOptions.BodyText;
	}

	public string GetPopupButtonText(int id)
	{
		return id == 0 ? _flowItem.PopupOptions.YesText : _flowItem.PopupOptions.NoText;
	}

	private FlowItem GetFlowItem(string viewName, string buttonName)
	{
		int i = 0, l = _flow.Count;
		for(;i<l;++i)
		{
			if(_flow[i].EntryPoint == viewName)
			{
				if(_flow[i].ButtonName == buttonName)
				{
					return _flow[i];
				}
			}
		}

		return new FlowItem();
	}

	private FlowItem GetFlowItemWithEntryPoint(string entry, string exit)
	{
		int i = 0, l = _flow.Count;
		for(;i<l;++i)
		{
			if(_flow[i].Options.ExitPoint == exit)
			{
				if(_flow[i].EntryPoint == entry)
				{
					return _flow[i];
				}
			}
		}
		
		return new FlowItem();
	}

	private FlowItem GetFlowItemWithEntryPoint(string exit)
	{
		int i = 0, l = _flow.Count;
		for(;i<l;++i)
		{
			if(_flow[i].Options.ExitPoint == exit)
			{
				return _flow[i];
			}
		}
		
		return new FlowItem();
	}

	#region button clicked, now lets move

	private void PopupOK()
	{
		MoveNextPopup(0);
	}

	private void PopupNo()
	{
		MoveNextPopup(1);
	}

	private void MoveNext()
	{
		switch (GetOpenAsType ()) 
		{
		case ViewType.View:

			if(HasNextPointInFlow())
			{
				if(HasTransition())
				{
					_view.TransitionTo(GetNextPointInFlow(),GetTransition() );
				}
				else
				{
					_view.RequestView( GetNextPointInFlow() );
				}
			}
			switch (OpenOrCloseView ()) 
			{
			case ViewOpenType.Open:
				break;

			case ViewOpenType.Close:
				_view.RequestOverlayClose( _view.GetType() );
				break;
			}
			break;
		case ViewType.Overlay:

			if(HasNextPointInFlow())
			{
				if(HasPopup())
				{
					_view.RequestModalPopup(GetPopup(),PopupOK,GetPopupButtonText(0),PopupNo,GetPopupButtonText(1), GetPopupBodyText());
				}
				else
				{
					_view.RequestOverlay( GetNextPointInFlow() );
				}
			}

			switch (OpenOrCloseView ()) 
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

	private void MoveNextPopup(int popupButtonID)
	{
		switch (GetPopupOpenType (popupButtonID)) 
		{
		case ViewType.View:
			switch (OpenOrCloseModalPopup (popupButtonID)) 
			{
			case ViewOpenType.Open:
				if(PopupOptionHasTransition(popupButtonID))
				{
					_view.TransitionTo(GetPopupExitPoint(popupButtonID),GetPopupOptionTransition(popupButtonID) );
				}
				else
				{
					_view.RequestView( GetPopupExitPoint(popupButtonID) );
				}
				break;
			case ViewOpenType.Close:
				_view.RequestOverlayClose( _view.GetType() );
				break;
			}
			break;
		case ViewType.Overlay:
			switch (OpenOrCloseModalPopup (popupButtonID)) 
			{
			case ViewOpenType.Open:
				if(HasPopup(popupButtonID))
				{
					_view.RequestModalPopup(GetPopup(popupButtonID),PopupOK,GetPopupButtonText(0),PopupNo,GetPopupButtonText(1), GetPopupBodyText());
				}
				else
				{
					_view.RequestOverlay( GetPopupExitPoint(popupButtonID) );
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
