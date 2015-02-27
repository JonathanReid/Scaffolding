using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_4_6 || UNITY_5
using UnityEngine.EventSystems;
#endif

namespace Scaffolding
{
    [AddComponentMenu("Scaffolding/Backend/Touch Manager")]
    /// <summary>
    /// Scaffoldings InputManager is a multitouch input manager that looks after all the inputs used within scaffoldings AbstractInput class.
    /// Scaffolding is multitouch from the core and built upon the idea that you should be able to place your whole hand on the screen and still use buttons.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public delegate void InputEvent(InputTracker tracker);
        public delegate void InputEventDelta(Vector3 position);

        public event InputEvent EventPressed;
        public event InputEvent EventReleased;
        public event InputEvent EventDragged;
        public event InputEventDelta EventDraggedDelta;

		public List<Camera> InputCameras;

        private List<InputTracker> _trackers;
        private Dictionary<int, InputTracker> _trackerLookup;
        private Vector3 _mousePosition;
        private Vector3 _secondaryMouseStartPosition;
        /************************************************
         * InputManager setup.
         ************************************************/
        private void Awake()
        {
			DontDestroyOnLoad(gameObject);

			InputManager[] im = FindObjectsOfType<InputManager>();
			if(im.Length > 1)
			{
				DestroyImmediate(gameObject);
				return;
			}

            _trackers = new List<InputTracker>();
            _trackerLookup = new Dictionary<int, InputTracker>();

#if UNITY_4_6 || UNITY_5
			gameObject.AddComponent<EventSystem>();
			gameObject.AddComponent<StandaloneInputModule>();
			gameObject.AddComponent<TouchInputModule>();
#endif

			if(InputCameras == null)
			{
				InputCameras = new List<Camera>();
				if(Camera.main != null)
				{
					InputCameras.Add(Camera.main);
				}
			}
        }

        /// <summary>
        /// Adds a camera for the input system to raycast through
        /// </summary>
        /// <param name="c">C.</param>
		public void AddCameraToInputCameras(Camera c)
		{
			if(!InputCameras.Contains(c))
			{
				InputCameras.Add(c);
			}

			foreach (InputTracker tracker in _trackers)
			{
				tracker.HitCamera = InputCameras;
			}
		}

		private void OnLevelWasLoaded(int level) 
		{
			int i = InputCameras.Count-1,l = -1;
			for(;i>l;--i)
			{
				if(InputCameras[i] == null)
				{
					InputCameras.RemoveAt(i);
				}
			}

			if(Camera.main != null)
			{
				InputCameras.Add(Camera.main);
			}
		}

        /// <summary>
        /// Removes a camera from the input system.
        /// </summary>
        /// <param name="c">C.</param>
		public void RemoveCameraFromInputCameras(Camera c)
		{
			if(InputCameras.Contains(c))
			{
				InputCameras.Remove(c);
			}

			foreach (InputTracker tracker in _trackers)
			{
				tracker.HitCamera = InputCameras;
			}
		}
        /************************************************
         * Private methods.
         ************************************************/
        /// <summary>
        /// Update all the available trackers and refresh to get new instances.
        /// </summary>
        private void Update()
        {
            UpdateTrackers();

            List<InputTracker> ended = new List<InputTracker>();
            List<InputTracker> alive = new List<InputTracker>();

            foreach (InputTracker tracker in _trackers)
            {
                if (tracker.Alive)
                {
                    alive.Add(tracker);
                }
                else
                {
                    ended.Add(tracker);
                }
            }

            FilterDeadTrackers(ended);
            FilterAliveTrackers(alive);
            UpdateTrackerVelocity(alive);
        }

