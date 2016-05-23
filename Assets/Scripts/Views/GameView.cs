using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Scaffolding;
using UnityEngine.UI;

public class GameView : AbstractView {
	 
	public override void Setup(ViewManagerBase manager)
	{
		base.Setup(manager);
	}

	public override void OnShowStart(SObject data)
	{
		StartCoroutine(BigLongLoad());

		// Comment this out to make the transition into BarView work nicely...
		// But doing that breaks any attempt to transition out of it!
//			base.OnShowStart(data);
	}

	IEnumerator BigLongLoad()
	{
		// Do loading stuff here
		yield return new WaitForSeconds(3);
		Debug.Log("Big Long Load finished!");

		TransitionShowComplete();
	}

	public override void OnShowComplete()
	{
		// Should only get here when we've finished the load.
		Debug.Log("Screen transition finished!");

//		AddButtonPressedHandler("BackButton", BackButtonPressed);

		base.OnShowComplete();
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Space))
		{
			BackButtonPressed();
		}
	}

	public override void OnHideStart()
	{
		base.OnHideStart();
	}

	public override void OnHideComplete()
	{
		base.OnHideComplete();
	}

	private void BackButtonPressed()
	{
		TransitionTo<MainMenu, DoorsTransition>();
	}

}
