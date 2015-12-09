using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Scaffolding
{
	[ExecuteInEditMode]
	public class AutoFlowButtonGroup : MonoBehaviour
	{
		public ScaffoldingUGUIButton ButtonPrefab;
		public int AmountOfButtons;

		private int _amountOfButtons;
		private ScaffoldingUGUIButton _buttonPrefab;
		private List<Action<AbstractButton>> _buttonPressedCallback;
		private FlowController _flow;
		private AbstractView _view;

		public void Setup(AbstractView view)
		{
			_buttonPressedCallback = new List<Action<AbstractButton>>();
			for(int i = 0; i < AmountOfButtons; ++i)
			{
				ScaffoldingUGUIButton button = null;
				Transform t = transform.FindChild(ButtonPrefab.name + (i+1));
				if(t == null)
				{
					GameObject go = GameObject.Instantiate(ButtonPrefab.gameObject) as GameObject;
					go.transform.SetParent(transform);
					go.name = ButtonPrefab.name + (i+1);
					go.transform.localScale = Vector3.one;
					button = go.GetComponent<ScaffoldingUGUIButton>();
					button.Setup(view);
				}
				else
				{
					button = t.GetComponent<ScaffoldingUGUIButton>();
				}

				if(Application.isPlaying)
				{
					button.AddButtonPressedHandler(ButtonPressed);
				}
			}

			Debug.Log(view);
			_view = view;
			_flow = FindObjectOfType<FlowController>();
		}

		public void RegisterForButtonPressedCallbacks(Action<AbstractButton> callback)
		{
			_buttonPressedCallback.Add(callback);
		}

		private void Update()
		{
			if(!Application.isPlaying)
			{
				if(_amountOfButtons != AmountOfButtons || _buttonPrefab != ButtonPrefab)
				{
					ScaffoldingUGUIButton[] buttons = GetComponentsInChildren<ScaffoldingUGUIButton>(); 
					foreach(ScaffoldingUGUIButton t in buttons)
					{
						DestroyImmediate(t.gameObject);
					}
					Setup(null);
					_amountOfButtons = AmountOfButtons;
					_buttonPrefab = ButtonPrefab;
				}
			}
		}

		private void ButtonPressed(AbstractButton button)
		{
			for(int i = 0; i < _buttonPressedCallback.Count; ++i)
			{
				_buttonPressedCallback[i](button);
			}

			if(_flow.HasInformationForView(_view.name, transform.name))
			{
				_flow.SetItem(_view, transform.name);
			}
			else
			{
				if(transform.name.ToLower().Contains("back"))
				{
					_flow.RestoreSnapshot(_view, transform.name);
				}
				else
				{
					throw new System.ArgumentException("Button has no destination assigned to it in the flow");
				}
			}
		}
	}
}