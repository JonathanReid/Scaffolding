using UnityEngine;
using System.Collections;
using System;

namespace Scaffolding.Views
{
	public class RequestViewFacade : MonoBehaviour{

		internal AbstractView _lastScreen;
		internal AbstractView _currentScreen;
		internal AbstractView _targetScreen;
		internal SObject _targetScreenData;
		internal ScaffoldingConfig _scaffoldingConfig;
		internal ViewManager _viewManager;

		public void Setup(ScaffoldingConfig config, ViewManager manager)
		{
			_scaffoldingConfig = config;
			_viewManager = manager;
		}

		/// <summary>
		/// Request to reopen an already open view
		/// </summary>
		/// <param name="screenType">Screen type.</param>
		/// <param name="viewData">View data.</param>
		public void RequestForceReopenView(Type screenType, SObject viewData)
		{
			Type tp = screenType;
			//essentially resetting the view system here.
			//resetting everything back to nothing.
			if(tp == _currentScreen.GetType())
			{
				_lastScreen = _currentScreen;
				ScreenClosedComplete(tp);
			}
			_targetScreen = null;
			_currentScreen = null;
			RequestView(tp,viewData);
		}
		
		/// <summary>
		/// Requests the screen with data.
		/// </summary>
		/// <param name="screenType">Screen type.</param>
		/// <param name="data">Data.</param>
		public AbstractView RequestView(Type screenType, SObject data)
		{   
			if (_targetScreen == null || (_targetScreen.GetType() != screenType && !_targetScreen.IsSettingUp))
			{
				GameObject obj = GameObject.Instantiate(Resources.Load(_scaffoldingConfig.ViewPrefabPath(screenType.Name) + screenType.Name)) as GameObject;
				
				#if UNITY_4_6 || UNITY_5
				obj.transform.SetParent(_scaffoldingConfig.DetermineParentGameObjectPath().transform);
				#else
				obj.transform.parent = _scaffoldingConfig.DetermineParentGameObjectPath().transform;
				#endif
				obj.transform.name = screenType.Name;
				
				_targetScreen = obj.GetComponent<AbstractView>();
				
				string s = screenType.Name + "Model";
				Type t = ScaffoldingConfig.GetType(s);
				
				if(t != null)
				{
					AbstractModel model = FindObjectOfType(t) as AbstractModel;
					if(model != null)
					{
						model.RegisterView(_targetScreen);
					}
				}
				
				if(_scaffoldingConfig.ScaffoldingEnableAllGameobjects)
				{
					_targetScreen.gameObject.EnableAllChildren();
				}
				_targetScreen.Setup(this);
				
				
				obj.SetActive(false);
				
				_lastScreen = _currentScreen;
				
				_targetScreenData = data;
				
				if (_currentScreen != null)
				{
					if (_targetScreen.showingType == AbstractView.ShowingTypes.ShowImmediately)
					{
						ScreenClose(_currentScreen.GetType());
						ScreenOpen();   
					}
					else
					{
						ScreenClose(_currentScreen.GetType());
					}
				}
				else
				{
					ScreenOpen();   
				}
				
				return _targetScreen;
			}
			else
			{
				return null;
			}
		}
		
		private void ScreenOpen()
		{
			_targetScreen.gameObject.SetActive(true);
			
			_targetScreen.IsShowing = true;
			_targetScreen.OnShowStart(_targetScreenData);
			
			if(ViewManager.ViewShowStart != null)
			{
				ViewManager.ViewShowStart(_targetScreen);
			}
		}
		
		internal void ScreenShowComplete(Type screenType)
		{

			//move this to the overlay class.
			if (_currentOverlays.ContainsKey(screenType))
			{
				_currentOverlays[screenType].OnShowComplete();
			}
			else
			{
				_lastScreen = _currentScreen;
				_currentScreen = _targetScreen;
				_targetScreen.OnShowComplete();
			}

			if(ViewManager.ViewShowComplete != null)
			{
				ViewManager.ViewShowComplete(_targetScreen);
			}
		}
		
		private void ScreenClose(Type screenType)
		{
			if(!_currentScreen.IsHiding)
			{
				_currentScreen.IsHiding = true;
				_targetScreen.IsShowing = false;
				_currentScreen.OnHideStart();
				if(ViewClosed != null)
				{
					ViewClosed(_currentScreen);
				}

				if(ViewManager.ViewHideStart != null)
				{
					ViewManager.ViewHideStart(_targetScreen);
				}
			}
		}
		
		internal void ScreenClosedComplete(Type screenType)
		{
			if (_currentOverlays.ContainsKey(screenType))
			{
				Destroy(_currentOverlays[screenType].gameObject);
				_currentOverlays.Remove(screenType);
				
				if (_disabledInputsOnView.ContainsKey(_currentScreen.GetType()))
				{
					List<Type> overlays = _disabledInputsOnView[_currentScreen.GetType()];
					if (overlays.Contains(screenType))
					{
						overlays.Remove(screenType);
						if (overlays.Count == 0)
						{
							_disabledInputsOnView.Remove(_currentScreen.GetType());
							_currentScreen.EnableAllInputs();
						}
					}
				}
			}
			else if (_lastScreen != null && _lastScreen.GetType() == screenType)
			{
				if (_disabledInputsOnView.ContainsKey(_lastScreen.GetType()))
				{
					_disabledInputsOnView.Remove(_lastScreen.GetType());
				}
				Destroy(_lastScreen.gameObject);
				if(_targetScreen.showingType != AbstractView.ShowingTypes.ShowImmediately)
				{
					ScreenOpen();
				}
			}
			else if(_currentScreen.GetType() == screenType)
			{
				if(_loadScene)
				{
					StartLoadingScene();
				}
			}

			if(ViewManager.ViewHideComplete != null)
			{
				ViewManager.ViewHideComplete(_targetScreen);
			}

			Resources.UnloadUnusedAssets();
		}
	}
}