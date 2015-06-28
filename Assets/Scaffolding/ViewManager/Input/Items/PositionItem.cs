using UnityEngine;
using System.Collections;
using Scaffolding;

namespace Scaffolding
{
    [AddComponentMenu("Scaffolding/Positioning/PositionItem")]
    /// <summary>
    /// Positions the transform relative to a camera. Only works with Orthographic cameras currently.
    /// </summary>
    public class PositionItem : MonoBehaviour
    {
        public enum ScreenPosition
        {
            None,
            UpperLeft,
            UpperMiddle,
            UpperRight,
            Left,
            Middle,
            Right,
            LowerLeft,
            LowerMiddle,
            LowerRight
        }
        //for inspector.
        public ScreenPosition screenPosition;
        public Vector2 edgeBuffer;
        public string renderingCamera;
        public int renderingCameraIndex;
        public int renderingCameraLength;
        //internal.
        private Vector2 _pixelsFromEdge;
        private float _zPosition;
		private Camera _renderingCamera;
        /************************************************
         * Public methods.
         ************************************************/
        /// <summary>
        /// Resets the position to its current configuration.
        /// Used when the screen size has changed.
        /// </summary>
        public void SetPosition()
        {
			if(GameObject.Find(renderingCamera) != null && GameObject.Find(renderingCamera).GetComponent<Camera>() != null)
			{
				_renderingCamera = GameObject.Find(renderingCamera).GetComponent<Camera>();
			}
			else
			{
				_renderingCamera = Camera.main;
			}

            UpdatePosition(screenPosition, edgeBuffer);
        }
        /************************************************
         * Private methods.
         ************************************************/
        /// <summary>
        /// Sets the actual position of the object, figures out where it needs to sit in accordance to the camera.
        /// </summary>
        /// <param name="position">Position.</param>
        /// <param name="pixelsFromEdge">Pixels from edge.</param>
        private void UpdatePosition(ScreenPosition position, Vector2 pixelsFromEdge)
        {

            if (renderingCamera == null)
            {
                return;
            }

            Vector3 placedPosition = Vector3.zero;
            _zPosition = -_renderingCamera.transform.position.z + transform.position.z;

            _pixelsFromEdge = pixelsFromEdge;

            if (gameObject.IsRetina())
            {
                _pixelsFromEdge.x *= 2;
                _pixelsFromEdge.y *= 2;
            }

            switch (position)
            {     

            //Top Screen:
                case ScreenPosition.UpperLeft:
                    placedPosition = UpperLeft();
                    break;

                case ScreenPosition.UpperMiddle:
                    placedPosition = UpperMiddle();
                    break;

                case ScreenPosition.UpperRight:
                    placedPosition = UpperRight();
                    break;  

            //Middle:
                case ScreenPosition.Left:
                    placedPosition = Left();
                    break;

                case ScreenPosition.Middle:
                    placedPosition = Middle();
                    break;

                case ScreenPosition.Right:
                    placedPosition = Right();
                    break;

            //Bottom Screen:
                case ScreenPosition.LowerLeft:
                    placedPosition = LowerLeft();
                    break;

                case ScreenPosition.LowerMiddle:
                    placedPosition = LowerMiddle();
                    break;

                case ScreenPosition.LowerRight:
                    placedPosition = LowerRight();
                    break;          
            }

            if (screenPosition != ScreenPosition.None)
                transform.position = placedPosition;
        }
        /************************************************
         * Position setting.
         ************************************************/
        private Vector3 UpperLeft()
        {
			return _renderingCamera.ScreenToWorldPoint(new Vector3(_pixelsFromEdge.x, _renderingCamera.pixelHeight - _pixelsFromEdge.y, _zPosition));
        }

        private Vector3 UpperMiddle()
        {
			return _renderingCamera.ScreenToWorldPoint(new Vector3((_renderingCamera.pixelWidth / 2) + _pixelsFromEdge.x, _renderingCamera.pixelHeight - _pixelsFromEdge.y, _zPosition));
        }

        private Vector3 UpperRight()
        {
			return _renderingCamera.ScreenToWorldPoint(new Vector3(_renderingCamera.pixelWidth - _pixelsFromEdge.x, _renderingCamera.pixelHeight - _pixelsFromEdge.y, _zPosition));
        }

        private Vector3 Left()
        {
			return _renderingCamera.ScreenToWorldPoint(new Vector3(_pixelsFromEdge.x, (_renderingCamera.pixelHeight / 2) - _pixelsFromEdge.y, _zPosition));
        }

        private Vector3 Middle()
        {
			return _renderingCamera.ScreenToWorldPoint(new Vector3((_renderingCamera.pixelWidth / 2) + _pixelsFromEdge.x, (_renderingCamera.pixelHeight / 2) - _pixelsFromEdge.y, _zPosition));
        }

        private Vector3 Right()
        {
			return _renderingCamera.ScreenToWorldPoint(new Vector3(_renderingCamera.pixelWidth - _pixelsFromEdge.x, (_renderingCamera.pixelHeight / 2) - _pixelsFromEdge.y, _zPosition));
        }

        private Vector3 LowerLeft()
        {
			return _renderingCamera.ScreenToWorldPoint(new Vector3(_pixelsFromEdge.x, _pixelsFromEdge.y, _zPosition));
        }

        private Vector3 LowerMiddle()
        {
			return _renderingCamera.ScreenToWorldPoint(new Vector3((_renderingCamera.pixelWidth / 2) + _pixelsFromEdge.x, _pixelsFromEdge.y, _zPosition));
        }

        private Vector3 LowerRight()
        {
			return _renderingCamera.ScreenToWorldPoint(new Vector3(_renderingCamera.pixelWidth - _pixelsFromEdge.x, _pixelsFromEdge.y, _zPosition));
        }
    }
}
