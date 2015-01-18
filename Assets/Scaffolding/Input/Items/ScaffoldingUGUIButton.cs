#if UNITY_4_6 || UNITY_5_0
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Scaffolding
{
	public class ScaffoldingUGUIButton : AbstractButton, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler {

		private Button _button;
		private bool _enabled;

		public override void Setup (AbstractView view)
		{
			_button = GetComponent<Button>();
			_button.onClick.AddListener(ButtonClicked);
			base.Setup (view);
		}

		private void ButtonClicked()
		{
			if(_enabled)
			{
				ButtonPressed();
			}
		}

		public void OnPointerDown(PointerEventData data)
		{
			if(_enabled)
			{
				ButtonDown();
			}
		}

		public void OnPointerUp(PointerEventData data)
		{
			if(_enabled)
			{
				RunButtonUpCallbacks();
			}
		}

		public void OnPointerExit(PointerEventData data)
		{
			if(_enabled)
			{
				RunButtonExitCallbacks();
			}
		}

		public void OnPointerEnter(PointerEventData data)
		{
			if(_enabled)
			{
				RunButtonEnterCallbacks();
			}
		}

		public override void Cleanup ()
		{
			_button = null;
			base.Cleanup ();
		}

		public override void ToggleEnabledInput (bool enabled)
		{
			_enabled = enabled;
			base.ToggleEnabledInput(enabled);
		}

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