using UnityEngine;
using System.Collections;
using System;

namespace Scaffolding
{
	[RequireComponent(typeof(Animation))]
	public abstract class AbstractSkinnableView : ViewRequest {

		public delegate void AbstractSkinnableViewEvent(AbstractSkinnableView sender);
		
		public event AbstractSkinnableViewEvent EventShowComplete = delegate{};
		public event AbstractSkinnableViewEvent EventHideComplete = delegate{};

		public AnimationClip inTransition;
		public AnimationClip outTransition;
		private Animation _animator;
		private SkinnedViewController _viewController;

		void Awake()
		{
			GameObject.DontDestroyOnLoad(gameObject);
		}

		/// <summary>
		/// Return a button that is a child of the view by name.
		/// </summary>
		public AbstractButton GetButtonForName(string name)
		{
			return _viewController.GetButtonForName(name);
		}
		
		/// <summary>
		/// Adds a button pressed handler for the named button.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="handler">Handler.</param>
		public void AddButtonPressedHandler(string name, Action handler)
		{
			GetButtonForName(name).AddButtonPressedHandlerNoButton(handler);
		}
		
		/// <summary>
		/// Adds a button down handler for the named button.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="handler">Handler.</param>
		public void AddButtonDownHandler(string name, Action handler)
		{
			GetButtonForName(name).AddButtonDownHandlerNoButton(handler);
		}
		
		#if UNITY_4_6 || UNITY_5
		//UNITY 4.6 ui
		
		/// <summary>
		/// Adds a button Up handler for the named button.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="handler">Handler.</param>
		public void AddButtonUpHandler(string name, Action handler)
		{
			GetButtonForName(name).AddButtonUpHandlerNoButton(handler);
		}
		
		/// <summary>
		/// Adds a button Exit handler for the named button.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="handler">Handler.</param>
		public void AddButtonExitHandler(string name, Action handler)
		{
			GetButtonForName(name).AddButtonExitHandlerNoButton(handler);
		}
		
		/// <summary>
		/// Adds a button Enter handler for the named button.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="handler">Handler.</param>
		public void AddButtonEnterHandler(string name, Action handler)
		{
			GetButtonForName(name).AddButtonEnterHandlerNoButton(handler);
		}
		#endif

		public virtual void Setup(ViewManagerBase manager, SkinnedViewController viewController)
		{
			_manager = manager;

			_viewController = viewController;

			_animator = gameObject.GetComponent<Animation>();
			if (_animator == null)
				_animator = gameObject.AddComponent<Animation>();

		}

		public virtual void OnShowStart(SObject data)
		{
			if (inTransition != null)
			{
				inTransition.wrapMode = WrapMode.Once;
				_manager.AddAnimationEventToTransition(this.GetType(), inTransition, "OnShowComplete");
				_animator.AddClip(inTransition, inTransition.name);
				_animator.Play(inTransition.name);
			}
			else
			{
				OnShowComplete();   
			}
		}

		public virtual void OnShowComplete()
		{
			EventShowComplete(this);
		}

		public virtual void OnHideStart()
		{
			if (outTransition != null)
			{
				outTransition.wrapMode = WrapMode.Once;
				_manager.AddAnimationEventToTransition(this.GetType(), outTransition, "OnHideComplete");
				_animator.AddClip(outTransition, outTransition.name);
				_animator.Play(outTransition.name);
			}
			else
			{
				OnHideComplete();   
			}
		}

		public virtual void OnHideComplete()
		{
			EventHideComplete(this);
		}

	}
}