using UnityEngine;
using System.Collections;
using Scaffolding;

namespace Scaffolding
{
    [AddComponentMenu("Scaffolding/Inputs/Rotate This Item Input")]
    /// <summary>
    /// RotateThisInput attaches to a gameobject as a quick solution to get something to respond to the rotate gesture.
    /// Extends AbstractInput so falls under the same rules as other inputs in regards to views.
    /// E.g colliders get disabled during OnShowStart and OnHideStart phases.
    /// 
    /// Can be tested in the editor by holding down left CMD on mac or left CTRL on PC.
    /// </summary>
    public class RotateThisInput : RotateInput
    {
        public string inputCamera;
        public int inputCameraIndex;
        public int inputCameraLength;
        private bool _enabled;
		private Camera _inputCamera;
        /************************************************
         * RotateThisInput setup.
         ************************************************/
        /// <summary>
        /// Run by the view during it's setup phase.
        /// </summary>
        /// <param name="view">View.</param>
        public override void Setup(AbstractView view)
        {
            base.Setup(view);
            RegisterRotateCallback(HandleRotateCallback);

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
        /// Handles the rotate callback.
        /// </summary>
        /// <param name="delta">Delta.</param>
        private void HandleRotateCallback(float delta)
        {
            if (_enabled)
            {
                Vector3 rot = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, rot.z + delta));
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