using UnityEngine;
using System;
using System.Collections;
using Scaffolding;

public enum Direction
{
    None,
    Left,
    Right,
    Up,
    Down
}
namespace Scaffolding
{
    [AddComponentMenu("Scaffolding/Inputs/Drag Input")]
    /// <summary>
    /// Drag input is a quick dragging solution using AbstractInput.
    /// </summary>
    public class DragInput : AbstractInput
    {
        public bool lockHorizontal;
        public bool lockVertical;
        private Vector3 _dragDelta;
        private bool _mousePressed;
        private Direction _direction;
        private Action<Vector3> _dragCallback;
        private Action<Direction> _directionCallback;
        /************************************************
         * Getters & setters.
         ************************************************/
        /// <summary>
        /// Gets a value indicating whether this <see cref="Scaffolding.DragInput"/> mouse pressed.
        /// </summary>
        /// <value><c>true</c> if mouse pressed; otherwise, <c>false</c>.</value>
        public bool MousePressed
        {
            get
            {
                return _mousePressed;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Scaffolding.DragInput"/> lock horizontal.
        /// </summary>
        /// <value><c>true</c> if lock horizontal; otherwise, <c>false</c>.</value>
        public bool LockHorizontal
        {
            get
            {
                return lockHorizontal;
            }
            set
            {
                lockHorizontal = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Scaffolding.DragInput"/> lock vertical.
        /// </summary>
        /// <value><c>true</c> if lock vertical; otherwise, <c>false</c>.</value>
        public bool LockVertical
        {
            get
            {
                return lockVertical;
            }
            set
            {
                lockVertical = value;
            }
        }
        /************************************************
         * SwipeInput setup.
         ************************************************/
        /// <summary>
        /// The inputs clean up phase.
        /// </summary>
        public override void Cleanup()
        {
            base.Cleanup();
            _dragCallback = null;
            _directionCallback = null;
        }
        /************************************************
         * Public methods.
         ************************************************/
        /// <summary>
        /// Registers the drag callback.
        /// </summary>
        /// <summary>
        /// Example:
        /// public void MyCallbackFunction(Vector3 deltaMovement)
        /// {
        ///     transform.position += deltaMovement;
        /// }
        /// 
        /// _dragInput.RegisterDragCallback(MyCallbackFunction);
        /// 
        /// </summary>
        /// <param name="callback">Callback.</param>
        public void RegisterDragCallback(Action<Vector3> callback)
        {
            _dragCallback = callback;
        }

        /// <summary>
        /// Registers the direction callback.
        /// </summary>
        /// <summary>
        /// Example:
        /// public void MyCallbackFunction(Direction direction)
        /// {
        ///     if(direction == Direction.Left)
        ///     {
        ///         //dragged left...
        ///     }
        /// }
        /// 
        /// _dragInput.RegisterDirectionCallback(MyCallbackFunction);
        /// 
        /// </summary>
        /// <param name="callback">Callback.</param>
        public void RegisterDirectionCallback(Action<Direction> callback)
        {
            _directionCallback = callback;
        }
        /************************************************
         * Private methods.
         ************************************************/
        /// <summary>
        /// Calculates the direction the input was moved in.
        /// Also locks the delta to a given axis.
        /// </summary>
        private void FilterInputMovement()
        {
            if (lockHorizontal || lockVertical)
            {
                if (lockHorizontal && (_direction == Direction.Left || _direction == Direction.Right || _direction == Direction.None))
                {
                    if (Mathf.Abs(_dragDelta.x) > Mathf.Abs(_dragDelta.y))
                    {
                        //drag x
                        if (_dragDelta.x < 0)
                        {
                            //left
                            _direction = Direction.Left;
                            if (_directionCallback != null)
                            {
                                _directionCallback(_direction);
                            }
                        }
                        else if (_dragDelta.x > 0)
                        {
                            //right
                            _direction = Direction.Right;
                            if (_directionCallback != null)
                            {
                                _directionCallback(_direction);
                            }
                        }
                    }           
                }
                else
                {
                    _dragDelta.x = 0;
                }
                if (lockVertical && (_direction == Direction.Up || _direction == Direction.Down || _direction == Direction.None))
                {
                    if (Mathf.Abs(_dragDelta.x) < Mathf.Abs(_dragDelta.y))
                    {
                        //drag y
                        if (_dragDelta.y < 0)
                        {
                            //drag down
                            _direction = Direction.Down;
                            if (_directionCallback != null)
                            {
                                _directionCallback(_direction);
                            }
                        }
                        else if (_dragDelta.y > 0)
                        {
                            //drag up
                            _direction = Direction.Up;
                            if (_directionCallback != null)
                            {
                                _directionCallback(_direction);
                            }
                        }
                    }
                }
                else
                {
                    _dragDelta.y = 0;
                }
            }
        }
        /************************************************
         * AbstractInput overrides.
         ************************************************/
        /// <summary>
        /// EventDragged, dispatched by Scaffoldings InputManager
        /// Passes through an average delta position value of all known touches.
        /// </summary>
        /// <param name="position">Position.</param>
        public override void HandleEventDraggedDelta(Vector3 delta)
        {
            base.HandleEventDraggedDelta(delta);
            _dragDelta = delta;
            FilterInputMovement();
            if (_dragCallback != null)
            {
                _dragCallback(_dragDelta);
            }
        }

        /// <summary>
        /// EventPressed, dispatched by Scaffoldings InputManager
        /// Passes through a InputTracker of the current touch.
        /// </summary>
        /// <param name="tracker">Tracker.</param>
        public override void HandleEventPressed(InputTracker tracker)
        {
            if (tracker.FingerId > 0)
                return;
            base.HandleEventPressed(tracker);
            _mousePressed = true;
        }

        /// <summary>
        /// EventReleased, dispatched by Scaffoldings InputManager
        /// Passes through a InputTracker of the current touch.
        /// </summary>
        /// <param name="tracker">Tracker.</param>
        public override void HandleEventReleased(InputTracker tracker)
        {
            if (tracker.FingerId > 0)
                return;
            base.HandleEventReleased(tracker);
            _dragDelta = Vector3.zero;
            _mousePressed = false;
        }
    }
}
