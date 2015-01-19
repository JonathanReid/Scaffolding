using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Scaffolding
{
	public class ViewManagerBase : MonoBehaviour, IRequestable {

		public delegate void ViewManagerEvent(AbstractView changedView);
		public static event ViewManagerEvent ViewOpened;
		public static event ViewManagerEvent ViewClosed;
		public static event ViewManagerEvent OverlayOpened;
		public static event ViewManagerEvent OverlayClosed;
		/************************************************
         * internals
         ************************************************/
		internal AbstractView _lastScreen;
		internal AbstractView _currentScreen;
		internal AbstractView _targetScreen;
		internal Type _loadingOverlay;
		internal Type _loadingOverlayTargetView;
		internal SObject _loadingOverlayTargetViewData;
		internal SObject _targetScreenData;
		internal Dictionary<Type, AbstractView> _currentOverlays;
		internal AbstractView _targetOverlay;
		internal SObject _currentOverlayData;

		internal Dictionary<Type, List<Type>> _disabledInputsOnView;
		internal Dictionary<Type, List<AnimationClip>> _registeredAnimationEvents;
		internal ScaffoldingConfig _scaffoldingConfig;

		public void AddAnimationEventToTransition(Type targetView, AnimationClip clip, string eventName)
		{
			if (_registeredAnimationEvents == null)
			{
				_registeredAnimationEvents = new Dictionary<Type, List<AnimationClip>>();
			}
			
			if (!_registeredAnimationEvents.ContainsKey(targetView))
			{
				//no keys, add a new event
				AnimationEvent evt = new AnimationEvent();
				evt.time = clip.length;
				evt.functionName = eventName;
				
				clip.AddEvent(evt);
				List<AnimationClip> clips = new List<AnimationClip>();
				clips.Add(clip);
				
				_registeredAnimationEvents.Add(targetView, clips);
			}
			else
			{
				//check if the clip has already been registered
				List<AnimationClip> clips = _registeredAnimationEvents[targetView];
				if (!clips.Contains(clip))
				{
					AnimationEvent evt = new AnimationEvent();
					evt.time = clip.length;
					evt.functionName = eventName;
					
					clip.AddEvent(evt);
					clips.Add(clip);
					_registeredAnimationEvents[targetView] = clips;
				}
			}
		}

		/// <summary>
		/// Requests the screen to open.
		/// </summary>
		/// <param name="screenType">Screen type.</param>
		public void RequestView<T>() where T : AbstractView
		{
			RequestView(typeof(T),null);
		}
		
		/// <summary>
		/// Requests the screen to open.
		/// </summary>
		/// <param name="screenType">Screen type.</param>
		public void RequestView(Type screenType)
		{
			RequestView(screenType, null);
		}
		
		/// <summary>
		/// Requests the view with a loading view.
		/// This is useful if the screen you are moving to is very heavy and takes a long time to load.
		/// You can open an overlay to mask this loading while you wait.
		/// Once the loading is complete, the overlay will close.
		/// </summary>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		/// <typeparam name="L">The 2nd type parameter.</typeparam>
		public void RequestViewWithLoadingOverlay<T,L>() where T : AbstractView where L : AbstractView
		{
			RequestViewWithLoadingOverlay(typeof(T),typeof(L),null);
		}
		
		/// <summary>
		/// Requests the view with a loading view.
		/// This is useful if the screen you are moving to is very heavy and takes a long time to load.
		/// You can open an overlay to mask this loading while you wait.
		/// Once the loading is complete, the overlay will close.
		/// </summary>
		/// <param name="screenType">Screen type.</param>
		/// <param name="loadingViewType">Loading view type.</param>
		public void RequestViewWithLoadingOverlay(Type screenType, Type loadingViewType)
		{
			RequestViewWithLoadingOverlay(screenType,loadingViewType,null);
		}
		/// <summary>
		/// Requests the view with a loading view.
		/// This is useful if the screen you are moving to is very heavy and takes a long time to load.
		/// You can open an overlay to mask this loading while you wait.
		/// Once the loading is complete, the overlay will close.
		/// </summary>
		/// <param name="viewData">View data.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		/// <typeparam name="L">The 2nd type parameter.</typeparam>
		public void RequestViewWithLoadingOverlay<T,L>(SObject viewData) where T : AbstractView where L : AbstractView
		{
			RequestViewWithLoadingOverlay(typeof(T),typeof(L),viewData);
		}
		
		/// <summary>
		/// Requests the view with a loading view.
		/// This is useful if the screen you are moving to is very heavy and takes a long time to load.
		/// You can open an overlay to mask this loading while you wait.
		/// Once the loading is complete, the overlay will close.
		/// </summary>
		/// <param name="screenType">Screen type.</param>
		/// <param name="loadingViewType">Loading view type.</param>
		/// <param name="viewData">ViewData.</param>
		public void RequestViewWithLoadingOverlay(Type screenType, Type loadingViewType, SObject viewData)
		{
			//load the loading view as an overlay, then load the screen behind it. 
			// when screen is known to be "done", close overlay.
			_loadingOverlay = loadingViewType;
			_loadingOverlayTargetView = screenType;
			_loadingOverlayTargetViewData = viewData;
			RequestOverlay(_loadingOverlay);
		}
		
		/// <summary>
		/// Requests the screen with data.
		/// </summary>
		/// <param name="screenType">Screen type.</param>
		/// <param name="data">Data.</param>
		public void RequestView<T>(SObject viewData) where T : AbstractView
		{
			RequestView(typeof(T),viewData);
		}
		
		/// <summary>
		/// Request to reopen an already open view
		/// </summary>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void RequestForceReopenView<T>() where T : AbstractView
		{
			RequestForceReopenView(typeof(T),null);
		}
		
		/// <summary>
		/// Request to reopen an already open view
		/// </summary>
		/// <param name="screenType">Screen type.</param>
		public void RequestForceReopenView(Type screenType)
		{
			RequestForceReopenView(screenType,null);
		}
		
		/// <summary>
		/// Request to reopen an already open view
		/// </summary>
		/// <param name="viewData">View data.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void RequestForceReopenView<T>(SObject viewData) where T: AbstractView
		{
			RequestForceReopenView(typeof(T),viewData);
		}
		
		/// <summary>
		/// Request to reopen an already open view
		/// </summary>
		/// <param name="screenType">Screen type.</param>
		/// <param name="viewData">View data.</param>
		public void RequestForceReopenView(Type screenType, SObject viewData)
		{
			Type tp = screenType;
			//essentially resetting the view system here.
			//resetting everything back to nothing.
			if(tp == _currentScreen.GetType())
			{
				_lastScreen = _currentScreen;
				ScreenClosedComplete(tp);
			}
			_targetScreen = null;
			_currentScreen = null;
			RequestView(tp,viewData);
		}
		
		/// <summary>
		/// Requests the screen with data.
		/// </summary>
		/// <param name="screenType">Screen type.</param>
		/// <param name="data">Data.</param>
		public void RequestView(Type screenType, SObject data)
		{   
			if (_targetScreen == null || (_targetScreen.GetType() != screenType && !_targetScreen.IsSettingUp))
			{
				GameObject obj = GameObject.Instantiate(Resources.Load(_scaffoldingConfig.ViewPrefabPath() + screenType.Name)) as GameObject;
				
				#if UNITY_4_6 || UNITY_5
				obj.transform.SetParent(_scaffoldingConfig.DetermineParentGameObjectPath().transform);
				#else
				obj.transform.parent = _scaffoldingConfig.DetermineParentGameObjectPath().transform;
				#endif
				obj.transform.name = screenType.Name;
				
				_targetScreen = obj.GetComponent<AbstractView>();
				
				string s = screenType.Name + "Model";
				Type t = System.Type.GetType(s);
				
				if(t != null)
				{
					AbstractModel model = FindObjectOfType(t) as AbstractModel;
					if(model != null)
					{
						model.RegisterView(_targetScreen);
					}
				}
				
				if(_scaffoldingConfig.ScaffoldingEnableAllGameobjects)
				{
					_targetScreen.gameObject.EnableAllChildren();
				}
				_targetScreen.Setup(this);
				
				
				obj.SetActive(false);
				
				_lastScreen = _currentScreen;
				
				_targetScreenData = data;
				
				if (_currentScreen != null)
				{
					if (_targetScreen.showingType == AbstractView.ShowingTypes.ShowImmediately)
					{
						ScreenClose(_currentScreen.GetType());
						ScreenOpen();   
					}
					else
					{
						ScreenClose(_currentScreen.GetType());
					}
				}
				else
				{
					ScreenOpen();   
				}
			}
		}
		
		private void ScreenOpen()
		{
			_targetScreen.gameObject.SetActive(true);
			
			_targetScreen.OnShowStart(_targetScreenData);
			if (_loadingOverlay != null)
			{
				RequestOverlayClose(_loadingOverlay);
				_loadingOverlay = null;
			}
			
			if(ViewOpened != null)
			{
				ViewOpened(_targetScreen);
			}
		}
		
		internal void ScreenShowComplete(Type screenType)
		{
			if (_currentOverlays.ContainsKey(screenType))
			{
				_currentOverlays[screenType].OnShowComplete();
			}
			else
			{
				_lastScreen = _currentScreen;
				_currentScreen = _targetScreen;
				_targetScreen.OnShowComplete();
			}
		}
		
		private void ScreenClose(Type screenType)
		{
			_currentScreen.OnHideStart();
			if(ViewClosed != null)
			{
				ViewClosed(_currentScreen);
			}
		}
		
		internal void ScreenClosedComplete(Type screenType)
		{
			if (_currentOverlays.ContainsKey(screenType))
			{
				Destroy(_currentOverlays[screenType].gameObject);
				_currentOverlays.Remove(screenType);
				
				if (_disabledInputsOnView.ContainsKey(_currentScreen.GetType()))
				{
					List<Type> overlays = _disabledInputsOnView[_currentScreen.GetType()];
					if (overlays.Contains(screenType))
					{
						overlays.Remove(screenType);
						if (overlays.Count == 0)
						{
							_disabledInputsOnView.Remove(_currentScreen.GetType());
							_currentScreen.EnableAllInputs();
						}
					}
				}
			}
			else if (_lastScreen != null && _lastScreen.GetType() == screenType)
			{
				if (_disabledInputsOnView.ContainsKey(_lastScreen.GetType()))
				{
					_disabledInputsOnView.Remove(_lastScreen.GetType());
				}
				Destroy(_lastScreen.gameObject);
				if(_targetScreen.showingType != AbstractView.ShowingTypes.ShowImmediately)
				{
					ScreenOpen();
				}
			}
			
			Resources.UnloadUnusedAssets();
		}
		
		/************************************************
         * Overlays
         ************************************************/
		/// <summary>
		/// Requests an overlay to open
		/// </summary>
		/// <param name="overlayType">Overlay type.</param>
		public void RequestOverlay(Type overlayType)
		{
			RequestOverlay(overlayType, null);
		}
		
		/// <summary>
		/// Requests an overlay to open
		/// </summary>
		/// <param name="overlayType">Overlay type.</param>
		public void RequestOverlay<T>() where T :  AbstractView
		{
			RequestOverlay(typeof(T),null);
		}
		
		/// <summary>
		/// Requests an overlay to open
		/// </summary>
		/// <param name="overlayType">Overlay type.</param>
		public void RequestOverlay<T>(SObject viewData) where T :  AbstractView
		{
			RequestOverlay(typeof(T),viewData);
		}
		
		/// <summary>
		/// Requests the overlay open with data.
		/// </summary>
		/// <param name="overlayType">Overlay type.</param>
		/// <param name="data">Data.</param>
		public void RequestOverlay(Type overlayType, SObject viewData)
		{
			if (!_currentOverlays.ContainsKey(overlayType))
			{
				GameObject obj = GameObject.Instantiate(Resources.Load(_scaffoldingConfig.ViewPrefabPath() + overlayType.Name)) as GameObject;
				
				#if UNITY_4_6 || UNITY_5
				obj.transform.SetParent(_scaffoldingConfig.DetermineParentGameObjectPath().transform);
				#else
				obj.transform.parent = _scaffoldingConfig.DetermineParentGameObjectPath().transform;
				#endif
				
				obj.transform.name = overlayType.Name;
				
				string s = overlayType.Name + "Model";
				Type t = System.Type.GetType(s);
				
				if(t != null)
				{
					AbstractModel model = FindObjectOfType(t) as AbstractModel;
					if(model != null)
					{
						model.RegisterView(_targetScreen);
					}
				}
				
				_targetOverlay = obj.GetComponent<AbstractView>();
				
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
				OverlayHide(overlayType);
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
			
			if(_loadingOverlay == screenType)
			{
				RequestView(_loadingOverlayTargetView,_loadingOverlayTargetViewData);
			}
			
			if(OverlayOpened != null)
			{
				OverlayOpened(view);
			}
		}
		
		private void OverlayHide(Type screenType)
		{
			AbstractView view = _currentOverlays[screenType];
			view.OnHideStart();
			if(OverlayClosed != null)
			{
				OverlayClosed(view);
			}
		}

		/************************************************
         * Model
         ************************************************/

		public void RegisterViewToModel(AbstractView view, AbstractModel model)
		{
			model.RegisterView(view);
		}

		public void UnRegisterViewFromModel(AbstractView view)
		{
			view.NotifyModelOfViewClosed();
		}
	}
}