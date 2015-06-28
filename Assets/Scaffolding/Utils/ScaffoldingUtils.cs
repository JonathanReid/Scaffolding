using UnityEngine;
using System.Collections;
using System.IO;

public class ScaffoldingUtils {

	public static string RecursivelyFindFolderPath(string dir, string parentFolder)
	{
		string path = "";
		string[] paths = Directory.GetDirectories(dir);
		foreach(string s in paths)
		{
			if(s.Contains(parentFolder))
			{
				path = s;
				break;
			}
			else
			{
				path = RecursivelyFindFolderPath(s,parentFolder);
			}
		}

		return path;
	}
	
	public static string RecursivelyFindAsset(string dir, string objectName)
	{
		string asset = "";
		string[] paths = Directory.GetDirectories(dir);
		foreach(string s in paths)
		{
			string[] files = Directory.GetFiles(s);
			foreach(string f in files)
			{
				if(f.Contains(objectName))
				{
					asset = f;
					break;
				}
			}
			if(asset == "")
			{
				asset = RecursivelyFindAsset(s,objectName);
			}
			else
			{
				break;
			}
		}

		return asset;
	}

}
