//============================================================================================================================
//
// このクラスはシングルトンかつ、シーン切り替え時に破棄されないコンポーネントとなります。
// 任意のクラスに継承することでのみ使用可能です。
// ２つ以上のインスタンスが生成される際、あとから生成したインスタンスがゲームオブジェクトごと破棄されます。
// シーン切り替え時に破棄したい場合は「Singleton」クラスを使用してください。
//
// 使い方
// TestSingletonクラスを例とします。
// public class TestSingleton : Singleton_DontDestroy <TestSingleton>
//
//
// Awakeメソッドは基本的に実装せずにInitメソッドを使用してください。
// Initメソッドは基底クラスとなるSingletonクラスのAwakeメソッドで呼ばれており
// Awakeメソッドと同等の働きをします。
//
//============================================================================================================================

namespace OriginalLib.Behaviour
{
	/// <summary>
	/// シングルトンベースクラス
	/// DontDestroy向け
	/// </summary>
	/// <typeparam name="T">シングルトンにするクラス</typeparam>
	public abstract class Singleton_DontDestroy<T> : Singleton<T> where T : Singleton_DontDestroy<T>
	{
		protected new void Awake()
		{
			base.Awake();
			DontDestroyOnLoad(gameObject);
		}
	}
}