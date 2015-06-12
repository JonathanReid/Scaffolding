using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Scaffolding;
using Scaffolding.Transitions;

public class MainMenu : AbstractView {
	 
	public override void Setup(ViewManagerBase manager)
    {
        base.Setup(manager);
		GetButtonForName("Next").AddButtonPressedHandlerNoButton(NextPressed);
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

	private void NextPressed()
	{
		//Maybe, instead of having all these options, I just have components that do the in and the out, and then stack them together/
		//that way, I can have views that transition in one way, and out another.
		//and then those components get driven by the transition, which just looks after what it looks like.
		//so a fade in component, and a fade out component
		//or a fade in and a wipe out...



		TransitionTo<IntroView,PageTurnTransition>();
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
