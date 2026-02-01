using Cysharp.Threading.Tasks;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
	[SerializeField]
	private Button startButton;

	[SerializeField]
	private CreateUserData createUserData;

	private void Start()
	{
		startButton.onClick.AddListener(() => OnStartButtonClicked().Forget());
	}

	private async UniTask OnStartButtonClicked()
	{
		if (GameManager.Instance.userData == null)
		{
			var data = await createUserData.Create();

			using (UnityWebRequest www = new UnityWebRequest("http://13.115.106.60:25566/account/registration",UnityWebRequest.kHttpVerbPOST))
			{
				byte[] bodyRaw = Encoding.UTF8.GetBytes("{\"name\":\""+data.name+"\"}");
				www.uploadHandler = new UploadHandlerRaw(bodyRaw);
				www.downloadHandler = new DownloadHandlerBuffer();

				www.SetRequestHeader("Content-Type", "application/json; charset=utf-8");

				await www.SendWebRequest();

				if (www.result == UnityWebRequest.Result.Success)
				{
					Debug.Log($"User created successfully {www.downloadHandler.text}");
					GameManager.Instance.userData = JsonUtility.FromJson<UserData>(www.downloadHandler.text);

					PlayerPrefs.SetInt("UserID", GameManager.Instance.userData.id);
				}
				else
				{
					Debug.LogError("User creation failed: " + www.error);
					return;
				}
			}
		}
		else
		{
			using (UnityWebRequest www = new UnityWebRequest("http://13.115.106.60:25566/account/login", UnityWebRequest.kHttpVerbPOST))
			{
				byte[] bodyRaw = Encoding.UTF8.GetBytes("{\"id\":\"" + GameManager.Instance.userData.id + "\"}");
				www.uploadHandler = new UploadHandlerRaw(bodyRaw);
				www.downloadHandler = new DownloadHandlerBuffer();

				www.SetRequestHeader("Content-Type", "application/json; charset=utf-8");

				await www.SendWebRequest();

				if (www.result == UnityWebRequest.Result.Success)
				{
					Debug.Log($"User Login successfully {www.downloadHandler.text}");
					GameManager.Instance.userData = JsonUtility.FromJson<UserData>(www.downloadHandler.text);
				}
				else
				{
					Debug.LogError("User creation failed: " + www.error);
					return;
				}
			}
		}
		SceneManager.LoadSceneAsync("Home");

	}
}
