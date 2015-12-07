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
		public bool DontDestroyThisOnLoad;
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

		//scene loading
		internal bool _loadScene;
		private string _requestedSceneName;
		private LoadSceneType _loadType;

		internal Dictionary<Type, List<Type>> _disabledInputsOnView;
		internal Dictionary<Type, List<AnimationClip>> _registeredAnimationEvents;
		internal Dictionary<string, Type> _viewToOpenWithScene;
		internal Dictionary<string, Type> _overlayToOpenWithScene;
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
		/// Request a scene using a given loading type.
		/// </summary>
		/// <param name="loadType">Load type.</param>
		/// <param name="sceneName">Scene name.</param>
		public void RequestScene(LoadSceneType loadType, string sceneName)
		{
			if(!_loadScene)
			{
				_requestedSceneName = sceneName;
				_loadScene = true;
				_loadType = loadType;
				//only close screens, not overlays
				ScreenClose(_currentScreen.GetType());
			}
		}

		/// <summary>
		/// Request a scene, and open the specified view when scene finishes loading.
		/// </summary>
		/// <param name="loadType">Load type.</param>
		/// <param name="sceneName">Scene name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void RequestSceneWithView<T>(LoadSceneType loadType, string sceneName) where T : AbstractView
		{
			RequestSceneWithView(typeof(T),loadType,sceneName);
		}

		/// <summary>
		/// Request a scene, and open the specified view when scene finishes loading.
		/// </summary>
		/// <param name="viewType">View type.</param>
		/// <param name="loadType">Load type.</param>
		/// <param name="sceneName">Scene name.</param>
		public void RequestSceneWithView(Type viewType, LoadSceneType loadType, string sceneName)
		{
			OpenViewWhenSceneLoads(viewType,sceneName);
			RequestScene(loadType,sceneName);
		}

		/// <summary>
		/// Request a scene, and open the specified overlay when scene finishes loading.
		/// </summary>
		/// <param name="loadType">Load type.</param>
		/// <param name="sceneName">Scene name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void RequestSceneWithOverlay<T>(LoadSceneType loadType, string sceneName) where T : AbstractView
		{
			RequestSceneWithOverlay(typeof(T),loadType,sceneName);
		}

		/// <summary>
		/// Request a scene, and open the specified view when scene finishes loading.
		/// </summary>
		/// <param name="viewType">View type.</param>
		/// <param name="loadType">Load type.</param>
		/// <param name="sceneName">Scene name.</param>
		public void RequestSceneWithOverlay(Type viewType, LoadSceneType loadType, string sceneName)
		{
			OpenOverlayWhenSceneLoads(viewType,sceneName);
			RequestScene(loadType,sceneName);
		}

		/// <summary>
		/// Starts the loading scene. Called by HideComplete() further on down.
		/// Called when a view closes.
		/// </summary>
		private void StartLoadingScene()
		{
			switch (_loadType) {
			case LoadSceneType.Load:
				StartCoroutine(LoadScene());
				break;
			case LoadSceneType.LoadAdditive:
				StartCoroutine(LoadSceneAdditive());
				break;
			case LoadSceneType.LoadAsync:
				StartCoroutine(LoadSceneAsync());
				break;
			case LoadSceneType.LoadAdditiveAsync:
				StartCoroutine(LoadSceneAsyncAdditve());
				break;
			default:
				throw new ArgumentOutOfRangeException ();
			}

		}

		IEnumerator LoadScene()
		{
			yield return new WaitForEndOfFrame();
			Application.LoadLevel(_requestedSceneName);
			yield return new WaitForEndOfFrame();
			_loadScene = false;
			LevelWasLoaded();
		}

		IEnumerator LoadSceneAdditive()
		{
			yield return new WaitForEndOfFrame();
			Application.LoadLevelAdditive(_requestedSceneName);
			yield return new WaitForEndOfFrame();
			_loadScene = false;
			LevelWasLoaded();
		}

		IEnumerator LoadSceneAsync()
		{
			yield return new WaitForEndOfFrame();
			AsyncOperation async = Application.LoadLevelAsync(_requestedSceneName);
			yield return async;
			_loadScene = false;
			LevelWasLoaded();
		}

		IEnumerator LoadSceneAsyncAdditve()
		{
			yield return new WaitForEndOfFrame();
			AsyncOperation async = Application.LoadLevelAdditiveAsync(_requestedSceneName);
			yield return async;
			_loadScene = false;
			LevelWasLoaded();
		}

		/// <summary>
		/// Opens the view when scene loads.
		/// </summary>
		/// <param name="sceneName">Scene name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void OpenViewWhenSceneLoads<T>(string sceneName) where T : AbstractView
		{
			OpenViewWhenSceneLoads(typeof(T),sceneName);
		}

		/// <summary>
		/// Opens the view when scene loads.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="sceneName">Scene name.</param>
		public void OpenViewWhenSceneLoads(Type type, string sceneName)
		{
			if(!DontDestroyThisOnLoad)
			{
				Debug.LogWarning("Scaffolding:: You are trying to set a view to open when a scene loads, but nothing will happen as the view manager will be destroyed between scenes");
			}
			else
			{
				if(_viewToOpenWithScene == null)
				{
					_viewToOpenWithScene = new Dictionary<string, Type>();
				}

				if(_viewToOpenWithScene.ContainsKey(sceneName))
				{
					_viewToOpenWithScene[sceneName] = type;
				}
				else
				{
					_viewToOpenWithScene.Add(sceneName, type);
				}
			}
		}

		/// <summary>
		/// Opens the overlay when scene loads.
		/// </summary>
		/// <param name="sceneName">Scene name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void OpenOverlayWhenSceneLoads<T>(string sceneName) where T : AbstractView
		{
			OpenOverlayWhenSceneLoads(typeof(T),sceneName);
		}

		/// <summary>
		/// Opens the overlay when scene loads.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="sceneName">Scene name.</param>
		public void OpenOverlayWhenSceneLoads(Type type, string sceneName)
		{
			if(!DontDestroyThisOnLoad)
			{
				Debug.LogWarning("Scaffolding:: You are trying to set an overlay to open when a scene loads, but nothing will happen as the view manager will be destroyed between scenes");
			}
			else
			{
				if(_overlayToOpenWithScene == null)
				{
					_overlayToOpenWithScene = new Dictionary<string, Type>();
				}

				if(_overlayToOpenWithScene.ContainsKey(sceneName))
				{
					_overlayToOpenWithScene[sceneName] = type;
				}
				else
				{
					_overlayToOpenWithScene.Add(sceneName, type);
				}
			}
		}

		private void LevelWasLoaded() 
		{
			if(DontDestroyThisOnLoad)
			{
				if(_viewToOpenWithScene != null && _viewToOpenWithScene.ContainsKey(_requestedSceneName))
				{
					Type t = _viewToOpenWithScene[_requestedSceneName];
					_viewToOpenWithScene.Remove(_requestedSceneName);
					RequestView(t);
				}
				else if(_overlayToOpenWithScene != null && _overlayToOpenWithScene.ContainsKey(_requestedSceneName))
				{
					Type t = _overlayToOpenWithScene[_requestedSceneName];
					_overlayToOpenWithScene.Remove(_requestedSceneName);
					RequestOverlay(t);
				}
				else 
				{
					//use defaults that are in the scene!
					ScaffoldingStartingView sv = _scaffoldingConfig.GetViewDataForScene(Application.loadedLevelName);
					Type t = ScaffoldingExtensions.GetType(sv.StartingViewName);

					if (t != null)
					{
						switch(sv.StartingViewType)
						{
						case ViewType.View:
							RequestView(t);
							break;
						case ViewType.Overlay:
							RequestOverlay(t);
							break;
						}
					}
					else
					{
						Debug.LogWarning("Scaffolding:: No views or overlays have been set to open when this scene loads.");
					}
				}
			} 
		}

		/// <summary>
		/// Requests the screen to open.
		/// </summary>
		/// <param name="screenType">Screen type.</param>
		public AbstractView RequestView<T>() where T : AbstractView
		{
			return RequestView(typeof(T),null);
		}
		
		/// <summary>
		/// Requests the screen to open.
		/// </summary>
		/// <param name="screenType">Screen type.</param>
		public AbstractView RequestView(Type screenType)
		{
			return RequestView(screenType, null);
		}

		/// <summary>
		/// Requests the screen with data.
		/// </summary>
		/// <param name="screenType">Screen type.</param>
		/// <param name="data">Data.</param>
		public AbstractView RequestView<T>(SObject viewData) where T : AbstractView
		{
			return RequestView(typeof(T),viewData);
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
		public AbstractView RequestView(Type screenType, SObject data)
		{   
			if (_targetScreen == null || (_targetScreen.GetType() != screenType && !_targetScreen.IsSettingUp))
			{
				GameObject obj = GameObject.Instantiate(Resources.Load(_scaffoldingConfig.ViewPrefabPath(screenType.Name) + screenType.Name)) as GameObject;
				
				#if UNITY_4_6 || UNITY_5
				obj.transform.SetParent(_scaffoldingConfig.DetermineParentGameObjectPath().transform);
				#else
				obj.transform.parent = _scaffoldingConfig.DetermineParentGameObjectPath().transform;
				#endif
				obj.transform.name = screenType.Name;
				
				_targetScreen = obj.GetComponent<AbstractView>();
				
				string s = screenType.Name + "Model";
				Type t = ScaffoldingExtensions.GetType(s);
				
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

				return _targetScreen;
			}
			else
			{
				return null;
			}
		}
		
		private void ScreenOpen()
		{
			_targetScreen.gameObject.SetActive(true);

			_targetScreen.IsShowing = true;
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
			if(!_currentScreen.IsHiding)
			{
				_currentScreen.IsHiding = true;
				_targetScreen.IsShowing = false;
				_currentScreen.OnHideStart();
				if(ViewClosed != null)
				{
					ViewClosed(_currentScreen);
				}
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
			else if(_currentScreen.GetType() == screenType)
			{
				if(_loadScene)
				{
					StartLoadingScene();
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

		public AbstractModalPopup RequestModalPopup<T>(Action buttonOKCallback, string buttonOKText, Action buttonDismissCallback, string buttonDismissText, string bodyText) where T : AbstractModalPopup
		{
			SObject data = new SObject();
			data.AddString(AbstractModalPopup.BUTTON_OK_TEXT, buttonOKText);
			data.AddAction(AbstractModalPopup.BUTTON_OK_CALLBACK, buttonOKCallback);
			data.AddString(AbstractModalPopup.BUTTON_DISMISS_TEXT, buttonDismissText);
			data.AddAction(AbstractModalPopup.BUTTON_DISMISS_CALLBACK, buttonDismissCallback);
			data.AddString(AbstractModalPopup.BODY_TEXT, bodyText);

			return RequestOverlay(typeof(T),data) as AbstractModalPopup;
		}

		public AbstractModalPopup RequestModalPopup(Type popupType, Action buttonOKCallback, string buttonOKText, Action buttonDismissCallback, string buttonDismissText, string bodyText)
		{
			SObject data = new SObject();
			data.AddString(AbstractModalPopup.BUTTON_OK_TEXT, buttonOKText);
			data.AddAction(AbstractModalPopup.BUTTON_OK_CALLBACK, buttonOKCallback);
			data.AddString(AbstractModalPopup.BUTTON_DISMISS_TEXT, buttonDismissText);
			data.AddAction(AbstractModalPopup.BUTTON_DISMISS_CALLBACK, buttonDismissCallback);
			data.AddString(AbstractModalPopup.BODY_TEXT, bodyText);
			
			return RequestOverlay(popupType,data) as AbstractModalPopup;
		}

		public AbstractModalPopup RequestModalPopup<T>(Action buttonOKCallback, string buttonOKText, string bodyText) where T : AbstractModalPopup
		{
			SObject data = new SObject();
			data.AddString(AbstractModalPopup.BUTTON_OK_TEXT, buttonOKText);
			data.AddAction(AbstractModalPopup.BUTTON_OK_CALLBACK, buttonOKCallback);
			data.AddString(AbstractModalPopup.BODY_TEXT, bodyText);
			
			return RequestOverlay(typeof(T),data) as AbstractModalPopup;
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
				
				string s = overlayType.Name + "Model";
				Type t = ScaffoldingExtensions.GetType(s);
				
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

		public void TransitionTo<T,T1>() where T :  AbstractView where T1 : AbstractTransition
		{
			TransitionTo<T,T1>(null);
		}

		public void TransitionTo<T,T1>(SObject data) where T :  AbstractView where T1 : AbstractTransition
		{
			//Open the transtion
			//when transition is open
			//request view
			//when view has given the all clear that its loaded
			//cload the transition

			//probably needs to happen in a coroutine with callbacks.
			StartCoroutine(TransitionState(typeof(T1),typeof(T),data));
		}

		public void TransitionTo(Type view, Type transition)
		{
			TransitionTo(view, transition, null);
		}
		
		public void TransitionTo(Type view, Type transition, SObject data)
		{
			//Open the transtion
			//when transition is open
			//request view
			//when view has given the all clear that its loaded
			//cload the transition
			
			//probably needs to happen in a coroutine with callbacks.
			StartCoroutine(TransitionState(transition, view, data));
		}

		IEnumerator TransitionState(Type transitionType, Type viewType, SObject data)
		{
			_currentScreen.ToggleEnabledInputs(false);
			AbstractTransition transition = RequestOverlay(transitionType,data) as AbstractTransition;

			while(transition.TransitionStarting)
			{
				yield return new WaitForEndOfFrame();
			}

			yield return new WaitForEndOfFrame();

			_waitingForViewToLoad = true;
			_viewWaitingForLoad = viewType;
			RequestView(viewType);

			while(_waitingForViewToLoad)
			{
				yield return new WaitForEndOfFrame();
			}

			yield return new WaitForEndOfFrame();

			RequestOverlayClose(transitionType);

			_viewWaitingForLoad = null;
		}

		private bool _waitingForViewToLoad;
		private Type _viewWaitingForLoad;

		public void ViewCompletedLoading(Type t)
		{
			if(_waitingForViewToLoad && t == _viewWaitingForLoad)
			{
				_waitingForViewToLoad = false;
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