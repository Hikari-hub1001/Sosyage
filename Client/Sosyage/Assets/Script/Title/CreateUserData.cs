using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateUserData : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField inputField;

	[SerializeField]
	private Button button;

	private bool createFlag = false;

	private void Start()
	{
		button.onClick.AddListener(() => createFlag = true);
	}

	public async UniTask<UserData> Create()
	{
		gameObject.SetActive(true);

		await UniTask.WaitUntil(() => createFlag);

		var userData = new UserData
		{
			name = inputField.text
		};

		gameObject.SetActive(false);

		return userData;
	}
}
