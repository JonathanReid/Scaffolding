using UnityEngine;
using System.Collections;
using Scaffolding;

namespace Scaffolding
{
    [AddComponentMenu("Scaffolding/Inputs/Pinch This Item Input")]
    /// <summary>
    /// PinchThisInput attaches to a gameobject as a quick solution to get something to respond to the pinch gesture.
    /// Extends AbstractInput so falls under the same rules as other inputs in regards to views.
    /// E.g colliders get disabled during OnShowStart and OnHideStart phases.
    /// 
    /// Can be tested in the editor by holding down left CMD on mac or left CTRL on PC.Pinch this input.
    /// </summary>
    public class PinchThisInput : PinchInput
    {
        public string inputCamera;
        public int inputCameraIndex;
        public int inputCameraLength;
        public float smallestScale = 0;
        public float largestScale = 2;
        private bool _enabled;
		private Camera _inputCamera;
        /************************************************
         * PinchThisInput setup.
         ************************************************/
        /// <summary>
        /// Run by the view during it's setup phase.
        /// </summary>
        /// <param name="view">View.</param>
        public override void Setup(AbstractView view)
        {
            base.Setup(view);
            RegisterPinchCallback(HandlePinch);

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
         * Private methods.
         ************************************************/
        /// <summary>
        /// Handles the pinch gesture.
        /// </summary>
        /// <param name="delta">Delta.</param>
        private void HandlePinch(float delta)
        {
            if (_enabled)
            {

                Vector3 scale = transform.localScale;
                if (scale.x >= smallestScale && scale.x <= largestScale)
                {
                    transform.localScale = new Vector3(scale.x + delta / (Screen.height / 2), scale.y + delta / (Screen.height / 2), 1);
                }
                else
                {
                    if (scale.x < smallestScale)
                    {
                        transform.localScale = new Vector3(smallestScale, smallestScale, 1);
                    }
                    else if (scale.x > largestScale)
                    {
                        transform.localScale = new Vector3(largestScale, largestScale, 1);
                    }
                }
            }
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
                    _enabled = true;
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
            _enabled = false;
        }
    }
}