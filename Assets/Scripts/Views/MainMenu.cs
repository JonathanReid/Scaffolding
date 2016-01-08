using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Scaffolding;

public class ExampleClass : MonoBehaviour {
	 
	private const int MAGIC_NUMBER = 100;

    public float ExampleFloat;
	public string ExampleString;

	private GameObject _gameObjectReference;
	private string _internalString;

	public void Setup()
	{
		_gameObjectReference = transform.FindChild("MyGameObject").GetComponent<GameObject>();
	}

	private void Show()
	{

	}

	private void Hide()
	{

	}

}
