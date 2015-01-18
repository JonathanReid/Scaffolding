using UnityEngine;
using System.Collections;
using System;

namespace Scaffolding
{
    public enum SwipeDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    [AddComponentMenu("Scaffolding/Inputs/Swipe Input")]
    /// <summary>
    /// Swipe input replicates the swipe gesture and dispatches a callback for a chosen swipe direction.
    /// </summary>
    public class SwipeInput : AbstractInput
    {
        public float velocityThreshold;
        private Vector3 _dragDelta;
        private Vector3 _velocity;
        private bool _gestureFound;
        private SwipeDirection _direction;
        private Action<SwipeDirection> _gestureCallback;
        /************************************************
         * SwipeInput setup.
         ************************************************/
        /// <summary>
        /// The inputs clean up phase.
        /// </summary>
        public override void Cleanup()
        {
            _gestureCallback = null;
            base.Cleanup();
        }
        /************************************************
         * Public methods.
         ************************************************/
        /// <summary>
        /// Register a callback handler fo the swipe input.
        /// </summary>
        /// <summary>
        /// Example:
        /// public void MyCallbackFunction(SwipeDirection direction)
        /// {
        /// //do swipe based code here...
        /// }
        /// 
        /// _swipeInput.RegisterSwipeCallback(MyCallbackFunction);
        /// 
        /// </summary>
        /// <param name="callback">Callback.</param>
        public void RegisterSwipeCallback(Action<SwipeDirection> callback)
        {
            _gestureCallback = callback;
        }
        /************************************************
         * Private methods.
         ************************************************/
        /// <summary>
        /// Calculates the direction of the swipe. 
        /// When the velocity of the movement is over velocityThreshold, then the callback will be fired with the direction.
        /// </summary>
        private void CalculateDirection()
        {
            if (Mathf.Abs(_dragDelta.x) > Mathf.Abs(_dragDelta.y))
            {
                //drag x
                if (_dragDelta.x < 0 && Mathf.Abs(_velocity.x) > velocityThreshold)
                {
                    //left
                    _direction = SwipeDirection.Left;
                    _gestureFound = true;
                    if (_gestureCallback != null)
                    {
                        _gestureCallback(_direction);
                    }
                }
                else if (_dragDelta.x > 0 && Mathf.Abs(_velocity.x) > velocityThreshold)
                {
                    //right
                    _direction = SwipeDirection.Right;
                    _gestureFound = true;
                    if (_gestureCallback != null)
                    {
                        _gestureCallback(_direction);
                    }
                }
            }           

            if (Mathf.Abs(_dragDelta.x) < Mathf.Abs(_dragDelta.y))
            {
                //drag y
                if (_dragDelta.y < 0 && Mathf.Abs(_velocity.y) > velocityThreshold)
                {
                    //drag down
                    _direction = SwipeDirection.Down;
                    _gestureFound = true;
                    if (_gestureCallback != null)
                    {
                        _gestureCallback(_direction);
                    }
                }
                else if (_dragDelta.y > 0 && Mathf.Abs(_velocity.y) > velocityThreshold)
                {
                    //drag up
                    _direction = SwipeDirection.Up;
                    _gestureFound = true;
                    if (_gestureCallback != null)
                    {
                        _gestureCallback(_direction);
                    }
                }
            }
        }
        /************************************************
         * AbstractInput overrides.
         ************************************************/
        /// <summary>
        /// EventDragged, dispatched by Scaffoldings InputManager
        /// Passes through a InputTracker of the current touch.
        /// </summary>
        /// <param name="tracker">Tracker.</param>
        public override void HandleEventDragged(InputTracker tracker)
        {
            base.HandleEventDragged(tracker);
            _dragDelta = tracker.DeltaPosition;
            _velocity = tracker.Velocity;
            if (!_gestureFound)
            {
                CalculateDirection();
            }
        }
    }
}