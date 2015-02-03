using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Scaffolding;

public class SceneOneStartingView : AbstractView {
	 
    public override void Setup(ViewManagerBase manager)
    {
        base.Setup(manager);
		AddButtonPressedHandler("OpenSceneAdditive",OpenSceneAdditive);
		AddButtonPressedHandler("OpenScene",OpenScene);
    }

	private void OpenSceneAdditive()
	{
		Application.LoadLevelAdditive("DemoScene");
	}

	private void OpenScene()
	{
		Application.LoadLevel("DemoScene");
	}

    public override void OnShowStart(SObject data)
    {
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
