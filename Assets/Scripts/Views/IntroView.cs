using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Scaffolding;

public class IntroView : AbstractView {

	private InputManager _inputManager;
	private InputTracker _currentTracker;
	 
	public override void Setup(ViewManagerBase manager)
    {
        base.Setup(manager);

		_inputManager = FindObjectOfType<InputManager>();
		_inputManager.EventPressed += HandleEventPressed;
		_inputManager.EventReleased += HandleEventReleased;
		_inputManager.EventDragged += HandleEventDragged;
    }

	void HandleEventPressed (InputTracker tracker)
	{
		if(_currentTracker == null)
		{
			_currentTracker = tracker;
		}
	}

	void HandleEventDragged (InputTracker tracker)
	{
		if(tracker == _currentTracker)
		{
			//do some dragging code.
		}
	}

	void HandleEventReleased (InputTracker tracker)
	{
		if(_currentTracker == tracker)
		{
			_currentTracker = null;
		}
	}

    public override void OnShowStart(SObject data)
    {
        base.OnShowStart(data);
    }

    public override void OnShowComplete()
    {
        base.OnShowComplete();
		LoadComplete();
    }

	public override void LoadComplete ()
	{
		base.LoadComplete ();
	}

    public override void OnHideStart()
    {
        base.OnHideStart();
    }

    public override void OnHideComplete()
    {
        base.OnHideComplete();
    }
}
