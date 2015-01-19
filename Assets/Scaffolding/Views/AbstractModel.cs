using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Scaffolding
{
	public abstract class AbstractModel : MonoBehaviour {

		internal List<AbstractView> knownViews;

        /// <summary>
        /// When the view associated with this model is opened, it gets registered with the model here.
        /// </summary>
        /// <param name="view">View.</param>
		public virtual void RegisterView(AbstractView view)
		{
			if(knownViews == null)
			{
				knownViews = new List<AbstractView>();
			}

			knownViews.Add(view);
			view.ViewEvent += HandleViewEvent;
			view.ViewClosedEvent += HandleViewClosedEvent;
		}

		public virtual void HandleViewClosedEvent (AbstractView sender, SObject obj)
		{
			ViewClosed(sender);
		}

		public virtual void HandleViewEvent (AbstractView sender, SObject obj)
		{
			
		}

		/// <summary>
        /// Gets called when the view associated with this model is closed.
        /// </summary>
		public virtual void ViewClosed(AbstractView view)
		{
			if(knownViews.Contains(view))
			{
				view.ViewEvent -= HandleViewEvent;
				view.ViewClosedEvent -= HandleViewClosedEvent;
				knownViews.Remove(view);
				view = null;
			}
		}

	}
}
