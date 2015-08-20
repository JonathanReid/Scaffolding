using UnityEngine;
using System.Collections;
using System;

namespace Scaffolding
{
	public enum LoadSceneType
	{
		Load,
		LoadAdditive,
		LoadAsync,
		LoadAdditiveAsync,
	}

	public interface IRequestable {

		void RequestScene(LoadSceneType loadType, string sceneName);
		void RequestSceneWithView<T>(LoadSceneType loadType, string sceneName) where T : AbstractView;
		void RequestSceneWithView(Type viewType, LoadSceneType loadType, string sceneName);
		void RequestSceneWithOverlay<T>(LoadSceneType loadType, string sceneName) where T : AbstractView;
		void RequestSceneWithOverlay(Type viewType, LoadSceneType loadType, string sceneName);
		AbstractView RequestOverlay<T>() where T :AbstractView;
		AbstractView RequestOverlay(Type type);
		void RequestOverlayClose<T>() where T :AbstractView;
		void RequestOverlayClose(Type type);
		void RequestOverlayForceClose<T>() where T :AbstractView;
		void RequestOverlayForceClose(Type type);
		AbstractView RequestView<T>() where T :AbstractView;
		AbstractView RequestView(Type type);
		void RequestForceReopenView<T>() where T:AbstractView;
		void RequestForceReopenView(Type type);
		void TransitionTo<T,T1>() where T :  AbstractView where T1 : AbstractTransition;
		AbstractModalPopup RequestModalPopup<T>(Action buttonOKCallback, string buttonOKText, Action buttonDismissCallback, string buttonDismissText, string bodyText) where T : AbstractModalPopup;
		AbstractModalPopup RequestModalPopup<T>(Action buttonOKCallback, string buttonOKText, string bodyText) where T : AbstractModalPopup;
	}
}