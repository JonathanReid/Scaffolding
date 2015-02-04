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
		OpenViewWhenSceneLoads<MainMenu>("DemoScene");
		OpenViewWhenSceneLoads<SceneOneStartingView>("StartingScene");
    }

	private void OpenSceneAdditive()
	{
		RequestScene(LoadSceneType.LoadAdditiveAsync,"DemoScene");
	}

	private void OpenScene()
	{
		RequestScene(LoadSceneType.LoadAsync,"DemoScene");
	}

    public override void OnShowStart(SObject data)
    {
        base.OnShowStart(data);
    }

    public override void OnShowComplete()
    {
        base.OnShowComplete();
		RequestOverlayClose<SceneOneOverlay>();
    }

    public override void OnHideStart()
    {
		RequestOverlay<SceneOneOverlay>();
        base.OnHideStart();
    }

    public override void OnHideComplete()
    {

        base.OnHideComplete();
    }
}
