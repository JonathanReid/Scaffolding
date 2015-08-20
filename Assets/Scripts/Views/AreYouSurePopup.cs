using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Scaffolding;

public class AreYouSurePopup : AbstractModalPopup {
	 
	private Text _bodyText;

	public override void Setup (ViewManagerBase manager)
	{
		_bodyText = transform.FindChild("Canvas/Panel/PanelLayer/Text").GetComponent<Text>();
		base.Setup (manager);
	}

	public override void AddDismissButton (string buttonDismissText)
	{
		base.AddDismissButton (buttonDismissText);
		ButtonDismiss.GetComponentInChildren<Text>().text = buttonDismissText;
	}

	public override void AddOKButton (string buttonOKText)
	{
		base.AddOKButton (buttonOKText);
		ButtonOk.GetComponentInChildren<Text>().text = buttonOKText;
	}

   public override void SetBodyText (string bodyText)
	{
		base.SetBodyText (bodyText);
		_bodyText.text = bodyText;
	}
}
