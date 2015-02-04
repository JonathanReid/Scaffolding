using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Scaffolding;

public class MainMenu : AbstractView {
	 
	public override void Setup(ViewManagerBase manager)
    {
        base.Setup(manager);
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
        base.OnHideStart();
    }

    public override void OnHideComplete()
    {
        base.OnHideComplete();
    }

	public void LoadMainScene()
	{
		RequestOverlay<SceneOneOverlay>();
		RequestScene(LoadSceneType.LoadAsync, "StartingScene");
//		Application.LoadLevel("StartingScene");
	}

	public void OpenMadeByLink()
	{
		Debug.Log("Made by link");
		Application.OpenURL("http://www.jonsgames.com");
	}
	
	public void OpenSupportedByLink()
	{
		Application.OpenURL("http://www.preloaded.com");
	}
}
