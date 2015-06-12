using UnityEngine;
using System;
using UnityEngine.UI;
using DG.Tweening;

namespace Scaffolding.Transitions
{
	public class DoorsComponent : TransitionComponentBase {

		public enum DoorsTransitionDirection
		{
			Horizontal,
			Vertical,
		}

		public enum DoorsTransitionStartPoint
		{
			ScreenEdge,
			ScreenCenter,
		}

		public RectTransform LeftDoor;
		public RectTransform RightDoor;

		public DoorsTransitionDirection Direction;
		public DoorsTransitionStartPoint To;
		public DoorsTransitionStartPoint From;

		private Vector2 _leftTarget = Vector2.zero;
		private Vector2 _rightTarget = Vector2.zero;
		
		public override void Setup (Transform parent, Action transitionCallback)
		{
			base.Setup (parent, transitionCallback);
		}
		
		public override void Transition ()
		{
			base.Transition ();

			SetDirection(Direction, To, ref _leftTarget,ref _rightTarget);

			LeftDoor.DOAnchorPos(_leftTarget,Duration).SetEase(Ease).OnComplete(TransitionComplete);
			RightDoor.DOAnchorPos(_rightTarget,Duration).SetEase(Ease);
		}
		
		public override void SetBaseValues ()
		{
			SetDirection(Direction, From, ref _leftTarget,ref _rightTarget);
			LeftDoor.anchoredPosition = _leftTarget;
			RightDoor.anchoredPosition = _rightTarget;
			base.SetBaseValues ();
		}

		private void SetDirection(DoorsTransitionDirection direction, DoorsTransitionStartPoint start, ref Vector2 panel1, ref Vector2 panel2)
		{
			if(start == DoorsTransitionStartPoint.ScreenEdge)
			{
				switch (direction) {
				case DoorsTransitionDirection.Horizontal:
					panel1 = new Vector2(-Screen.width,0);
					panel2 = new Vector2(Screen.width,0);
					break;
				case DoorsTransitionDirection.Vertical:
					panel1 = new Vector2(0,Screen.height);
					panel2 = new Vector2(0,-Screen.height);
					break;
				}
			}
			if(start == DoorsTransitionStartPoint.ScreenCenter)
			{
				switch (direction) {
				case DoorsTransitionDirection.Horizontal:
					panel1 = new Vector2(-Screen.width/2,0);
					panel2 = new Vector2(Screen.width/2,0);
					break;
				case DoorsTransitionDirection.Vertical:
					panel1 = new Vector2(0,Screen.height/2);
					panel2 = new Vector2(0,-Screen.height/2);
					break;
				}
			}
		}
	}		
}