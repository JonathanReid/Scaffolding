using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Scaffolding
{
    [AddComponentMenu("Scaffolding/Backend/View Manager")]
    public class ViewManager : ViewManagerBase, IRequestable
    {

        private bool _overridingStartup = false;

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
						RequestOverlay(t);
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

		public bool IsOverlayShowing(Type view)
		{
			return _currentOverlays.ContainsKey(view);
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

		public bool IsViewShowing<T>() where T:AbstractView
		{
			return IsViewShowing(typeof(T));
		}

		public bool IsViewShowing(Type view)
		{
			return _currentScreen.GetType() == view;
		}
    }
}
