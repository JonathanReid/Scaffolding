using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Scaffolding.Views;
using Scaffolding.Overlays;

namespace Scaffolding
{
	public class ViewManagerBase : MonoBehaviour, IRequestable {

		public delegate void ViewManagerEvent(AbstractView changedView);
		public static event ViewManagerEvent ViewShowStart;
		public static event ViewManagerEvent ViewShowComplete;
		public static event ViewManagerEvent ViewHideStart;
		public static event ViewManagerEvent ViewHideComplete;

		public static event ViewManagerEvent OverlayShowStart;
		public static event ViewManagerEvent OverlayShowComplete;
		public static event ViewManagerEvent OverlayHideStart;
		public static event ViewManagerEvent OverlayHideComplete;
		/************************************************
         * internals
         ************************************************/
		public bool DontDestroyThisOnLoad;

		internal Type _loadingOverlay;
		internal Type _loadingOverlayTargetView;
		internal SObject _loadingOverlayTargetViewData;



		//scene loading
		internal bool _loadScene;
		private string _requestedSceneName;
		private LoadSceneType _loadType;


		internal Dictionary<Type, List<AnimationClip>> _registeredAnimationEvents;
		internal Dictionary<string, Type> _viewToOpenWithScene;
		internal Dictionary<string, Type> _overlayToOpenWithScene;
		internal ScaffoldingConfig _scaffoldingConfig;

		internal RequestViewFacade _viewFacade;

		public void Setup()
		{
			_viewFacade = gameObject.AddComponent<RequestViewFacade>();
		}

		/// <summary>
		/// Requests the screen to open.
		/// </summary>
		/// <param name="screenType">Screen type.</param>
		public AbstractView RequestView<T>() where T : AbstractView
		{
			return _viewFacade.RequestView(typeof(T),null);
		}
		
		/// <summary>
		/// Requests the screen to open.
		/// </summary>
		/// <param name="screenType">Screen type.</param>
		public AbstractView RequestView(Type screenType)
		{
			return _viewFacade.RequestView(screenType, null);
		}
		
		/// <summary>
		/// Requests the screen with data.
		/// </summary>
		/// <param name="screenType">Screen type.</param>
		/// <param name="data">Data.</param>
		public AbstractView RequestView<T>(SObject viewData) where T : AbstractView
		{
			return _viewFacade.RequestView(typeof(T),viewData);
		}
		
		/// <summary>
		/// Request to reopen an already open view
		/// </summary>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void RequestForceReopenView<T>() where T : AbstractView
		{
			_viewFacade.RequestForceReopenView(typeof(T),null);
		}
		
		/// <summary>
		/// Request to reopen an already open view
		/// </summary>
		/// <param name="screenType">Screen type.</param>
		public void RequestForceReopenView(Type screenType)
		{
			_viewFacade.RequestForceReopenView(screenType,null);
		}
		
		/// <summary>
		/// Request to reopen an already open view
		/// </summary>
		/// <param name="viewData">View data.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void RequestForceReopenView<T>(SObject viewData) where T: AbstractView
		{
			_viewFacade.RequestForceReopenView(typeof(T),viewData);
		}

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
					Type t = ScaffoldingConfig.GetType(sv.StartingViewName);

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


		
		/************************************************
         * Overlays
         ************************************************/


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

		IEnumerator TransitionState(Type transitionType, Type viewType, SObject data)
		{
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