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

		void OpenViewWhenSceneLoads<T>(string sceneName) where T : AbstractView;
		void OpenViewWhenSceneLoads(Type type, string sceneName);
		void OpenOverlayWhenSceneLoads<T>(string sceneName) where T : AbstractView;
		void OpenOverlayWhenSceneLoads(Type type, string sceneName);
		void RequestScene(LoadSceneType loadType, string sceneName);
		void RequestOverlay<T>() where T :AbstractView;
		void RequestOverlay(Type type);
		void RequestOverlayClose<T>() where T :AbstractView;
		void RequestOverlayClose(Type type);
		void RequestOverlayForceClose<T>() where T :AbstractView;
		void RequestOverlayForceClose(Type type);
		void RequestView<T>() where T :AbstractView;
		void RequestView(Type type);
		void RequestForceReopenView<T>() where T:AbstractView;
		void RequestForceReopenView(Type type);
		void RequestViewWithLoadingOverlay<T, L>() where T :AbstractView where  L :AbstractView;
		void RequestViewWithLoadingOverlay(Type type, Type loadingType);

	}
}