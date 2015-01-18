using UnityEngine;
using System.Collections;

namespace Scaffolding
{
	public abstract class AbstractModel : MonoBehaviour {

		internal AbstractView view;

        /// <summary>
        /// When the view associated with this model is opened, it gets registered with the model here.
        /// </summary>
        /// <param name="view">View.</param>
		public virtual void RegisterView(AbstractView view)
		{
			this.view = view;
			this.view.ViewEvent += HandleViewEvent;
		}

		public virtual void HandleViewEvent (SObject obj)
		{
			
		}

		/// <summary>
        /// Gets called when the view associated with this model is closed.
        /// </summary>
		public virtual void ViewClosed()
		{
			view.ViewEvent -= HandleViewEvent;
			view = null;
		}

	}
}
