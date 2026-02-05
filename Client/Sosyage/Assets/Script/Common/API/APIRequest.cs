using Cysharp.Threading.Tasks;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class APIRequest
{
	private const string DOMAIN = "http://13.115.106.60:25566/";

	public static async UniTask<T> SendRequest<T>(string path, string param)
	{
		using (UnityWebRequest req = new UnityWebRequest(DOMAIN + path, UnityWebRequest.kHttpVerbPOST))
		{
			byte[] bodyRaw = Encoding.UTF8.GetBytes(param);
			req.uploadHandler = new UploadHandlerRaw(bodyRaw);
			req.downloadHandler = new DownloadHandlerBuffer();

			req.SetRequestHeader("Content-Type", "application/json; charset=utf-8");

			await req.SendWebRequest();

			if (req.result == UnityWebRequest.Result.Success)
			{
				Debug.Log($"User created successfully {req.downloadHandler.text}");
				return JsonUtility.FromJson<T>(req.downloadHandler.text);
			}
			else
			{

			}
		}
		// Implement your API request logic here
		return default(T);
	}
}
