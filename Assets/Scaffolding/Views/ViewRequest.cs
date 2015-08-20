using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Scaffolding
{
    public class ViewRequest : MonoBehaviour, IRequestable
    {

		internal ViewManagerBase _manager;
        internal Dictionary<Type, SObject> _viewDataforTransitions;
        internal bool _isSettingUp;

        /// <summary>
        /// Gets a value indicating whether or not this view is in the hide state.
        /// </summary>
        /// <value><c>true</c> if this instance is hiding; otherwise, <c>false</c>.</value>
        internal bool IsHiding;
        
        /// <summary>
        /// Gets a value indicating whether or not this view is in the showing state.
        /// </summary>
        /// <value><c>true</c> if this instance is showing; otherwise, <c>false</c>.</value>
		internal bool IsShowing;

        public bool IsSettingUp
        {
            get
            {
                return _isSettingUp;
            }
        }

		public virtual void RegisterViewToModel(AbstractView view, AbstractModel model)
		{
			_manager.RegisterViewToModel(view,model);
		}

        /// <summary>
        /// Get the data that has been packaged for delivery to a view.
        /// </summary>
        public SObject GetViewDataForTransition(Type type)
        {
            return (_viewDataforTransitions != null && _viewDataforTransitions.ContainsKey(type)) ? _viewDataforTransitions [type] : null;
        }

		public void RequestScene(LoadSceneType loadingType, string sceneName)
		{
			_manager.RequestScene(loadingType, sceneName);
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
			_manager.RequestSceneWithView(viewType,loadType,sceneName);
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
			_manager.RequestSceneWithOverlay(viewType,loadType,sceneName);
		}
    
        /// <summary>
        /// Requests an overlay to open.
        /// </summary>
        /// <summary>
        /// Example:
        /// RequestOverlay<MyView>();
        /// </summary>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
		public virtual AbstractView RequestOverlay<T>() where T :AbstractView
        {
 	       return RequestOverlay(typeof(T));
        }
    
        /// <summary>
        /// Request an overlay with type to open.
        /// </summary>
        /// <summary>
        /// Example:
        /// RequestOverlay(typeof(MyView));
        /// </summary>
		public virtual AbstractView RequestOverlay(Type type)
        {
 	       	AbstractView v = _manager.RequestOverlay(type, GetViewDataForTransition(type));
            RemoveDataForView(type);
			return v;
        }
    
        /// <summary>
        /// Requests an overlay to open, with the option to disable inputs on the currently open screen
        /// </summary>
        /// <param name="disableInputsOnScreen">If set to <c>true</c> disable inputs on screen.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
		public virtual AbstractView RequestOverlay<T>(bool disableInputsOnScreen)
        {
 	       return RequestOverlay(typeof(T), disableInputsOnScreen);
        }

        /// <summary>
        /// Requests an overlay to open, with the option to disable inputs on the currently open screen
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="disableInputsOnScreen">If set to <c>true</c> disable inputs on screen.</param>
		public virtual AbstractView RequestOverlay(Type type, bool disableInputsOnScreen)
        {
	        SObject vo = GetViewDataForTransition(type);
	        if (vo == null)
	            vo = new SObject();
	        vo.AddBool("Scaffolding:DisableInputsOnOverlay", disableInputsOnScreen);
	        SendDataToView(type, vo);
	        return RequestOverlay(type);
        }
    
    
        /// <summary>
        /// Request an overlay to close.
        /// </summary>
        /// <summary>
        /// Example:
        /// RequestOverlayClose<MyView>();
        /// </summary>
        public virtual void RequestOverlayClose<T>() where T :AbstractView
        {
 	       RequestOverlayClose(typeof(T));
        }
    
        /// <summary>
        /// Request an overlay to close of type.
        /// </summary>
        /// <summary>
        /// Example:
        /// RequestOverlayClose(typeof(MyView));
        /// </summary>
        public virtual void RequestOverlayClose(Type type)
        {
 	       _manager.RequestOverlayClose(type);
        }
    
        /// <summary>
        /// Force an overlay to close, this skips the OnHideStart method and goes straight to HideComplete.
        /// </summary>
        /// <summary>
        /// Example:
        /// RequestOverlayForceClose<MyView>();
        /// </summary>
        public virtual void RequestOverlayForceClose<T>() where T :AbstractView
        {
 	       RequestOverlayForceClose(typeof(T));
        }
    
        /// <summary>
        /// Force an overlay to close, this skips the OnHideStart method and goes straight to HideComplete.
        /// </summary>
        /// <summary>
        /// Example:
        /// RequestOverlayForceClose(typeof(MyView));
        /// </summary>
        public virtual void RequestOverlayForceClose(Type type)
        {
 	       _manager.RequestOverlayForceClose(type);
        }
    
        /// <summary>
        /// Request a view.
        /// </summary>
        /// <summary>
        /// Example:
        /// RequestView<MyView>();
        /// </summary>
		public virtual AbstractView RequestView<T>() where T :AbstractView
        {
 	       return RequestView(typeof(T));
        }
    
        /// <summary>
        /// Request a view with type.
        /// </summary>
        /// <summary>
        /// Example:
        /// RequestView(typeof(MyView));
        /// </summary>
		public virtual AbstractView RequestView(Type type)
        {
			AbstractView v = _manager.RequestView(type, GetViewDataForTransition(type));
			RemoveDataForView(type);
			return v;
        }
    
        /// <summary>
        /// Request to reopen a view
        /// </summary>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public virtual void RequestForceReopenView<T>() where T:AbstractView
        {
 	       _manager.RequestForceReopenView<T>(GetViewDataForTransition(typeof(T)));
        }
    
        /// <summary>
        /// Request to reopen a view
        /// </summary>
        /// <param name="type">Type.</param>
        public virtual void RequestForceReopenView(Type type)
        {
 	       _manager.RequestForceReopenView(type, GetViewDataForTransition(type));
        }
    
        /************************************************
         * Adding data to a view transition
         ************************************************/
    
        /// <summary>
        /// Package up data to send to a target view.
        /// Useful to send data between views, can be packaged anytime before the change view request happens.
        /// </summary>
        /// <summary>
        /// Example:
        /// ValueObject obj = new ValueObject();
        /// obj.PutInt("Key", 10);
        /// SendDataToView<MyView>(obj);
        /// </summary>
        public void SendDataToView<T>(SObject data) where T : AbstractView
        {
            SendDataToView(typeof(T), data);
        }
    
        /// <summary>
        /// Package up data to send to a target view.
        /// Useful to send data between views, can be packaged anytime before the change view request happens.
        /// </summary>
        /// <summary>
        /// Example:
        /// ValueObject obj = new ValueObject();
        /// obj.PutInt("Key", 10);
        /// SendDataToView(typeOf(MyView),obj);
        /// </summary>
        public void SendDataToView(Type targetView, SObject data)
        {
            if (_viewDataforTransitions == null)
                _viewDataforTransitions = new Dictionary<Type, SObject>();
        
            if (_viewDataforTransitions.ContainsKey(targetView))
            {
                _viewDataforTransitions [targetView] = data;
            } else
            {
                _viewDataforTransitions.Add(targetView, data);
            }
        }
    
        /// <summary>
        /// Delete any data you want to send to a view.
        /// </summary>
        /// <param name="targetView">Target view.</param>
        public void RemoveDataForView<T>() where T : AbstractView
        {
            RemoveDataForView(typeof(T));
        }
    
        /// <summary>
        /// Delete any data you want to send to a view.
        /// </summary>
        /// <param name="targetView">Target view.</param>
        public void RemoveDataForView(Type targetView)
        {
            if (_viewDataforTransitions == null)
                return;
        
            if (_viewDataforTransitions.ContainsKey(targetView))
            {
                _viewDataforTransitions [targetView] = null;
                _viewDataforTransitions.Remove(targetView);
            }
        }

		public void TransitionTo<T,T1>() where T :  AbstractView where T1 : AbstractTransition
		{
			_manager.TransitionTo<T,T1>(GetViewDataForTransition(typeof(T1)));
		}

		public virtual AbstractModalPopup RequestModalPopup<T>(Action buttonOKCallback, string buttonOKText, Action buttonDismissCallback, string buttonDismissText, string bodyText) where T : AbstractModalPopup
		{
			return _manager.RequestModalPopup<T>(buttonOKCallback,buttonOKText,buttonDismissCallback,buttonDismissText,bodyText);
		}
		
		public virtual AbstractModalPopup RequestModalPopup<T>(Action buttonOKCallback, string buttonOKText, string bodyText) where T : AbstractModalPopup
		{
			return _manager.RequestModalPopup<T>(buttonOKCallback,buttonOKText,bodyText);
		}
    }
}