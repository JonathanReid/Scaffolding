
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Scaffolding
{
    [AddComponentMenu("Scaffolding/Backend/View Manager")]
    public class ViewManager : MonoBehaviour
    {
		public delegate void ViewManagerEvent(AbstractView changedView);
		public static event ViewManagerEvent ViewOpened;
		public static event ViewManagerEvent ViewClosed;
		public static event ViewManagerEvent OverlayOpened;
		public static event ViewManagerEvent OverlayClosed;
        /************************************************
         * internals
         ************************************************/
        private AbstractView _lastScreen;
        private AbstractView _currentScreen;
        private AbstractView _targetScreen;
        private Type _loadingOverlay;
        private Type _loadingOverlayTargetView;
        private SObject _loadingOverlayTargetViewData;
        private SObject _targetScreenData;
        private Dictionary<Type, AbstractView> _currentOverlays;
        private AbstractView _targetOverlay;
        private SObject _currentOverlayData;
        private bool _overridingStartup = false;
        private Dictionary<Type, List<Type>> _disabledInputsOnView;
        private Dictionary<Type, List<AnimationClip>> _registeredAnimationEvents;
		private ScaffoldingConfig _scaffoldingConfig;
        /************************************************
         * Setup
         ************************************************/
        /// <summary>
        /// Overrides the startup sequence.
        /// </summary>
        /// <returns>The callback to contiue the startup sequence when you are finished.</returns>
        public Action OverrideStartupSequence()
        {
            _overridingStartup = true;
            return Init;
        }

        protected void Start()
        {
            if (!_overridingStartup)
            {
                Init();
            }
        }

        /// <summary>
        /// The startup sequence. Scaffolding starts from here.
        /// </summary>
        public void Init()
        {
			_scaffoldingConfig = Resources.Load<ScaffoldingConfig>("SCConfig");
            _disabledInputsOnView = new Dictionary<Type, List<Type>>();

            _currentOverlays = new Dictionary<Type, AbstractView>();

			Type t = System.Type.GetType(_scaffoldingConfig.StartingView);

            if (t != null && GameObject.FindObjectOfType<AbstractView>() == null)
            {
                switch(_scaffoldingConfig.StartingViewType)
				{
					case ViewType.View:
						RequestView(t);
						break;
					case ViewType.Overlay:
						RequestOverlayOpen(t);
						break;
				}
            }
            else
            {
                AbstractView v = GameObject.FindObjectOfType<AbstractView>();
                if (v != null)
                {
					Destroy(v.gameObject);
                    RequestView(v.GetType());
                }
                else
                {
                    Debug.LogWarning("Scaffolding -- ViewManager: No views assigned to start with!");
                }
            }
            
        }
        /************************************************
         * public getters
         ************************************************/
        /// <summary>
        /// Gets the last Screen.
        /// </summary>
        /// <value>The last Screen.</value>
        public AbstractView LastScreen
        {
            get
            {
                return _lastScreen;
            }
        }

        /// <summary>
        /// Gets the current screen.
        /// </summary>
        /// <value>The current screen.</value>
        public AbstractView CurrentScreen
        {
            get
            {
                return _currentScreen;
            }
        }

        /// <summary>
        /// Gets the target screen.
        /// </summary>
        /// <value>The target screen.</value>
        public AbstractView TargetScreen
        {
            get
            {
                return _targetScreen;
            }
        }

        /// <summary>
        /// Gets the current overlays.
        /// </summary>
        /// <returns>The current overlays.</returns>
        public Dictionary<Type,AbstractView> GetCurrentOverlays()
        {
            return _currentOverlays;
        }

        /// <summary>
        /// Determines whether this instance of the overlay is showing.
        /// </summary>
        /// <returns><c>true</c> if this instance is overlay showing; otherwise, <c>false</c>.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
		public bool IsOverlayShowing<T>() where T:AbstractView
		{
			Type tp = typeof(T);
			return _currentOverlays.ContainsKey(tp);
		}

        /// <summary>
        /// Determines whether the type is an overlay.
        /// </summary>
        /// <returns><c>true</c> if the type is an overlay, otherwise; <c>false</c>.</returns>
        /// <param name="type">Type.</param>
        public bool IsViewAnOverlay(Type type)
        {
            return _currentOverlays.ContainsKey(type);
        }
        /************************************************
         * Screens
         ************************************************/
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
		public void RequestViewWithLoadingView<T,L>() where T : AbstractView where L : AbstractView
		{
			RequestViewWithLoadingView(typeof(T),typeof(L),null);
		}

		/// <summary>
		/// Requests the view with a loading view.
		/// This is useful if the screen you are moving to is very heavy and takes a long time to load.
		/// You can open an overlay to mask this loading while you wait.
		/// Once the loading is complete, the overlay will close.
		/// </summary>
		/// <param name="screenType">Screen type.</param>
		/// <param name="loadingViewType">Loading view type.</param>
		public void RequestViewWithLoadingView(Type screenType, Type loadingViewType)
		{
			RequestViewWithLoadingView(screenType,loadingViewType,null);
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
		public void RequestViewWithLoadingView<T,L>(SObject viewData) where T : AbstractView where L : AbstractView
		{
			RequestViewWithLoadingView(typeof(T),typeof(L),viewData);
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
        public void RequestViewWithLoadingView(Type screenType, Type loadingViewType, SObject viewData)
		{
            //load the loading view as an overlay, then load the screen behind it. 
            // when screen is known to be "done", close overlay.
            _loadingOverlay = loadingViewType;
            _loadingOverlayTargetView = screenType;
            _loadingOverlayTargetViewData = viewData;
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
		public void RequestViewForceReopen<T>() where T : AbstractView
		{
			RequestViewForceReopen(typeof(T),null);
		}

        /// <summary>
        /// Request to reopen an already open view
        /// </summary>
        /// <param name="screenType">Screen type.</param>
		public void RequestViewForceReopen(Type screenType)
		{
			RequestViewForceReopen(screenType,null);
		}

        /// <summary>
        /// Request to reopen an already open view
        /// </summary>
        /// <param name="viewData">View data.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
		public void RequestViewForceReopen<T>(SObject viewData) where T: AbstractView
		{
			RequestViewForceReopen(typeof(T),viewData);
		}

        /// <summary>
        /// Request to reopen an already open view
        /// </summary>
        /// <param name="screenType">Screen type.</param>
        /// <param name="viewData">View data.</param>
		public void RequestViewForceReopen(Type screenType, SObject viewData)
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
				UnlinkViewFromModel(screenType);
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
				UnlinkViewFromModel(screenType);
                Destroy(_lastScreen.gameObject);
				if(_targetScreen.showingType != AbstractView.ShowingTypes.ShowImmediately)
				{
                	ScreenOpen();
				}
            }

            Resources.UnloadUnusedAssets();
        }

		private void UnlinkViewFromModel(Type screenType)
		{
			string s = screenType.Name + "Model";
			Type t = System.Type.GetType(s);
			
			if(t != null)
			{
				AbstractModel model = FindObjectOfType(t) as AbstractModel;
				if(model != null)
				{
					model.ViewClosed();
				}
			}
		}

        /************************************************
         * Overlays
         ************************************************/
        /// <summary>
        /// Requests an overlay to open
        /// </summary>
        /// <param name="overlayType">Overlay type.</param>
        public void RequestOverlayOpen(Type overlayType)
        {
            RequestOverlayOpen(overlayType, null);
        }

		/// <summary>
		/// Requests an overlay to open
		/// </summary>
		/// <param name="overlayType">Overlay type.</param>
		public void RequestOverlayOpen<T>() where T :  AbstractView
		{
			RequestOverlayOpen(typeof(T),null);
		}

		/// <summary>
		/// Requests an overlay to open
		/// </summary>
		/// <param name="overlayType">Overlay type.</param>
		public void RequestOverlayOpen<T>(SObject viewData) where T :  AbstractView
		{
			RequestOverlayOpen(typeof(T),viewData);
		}

        /// <summary>
        /// Requests the overlay open with data.
        /// </summary>
        /// <param name="overlayType">Overlay type.</param>
        /// <param name="data">Data.</param>
        public void RequestOverlayOpen(Type overlayType, SObject viewData)
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
    }
}
