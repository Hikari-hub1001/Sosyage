using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
	[SerializeField]
	private Button startButton;

	[SerializeField]
	private CreateUserData createUserData;


	[SerializeField]
	private Button deleteButton;
	[SerializeField]
	private Button cacheButton;

	[SerializeField]
	private GameObject loadPanel;
	[SerializeField]
	private Slider loadSlider;
	[SerializeField]
	private TMP_Text loadText;

	private CancellationToken token;

	private void Start()
	{
		startButton.onClick.AddListener(() => OnStartButtonClicked().Forget());
		deleteButton.onClick.AddListener(() =>
		{
			PlayerPrefs.DeleteKey("UserID");
			GameManager.Instance.userData = null;
		});
		cacheButton.onClick.AddListener(() => {
			AssetBundle.UnloadAllAssetBundles(true);
			Caching.ClearCache();
		});
	}

	private async UniTask AddressableCheck()
	{

		var size = await AddressablesRemoteLabelDownloader.GetDownloadSizeBytesAsync("Remote", token);
		Debug.Log($"Download Size: {size} bytes");

		if (size > 0)
		{
			loadPanel.SetActive(true);
			await AddressablesRemoteLabelDownloader.DownloadAsync("Remote", (progress) =>
			{
				loadSlider.value = progress;
				loadText.text = $"{progress * 100f:0.00}%";
			}, token);
		}

	}

	private async UniTask OnStartButtonClicked()
	{
		if (GameManager.Instance.userData == null)
		{
			var data = await createUserData.Create();

			var res = await APIRequest.SendRequest<UserData>("account/registration", "{\"name\":\"" + data.name + "\"}");
			GameManager.Instance.userData = res;
			PlayerPrefs.SetInt("UserID", GameManager.Instance.userData.id);
		}
		else
		{
			var res = await APIRequest.SendRequest<UserData>("account/login", "{\"id\":\"" + GameManager.Instance.userData.id + "\"}");

			GameManager.Instance.userData = res;
		}

		await AddressableCheck();

		SceneManager.LoadSceneAsync("Home");

	}
}
