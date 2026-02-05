using Cysharp.Threading.Tasks;
using OriginalLib.Behaviour;
using System;
using UnityEngine;

public class GameManager : Singleton_DontDestroy<GameManager>
{
	[NonSerialized]
	public UserData userData = null;

	protected async override void Init()
	{
		base.Init();
		Debug.Log(PlayerPrefs.HasKey("UserID"));
		Debug.Log(PlayerPrefs.GetInt("UserID"));

		if (PlayerPrefs.HasKey("UserID") && PlayerPrefs.GetInt("UserID", -1) != -1)
		{
			userData = new UserData();
			userData.id = PlayerPrefs.GetInt("UserID");
		}

		//AssetBundle.UnloadAllAssetBundles(true);
		//UnityEngine.Caching.ClearCache();

		await AddressablesRemoteLabelDownloader.EnsureInitialized();
	}

}
