using UnityEngine;
using System.Collections;

namespace Scaffolding
{
	public class AutoFlowButton : ScaffoldingUGUIButton {

		private FlowController _flow;

		public override void Setup (AbstractView view)
		{
			base.Setup (view);

			_flow = FindObjectOfType<FlowController>();
		}

		//need to set the view and button name on the flow to make looking it up less painfull.
		//really need to extract this into smaller reusable functions too.
		public override void ButtonPressed ()
		{
			base.ButtonPressed ();

			if(_flow.HasInformationForView(_view.name, transform.name))
			{
				_flow.SetItem(_view, transform.name);
			}
			else
			{
				if(transform.name.ToLower().Contains("back"))
				{
					_flow.RestoreSnapshot(_view, transform.name);
				}
				else
				{
					throw new System.ArgumentException("Button has no destination assigned to it in the flow");
				}
			}

		}
	}
}