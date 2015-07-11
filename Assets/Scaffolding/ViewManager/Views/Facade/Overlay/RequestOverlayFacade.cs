using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Scaffolding.Overlays
{
	public class RequestOverlayFacade : MonoBehaviour {

		internal Dictionary<Type, AbstractView> _currentOverlays;
		internal AbstractView _targetOverlay;
		internal SObject _currentOverlayData;

		internal Dictionary<Type, List<Type>> _disabledInputsOnView;

		internal ScaffoldingConfig _scaffoldingConfig;
		internal ViewManager _viewManager;

		public void Setup(ScaffoldingConfig config, ViewManager manager)
		{
			_scaffoldingConfig = config;
			_viewManager = manager;
		}

		/// <summary>
		/// Requests an overlay to open
		/// </summary>
		/// <param name="overlayType">Overlay type.</param>
		public AbstractView RequestOverlay(Type overlayType)
		{
			return RequestOverlay(overlayType, null);
		}
		
		/// <summary>
		/// Requests an overlay to open
		/// </summary>
		/// <param name="overlayType">Overlay type.</param>
		public AbstractView RequestOverlay<T>() where T :  AbstractView
		{
			return RequestOverlay(typeof(T),null);
		}
		
		/// <summary>
		/// Requests an overlay to open
		/// </summary>
		/// <param name="overlayType">Overlay type.</param>
		public AbstractView RequestOverlay<T>(SObject viewData) where T :  AbstractView
		{
			return RequestOverlay(typeof(T),viewData);
		}
		
		/// <summary>
		/// Requests the overlay open with data.
		/// </summary>
		/// <param name="overlayType">Overlay type.</param>
		/// <param name="data">Data.</param>
		public AbstractView RequestOverlay(Type overlayType, SObject viewData)
		{
			if (!_currentOverlays.ContainsKey(overlayType))
			{
				GameObject obj = GameObject.Instantiate(Resources.Load(_scaffoldingConfig.ViewPrefabPath(overlayType.Name)+overlayType.Name)) as GameObject;
				
				#if UNITY_4_6 || UNITY_5
				obj.transform.SetParent(_scaffoldingConfig.DetermineParentGameObjectPath().transform);
				#else
				obj.transform.parent = _scaffoldingConfig.DetermineParentGameObjectPath().transform;
				#endif
				
				obj.transform.name = overlayType.Name;
				
				_targetOverlay = obj.GetComponent<AbstractView>();

				string s = overlayType.Name + "Model";
				Type t = ScaffoldingConfig.GetType(s);
				
				if(t != null)
				{
					AbstractModel model = FindObjectOfType(t) as AbstractModel;
					if(model != null)
					{
						model.RegisterView(_targetOverlay);
					}
				}
				
				if(_scaffoldingConfig.ScaffoldingEnableAllGameobjects)
				{
					_targetOverlay.gameObject.EnableAllChildren();
				}
				_targetOverlay.Setup(this);
				obj.SetActive(false);
				
				if (viewData != null && viewData.GetBool("Scaffolding:DisableInputsOnOverlay"))
				{
					if (_disabledInputsOnView.ContainsKey(_currentScreen.GetType()))
					{
						_disabledInputsOnView[_currentScreen.GetType()].Add(overlayType);
					}
					else
					{
						List<Type> overlays = new List<Type>();
						overlays.Add(overlayType);
						_disabledInputsOnView.Add(_currentScreen.GetType(), overlays);
					}
					_currentScreen.DisableAllInputs();
				}
				
				_currentOverlays.Add(overlayType, _targetOverlay);
				OverlayOpen(_targetOverlay.GetType(), viewData); 
				
				return _targetOverlay;
			}
			else
			{
				return null;
			}
		}
		
		/// <summary>
		/// Requests the overlay to close
		/// </summary>
		/// <param name="overlayType">Overlay type.</param>
		public void RequestOverlayClose<T>() where T : AbstractView
		{
			RequestOverlayClose(typeof(T));
		}
		
		/// <summary>
		/// Requests the overlay to close
		/// </summary>
		/// <param name="overlayType">Overlay type.</param>
		public void RequestOverlayClose(Type overlayType)
		{
			if (_currentOverlays.ContainsKey(overlayType))
			{
				if(!_currentOverlays[overlayType].IsHiding)
				{
					OverlayHide(overlayType);
				}
			}
		}
		
		/// <summary>
		/// Requests the overlay to force close.
		/// This will skip the HideStart() method of the target overlay.
		/// </summary>
		/// <param name="overlayType">Overlay type.</param>
		public void RequestOverlayForceClose<T>() where T : AbstractView
		{
			RequestOverlayForceClose(typeof(T));
		}
		
		/// <summary>
		/// Requests the overlay to force close.
		/// This will skip the HideStart() method of the target overlay.
		/// </summary>
		/// <param name="overlayType">Overlay type.</param>
		public void RequestOverlayForceClose(Type overlayType)
		{
			if (_currentOverlays.ContainsKey(overlayType))
			{
				AbstractView view = _currentOverlays[overlayType];
				view.OnHideComplete();
			}
		}
		
		/// <summary>
		/// Requests all open overlays to close.
		/// </summary>
		public void RequestOverlayCloseAll()
		{   
			foreach (Type t in _currentOverlays.Keys)
			{
				RequestOverlayClose(t);
			}
		}
		
		/// <summary>
		/// Requests all open overlays to be forced close.
		/// </summary>
		public void RequestOverlayForceCloseAll()
		{   
			foreach (Type t in _currentOverlays.Keys)
			{
				AbstractView view = _currentOverlays[t];
				view.OnHideComplete();
			}
		}
		
		private void OverlayOpen(Type screenType, SObject data)
		{
			AbstractView view = _currentOverlays[screenType];
			view.gameObject.SetActive(true);
			view.OnShowStart(data);
			view.IsShowing = true;
			
			if(OverlayOpened != null)
			{
				OverlayOpened(view);
			}
		}
		
		private void OverlayHide(Type screenType)
		{
			AbstractView view = _currentOverlays[screenType];
			if(!view.IsHiding)
			{
				view.IsHiding = true;
				view.IsShowing = false;
				
				view.OnHideStart();
				if(OverlayClosed != null)
				{
					OverlayClosed(view);
				}
			}
		}
	}
}