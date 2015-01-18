using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffolding
{
    /// <summary>
    /// Input tracker, tracks every finger or mouse press that happens within Scaffoldings views.
    /// </summary>
    public class InputTracker
    {
        private List<float> _recordedTime;
        private List<Vector3> _recordedPositions;
        private int _fingerId;
        private bool _alive;
        private Vector3 _position;
        private Vector3 _deltaPosition;
        private Vector2 _velocity;
		private GameObject _hitObject;
		private List<Camera> _hitCameras;

        /************************************************
         * Getters & setters.
         ************************************************/
        /// <summary>
        /// Gets the finger identifier.
        /// </summary>
        /// <value>The finger identifier.</value>
        public int FingerId
        {
            get { return _fingerId; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Scaffolding.InputTracker"/> is dirty.
        /// </summary>
        /// <value><c>true</c> if is dirty; otherwise, <c>false</c>.</value>
        public bool Alive
        {
            get { return _alive; }
        }

        /// <summary>
        /// Gets the current position of the input.
        /// </summary>
        /// <value>The position.</value>
        public Vector3 Position
        {
            get { return _position; }
        }

        /// <summary>
        /// Gets the delta position.
        /// </summary>
        /// <value>The delta position.</value>
        public Vector3 DeltaPosition
        {
            get { return _deltaPosition; }
        }
       
        /// <summary>
        /// Gets the velocity.
        /// </summary>
        /// <value>The velocity.</value>
        public Vector2 Velocity
        {
            get{ return _velocity; }
        }

        /// <summary>
        /// Gets the object hit by this tracker
        /// </summary>
        /// <value>The hit object.</value>
		public GameObject HitObject
		{
			get
			{
				return _hitObject;
			}
		}

        /// <summary>
        /// Determines if the object passed through was hit by this tracker
        /// </summary>
        /// <returns><c>true</c>, if game object was hit, <c>false</c> otherwise.</returns>
        /// <param name="obj">Object.</param>
		public bool HitGameObject(GameObject obj)
		{
			return _hitObject != null ? obj == _hitObject : false;
		}

        /// <summary>
        /// Gets or sets the list of cameras used for input
        /// </summary>
        /// <value>The hit camera.</value>
		public List<Camera> HitCamera
		{
			get
			{
				return _hitCameras;
			}
			set 
			{
				_hitCameras = value;
			}
		}
        /************************************************
         * InputTracker setup.
         ************************************************/
        /// <summary>
        /// Initializes a new instance of the <see cref="Scaffolding.InputTracker"/> class.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="position">Position.</param>
        public InputTracker(int id, Vector3 position, List<Camera> hitCameras)
        {
            _fingerId = id;
            _position = position;
			_hitCameras = hitCameras;

            StartTracking();
            UpdateTracker(position);
        }

        /************************************************
         * Public methods.
         ************************************************/
        /// <summary>
        /// Update the specified position.
        /// </summary>
        /// <param name="position">Position.</param>
        public void UpdateTracker(Vector3 position)
        {
            _alive = true;

            Vector3 pos = new Vector3(position.x, position.y, 0);
            _deltaPosition = pos - _position;
            _position = position;

			UpdateHitObject();
        }

		private void UpdateHitObject()
		{
			int i = 0, l = _hitCameras.Count;
			for(;i<l;++i)
			{
				Camera c = _hitCameras[i];
				Ray ray = c.ScreenPointToRay(_position);
				RaycastHit hitinfo;
				if(Physics.Raycast(ray, out hitinfo))
				{
					_hitObject = hitinfo.collider.gameObject;
				}
				else
				{
					Vector3 pos = c.ScreenToWorldPoint(_position);
					RaycastHit2D hit2d = Physics2D.Raycast(pos, Vector3.forward);
					if(hit2d.collider != null)
					{
						_hitObject = hit2d.collider.gameObject;
					}
					else
					{
						_hitObject = null;
					}
				}
			}
		}

        /// <summary>
        /// Sets the new velocity data.
        /// </summary>
        /// <param name="time">Time.</param>
        /// <param name="position">Position.</param>
        public void SetNewVelocityData(float time, Vector3 position)
        {
            if (_recordedTime == null)
                _recordedTime = new List<float>();

            if (_recordedPositions == null)
                _recordedPositions = new List<Vector3>();

            _recordedTime.Add(time);
            _recordedPositions.Add(position);

            if (_recordedTime.Count > 10)
                _recordedTime.RemoveAt(0);

            if (_recordedPositions.Count > 10)
                _recordedPositions.RemoveAt(0);

            CalculateVelocity();
        }

        /// <summary>
        /// Clean this instance.
        /// </summary>
        public void KillTracker()
        {
            _alive = false;
        }

        /// <summary>
        /// Start tracking the input
        /// </summary>
        public virtual void StartTracking()
        {
            //for override.
        }

        /// <summary>
        /// Stop tracking the input
        /// </summary>
        public virtual void StopTracking()
        {
            //for override.
        }

        /************************************************
         * Private methods.
         ************************************************/
        /// <summary>
        /// Calculates the velocity.
        /// </summary>
        private void CalculateVelocity()
        {
            _velocity.x = (_recordedPositions[0].x - _position.x) / (Time.realtimeSinceStartup - _recordedTime[0]);
            _velocity.y = (_recordedPositions[0].y - _position.y) / (Time.realtimeSinceStartup - _recordedTime[0]);
        }
        
    }
}

