using System;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace BuildTools
{
	public static class AddressablesBuilder
	{
		[MenuItem("Build/Addressables", priority = 100)]
		// bat ‚Ì -executeMethod ‚©‚çŒÄ‚Î‚ê‚é‘z’è
		public static void BuildAddressables()
		{
			try
			{
				AddressableAssetSettings.CleanPlayerContent();

				AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
				if (!string.IsNullOrEmpty(result.Error))
					throw new Exception("BuildPlayerContent failed: " + result.Error);


				// bat ‚Ì errorlevel ‚ðˆÀ’è‚³‚¹‚½‚¢‚¯ EditorApplication.Exit ‚Å–¾Ž¦‚µ‚Æ‚­
				EditorApplication.Exit(0);
			}
			catch (Exception e)
			{
				Debug.LogError("[Addressables] FAILED\n" + e);
				EditorApplication.Exit(1);
			}
		}

	}
}
