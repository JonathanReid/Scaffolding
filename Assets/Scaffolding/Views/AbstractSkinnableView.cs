using UnityEngine;
using System.Collections;

namespace Scaffolding
{
	public abstract class AbstractSkinnableView : MonoBehaviour {

		public delegate void AbstractSkinnableViewEvent(AbstractSkinnableView sender);
		
		public event AbstractSkinnableViewEvent EventShowComplete = delegate{};
		public event AbstractSkinnableViewEvent EventHideComplete = delegate{};

		protected ViewManagerBase _manager;

		public virtual void Setup(ViewManagerBase manager)
		{
			_manager = manager;
		}

		public virtual void OnShowStart(SObject data)
		{
			
		}

		public virtual void OnShowComplete()
		{
			EventShowComplete(this);
		}

		public virtual void OnHideStart()
		{

		}

		public virtual void OnHideComplete()
		{
			EventHideComplete(this);
		}

	}
}