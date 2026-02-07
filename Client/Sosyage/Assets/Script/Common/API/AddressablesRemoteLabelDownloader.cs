// AddressablesRemoteLabelDownloader.cs
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressablesRemoteLabelDownloader
{
	private const string PATH = "http://13.115.106.60:25566/static/assets";

	static bool s_initialized;

	public static async UniTask EnsureInitialized(CancellationToken ct = default)
	{
		if (s_initialized) return;

		Addressables.InternalIdTransformFunc = (e) =>
		{
			//Debug.Log(e.InternalId);
			if (e.InternalId.StartsWith("ServerData"))
			{
				return e.InternalId.Replace("ServerData", PATH);
			}
			return e.InternalId;
		};
		await GetCatalog();

	}

	/// <summary>
	/// 指定ラベル（例: "Remote"）の必要ダウンロード量(byte)を返す。0なら既に揃っとる。
	/// </summary>
	public static async UniTask<long> GetDownloadSizeBytesAsync(string label = "Remote", CancellationToken ct = default)
	{
		var sizeH = await Addressables.GetDownloadSizeAsync(label).ToUniTask(cancellationToken: ct);
		try
		{
			Debug.Log(sizeH);
			return sizeH; // bytes
		}
		finally
		{
			Addressables.Release(sizeH);
		}
	}

	/// <summary>
	/// 指定ラベル（例: "Remote"）の依存（＝リモートbundle）をダウンロードしてキャッシュ。
	/// サイズチェックはしない（呼び出し側で先に判断する想定）。
	/// </summary>
	public static async UniTask DownloadAsync(
		string label = "Remote",
		Action<float> onProgress = null,
		CancellationToken ct = default)
	{
		// 初期化はサイズ確認側で済んでる前提でもいいけど、安全に呼んどく
		//await EnsureInitializedAsync(ct);

		var dlH = Addressables.DownloadDependenciesAsync(label);
		try
		{
			while (!dlH.IsDone)
			{
				ct.ThrowIfCancellationRequested();
				onProgress?.Invoke(dlH.GetDownloadStatus().Percent);
				await UniTask.Yield(PlayerLoopTiming.Update, ct);
			}

			onProgress?.Invoke(1f);

			if (dlH.Status != AsyncOperationStatus.Succeeded)
				throw new Exception($"DownloadDependenciesAsync failed: {dlH.OperationException}");
		}
		finally
		{
			Addressables.Release(dlH);
		}
	}

	private static async UniTask GetCatalog()
	{
		using (UnityWebRequest req = new UnityWebRequest(PATH + "/catalog_0.json", UnityWebRequest.kHttpVerbGET))
		{
			req.downloadHandler = new DownloadHandlerFile(Application.persistentDataPath + "/catalog_0.json")
			{
				removeFileOnAbort = true
			};
			await req.SendWebRequest();

			if (req.result == UnityWebRequest.Result.Success)
			{
			}
			else
			{

			}
		}

	}
}
