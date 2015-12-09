#if UNITY_4_6 || UNITY_5
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Scaffolding
{
	/// <summary>
	/// Scaffolding UGUI button, When using Unity 4.6 or higher, UGUI buttons are automatically used. This button type favours the UGUI event system
	/// </summary>
	public class ScaffoldingUGUIButton : AbstractButton, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler {

		protected Button _button;
		protected bool _enabled;
		private Selectable.Transition _buttonTransition;

		public override void Setup (AbstractView view)
		{
			_button = GetComponent<Button>();
			_buttonTransition = _button.transition;
			_button.onClick.RemoveListener(ButtonClicked);
			_button.onClick.AddListener(ButtonClicked);
			base.Setup (view);
		}

		/// <summary>
		/// When the button is clicked, called by .onClick from the uGUI button
		/// </summary>
		private void ButtonClicked()
		{
			if(_enabled)
			{
				ButtonPressed();
			}
		}

		/// <summary>
		/// Called when the pointer over the button is down
		/// </summary>
		/// <param name="data">Data.</param>
		public virtual void OnPointerDown(PointerEventData data)
		{
			if(_enabled)
			{
				ButtonDown();
			}
		}

		/// <summary>
		/// Called when the pointer over the button is up.
		/// </summary>
		/// <param name="data">Data.</param>
		public virtual void OnPointerUp(PointerEventData data)
		{
			if(_enabled)
			{
				RunButtonUpCallbacks();
			}
		}

		/// <summary>
		/// Called when the pointer exits the button
		/// </summary>
		/// <param name="data">Data.</param>
		public virtual void OnPointerExit(PointerEventData data)
		{
			if(_enabled)
			{
				RunButtonExitCallbacks();
			}
		}

		/// <summary>
		/// Called when the pointer enters the button
		/// </summary>
		/// <param name="data">Data.</param>
		public virtual void OnPointerEnter(PointerEventData data)
		{
			if(_enabled)
			{
				RunButtonEnterCallbacks();
			}
		}

		/// <summary>
		/// Called when the button is destroyed, cleans reference to the button
		/// </summary>
		public override void Cleanup ()
		{
			_button = null;
			base.Cleanup ();
		}

		/// <summary>
		/// Toggles the enabled state of the button. Stops events being called by the button.
		/// The button is still visually active, just not responsive.
		/// </summary>
		/// <param name="enabled">If set to <c>true</c> enabled.</param>
		public override void ToggleEnabledInput (bool enabled)
		{
			_enabled = enabled;
			_button.transition = enabled? _buttonTransition : Selectable.Transition.None;
			_button.interactable = enabled;
			base.ToggleEnabledInput(enabled);
		}

		/// <summary>
		/// Changes the visual state of the button.
		/// Inactive makes the button disabled.
		/// </summary>
		/// <param name="state">State.</param>
		public override void ChangeState (ButtonState state)
		{
			base.ChangeState (state);
			switch (state) {
			case ButtonState.Up:
				_button.interactable = true;
				break;
			case ButtonState.Down:
				_button.interactable = true;
				break;
			case ButtonState.Inactive:
				_button.interactable = false;
				break;
			}
		}

		/// <summary>
		/// Runs the button pressed callbacks
		/// </summary>
		public override void ButtonPressed ()
		{
			if(_enabled)
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
		
				base.ButtonPressed ();
			}
		}

		/// <summary>
		/// Runs the button down callbacks
		/// </summary>
		public override void ButtonDown ()
		{
			if(_enabled)
			{
				RunButtonDownCallbacks();
				base.ButtonDown ();
			}
		}

	}
}
#endif