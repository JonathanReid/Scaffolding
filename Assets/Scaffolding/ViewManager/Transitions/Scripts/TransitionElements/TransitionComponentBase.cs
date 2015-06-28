using UnityEngine;
using System;
using UnityEngine.UI;

namespace Scaffolding.Transitions
{
	public class TransitionComponentBase : MonoBehaviour {

		public float Duration = 1;
		public AnimationCurve Ease = AnimationCurve.EaseInOut(0,0,1,1);

		protected Image[] _panels;
		protected Action _callback;
		protected Transform _parent;

		public virtual void Setup(Transform parent, Action transitionCallback)
		{
			_callback = transitionCallback;
			_parent = parent;
		}

		public virtual void Transition()
		{
			SetBaseValues ();
		}

		public virtual void SetBaseValues()
		{
			_panels = _parent.GetComponentsInChildren<Image>();
		}

		public virtual void TransitionComplete()
		{
			_callback();
		}
	}
}