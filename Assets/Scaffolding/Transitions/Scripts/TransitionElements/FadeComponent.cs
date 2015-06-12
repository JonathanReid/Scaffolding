using UnityEngine;
using System;
using UnityEngine.UI;
using DG.Tweening;

namespace Scaffolding.Transitions
{
	public class FadeComponent : TransitionComponentBase {

		[Range(0,1)]
		public float From = 1;
		[Range(0,1)]
		public float To = 1;

		private CanvasGroup _canvasGroup;

		public override void Setup (Transform parent, Action transitionCallback)
		{
			_canvasGroup = parent.GetComponentInChildren<CanvasGroup>();
			if(_canvasGroup == null)
			{
				_canvasGroup = parent.GetComponentInChildren<Canvas>().gameObject.AddComponent<CanvasGroup>();
			}
			base.Setup (parent, transitionCallback);
		}

		public override void Transition ()
		{
			base.Transition ();
			_canvasGroup.DOFade(To,Duration).SetEase(Ease).OnComplete(TransitionComplete);
		}

		public override void SetBaseValues ()
		{

			_canvasGroup.alpha = From;
			base.SetBaseValues ();
		}
	}
}