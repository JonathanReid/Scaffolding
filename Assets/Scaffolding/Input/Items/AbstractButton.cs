using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Scaffolding
{
	public class AbstractButton : AbstractInput {

		/************************************************
         * for inspector
         ************************************************/
		public enum ButtonActionType
		{
			DoNothing,
			Open,
			Close,
		}
		
		public enum ButtonState
		{
			Up,
			Down,
			Inactive
		}

		/************************************************
         * scaffolding target view editor
         ************************************************/
		public string targetView;
		public int targetViewIndex;
		public int targetViewLength;
		/************************************************
         * scaffolding target camera editor
         ************************************************/
		public string inputCamera;
		public int inputCameraIndex;
		public int inputCameraLength;
		/************************************************
         * scaffolding editor action saving
         ************************************************/
		public string loadingOverlay;
		public bool openAsScreen = true;
		public bool disableInputsOnOverlay = false;
		public bool openLoadingOverlay = false;
		public ButtonActionType buttonActionType;
		public List<int> selectedScriptIndex;
		public List<int> selectedScriptLength;
		public List<string> selectedScript;
		public List<int> selectedMethodIndex;
		public List<string> selectedMethod;
		public List<int> selectedMethodLength;
		public AnimationClip animationClip;
		public int loadingOverlayIndex;
		public int loadingOverlayLength;

		private List<Action<AbstractButton>> _registeredInputPressedCallbacks;
		private List<Action<AbstractButton>> _registeredInputDownCallbacks;
		private List<Action> _registeredInputPressedCallbacksNoButton;
		private List<Action> _registeredInputDownCallbacksNoButton;

#if UNITY_4_6 || UNITY_5
		//unity 4.6 button events
		private List<Action<AbstractButton>> _registeredInputUpCallbacks;
		private List<Action<AbstractButton>> _registeredInputEnterCallbacks;
		private List<Action<AbstractButton>> _registeredInputExitCallbacks;
		private List<Action> _registeredInputUpCallbacksNoButton;
		private List<Action> _registeredInputEnterCallbacksNoButton;
		private List<Action> _registeredInputExitCallbacksNoButton;
#endif

		private bool _active;
		
		public override void Setup (AbstractView view)
		{
			_registeredInputDownCallbacks = new List<Action<AbstractButton>>();
			_registeredInputDownCallbacksNoButton = new List<Action>();
			_registeredInputPressedCallbacks = new List<Action<AbstractButton>>();
			_registeredInputPressedCallbacksNoButton = new List<Action>();
			base.Setup (view);
		}
		
		public override void Cleanup ()
		{
			_registeredInputDownCallbacks = null;
			_registeredInputDownCallbacksNoButton = null;
			_registeredInputPressedCallbacks = null;
			_registeredInputPressedCallbacksNoButton = null;
			base.Cleanup ();
		}

		public virtual void ButtonPressed()
		{

		}
		
		internal void OpenView()
		{
			if (targetView != null)
			{
				if (openAsScreen)
				{
					if (loadingOverlay != "")
					{
						_view.RequestViewWithLoadingOverlay(System.Type.GetType(targetView), System.Type.GetType(loadingOverlay));
					}
					else
					{
						_view.RequestView(System.Type.GetType(targetView));
					}
				}
				else
				{
					Type t = System.Type.GetType(targetView);
					_view.RequestOverlay(t,disableInputsOnOverlay);
				}
			}
		}

		/// <summary>
		/// Closes the overlay.
		/// </summary>
		internal void CloseOverlay()
		{
			if (targetView != null)
			{
				Type t = System.Type.GetType(targetView);
				_view.RequestOverlayClose (t);
			}
		}
		
		public virtual void ButtonDown()
		{

		}

		public void ToggleButtonInactive(bool active)
		{
			ToggleEnabledInput(active);
			_active = active;
			if (active)
			{
				ChangeState(ButtonState.Up);
			}
			else
			{
				ChangeState(ButtonState.Inactive);
			}
		}
		
		/************************************************
         * private methods 
         ************************************************/
		/// <summary>
		/// Changes the buttons visual state.
		/// </summary>
		/// <param name="state">State.</param>
		public virtual void ChangeState(ButtonState state)
		{
			if(!_active)
				return;
		}
		
		/// <summary>
		/// Set the Screen to be opened when the button is pressed.
		/// </summary>
		/// <param name="targetView">Target view.</param>
		public void OpenViewOnPressed(Type target)
		{
			targetView = target.Name;
			openAsScreen = true;
		}
		
		/// <summary>
		/// Set the Screen to be opened when the button is pressed.
		/// </summary>
		/// <param name="targetView">Target view.</param>
		public void OpenViewOnPressed<T>() where T : AbstractView
		{
			OpenViewOnPressed(typeof(T));
		}
		
		/// <summary>
		/// Set the screen to be opened when the button is pressed.
		/// Set the loading overlay to be opened first to mask heavy loading.
		/// </summary>
		/// <param name="targetView">Target view.</param>
		/// <param name="loadingView">Loading view.</param>
		public void OpenViewOnPressed(Type target, Type loadingView)
		{
			targetView = target.Name;
			loadingOverlay = loadingView.Name;
			openAsScreen = true;
		}
		
		/// <summary>
		/// Set the screen to be opened when the button is pressed.
		/// Set the loading overlay to be opened first to mask heavy loading.
		/// </summary>
		/// <param name="targetView">Target view.</param>
		/// <param name="loadingView">Loading view.</param>
		public void OpenViewOnPressed<T, L>() where T : AbstractView where L : AbstractView
		{
			OpenViewOnPressed(typeof(T),typeof(L));
		}
		
		/// <summary>
		/// Set the overlay to be opened when the button is pressed.
		/// </summary>
		/// <param name="targetOverlay">Target overlay.</param>
		public void OpenOverlayOnPressed(Type targetOverlay)
		{
			targetView = targetOverlay.Name;
			openAsScreen = false;
		}
		
		/// <summary>
		/// Set the overlay to be opened when the button is pressed.
		/// </summary>
		/// <param name="targetOverlay">Target overlay.</param>
		public void OpenOverlayOnPressed<T>() where T : AbstractView
		{
			OpenOverlayOnPressed(typeof(T));
		}
		
		/// <summary>
		/// Set the overlay to be closed when the button is pressed.
		/// </summary>
		/// <param name="targetOverlay">Target overlay.</param>
		public void CloseOverlayOnPressed(Type targetOverlay)
		{
			targetView = targetOverlay.Name;
			openAsScreen = false;
		}
		
		/// <summary>
		/// Set the overlay to be closed when the button is pressed.
		/// </summary>
		/// <param name="targetOverlay">Target overlay.</param>
		public void CloseOverlayOnPressed<T>() where T : AbstractView
		{
			CloseOverlayOnPressed(typeof(T));
		}

		/// <summary>
		/// Runs the button pressed callbacks.
		/// Button pressed happens only after successfully clicking on the button and releasing on the same button.
		/// </summary>
		internal void RunButtonPressedCallbacks()
		{
			RunCallbacksButton(_registeredInputPressedCallbacks);
			RunCallbacksNoButton(_registeredInputPressedCallbacksNoButton);
		}
		
		/// <summary>
		/// Runs the button down callbacks.
		/// Button down only happens on sucessful click of button.
		/// </summary>
		internal void RunButtonDownCallbacks()
		{
			RunCallbacksButton(_registeredInputDownCallbacks);
			RunCallbacksNoButton(_registeredInputDownCallbacksNoButton);
		}

#if UNITY_4_6 || UNITY_5
		/// <summary>
		/// Runs the button up callbacks.
		/// Button up only happens on sucessful click of button.
		/// </summary>
		internal void RunButtonUpCallbacks()
		{
			RunCallbacksButton(_registeredInputUpCallbacks);
			RunCallbacksNoButton(_registeredInputUpCallbacksNoButton);
		}

		/// <summary>
		/// Runs the button Enter callbacks.
		/// </summary>
		internal void RunButtonEnterCallbacks()
		{
			RunCallbacksButton(_registeredInputEnterCallbacks);
			RunCallbacksNoButton(_registeredInputEnterCallbacksNoButton);
		}

		/// <summary>
		/// Runs the button Exit callbacks.
		/// </summary>
		internal void RunButtonExitCallbacks()
		{
			RunCallbacksButton(_registeredInputExitCallbacks);
			RunCallbacksNoButton(_registeredInputExitCallbacksNoButton);
		}
#endif

		private void RunCallbacksButton(List<Action<AbstractButton>> list)
		{
			if (list != null)
			{
				int i = 0, l = list.Count;
				for (; i < l; ++i)
				{
					list[i](this);
				}
			}
		}
		
		private void RunCallbacksNoButton(List<Action> list)
		{
			if (list != null)
			{
				int i = 0, l = list.Count;
				for (; i < l; ++i)
				{
					list[i]();
				}
			}
		}
		
		/// <summary>
		/// Registers the button pressed callback.
		/// </summary>
		/// <param name="action">Method to callback after pressed happened. Needs Button as params</param>
		public void AddButtonPressedHandler(Action<AbstractButton> action)
		{
			if (_registeredInputPressedCallbacks == null)
				_registeredInputPressedCallbacks = new List<Action<AbstractButton>>();
			
			_registeredInputPressedCallbacks.Add(action);
		}
		/// <summary>
		/// Registers the button pressed callback..
		/// </summary>
		/// <param name="action">Action.</param>
		public void AddButtonPressedHandlerNoButton(Action action)
		{
			if (_registeredInputPressedCallbacksNoButton == null)
				_registeredInputPressedCallbacksNoButton = new List<Action>();
			
			_registeredInputPressedCallbacksNoButton.Add(action);
		}
		
		/// <summary>
		/// Registers the button down callback.
		/// </summary>
		/// <param name="action">Method to callback after down happened. Needs Button as params</param>
		public void AddButtonDownHandler(Action<AbstractButton> action)
		{
			if (_registeredInputDownCallbacks == null)
				_registeredInputDownCallbacks = new List<Action<AbstractButton>>();
			
			_registeredInputDownCallbacks.Add(action);
		}
		
		/// <summary>
		/// Registers the button down callback.
		/// </summary>
		/// <param name="action">Action.</param>
		public void AddButtonDownHandlerNoButton(Action action)
		{
			if (_registeredInputDownCallbacksNoButton == null)
				_registeredInputDownCallbacksNoButton = new List<Action>();
			
			_registeredInputDownCallbacksNoButton.Add(action);
		}

#if UNITY_4_6 || UNITY_5
		//UNITY 4.6 button events

		/// <summary>
		/// Registers the button up callback.
		/// </summary>
		/// <param name="action">Method to callback after up happened. Needs Button as params</param>
		public void AddButtonUpHandler(Action<AbstractButton> action)
		{
			if (_registeredInputUpCallbacks == null)
				_registeredInputUpCallbacks = new List<Action<AbstractButton>>();
			
			_registeredInputUpCallbacks.Add(action);
		}
		
		/// <summary>
		/// Registers the button up callback.
		/// </summary>
		/// <param name="action">Action.</param>
		public void AddButtonUpHandlerNoButton(Action action)
		{
			if (_registeredInputUpCallbacksNoButton == null)
				_registeredInputUpCallbacksNoButton = new List<Action>();
			
			_registeredInputUpCallbacksNoButton.Add(action);
		}

		/// <summary>
		/// Registers the button enter callback.
		/// </summary>
		/// <param name="action">Method to callback after enter happened. Needs Button as params</param>
		public void AddButtonEnterHandler(Action<AbstractButton> action)
		{
			if (_registeredInputEnterCallbacks == null)
				_registeredInputEnterCallbacks = new List<Action<AbstractButton>>();
			
			_registeredInputEnterCallbacks.Add(action);
		}
		
		/// <summary>
		/// Registers the button enter callback.
		/// </summary>
		/// <param name="action">Action.</param>
		public void AddButtonEnterHandlerNoButton(Action action)
		{
			if (_registeredInputEnterCallbacksNoButton == null)
				_registeredInputEnterCallbacksNoButton = new List<Action>();
			
			_registeredInputEnterCallbacksNoButton.Add(action);
		}

		/// <summary>
		/// Registers the button exit callback.
		/// </summary>
		/// <param name="action">Method to callback after exit happened. Needs Button as params</param>
		public void AddButtonExitHandler(Action<AbstractButton> action)
		{
			if (_registeredInputExitCallbacks == null)
				_registeredInputExitCallbacks = new List<Action<AbstractButton>>();
			
			_registeredInputExitCallbacks.Add(action);
		}
		
		/// <summary>
		/// Registers the button exit callback.
		/// </summary>
		/// <param name="action">Action.</param>
		public void AddButtonExitHandlerNoButton(Action action)
		{
			if (_registeredInputExitCallbacksNoButton == null)
				_registeredInputExitCallbacksNoButton = new List<Action>();
			
			_registeredInputExitCallbacksNoButton.Add(action);
		}
#endif
	}
}