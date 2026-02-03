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

			var res = await  APIRequest.SendRequesr<UserData>("account/registration", "{\"name\":\"" + data.name + "\"}");
			GameManager.Instance.userData = res;
			PlayerPrefs.SetInt("UserID", GameManager.Instance.userData.id);
		}
		else
		{
			var res = await APIRequest.SendRequesr<UserData>("account/login", "{\"id\":\"" + GameManager.Instance.userData.id + "\"}");

			GameManager.Instance.userData = res;
		}
		SceneManager.LoadSceneAsync("Home");

	}
}
