using UnityEngine;
using System;
using UnityEngine.UI;
using DG.Tweening;

namespace Scaffolding.Transitions
{
	public class BasicWipeComponent : TransitionComponentBase {
		
		public enum TransitionDirection
		{
			Left,
			Right,
			Up,
			Down,
		}

		public enum TransitionStartPoint
		{
			ScreenEdge,
			ScreenCenter,
		}
		

		public TransitionDirection Direction;
		public TransitionStartPoint To;
		public TransitionStartPoint From;
		
		private RectTransform _panel;
		private Vector2 _target = Vector2.zero;
		
		public override void Setup (Transform parent, Action transitionCallback)
		{
			base.Setup (parent, transitionCallback);
			_panel = parent.GetComponentInChildren<Image>().rectTransform;
		}
		
		public override void Transition ()
		{
			base.Transition ();
			
			_target = SetDirection(Direction, _target);
			if(To == TransitionStartPoint.ScreenCenter)
			{
				_target = Vector2.zero;
			}
			
			_panel.DOAnchorPos(_target,Duration).SetEase(Ease).OnComplete(TransitionComplete);
		}
		
		public override void SetBaseValues ()
		{
			_target = SetDirection(Direction, _target);
			if(From == TransitionStartPoint.ScreenCenter)
			{
				_target = Vector2.zero;
			}
			_panel.anchoredPosition = _target;
			base.SetBaseValues ();
		}
		
		private Vector2 SetDirection(TransitionDirection direction, Vector2 output)
		{
			switch (direction) {
			case TransitionDirection.Left:
				output = new Vector2(-Screen.width,0);
				break;
			case TransitionDirection.Right:
				output = new Vector2(Screen.width,0);
				break;
			case TransitionDirection.Up:
				output = new Vector2(0,Screen.height);
				break;
			case TransitionDirection.Down:
				output = new Vector2(0,-Screen.height);
				break;
			}
			
			return output;
		}
	}		
}