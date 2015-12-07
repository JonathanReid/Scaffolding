using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Scaffolding;

namespace Scaffolding
{
    [AddComponentMenu("Scaffolding/Inputs/Drag This Item Input")]
    /// <summary>
    /// DragThisInput attaches to a gameobject as a quick solution to get something draggable.
    /// Extends AbstractInput so falls under the same rules as other inputs in regards to views.
    /// E.g colliders get disabled during OnShowStart and OnHideStart phases.
    /// </summary>
    public class DragThisInput : DragInput
    {
        public string inputCamera;
        public int inputCameraIndex;
        public int inputCameraLength;
        public int axisDropDownIndex;
        private List<int> _inputs;
		private Camera _inputCamera;
        /************************************************
         * DragThisInput setup.
         ************************************************/
        /// <summary>
        /// Run by the view during it's setup phase.
        /// </summary>
        /// <param name="view">View.</param>
        public override void Setup(AbstractView view)
        {
            base.Setup(view);
            _inputs = new List<int>();

            RegisterDragCallback(HandleDraggedCallback);

            _collider = gameObject.GetComponent<Collider>();
            if (_collider == null)
            {
                _collider = gameObject.AddComponent<BoxCollider>();
            }

			_inputCamera = GameObject.Find(inputCamera).GetComponent<Camera>();

			if (_inputCamera == null)
            {
				_inputCamera = Camera.main;
            }
        }
        /************************************************
         * Public methods.
         ************************************************/
        /// <summary>
        /// Handles the dragged callback.
        /// </summary>
        /// <param name="delta">Delta.</param>
        public void HandleDraggedCallback(Vector3 delta)
        {
            transform.position += delta;
        }
        /************************************************
         * AbstractInput overrides.
         ************************************************/
        /// <summary>
        /// EventPressed, dispatched by Scaffoldings InputManager
        /// Passes through a InputTracker of the current touch.
        /// </summary>
        /// <param name="tracker">Tracker.</param>
        public override void HandleEventPressed(InputTracker tracker)
        {
            base.HandleEventPressed(tracker);
			Ray ray = _inputCamera.ScreenPointToRay(tracker.Position);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity))
            {
                if (hitInfo.collider == _collider)
                {
                    if (!_inputs.Contains(tracker.FingerId))
                    {
                        _inputs.Add(tracker.FingerId);
                    }
                }
            }
        }

        /// <summary>
        /// EventReleased, dispatched by Scaffoldings InputManager
        /// Passes through a InputTracker of the current touch.
        /// </summary>
        /// <param name="tracker">Tracker.</param>
        public override void HandleEventReleased(InputTracker tracker)
        {
            base.HandleEventReleased(tracker);
            if (_inputs.Contains(tracker.FingerId))
            {
                _inputs.Remove(tracker.FingerId);
            }
        }
    }
}