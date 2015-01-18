using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Scaffolding
{
    /// <summary>
    /// Value object.
    /// Used for passing data between views.
    /// Pack any data you require into this, and OnShowStart can retrieve it.
    /// </summary>
		public class SObject
    {
        private Dictionary<string, int> _intDict;
		private Dictionary<string, float> _floatDict;
		private Dictionary<string, string> _stringDict;
		private Dictionary<string, bool> _boolDict;
		private Dictionary<string, List<System.Object>> _objectListDict;
		private Dictionary<string, System.Object[]> _objectArrayDict;
		private Dictionary<string, System.Object> _objectDict;
		private Dictionary<string, System.Action> _actionDict;

        /// <summary>
        /// Add an int into the object.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public void AddInt(string key, int value)
        {
            if (_intDict == null)
                _intDict = new Dictionary<string, int>();

            if (_intDict.ContainsKey(key))
                _intDict[key] = value;
            else
            {
                _intDict.Add(key, value);
            }
        }

        /// <summary>
        /// Add a string into the object.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public void AddString(string key, string value)
        {
            if (_stringDict == null)
                _stringDict = new Dictionary<string, string>();

            if (_stringDict.ContainsKey(key))
                _stringDict[key] = value;
            else
            {
                _stringDict.Add(key, value);
            }
        }

        /// <summary>
        /// Add a float into the object.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public void AddFloat(string key, float value)
        {
            if (_floatDict == null)
                _floatDict = new Dictionary<string, float>();

            if (_floatDict.ContainsKey(key))
                _floatDict[key] = value;
            else
            {
                _floatDict.Add(key, value);
            }
        }

        /// <summary>
        /// Add a bool into the object.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">If set to <c>true</c> value.</param>
        public void AddBool(string key, bool value)
        {
            if (_boolDict == null)
                _boolDict = new Dictionary<string, bool>();

            if (_boolDict.ContainsKey(key))
                _boolDict[key] = value;
            else
            {
                _boolDict.Add(key, value);
            }
        }

        /// <summary>
        /// Add an object list into the object.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public void AddObjectList(string key, List<System.Object> value)
        {
            if (_objectListDict == null)
                _objectListDict = new Dictionary<string, List<System.Object>>();

            if (_objectListDict.ContainsKey(key))
                _objectListDict[key] = value;
            else
            {
                _objectListDict.Add(key, value);
            }
        }

        /// <summary>
        /// Add an object array into the object.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public void AddObjectArray(string key, System.Object[] value)
        {
            if (_objectArrayDict == null)
                _objectArrayDict = new Dictionary<string, System.Object[]>();

            if (_objectArrayDict.ContainsKey(key))
                _objectArrayDict[key] = value;
            else
            {
                _objectArrayDict.Add(key, value);
            }
        }

        /// <summary>
        /// Add an object into the object.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public void AddObject(string key, System.Object value)
        {
            if (_objectDict == null)
                _objectDict = new Dictionary<string, System.Object>();

            if (_objectDict.ContainsKey(key))
                _objectDict[key] = value;
            else
            {
                _objectDict.Add(key, value);
            }
        }

		/// <summary>
		/// Add an action into the object.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public void AddAction(string key, System.Action value)
		{
			if (_actionDict == null)
				_actionDict = new Dictionary<string, System.Action>();
			
			if (_actionDict.ContainsKey(key))
				_actionDict[key] = value;
			else
			{
				_actionDict.Add(key, value);
			}
		}

		public bool HasKey(string key)
		{
			//using reflection to work through all the private dictionaries
			//saves swithching between them and is extendable by default.
			FieldInfo[] info = this.GetType().GetFields(BindingFlags.NonPublic | 
			                                            BindingFlags.Instance);
			foreach(FieldInfo inf in info)
			{
				IDictionary dict = inf.GetValue(this) as IDictionary;
				if(dict != null && dict.Contains(key))
				{
					return true;
				}
			}
			return false;
		}

        /// <summary>
        /// Gets the int with Key.
        /// </summary>
        /// <returns>The int.</returns>
        /// <param name="key">Key.</param>
        public int GetInt(string key)
        {
            if (_intDict.ContainsKey(key))
                return _intDict[key];
            else
            {
                Debug.LogWarning("No value could be found for key: " + key);
                return 0;
            }
        }

        /// <summary>
        /// Gets the string with Key.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="key">Key.</param>
        public string GetString(string key)
        {
            if (_stringDict.ContainsKey(key))
                return _stringDict[key];
            else
            {
                Debug.LogWarning("No value could be found for key: " + key);
                return "";
            }
        }

        /// <summary>
        /// Gets the float with Key.
        /// </summary>
        /// <returns>The float.</returns>
        /// <param name="key">Key.</param>
        public float GetFloat(string key)
        {
            if (_floatDict.ContainsKey(key))
                return _floatDict[key];
            else
            {
                Debug.LogWarning("No value could be found for key: " + key);
                return 0f;
            }
        }

        /// <summary>
        /// Gets the bool with Key.
        /// </summary>
        /// <returns><c>true</c>, if bool was gotten, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        public bool GetBool(string key)
        {
            if (_boolDict.ContainsKey(key))
                return _boolDict[key];
            else
            {
                Debug.LogWarning("No value could be found for key: " + key);
                return false;
            }
        }

        /// <summary>
        /// Gets the object list with Key.
        /// </summary>
        /// <returns>The object list.</returns>
        /// <param name="key">Key.</param>
        public List<System.Object> GetObjectList(string key)
        {
            if (_objectListDict.ContainsKey(key))
                return _objectListDict[key];
            else
            {
                Debug.LogWarning("No value could be found for key: " + key);
                return new List<System.Object>();
            }
        }

        /// <summary>
        /// Gets the object array with Key.
        /// </summary>
        /// <returns>The object array.</returns>
        /// <param name="key">Key.</param>
        public System.Object[] GetObjectArray(string key)
        {
            if (_objectArrayDict.ContainsKey(key))
                return _objectArrayDict[key];
            else
            {
                Debug.LogWarning("No value could be found for key: " + key);
                return new System.Object[0];
            }
        }

        /// <summary>
        /// Gets the object with Key.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="key">Key.</param>
        public System.Object GetObject(string key)
        {
            if (_objectDict.ContainsKey(key))
                return _objectDict[key];
            else
            {
                Debug.LogWarning("No value could be found for key: " + key);
                return new UnityEngine.Object();
            }
        }

		/// <summary>
		/// Gets the action with Key.
		/// </summary>
		/// <returns>The object.</returns>
		/// <param name="key">Key.</param>
		public System.Action GetAction(string key)
		{
			if (_actionDict.ContainsKey(key))
				return _actionDict[key];
			else
			{
				Debug.LogWarning("No value could be found for key: " + key);
				return null;
			}
		}
    }
}