        /// <summary>
        /// Update all the trackers available and find and kill any unused ones.
        /// </summary>
        private void UpdateTrackers()
        {
            // Set all trackers to be killed, if they arent picked up again during the frame, they arnt being used.
            foreach (InputTracker tracker in _trackers)
            {
                tracker.KillTracker();
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
            {
                //update or start trackers.
                int i = 0, l = UnityEngine.Input.touchCount;
                for (; i < l; ++i)
                {
                    Touch t = UnityEngine.Input.GetTouch(i);

                    // try to get our tracker for this finger id
                    InputTracker tracker = _trackerLookup.ContainsKey(t.fingerId) ? _trackerLookup[t.fingerId] : null;

                    if (tracker != null)
                        tracker.UpdateTracker(t.position);
                    else
                        tracker = BeginTracking(t.fingerId, t.position);
                }
            }
            else
            {
                // try to get our tracker for the mouse or simulated second finger.
                InputTracker tracker = null;
                if (UnityEngine.Input.GetMouseButton(0))
                {
                    tracker = _trackerLookup.ContainsKey(0) ? _trackerLookup[0] : null;

                    if (tracker != null)
                    {
                        tracker.UpdateTracker(UnityEngine.Input.mousePosition);
                    }
                    else
                        tracker = BeginTracking(0, UnityEngine.Input.mousePosition);
                }
               
                tracker = null;
                if (UnityEngine.Input.GetKey(KeyCode.LeftCommand) || UnityEngine.Input.GetKey(KeyCode.LeftControl))
                {
                    tracker = _trackerLookup.ContainsKey(1) ? _trackerLookup[1] : null;

                    if (tracker != null)
                    {
                        tracker.UpdateTracker(_secondaryMouseStartPosition - (UnityEngine.Input.mousePosition - _secondaryMouseStartPosition));
                    }
                    else
                    {
                        _secondaryMouseStartPosition = UnityEngine.Input.mousePosition - Vector3.right * 50;
                        tracker = BeginTracking(1, _secondaryMouseStartPosition - Vector3.right * 50);
                    }
                }
            }
        }
        /// <summary>
        /// Fully kill any trackers we've found to not be in use.
        /// </summary>
        /// <param name="ended">Ended.</param>
        private void FilterDeadTrackers(List<InputTracker> ended)
        {
            int i = 0, l = ended.Count;
            for (; i < l; ++i)
            {
                EndTracking(ended[i]);
            }
        }

        /// <summary>
        /// Handle all the trackers we have that are alive.
        /// Fire off the dragged event if they have moved.
        /// </summary>
        /// <param name="alive">Alive.</param>
        private void FilterAliveTrackers(List<InputTracker> alive)
        {
            //calculate average delta moved
            Vector3 averageDelta = Vector3.zero;    
            int activeCount = 0;
            float averageX = 0;
            float averageY = 0;
            int i = 0, l = alive.Count;
            InputTracker tracker = null;
            for (; i < l; ++i)
            {
                tracker = alive[i];

                // you want to filter the movement slightly so that little finger movements dont affect real movements.
                if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
                {
	                averageX += tracker.DeltaPosition.x;
	                averageY += tracker.DeltaPosition.y;

	                if (EventDragged != null)
	                {
	                    EventDragged(tracker);
	                }
	                activeCount++;
                }
                else
                {
                    if (EventDragged != null)
                    {
                        EventDragged(tracker);
                    }
                }
            }
            averageX = averageX / activeCount;
            if (float.IsNaN(averageX))
            {
                averageX = 0;
            }
            averageY = averageY / activeCount;
            if (float.IsNaN(averageY))
            {
                averageY = 0;
            }
            averageDelta = new Vector3(averageX, averageY, 0);
            if (EventDraggedDelta != null)
            {
                EventDraggedDelta(averageDelta);
            }
        }

        /// <summary>
        /// Updates the velocity for each tracker.
        /// The tracker handles the calculations indivudually, this just gives them their data.
        /// </summary>
        /// <param name="trackers">Trackers.</param>
        private void UpdateTrackerVelocity(List<InputTracker> trackers)
        {
            int i = 0, l = trackers.Count;
            InputTracker tracker = null;
            for (; i < l; ++i)
            {
                tracker = trackers[i];
                tracker.SetNewVelocityData(Time.realtimeSinceStartup, tracker.Position + tracker.DeltaPosition);
            }
        }

        /// <summary>
        /// Start tracking with a brand new tracker.
        /// </summary>
        /// <returns>The tracking.</returns>
        /// <param name="id">Identifier.</param>
        /// <param name="position">Position.</param>
        private InputTracker BeginTracking(int id, Vector3 position)
        {
			InputTracker tracker = new InputTracker(id, position, InputCameras);

            if (_trackers == null)
                _trackers = new List<InputTracker>();

            _trackers.Add(tracker);

            if (_trackerLookup == null)
                _trackerLookup = new Dictionary<int, InputTracker>();

            _trackerLookup.Add(id, tracker);

            if (EventPressed != null)
            {
                EventPressed(tracker);
            }
            
            return tracker;
        }

        /// <summary>
        /// Destroy a tracker that isn't used.
        /// </summary>
        /// <param name="tracker">Tracker.</param>
        private void EndTracking(InputTracker tracker)
        {
            tracker.StopTracking();
            
            _trackers.Remove(tracker);
            _trackerLookup.Remove(tracker.FingerId);

            if (EventReleased != null)
            {
                EventReleased(tracker);
            }

            tracker = null;
        }
    }
}

