using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Scaffolding;
using UnityEngine.UI;

public class GameView : AbstractView {
	 
    public override void Setup(ViewManagerBase manager)
    {
        base.Setup(manager);
		SetLevelStarted();
    }

	private void SetLevelStarted()
	{
		Text text = transform.FindChild("Canvas/LevelTitle").GetComponent<Text>();
		text.text += " " + GameData.LevelStarted;
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
