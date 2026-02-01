using System;
using UnityEditor;
using UnityEngine;

namespace OriginalLib.EditorSuport
{
	public static class HierarchyGUI_ShowComponent
	{
		private const int ICON_SIZE = 16;

		private static bool IconShow;

		[InitializeOnLoadMethod]
		private static void Initialize()
		{

			IconShow = Convert.ToBoolean(EditorUserSettings.GetConfigValue("ComponentIcon"));
			EditorApplication.delayCall +=
				() =>
				{
					Menu.SetChecked("OriginalLib/ComponentIcon", IconShow);
				};

			EditorApplication.hierarchyWindowItemOnGUI += OnGUI;
		}

		private static void OnGUI(int instanceID, Rect selectionRect)
		{
			if (!IconShow) return;

			//Texture scriptIcon = EditorGUIUtility.IconContent("cs Script Icon").image;
			// instanceID をオブジェクト参照に変換
			var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
			if (go == null)
			{
				return;
			}

			// オブジェクトが所持しているコンポーネント一覧を取得
			var components = go.GetComponents<Component>();
			if (components.Length == 0)
			{
				return;
			}

			selectionRect.x = selectionRect.xMax - ICON_SIZE * components.Length;
			selectionRect.width = ICON_SIZE;

			foreach (var component in components)
			{
				// コンポーネントのアイコン画像を取得
				var texture2D = AssetPreview.GetMiniThumbnail(component);
				//if (texture2D == null) continue;
				if (texture2D != null)
				{
					GUI.DrawTexture(selectionRect, texture2D);
					selectionRect.x += ICON_SIZE;
				}

				if (component == null || HasMissingReference(component))
				{
					Texture warningIcon = EditorGUIUtility.IconContent("console.warnicon").image;
					GUI.DrawTexture(selectionRect, warningIcon);
					selectionRect.x += ICON_SIZE;
				}

			}
		}

		public static bool HasMissingReference(Component c)
		{
			{
				var so = new SerializedObject(c);
				var sp = so.GetIterator();

				while (sp.NextVisible(true))
				{
					if (sp.propertyType != SerializedPropertyType.ObjectReference) continue;
					if (sp.objectReferenceValue != null) continue;
					if (!sp.hasChildren) continue;
					var fileId = sp.FindPropertyRelative("m_FileID");
					if (fileId == null) continue;
					if (fileId.intValue == 0) continue;

					return true;
				}
			}

			return false;
		}


		[MenuItem("OriginalLib/ComponentIcon", priority = 10)]
		private static void ComponentIconShow()
		{
			IconShow = !IconShow;
			Menu.SetChecked("OriginalLib/ComponentIcon", IconShow);
			EditorUserSettings.SetConfigValue("ComponentIcon", IconShow.ToString());
		}



	}
}