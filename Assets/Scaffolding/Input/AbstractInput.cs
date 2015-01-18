using System;
using UnityEngine;
using System.Collections.Generic;
using Scaffolding;

namespace Scaffolding
{
    [AddComponentMenu("Scaffolding/Inputs/Abstract Input")]
    /// <summary>
    /// Abstract input is responsible for all inputs responding to a touch or input from InputManager.
    /// </summary>
    public abstract class AbstractInput : MonoBehaviour
    {
        private InputManager _inputManager;
        internal AbstractView _view;
        internal Collider _collider;
        /************************************************
         * for override
         ************************************************/
        /// <summary>
        /// Run by the view during it's setup phase.
        /// </summary>
        public virtual void Setup(AbstractView view)
        {
            _view = view;
            _inputManager = GameObject.FindObjectOfType(typeof(InputManager)) as InputManager;
            
            if (_inputManager == null)
            {
                GameObject go = new GameObject();
                go.name = "InputManager";
                go.AddComponent<InputManager>();
                _inputManager = go.GetComponent<InputManager>();
            }

            _inputManager.EventPressed += HandleEventPressed;
            _inputManager.EventReleased += HandleEventReleased;
            _inputManager.EventDragged += HandleEventDragged;
            _inputManager.EventDraggedDelta += HandleEventDraggedDelta;
        }

        /// <summary>
        /// When the input is destroyed, clean up after itself.
        /// </summary>
        public virtual void OnDestroy()
        {
            Kill();
            Cleanup();
        }

        /// <summary>
        /// The inputs clean up phase.
        /// </summary>
        public virtual void Cleanup()
        {
            _view = null;
            _inputManager = null;
            _collider = null;

        }

        /// <summary>
        /// Removing any event references the input has
        /// </summary>
        public virtual void Kill()
        {
            if (_inputManager != null)
            {
                _inputManager.EventPressed -= HandleEventPressed;
                _inputManager.EventReleased -= HandleEventReleased;
                _inputManager.EventDragged -= HandleEventDragged;
                _inputManager.EventDraggedDelta -= HandleEventDraggedDelta;
            }
        }

        /// <summary>
        /// Toggles the enabled state of the collider associated with the input.
        /// </summary>
        /// <param name="enabled">If set to <c>true</c> enabled.</param>
        public virtual void ToggleEnabledInput(bool enabled)
        {
            if (_collider != null)
            {
                _collider.enabled = enabled;
            }
        }

        /// <summary>
        /// EventPressed, dispatched by Scaffoldings InputManager
        /// Passes through a InputTracker of the current touch.
        /// </summary>
        public virtual void HandleEventPressed(InputTracker tracker)
        {

        }

        /// <summary>
        /// EventReleased, dispatched by Scaffoldings InputManager
        /// Passes through a InputTracker of the current touch.
        /// </summary>
        public virtual void HandleEventReleased(InputTracker tracker)
        {

        }

        /// <summary>
        /// EventDragged, dispatched by Scaffoldings InputManager
        /// Passes through a InputTracker of the current touch.
        /// </summary>
        public virtual void HandleEventDragged(InputTracker tracker)
        {

        }

        /// <summary>
        /// EventDragged, dispatched by Scaffoldings InputManager
        /// Passes through an average delta position value of all known touches.
        /// </summary>
        public virtual void HandleEventDraggedDelta(Vector3 delta)
        {
            
        }
    }
}

