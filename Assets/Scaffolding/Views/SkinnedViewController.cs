using UnityEngine;
using System.Collections;

namespace Scaffolding
{
	[ExecuteInEditMode]
	public class SkinnedViewController : AbstractView {

		protected AbstractSkinnableView _brandedView;
		protected SObject _cachedData;

		//just using this for editor tooling. When the view is opened in the editor, it should load up the correct version of its child
		//so that Art can edit things quickly
		void Awake()
		{
			if(Application.isEditor && !Application.isPlaying)
			{
				OpenSkinnedView();
			}
		}

		public override void Setup(ViewManagerBase manager)
		{
			OpenSkinnedView();
			_brandedView.Setup(manager,this);
			base.Setup(manager);
		}
		
		public override void OnShowStart(SObject data)
		{
			_brandedView.EventShowComplete += HandleEventShowComplete;
			_brandedView.OnShowStart(data);
			_cachedData = data;
		}

		void HandleEventShowComplete (AbstractSkinnableView sender)
		{
			_brandedView.EventShowComplete -= HandleEventShowComplete;
			base.OnShowStart(_cachedData);
		}
		
		public override void OnShowComplete()
		{
			base.OnShowComplete();
		}
		
		public override void OnHideStart()
		{
			_brandedView.EventHideComplete += HandleEventHideComplete;
			_brandedView.OnHideStart();
		}

		void HandleEventHideComplete (AbstractSkinnableView sender)
		{
			_brandedView.EventHideComplete -= HandleEventHideComplete;
			_brandedView.OnHideComplete();
		}
		
		public override void OnHideComplete()
		{
			base.OnHideComplete();
		}

		public virtual void OpenSkinnedView()
		{

		}

	}
}
