using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

namespace Scaffolding.Transitions
{
	[ExecuteInEditMode]
	public class PageTurnComponent: TransitionComponentBase {
		
		public enum TransitionDirection
		{
			Left,
			Right,
			Up,
			Down,
		}

		public enum TransitionStartPoint
		{
			ScreenEdge,
			ScreenCenter,
		}

		public bool Capture = false;
		public TransitionDirection Direction;
		public TransitionStartPoint To;
		public TransitionStartPoint From;

		[Range(0f,1f)]
		public float Amount = 0;
		
		private RectTransform _panel;
		private Image _image;
		private Vector2 _target = Vector2.zero;
		private GameObject _mask;
		private GameObject _pageCurl;
		
		public override void Setup (Transform parent, Action transitionCallback)
		{
			base.Setup (parent, transitionCallback);
			_image = parent.GetComponentInChildren<Image>();
			_panel = parent.GetComponentInChildren<Image>().rectTransform;
		}
		
		public override void Transition ()
		{
			base.Transition ();
			
			_target = SetDirection(Direction, _target);
			if(To == TransitionStartPoint.ScreenCenter)
			{
				_target = Vector2.zero;
			}

			Amount = 0;
			
			DOTween.To(()=> this.Amount, x=> this.Amount = x, 0.5f,Duration).OnComplete(TransitionComplete);
		}
		
		public override void SetBaseValues ()
		{
			_target = SetDirection(Direction, _target);
			if(From == TransitionStartPoint.ScreenCenter)
			{
				_target = Vector2.zero;
			}
			_panel.anchoredPosition = _target;

			if(Capture)
			{
				StartCoroutine(CaptureScreen());
			}

			TurnPage();

			base.SetBaseValues ();
		}

		IEnumerator CaptureScreen()
		{
			yield return new WaitForEndOfFrame();

			Texture2D tex = new Texture2D(Screen.width,Screen.height,TextureFormat.ARGB32,false,true);
			tex.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);
			tex.Apply();
			Sprite s = Sprite.Create(tex, new Rect(0,0,Screen.width,Screen.height), new Vector2(0.5f,0.5f));
			_image.sprite = s;
		}

		void Update()
		{
			if(Capture)
			{
//				TurnPage();
				PageCurl();
			}
		}

		private void TurnPage()
		{
			CanvasRenderer renderer = null;
			if(_mask == null)
			{
				_mask = new GameObject();
				_mask.name = "Mask";
				_mask.transform.SetParent(transform.FindChild("Canvas"));
				renderer = _mask.AddComponent<CanvasRenderer>();
				_mask.AddComponent<RectTransform>();
			}
			else
			{
				if(_mask.GetComponent<CanvasRenderer>())
				{
					renderer = _mask.GetComponent<CanvasRenderer>();
				}
				else
				{
					_mask.transform.SetParent(transform.FindChild("Canvas"));
					renderer = _mask.AddComponent<CanvasRenderer>();
					_mask.AddComponent<RectTransform>();
				}
			}

			List<UIVertex> verts = new List<UIVertex>();

			float am = Amount;

			float h = Mathf.Lerp(-Screen.height/2,Screen.height/2,am*2);
			float wt = Mathf.Lerp(Screen.width/2,-Screen.width/2,am*2-1);
			float w = Mathf.Lerp(Screen.width/2,-Screen.width/2,am);

			UIVertex v1 = new UIVertex();
			v1.position = new Vector3(Screen.width/2,-Screen.height/2,0);
			v1.uv0 = new Vector2(0, 1);
			v1.color = Color.white;
			verts.Add(v1);

			UIVertex v2 = new UIVertex();
			v2.position = new Vector3(w,-Screen.height/2,0);
			v2.uv0 = new Vector2(1, 0);
			v2.color = Color.white;
			verts.Add(v2);

			UIVertex v3 = new UIVertex();
			v3.position = new Vector3(wt,h,0);
			v3.uv0 = new Vector2(0, 0);
			v3.color = Color.white;
			verts.Add(v3);

			UIVertex v4 = new UIVertex();
			v4.position = new Vector3(Screen.width/2,Screen.height/2,0);
			v4.uv0 = new Vector2(1, 1);
			v4.color = Color.white;
			verts.Add(v4);

			renderer.Clear();
			Material mat = new Material(Resources.Load<Material>("DepthMask"));
			renderer.SetMaterial(mat, null);
			renderer.SetVertices(verts);
		}

		private void PageCurl()
		{
			CanvasRenderer renderer = null;
			if(_pageCurl == null)
			{
				_pageCurl = new GameObject();
				_pageCurl.name = "PageCurl";
				_pageCurl.transform.SetParent(transform.FindChild("Canvas"));
				renderer = _pageCurl.AddComponent<CanvasRenderer>();
				_pageCurl.AddComponent<RectTransform>();
			}
			else
			{
				if(_pageCurl.GetComponent<CanvasRenderer>())
				{
					renderer = _pageCurl.GetComponent<CanvasRenderer>();
				}
				else
				{
					_pageCurl.transform.SetParent(transform.FindChild("Canvas"));
					renderer = _pageCurl.AddComponent<CanvasRenderer>();
					_pageCurl.AddComponent<RectTransform>();
				}
			}
			
			List<UIVertex> verts = new List<UIVertex>();
			
			float am = Amount;
			
			float h = Mathf.Lerp(-Screen.height/2,Screen.height/2,am*2);
			float wt = Mathf.Lerp(Screen.width/2,-Screen.width/2,am*2-1);
			float w = Mathf.Lerp(Screen.width/2,-Screen.width/2,am);
			
			UIVertex v1 = new UIVertex();
			v1.position = new Vector3(w,-Screen.height/2,0);
			Debug.Log(w);
			v1.uv0 = new Vector2(0, 1);
			v1.color = Color.red;
			verts.Add(v1);
			
			UIVertex v2 = new UIVertex();
			v2.position = new Vector3(w,-Screen.height/2,0);
			v2.uv0 = new Vector2(1, 0);
			v2.color = Color.red;
			verts.Add(v2);
			
			UIVertex v3 = new UIVertex();
			v3.position = new Vector3(wt,h,0);
			v3.uv0 = new Vector2(0, 0);
			v3.color = Color.red;
			verts.Add(v3);
			
			UIVertex v4 = new UIVertex();
			v4.position = new Vector3(w-(Screen.width/2-wt),h,0);
			v4.uv0 = new Vector2(1, 1);
			v4.color = Color.red;
			verts.Add(v4);
			
			renderer.Clear();
			Material mat = new Material(Resources.Load<Material>("MaskedFrontMaterial"));
			renderer.SetMaterial(mat, null);
			renderer.SetVertices(verts);
		}
		
		private Vector2 SetDirection(TransitionDirection direction, Vector2 output)
		{
			switch (direction) {
			case TransitionDirection.Left:
				output = new Vector2(-Screen.width,0);
				break;
			case TransitionDirection.Right:
				output = new Vector2(Screen.width,0);
				break;
			case TransitionDirection.Up:
				output = new Vector2(0,Screen.height);
				break;
			case TransitionDirection.Down:
				output = new Vector2(0,-Screen.height);
				break;
			}
			
			return output;
		}
	}		
}