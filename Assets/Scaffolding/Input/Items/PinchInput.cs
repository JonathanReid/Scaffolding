using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Scaffolding;

namespace Scaffolding
{
    [AddComponentMenu("Scaffolding/Inputs/Pinch Input")]
    /// <summary>
    /// Pinch input is largely a touch input based class.
    /// It replicates the pinch gesture, and dispatches a callback while pinching.
    /// </summary>
    public class PinchInput : AbstractInput
    {
        private Dictionary<int, InputTracker> _trackers;
        private float _distance;
        private Action<float> _pinchCallback;
        /************************************************
         * PichInput setup.
         ************************************************/
        /// <summary>
        /// Run by the view during it's setup phase.
        /// </summary>
        /// <param name="view">View.</param>
        public override void Setup(AbstractView view)
        {
            base.Setup(view);
            _trackers = new Dictionary<int, InputTracker>();
            _distance = -1;
        }

        /// <summary>
        /// The inputs clean up phase.
        /// </summary>
        public override void Cleanup()
        {
            _trackers = null;
            _pinchCallback = null;
            base.Cleanup();
        }
        /************************************************
         * Public methods.
         ************************************************/
        /// <summary>
        /// Registers the pinch callback.
        /// </summary>
        /// <summary>
        /// Example:
        /// public void MyCallbackFunction(float deltaDistance)
        /// {
        /// //do pinch based code here...
        /// }
        /// 
        /// _pinchInput.RegisterPinchCallback(MyCallbackFunction);
        /// 
        /// </summary>
        /// <param name="callback">Callback.</param>
        public void RegisterPinchCallback(Action<float> callback)
        {
            _pinchCallback = callback;
        }
        /************************************************
         * Private methods.
         ************************************************/
        /// <summary>
        /// Updates the delta distance value between the two input points.
        /// When this changes, it fires the registered callback.
        /// </summary>
        private void UpdateDistance()
        {
            float dist = Vector3.Distance(_trackers[0].Position, _trackers[1].Position);
            if (_distance == -1)
            {
                _distance = dist;
            }
            if (_pinchCallback != null)
            {
                _pinchCallback(dist - _distance);
            }
            _distance = dist;
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
                _distance = -1;
            }
        }

        /// <summary>
        /// EventDragged, dispatched by the Scaffoldings InputManager
        /// Passes through an InputTracker of the current touch.
        /// </summary>
        /// <param name="tracker">Tracker.</param>
        public override void HandleEventDragged(InputTracker tracker)
        {
            base.HandleEventDragged(tracker);

            if (_trackers.Count == 2)
            {
                UpdateDistance();
            }

        }

        /// <summary>
        /// EventReleased, dispatched by the Scaffoldings InputManager
        /// Passes through a InputTracker of the current touch.
        /// </summary>
        /// <param name="tracker">Tracker.</param>
        public override void HandleEventReleased(InputTracker tracker)
        {
            base.HandleEventReleased(tracker);

            _distance = -1;
            if (_trackers.ContainsKey(tracker.FingerId))
            {
                _trackers.Remove(tracker.FingerId);
            }
        }
    }
}
