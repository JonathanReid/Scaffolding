using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Scaffolding;

public class LevelSelectView : AbstractView {
	 
    public override void Setup(ViewManagerBase manager)
    {
        base.Setup(manager);

		AutoFlowButtonGroup buttonGroup = GetComponentInChildren<AutoFlowButtonGroup>();
		buttonGroup.RegisterForButtonPressedCallbacks(ButtonClicked);
    }

    public override void OnShowStart(SObject data)
    {
        base.OnShowStart(data);
    }

    public override void OnShowComplete()
    {
        base.OnShowComplete();
    }

	private void ButtonClicked(AbstractButton button)
	{
		Debug.Log("Button clicked! " + button.name);
		GameData.LevelStarted = int.Parse(button.name.Replace("Button",""));
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
