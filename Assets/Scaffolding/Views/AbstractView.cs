using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Scaffolding
{
    [AddComponentMenu("Scaffolding/View/AbstractView")]
    [RequireComponent(typeof(Animation))]
    /// <summary>
    /// Abstract view. The base class for any view you wish to create, whether it is a Screen or Overlay.
    /// </summary>
    public abstract class AbstractView : ViewRequest
    {
		public delegate void AbstractViewEvent(AbstractView sender, SObject obj);
		public event AbstractViewEvent ViewEvent;
		public event AbstractViewEvent ViewClosedEvent;

        /************************************************
         * for inspector
         ************************************************/
        public enum ShowingTypes
        {
            WaitForPreviousHideTransition,
            ShowImmediately
        }

        [HideInInspector]
        public ShowingTypes showingType;
        [HideInInspector]
        public AnimationClip inTransition;
        [HideInInspector]
        public AnimationClip outTransition;
        [HideInInspector]
        public bool disableInputsIfOverlay;
        /************************************************
         * internals
        ************************************************/

        private Animation _animator;
        private Dictionary<string, AbstractButton> _allButtons;
        private AnimationEvent _inTransition;
        private AnimationEvent _outTransition;
        /************************************************
         * public getters
         ************************************************/

        /// <summary>
        /// Return a button that is a child of the view by name.
        /// </summary>
        public AbstractButton GetButtonForName(string name)
        {
            return _allButtons.ContainsKey(name) ? _allButtons[name] : null;
        }

        /// <summary>
        /// Adds a button pressed handler for the named button.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="handler">Handler.</param>
        public void AddButtonPressedHandler(string name, Action handler)
        {
            GetButtonForName(name).AddButtonPressedHandlerNoButton(handler);
        }

        /// <summary>
        /// Adds a button down handler for the named button.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="handler">Handler.</param>
        public void AddButtonDownHandler(string name, Action handler)
        {
            GetButtonForName(name).AddButtonDownHandlerNoButton(handler);
        }

#if UNITY_4_6 || UNITY_5
        //UNITY 4.6 ui

        /// <summary>
        /// Adds a button Up handler for the named button.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="handler">Handler.</param>
        public void AddButtonUpHandler(string name, Action handler)
        {
            GetButtonForName(name).AddButtonUpHandlerNoButton(handler);
        }
        
        /// <summary>
        /// Adds a button Exit handler for the named button.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="handler">Handler.</param>
        public void AddButtonExitHandler(string name, Action handler)
        {
            GetButtonForName(name).AddButtonExitHandlerNoButton(handler);
        }

        /// <summary>
        /// Adds a button Enter handler for the named button.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="handler">Handler.</param>
        public void AddButtonEnterHandler(string name, Action handler)
        {
            GetButtonForName(name).AddButtonEnterHandlerNoButton(handler);
        }
#endif

        /************************************************
         * private methods
         ************************************************/
        /// <summary>
        /// Internal method.
        /// Updates all the PositionItem items in the view.
        /// This makes sure all the items will be positioned correctly, no matter if the screen size changes or not.
        /// </summary>
        private void UpdatePosition()
        {
            PositionItem[] items = gameObject.GetComponentsInChildren<PositionItem>();
            foreach (PositionItem item in items)
            {
                item.SetPosition();
            }
        }
        /************************************************
         * for override
         ************************************************/
        /// <summary>
        /// Runs when the view is created.
        /// Use this instead of Awake or Start.
        /// </summary>
        public virtual void Setup(ViewManagerBase manager)
        {
            _isSettingUp = true;
            _manager = manager;
            _animator = gameObject.GetComponent<Animation>();
            if (_animator == null)
                _animator = gameObject.AddComponent<Animation>();
                
            AbstractInput[] inputs = gameObject.GetComponentsInChildren<AbstractInput>();
            _allButtons = new Dictionary<string, AbstractButton>();
            int index = 0;
            foreach (AbstractInput input in inputs)
            {
                input.Setup(this);

                if (input is AbstractButton)
                {
                    if (_allButtons.ContainsKey(input.name))
                    {
                        Debug.LogWarning("Scaffolding -- Duplicate Button Names: More than one button has the name: " + input.name + " on the view: " + this.name + " so it has been renamed");
                        input.name = input.name + index;
                    }
                    _allButtons.Add(input.name, input as AbstractButton);
                    index++;
                }
            }

            UpdatePosition();
        }

        /// <summary>
        /// Runs when the view is opened, and before any animation events have happened.
        /// Any AbstractInput derrived objects in the view are disabled at this point, to stop any clicks during the transitions.
        /// </summary>
        public virtual void OnShowStart(SObject data)
        {
            if (_isShowing)
                return;

            ToggleEnabledInputs(false);

            _isHiding = false;
            _isShowing = true;

            if (inTransition != null)
            {
                inTransition.wrapMode = WrapMode.Once;
                _manager.AddAnimationEventToTransition(this.GetType(), inTransition, "ShowComplete");
                _animator.AddClip(inTransition, inTransition.name);
                _animator.Play(inTransition.name);
            }
            else
            {
                ShowComplete();   
            }
        }

        private void ShowComplete()
        {
            _manager.ScreenShowComplete(this.GetType());
        }

        /// <summary>
        /// Runs after the "show" step has been completed, usually after any animations.
        /// Any AbstractInput derrived objects in the view are enabled at this step.
        /// </summary>
        public virtual void OnShowComplete()
        {
            ToggleEnabledInputs(true);
            _isSettingUp = false;
        }

        /// <summary>
        /// Runs when the view is requested to close, before any animations.
        /// Any AbstractInput derrived objects in the view are disabled at this point, to stop any clicks during the transitions.
        /// </summary>
        public virtual void OnHideStart()
        {
            if (_isHiding)
                return;

            ToggleEnabledInputs(false);

            _isHiding = true;
            _isShowing = false;

            if (outTransition != null)
            {
                outTransition.wrapMode = WrapMode.Once;
                _manager.AddAnimationEventToTransition(this.GetType(), outTransition, "OnHideComplete");
                _animator.AddClip(outTransition, outTransition.name);
                            
                _animator.Play(outTransition.name);
            }
            else
            {
                OnHideComplete();
            }
        }

        /// <summary>
        /// Runs when the view is completely closed. After this call, the view will be deleted.
        /// Any AbstractInput derrived objects in the view are enabled at this step.
        /// </summary>
        public virtual void OnHideComplete()
        {
            _manager.ScreenClosedComplete(this.GetType());
            _viewDataforTransitions = null;
            _manager = null;
            Destroy(gameObject);
        }

		public virtual void DispatchEventToModel(SObject obj)
		{
			if(ViewEvent != null)
			{
				ViewEvent(this, obj);
			}
		}

		public virtual void NotifyModelOfViewClosed()
		{
			if(ViewClosedEvent != null)
			{
				ViewClosedEvent(this,null);
			}
		}

        /************************************************
         * Inputs - callbacks and state changing
         ************************************************/
        /// <summary>
        /// Disable any inputs found in the view.
        /// </summary>
        public void DisableAllInputs()
        {
            ToggleEnabledInputs(false);
        }

        /// <summary>
        /// Enable any inputs found in the view.
        /// </summary>
        public void EnableAllInputs()
        {
            ToggleEnabledInputs(true);
        }

        /// <summary>
        /// Toggles the enabled state of all inputs in the view.
        /// </summary>
        /// <param name="enabled">If set to <c>true</c> enabled.</param>
        public void ToggleEnabledInputs(bool enabled)
        {
            try
            {
                AbstractInput[] inputs = transform.GetComponentsInChildren<AbstractInput>();
                foreach (AbstractInput input in inputs)
                {
                    input.ToggleEnabledInput(enabled);
                }
            }
            catch
            {

            }
        }
    }
}
