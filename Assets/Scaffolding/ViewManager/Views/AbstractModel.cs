using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Scaffolding
{
	/// <summary>
	/// The default base of all automatically created models.
	/// Allows for easy registration between itself and its paired view, can also register other views.
	/// </summary>
	public abstract class AbstractModel : MonoBehaviour {

		internal List<AbstractView> knownViews;

        /// <summary>
        /// When the view associated with this model is opened, it gets registered with the model here.
		/// This also registers the default ViewEvent delegate, for quick communication between registered views and the model.
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

		/// <summary>
		/// Handles the view closed event. Automatically called when any view registered to this model is closed.
		/// This deregisters the view from the model.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="obj">Object.</param>
		public virtual void HandleViewClosedEvent (AbstractView sender, SObject obj)
		{
			ViewClosed(sender);
		}

		/// <summary>
		/// Handles the view event. Called by any view registered to the model.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="obj">Object.</param>
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
