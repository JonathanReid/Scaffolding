using UnityEngine;
using System.Collections;
using Scaffolding;

public class AutoFlowButton : ScaffoldingUGUIButton {

	private FlowController _flow;
	private bool _buttonPressed;

	public override void Setup (AbstractView view)
	{
		base.Setup (view);

		_flow = FindObjectOfType<FlowController>();
	}

	//need to set the view and button name on the flow to make looking it up less painfull.
	//really need to extract this into smaller reusable functions too.
	public override void ButtonPressed ()
	{
		base.ButtonPressed ();

		if(!_buttonPressed)
		{

			if(_flow.HasInformationForView(_view.name, transform.name))
			{
				_flow.SetItem(_view.name, transform.name);
				MoveNext();
			}
			else
			{
				if(transform.name.ToLower().Contains("back"))
				{
					_flow.RestoreSnapshot(MoveNext, _view.name, transform.name);
				}
				else
				{
					throw new System.ArgumentException("Button has no destination assigned to it in the flow");
				}
			}

			_buttonPressed = true;
		}
	}

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
		switch (_flow.GetOpenAsType ()) 
		{
		case ViewType.View:

			if(_flow.HasNextPointInFlow())
			{
				if(_flow.HasTransition())
				{
					_view.TransitionTo(_flow.GetNextPointInFlow(),_flow.GetTransition() );
				}
				else
				{
					_view.RequestView( _flow.GetNextPointInFlow() );
				}
			}
			switch (_flow.OpenOrCloseView ()) 
			{
				case ViewOpenType.Open:
					break;
					
				case ViewOpenType.Close:
					_view.RequestOverlayClose( _view.GetType() );
					break;
			}
			break;
		case ViewType.Overlay:

			if(_flow.HasNextPointInFlow())
			{
				if(_flow.HasPopup())
				{
					_view.RequestModalPopup(_flow.GetPopup(),PopupOK,_flow.GetPopupButtonText(0),PopupNo,_flow.GetPopupButtonText(1), _flow.GetPopupBodyText());
				}
				else
				{
					_view.RequestOverlay( _flow.GetNextPointInFlow() );
				}
			}

			switch (_flow.OpenOrCloseView ()) 
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
		switch (_flow.GetPopupOpenType (popupButtonID)) 
		{
		case ViewType.View:
			switch (_flow.OpenOrCloseModalPopup (popupButtonID)) 
			{
			case ViewOpenType.Open:
//				_view.RequestOverlayClose( _view.GetType() );
				if(_flow.PopupOptionHasTransition(popupButtonID))
				{
					_view.TransitionTo(_flow.GetPopupExitPoint(popupButtonID),_flow.GetPopupOptionTransition(popupButtonID) );
				}
				else
				{
					_view.RequestView( _flow.GetPopupExitPoint(popupButtonID) );
				}
				break;
			case ViewOpenType.Close:
				_view.RequestOverlayClose( _view.GetType() );
				break;
			}
			break;
		case ViewType.Overlay:
			switch (_flow.OpenOrCloseModalPopup (popupButtonID)) 
			{
			case ViewOpenType.Open:
//				_view.RequestOverlayClose( _view.GetType() );
				if(_flow.HasPopup(popupButtonID))
				{
					_view.RequestModalPopup(_flow.GetPopup(popupButtonID),PopupOK,_flow.GetPopupButtonText(0),PopupNo,_flow.GetPopupButtonText(1), _flow.GetPopupBodyText());
				}
				else
				{
					_view.RequestOverlay( _flow.GetPopupExitPoint(popupButtonID) );
				}
				break;
				
			case ViewOpenType.Close:
				_view.RequestOverlayClose( _view.GetType() );
				break;
			}
			break;
		}
		;
	}
}
