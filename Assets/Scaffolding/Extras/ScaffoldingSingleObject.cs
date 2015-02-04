using UnityEngine;
using System.Collections;

namespace Scaffolding
{
	public class ScaffoldingSingleObject : MonoBehaviour {

		// Use this for initialization
		void Awake () {
			GameObject.DontDestroyOnLoad(gameObject);
			ScaffoldingSingleObject[] holders = FindObjectsOfType<ScaffoldingSingleObject>();
			if(holders.Length > 1)
			{
				DestroyImmediate(gameObject);
			}
		}

	}
}