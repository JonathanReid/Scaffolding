using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace Scaffolding
{
    /// <summary>
    /// The button class associated with the Scaffolding default buttons.
		/// Allows you to quickly add in button behaviour in your scene, such as opening a new screen or overlay.
    /// Helps facilitate a very quick flow setup.
    /// </summary>
    public class ScaffoldingButton : AbstractButton
    {
       
        /************************************************
         * scaffolding internals
         ************************************************/
        public bool created;
        public int scaffID = -1;
        /************************************************
         * internals
         ************************************************/
        private bool _hit;
        private Vector3 _clickedPos;
        private GameObject _upObj;
        private GameObject _downObj;
        private GameObject _inactiveObj;
        private Collider _upCollider;
        private Collider _downCollider;
        private ButtonState _currentState;
		private Camera _inputCamera;
        /************************************************
         * Button setup.
         ************************************************/
        /// <summary>
        /// Run by the view during it's setup phase.
        /// </summary>
        /// <param name="view">View.</param>
        public override void Setup(AbstractView view)
        {
            if (!transform.FindChild("_up"))
                Debug.LogError("Scaffolding -- Button: MISSING THE UP BUTTON STATE ON " + this.name + " ON VIEW " + view.name);
            else
                _upObj = transform.FindChild("_up").gameObject;

            if (!transform.FindChild("_down"))
                Debug.LogError("Scaffolding -- Button: MISSING THE DOWN BUTTON STATE ON " + this.name + " ON VIEW " + view.name);
            else
                _downObj = transform.FindChild("_down").gameObject;

            if (transform.FindChild("_inactive"))
                _inactiveObj = transform.FindChild("_inactive").gameObject;


            _upCollider = _upObj.GetComponentInChildren<Collider>();
            if (_upCollider == null)
                Debug.LogError("Scaffolding -- Button: No collider found on the up state of button " + this.name + " on view " + view.name);

            _downCollider = _downObj.GetComponentInChildren<Collider>();
            if (_downCollider == null)
                Debug.LogError("Scaffolding -- Button: No collider found on the down state of button " + this.name + " on view " + view.name);

            _collider = _upCollider;

            if (_collider == null)
            {
                Debug.LogError("Scaffolding -- Button: Cant find a collider to use in " + this.name + " on view " + view.name);
            }

			if(inputCamera == null)
				inputCamera = Camera.main.transform.name;

			_inputCamera = GameObject.Find(inputCamera).GetComponent<Camera>();

            if (_inputCamera == null)
            {
				_inputCamera = Camera.main;
            }

            ChangeState(ButtonState.Up);

            base.Setup(view);
        }

        /// <summary>
        /// Runs when parent view is closed, override this to clean up your extended button.
        /// </summary>
        public override void Cleanup()
        {
            _upObj = null;
            _downObj = null;
            _inactiveObj = null;
            base.Cleanup();
        }
        /************************************************
         * AbstractInput overrides.
         ************************************************/
        /// <summary>
        /// EventPressed, dispatched by Scaffoldings InputManager
        /// Passes through a InputTracker of the current touch.
        /// </summary>
        /// <param name="tracker">Tracker.</param>
        public override void HandleEventPressed(InputTracker tracker)
        {
            base.HandleEventPressed(tracker);

			if(tracker.HitGameObject(_collider.gameObject))
			{
				RunButtonDownCallbacks();
				_hit = true;
				_clickedPos = tracker.Position;
				ChangeState(ButtonState.Down);
			}
        }

        /// <summary>
        /// EventReleased, dispatched by Scaffoldings InputManager
        /// Passes through a InputTracker of the current touch.
        /// </summary>
        /// <param name="tracker">Tracker.</param>
        public override void HandleEventReleased(InputTracker tracker)
        {
            base.HandleEventReleased(tracker);

            if (_hit)
            {
				if(tracker.HitGameObject(_collider.gameObject))
				{
                    float threshold = 20;
					Vector3 pos = tracker.Position;
					if (Mathf.Abs(pos.x - _clickedPos.x) < threshold && Mathf.Abs(pos.y - _clickedPos.y) < threshold)
                    {
	                    RunButtonPressedCallbacks();
	                    
	                    switch (buttonActionType)
	                    {
	                        case ButtonActionType.Open:
	                            OpenView();
	                            break;
	                        case ButtonActionType.Close:
	                            CloseOverlay();
	                            break;
	                    }

	                    RunButtonMethods();
                    }
				}
            }
            if (_currentState == ButtonState.Down)
            {
                ChangeState(ButtonState.Up);
            }
            _hit = false;
        }

        /// <summary>
        /// Runs when view is closed or opened.
        /// Enables or Disables the button, and changes its visual state.
        /// </summary>
        public override void ToggleEnabledInput(bool enabled)
        {
            base.ToggleEnabledInput(enabled);
            _upCollider.enabled = _downCollider.enabled = enabled;
            if (enabled)
            {
                ChangeState(ButtonState.Up);
            }
        }

        /************************************************
         * private methods 
         ************************************************/
        /// <summary>
        /// Changes the buttons visual state.
        /// </summary>
        /// <param name="state">State.</param>
        public override void ChangeState(ButtonState state)
        {
			base.ChangeState(state);

            _upObj.SetActive(false);
            _downObj.SetActive(false);
            _inactiveObj.SetActive(false);
            switch (state)
            {
                case ButtonState.Up:
                    _upObj.SetActive(true);
                    _collider = _upCollider;
                    break;
                case ButtonState.Down:
                    _downObj.SetActive(true);
                    _collider = _downCollider;
                    break;
                case ButtonState.Inactive:
                    _inactiveObj.SetActive(true);
                    break;
            }
            _currentState = state;
        }

        /// <summary>
        /// Runs any methods attached to the button to run when clicked.
        /// </summary>
        private void RunButtonMethods()
        {
            int i = 0, l = selectedScript.Count;
            for (; i < l; ++i)
            {
                Type t = Type.GetType(selectedScript[i]);
                UnityEngine.Object o = GameObject.FindObjectOfType(t);
               
                if (o != null)
                {
                    MethodInfo method = t.GetMethod(selectedMethod[i]);
                    if (method.GetParameters().Length == 0)
                    {
                        method.Invoke(o, null);
                    }
                }
            }
        }

    }
}

