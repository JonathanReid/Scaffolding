using UnityEngine;
using System;
using System.Collections;

namespace Scaffolding
{
	public class AbstractModalPopup : AbstractView {

		public const string BUTTON_OK_CALLBACK = "ButtonOKCallback";
		public const string BUTTON_OK_TEXT = "ButtonOKText";

		public const string BUTTON_DISMISS_CALLBACK = "ButtonDismissCallback";
		public const string BUTTON_DISMISS_TEXT = "ButtonDismissText";

		public const string BODY_TEXT = "BodyText";

		private AbstractButton _buttonOK;
		public AbstractButton ButtonOk
		{
			get
			{
				return _buttonOK;
			}
		}
		private Action _buttonOKCallback;
		private AbstractButton _buttonDismiss;
		public AbstractButton ButtonDismiss
		{
			get
			{
				return _buttonDismiss;
			}
		}
		private Action _buttonDismissCallback;

		public override void Setup (ViewManagerBase manager)
		{
			base.Setup (manager);

		}

		public override void OnShowStart (SObject data)
		{
			base.OnShowStart (data);

			if(data != null)
			{
				string buttonOKText = string.Empty;
				string buttonDismissText = string.Empty;
				string bodyText = string.Empty;

				if(data.HasKey(BUTTON_OK_CALLBACK))
				{
					_buttonOKCallback = data.GetAction(BUTTON_OK_CALLBACK);
				}

				if(data.HasKey(BUTTON_DISMISS_CALLBACK))
				{
					_buttonDismissCallback = data.GetAction(BUTTON_DISMISS_CALLBACK);
				}

				if(data.HasKey(BUTTON_OK_TEXT))
				{
					buttonOKText = data.GetString(BUTTON_OK_TEXT);
				}
				
				if(data.HasKey(BUTTON_DISMISS_TEXT))
				{
					buttonDismissText = data.GetString(BUTTON_DISMISS_TEXT);
				}

				if(data.HasKey(BODY_TEXT))
				{
					bodyText = data.GetString(BODY_TEXT);
				}

				ModalOpened(buttonOKText, buttonDismissText, bodyText);
			}
		}

		public virtual void ModalOpened(string buttonOKText, string buttonDismissText, string bodyText)
		{
			if(!string.IsNullOrEmpty(buttonOKText) && string.IsNullOrEmpty(buttonDismissText))
			{
				DisplayOneButtonPopup(buttonOKText, bodyText);
			}
			else if (!string.IsNullOrEmpty(buttonOKText) && !string.IsNullOrEmpty(buttonDismissText))
			{
				DisplayTwoButtonPopup(buttonOKText,buttonDismissText, bodyText);
			}
		}

		public virtual void DisplayOneButtonPopup(string buttonOKText, string bodyText)
		{
			AddOKButton(buttonOKText);
			SetBodyText(bodyText);
		}

		public virtual void DisplayTwoButtonPopup(string buttonOKText, string buttonDismissText, string bodyText)
		{
			AddOKButton(buttonOKText);
			AddDismissButton(buttonDismissText);
			SetBodyText(bodyText);
		}

		public virtual void AddOKButton(string buttonOKText)
		{
			_buttonOK = GetButtonForName("ButtonOK");
			if(_buttonOK != null)
			{
				_buttonOK.AddButtonPressedHandlerNoButton(ButtonOKPressed);
			}
			else
			{
				Debug.LogError("No OK button found, please name it 'ButtonOK");
			}
		}

		public virtual void AddDismissButton(string buttonDismissText)
		{
			_buttonDismiss = GetButtonForName("ButtonDismiss");
			
			if(_buttonDismiss != null)
			{
				_buttonDismiss.AddButtonPressedHandlerNoButton(ButtonDismissedPressed);
			}
			else
			{
				Debug.LogError("No Dismiss button found, please name it 'ButtonDismiss");
			}
		}

		public virtual void SetBodyText(string bodyText)
		{

		}

		private void ButtonOKPressed()
		{
			if(_buttonOKCallback != null)
			{
				_buttonOKCallback();
			}
			ClosePopup();
		}

		private void ButtonDismissedPressed()
		{
			if(_buttonDismissCallback != null)
			{
				_buttonDismissCallback();
			}
			ClosePopup();
		}

		private void ClosePopup()
		{
			RequestOverlayClose(this.GetType());
		}

		public override void OnHideComplete ()
		{
			_buttonOKCallback = null;
			_buttonDismissCallback = null;
			_buttonOK = null;
			_buttonDismiss = null;

			base.OnHideComplete ();
		}
	}
}