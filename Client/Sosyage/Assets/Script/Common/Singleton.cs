//============================================================================================================================
//
// このクラスはシングルトンのコンポーネントとなります。
// 任意のクラスに継承することでのみ使用可能です。
// ２つ以上のインスタンスが生成される際、あとから生成したインスタンスがゲームオブジェクトごと破棄されます。
// シーン遷移が行われるタイミングで破棄されます。
// シーンをまたいで保持したい場合は「Singleton_DontDestroy」クラスを使用してください。
//
// 使い方
// TestSingletonクラスを例とします
// public class TestSingleton : Singleton <TestSingleton>
//
//
// Awakeメソッドは基本的に実装せずにInitメソッドを使用してください。
// InitメソッドはAwakeメソッドで呼ばれており
// Awakeメソッドと同等の働きをします。
//
//============================================================================================================================

using UnityEngine;

namespace OriginalLib.Behaviour
{

	/// <summary>
	/// シングルトンベースクラス
	/// </summary>
	/// <typeparam name="T">シングルトンにするクラス</typeparam>
	public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
	{

		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
#if UNITY_6000
					_instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);
#else
					_instance = FindObjectOfType<T>(true);
#endif
					if(_instance == null)
					{
						throw new System.NullReferenceException("Attached object not found");
					}
					_instance.Init();
				}
				return _instance;
			}
		}

		protected static T _instance;


		/// <summary>
		/// インスタンスが作成済みかチェックする
		/// </summary>
		/// <returns></returns>
		public static bool IsValid() { return _instance != null; }


		protected void Awake()
		{
			if (!IsValid())
			{
				_instance = this.GetComponent<T>();
				Init();
			}
			else if (Instance != this)
			{
				Debug.Log($"The second instance has been created. It will be discarded.\r\n{this}");
				if (Application.isPlaying)
				{
					Destroy(gameObject);
				}
				else
				{
					Debug.Log($"{this.gameObject.name} : {this}");
					DestroyImmediate(gameObject);
				}
			}
		}

		/// <summary>
		/// Awakeで処理したい初期化処理
		/// </summary>
		protected virtual void Init() { }

		/// <summary>
		/// オブジェクト破棄時にインスタンスも破棄する
		/// </summary>
		protected void OnDestroy()
		{
			if (Instance == this)
			{
				_instance = null;
			}
		}


	}

}