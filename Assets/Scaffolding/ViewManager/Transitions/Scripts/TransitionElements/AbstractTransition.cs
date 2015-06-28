using UnityEngine;
using System.Collections;
using Scaffolding.Transitions;

namespace Scaffolding
{
	public class AbstractTransition : AbstractView {

		public TransitionComponentBase InTransition;
		public TransitionComponentBase OutTransition;

		internal bool TransitionStarting;
		internal bool TransitionEnding;
		private Canvas _canvas;

		private Transform _inTransitionHolder;
		private Transform _outTransitionHolder;

		public void SetupHolders()
		{
			CreateTransitionHolders();
			CreateCanvas();
		}

		private void CreateTransitionHolders()
		{

			_inTransitionHolder = transform.FindChild("InTransition");
			if(_inTransitionHolder == null)
			{
				GameObject go = new GameObject();
				_inTransitionHolder = go.transform;
				go.transform.SetParent(transform);
				_inTransitionHolder.name = "InTransition";
			}

			InTransition = _inTransitionHolder.GetComponent<TransitionComponentBase>();

			if(InTransition == null)
			{
				InTransition = _inTransitionHolder.gameObject.AddComponent<TransitionComponentBase>();
			}

			_outTransitionHolder = transform.FindChild("OutTransition");
			if(_outTransitionHolder == null)
			{
				GameObject go = new GameObject();
				_outTransitionHolder = go.transform;
				go.transform.SetParent(transform);
				_outTransitionHolder.name = "OutTransition";
			}

			OutTransition = _outTransitionHolder.GetComponent<TransitionComponentBase>();

			if(OutTransition == null)
			{
				OutTransition = _outTransitionHolder.gameObject.AddComponent<TransitionComponentBase>();
			}
		}

		private void CreateCanvas()
		{
			_canvas = GetComponentInChildren<Canvas>();
			if(_canvas == null)
			{
				GameObject go = new GameObject();
				go.name = "Canvas";
				_canvas = go.AddComponent<Canvas>();
			}
		}

		public override void Setup (ViewManagerBase manager)
		{
			base.Setup (manager);
			InTransition.Setup(transform, TransitionShowComplete);
			OutTransition.Setup(transform, TransitionHideComplete);

			_canvas = GetComponentInChildren<Canvas>();
			_canvas.sortingOrder = 100;
		}

		public override void OnShowStart (SObject data)
		{
			TransitionStarting = true;
			TransitionShow(data);
		}

		public virtual void TransitionShow(SObject data)
		{
			InTransition.Transition();
		}

		public virtual void TransitionShowComplete()
		{
			OnShowComplete();
		}

		public override void OnShowComplete ()
		{
			TransitionStarting = false;
			base.OnShowComplete ();
		}

		public override void OnHideStart ()
		{
			TransitionEnding = true;
			TransitionHide();
		}

		public virtual void TransitionHide()
		{
			OutTransition.Transition();
		}

		public virtual void TransitionHideComplete()
		{
			OnHideComplete();
		}

		public override void OnHideComplete ()
		{
			TransitionEnding = false;
			base.OnHideComplete ();
		}
	}
}