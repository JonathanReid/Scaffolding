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
//		TransitionTo<IntroView,DoorsTransition>();
		RequestModalPopup<AreYouSurePopup>(OKPressed,"OK!", DismissPressed, "Cancel", "Are you sure you want to continue?");
	}

	private void OKPressed()
	{
		Debug.Log("OK!");
		TransitionTo<IntroView,DoorsTransition>();
	}

	private void DismissPressed()
	{
		Debug.Log("Cancel :(");
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
