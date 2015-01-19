using UnityEngine;
using System.Collections;
using System;

namespace Scaffolding
{
	public interface IRequestable {

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