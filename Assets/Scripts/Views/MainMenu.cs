using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Scaffolding;
using Scaffolding.Transitions;

public class MainMenu : AbstractView {
	 
    public override void Setup(ViewManagerBase manager)
    {
        base.Setup(manager);

		GetButtonForName("StartGame").AddButtonPressedHandlerNoButton(ButtonPressed);
    }

    public override void OnShowStart(SObject data)
    {
        base.OnShowStart(data);
    }

    public override void OnShowComplete()
    {
        base.OnShowComplete();
    }

	private void ButtonPressed()
	{
		TransitionTo<GameView,DoorsTransition>();
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
