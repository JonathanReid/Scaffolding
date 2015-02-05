using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Scaffolding;

public class IntroView : AbstractView {
	 
	public override void Setup(ViewManagerBase manager)
    {
        base.Setup(manager);
    }

    public override void OnShowStart(SObject data)
    {
		RequestOverlayClose<SceneOneOverlay>();
        base.OnShowStart(data);
    }

    public override void OnShowComplete()
    {
        base.OnShowComplete();
    }

    public override void OnHideStart()
    {
        base.OnHideStart();
    }

    public override void OnHideComplete()
    {
        base.OnHideComplete();
    }
}
