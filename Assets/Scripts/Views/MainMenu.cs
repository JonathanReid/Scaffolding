using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Scaffolding;

public class MainMenu : AbstractView
{
    public override void Setup(ViewManager manager)
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
    }

    public override void OnHideStart()
    {
        base.OnHideStart();
    }

    public override void OnHideComplete()
    {
        base.OnHideComplete();
    }

    public void OpenMadeByLink()
    {
        Application.OpenURL("http://www.twitter.com/_jonreid");
    }

    public void OpenSupportedByLink()
    {
        Application.OpenURL("http://www.preloaded.com");
    }
}
