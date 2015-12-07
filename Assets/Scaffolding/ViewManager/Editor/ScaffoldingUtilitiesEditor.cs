using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Scaffolding;

namespace Scaffolding.Editor
{
    public class ScaffoldingUtilitiesEditor : EditorWindow
    {
        private static readonly int VERSION = 1;
		private static readonly float RELEASE_ID = 0.3f;

		public static string ReleaseStringIdentifier(double _version, float _releaseId)
        {
			string id = _version.ToString("0");
            id += "." + _releaseId.ToString();
            return id;
        }

		[MenuItem("Tools/Scaffolding/About", false, 12900)]
        static void CreateNewView()
        {
            EditorUtility.DisplayDialog("About Scaffolding", "Scaffolding Version " + ReleaseStringIdentifier(VERSION, RELEASE_ID) + "\nCreated by Jon Reid", "OK");
        }

		[MenuItem("Tools/Scaffolding/Documentation", false, 10000)]
        static void OpenDocumentation()
        {
            Application.OpenURL("http://www.scaffoldingunity.com/documentation/");
        }

		[MenuItem("Tools/Scaffolding/Support", false, 10000)]
        static void OpenSupport()
        {
            Application.OpenURL(string.Format("mailto:support@scaffoldingunity.com?subject=Scaffolding%20{0:0.0}{1}%20Support", VERSION, RELEASE_ID));
        }

        public static int CheckIfMenuItemChanged(int lastCheckedLength, int index, List<string> names, string itemName)
        {
            if (lastCheckedLength != names.Count)
            {
                index = itemName == null ? 0 : names.IndexOf(itemName);
            }
            index = Mathf.Clamp(index, 0, names.Count);
            return index;
        }
    }
}
