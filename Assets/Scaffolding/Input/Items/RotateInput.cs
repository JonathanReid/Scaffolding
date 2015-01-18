using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Scaffolding;

namespace Scaffolding
{
    [AddComponentMenu("Scaffolding/Inputs/Rotate Input")]
    /// <summary>
    /// Rotate input is largely a touch input based class.
    /// It replicates the rotate gesture, and dispatches a callback while rotating.
    /// </summary>
    public class RotateInput : AbstractInput
    {
        private Dictionary<int, InputTracker> _trackers;
        private float _angle;
        private Action<float> _rotateCallback;
        /************************************************
         * RotateInput setup.
         ************************************************/
        /// <summary>
        /// Run by the view during it's setup phase.
        /// </summary>
        /// <param name="view">View.</param>
        public override void Setup(AbstractView view)
        {
            base.Setup(view);
            _trackers = new Dictionary<int, InputTracker>();
            _angle = -1;
        }

        /// <summary>
        /// The inputs clean up phase.
        /// </summary>
        public override void Cleanup()
        {
            _trackers = null;
            _rotateCallback = null;
            base.Cleanup();
        }
        /************************************************
         * Public methods.
         ************************************************/
        /// <summary>
        /// Register a callback handler for rotation.
        /// </summary>
        /// <summary>
        /// Example:
        /// public void MyCallbackFunction(float deltaAngle)
        /// {
        /// //do rotation based code here...
        /// }
        /// 
        /// _rotateInput.RegisterRotateCallback(MyCallbackFunction);
        /// 
        /// </summary>
        /// <param name="callback">Callback.</param>
        public void RegisterRotateCallback(Action<float> callback)
        {
            _rotateCallback = callback;
        }
        /************************************************
         * Private methods.
         ************************************************/
        /// <summary>
        /// Updates the delta angle value between the two input points.
        /// When this changes, it fires the registered callback.
        /// </summary>
        private void UpdateRotation()
        {
            Vector3 pos = _trackers[0].Position - _trackers[1].Position;
            float ang = Mathf.Atan2(pos.x, pos.y) * Mathf.Rad2Deg;
            if (ang < -180)
                ang += 180;

            if (_angle == -1)
            {
                _angle = ang;
            }

            if (_rotateCallback != null)
            {
                _rotateCallback(_angle - ang);
            }
            _angle = ang;
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
            if (!_trackers.ContainsKey(tracker.FingerId))
            {
                _trackers.Add(tracker.FingerId, tracker);
            }
            else
            {
                _angle = -1;
            }
        }

        /// <summary>
        /// EventDragged, dispatched by Scaffoldings InputManager
        /// Passes through a InputTracker of the current touch.
        /// </summary>
        /// <param name="tracker">Tracker.</param>
        public override void HandleEventDragged(InputTracker tracker)
        {
            base.HandleEventDragged(tracker);

            if (_trackers.Count == 2)
            {
                UpdateRotation();
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

            _angle = -1;
            if (_trackers.ContainsKey(tracker.FingerId))
            {
                _trackers.Remove(tracker.FingerId);
            }
        }
    }
}
