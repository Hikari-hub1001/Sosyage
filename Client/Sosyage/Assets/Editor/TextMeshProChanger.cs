using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace OriginalLib.EditorSuport
{
	public class TextMeshProChanger
	{
		[MenuItem("OriginalLib/Text To Tmp", priority = 100)]
		public static void TextToTmp()
		{
			if (Selection.gameObjects.Length < 1) return;
			foreach (GameObject go in Selection.gameObjects)
			{
				ChangeText(go);
			}
		}
		[MenuItem("OriginalLib/Dropdown To Tmp", priority = 101)]
		public static void DropdownToTmp()
		{
			if (Selection.gameObjects.Length < 1) return;
			foreach (GameObject go in Selection.gameObjects)
			{
				ChangeDropdown(go);
			}
		}

		[MenuItem("OriginalLib/InputField To Tmp", priority = 102)]
		public static void InputFieldToTmp()
		{
			if (Selection.gameObjects.Length < 1) return;
			foreach (GameObject go in Selection.gameObjects)
			{
				ChangeInputField(go);
			}
		}

		[MenuItem("OriginalLib/All To Tmp %t", priority = 103)]
		public static void AllToTmp()
		{
			if (Selection.gameObjects.Length < 1) return;

			DropdownToTmp();
			InputFieldToTmp();

			TextToTmp();
		}

		private static void ChangeText(GameObject go)
		{
			if (go == null) return;
			var component = go.GetComponent<Text>();

			if (component == null) return;

			//テキスト、色、スタイル、サイズ、アラインを保持
			string text = component.text;
			Color color = component.color;
			FontStyle fontStyle = component.fontStyle;
			int size = component.fontSize;
			TextAnchor align = component.alignment;

			GameObject.DestroyImmediate(component);

			var component2 = go.AddComponent<TextMeshProUGUI>();

			component2.text = text;
			component2.color = color;
			component2.fontStyle =
				fontStyle switch
				{
					FontStyle.Normal => FontStyles.Normal,
					FontStyle.Bold => FontStyles.Bold,
					FontStyle.Italic => FontStyles.Italic,
					FontStyle.BoldAndItalic => FontStyles.Bold | FontStyles.Italic,
					_ => FontStyles.Normal
				};
			component2.fontSize = (float)size;
			component2.alignment =
				align switch
				{
					TextAnchor.UpperLeft => TextAlignmentOptions.TopLeft,
					TextAnchor.UpperCenter => TextAlignmentOptions.Top,
					TextAnchor.UpperRight => TextAlignmentOptions.TopRight,
					TextAnchor.MiddleLeft => TextAlignmentOptions.Left,
					TextAnchor.MiddleCenter => TextAlignmentOptions.Center,
					TextAnchor.MiddleRight => TextAlignmentOptions.Right,
					TextAnchor.LowerLeft => TextAlignmentOptions.BottomLeft,
					TextAnchor.LowerCenter => TextAlignmentOptions.Bottom,
					TextAnchor.LowerRight => TextAlignmentOptions.BottomRight,
					_ => TextAlignmentOptions.Center

				};

		}

		private static void ChangeDropdown(GameObject go)
		{
			if (go == null) return;
			var component = go.GetComponent<Dropdown>();
			if (component == null) return;

			bool interactable = component.interactable;
			Selectable.Transition transition = component.transition;
			Navigation navigation = component.navigation;
			Graphic targetGraphic = component.targetGraphic;
			ColorBlock colors = component.colors;
			SpriteState spriteState = component.spriteState;
			AnimationTriggers animationTriggers = component.animationTriggers;
			RectTransform template = component.template;
			Text captionText = component.captionText;
			Image captionImage = component.captionImage;
			Text itemText = component.itemText;
			Image itemImage = component.itemImage;
			int value = component.value;
			float alphaFadeSpeed = component.alphaFadeSpeed;
			List<Dropdown.OptionData> options = component.options;

			GameObject.DestroyImmediate(component);

			TMP_Dropdown component2 = go.AddComponent<TMP_Dropdown>();

			component2.interactable = interactable;
			component2.transition = transition;
			component2.navigation = navigation;
			component2.targetGraphic = targetGraphic;
			component2.colors = colors;
			component2.spriteState = spriteState;
			component2.animationTriggers = animationTriggers;
			component2.template = template;
			GameObject @object = captionText?.gameObject;
			ChangeText(@object);
			component2.captionText = @object.GetComponent<TextMeshProUGUI>();
			component2.captionImage = captionImage;
			@object = itemText?.gameObject;
			ChangeText(@object);
			component2.itemText = @object.GetComponent<TextMeshProUGUI>();
			component2.itemImage = itemImage;
			component2.alphaFadeSpeed = alphaFadeSpeed;

			component2.options = new List<TMP_Dropdown.OptionData>();
			foreach (var option in options)
			{
#if UNITY_6000
				component2.options.Add(new TMP_Dropdown.OptionData(option.text, option.image, Color.white));
#else
				component2.options.Add(new TMP_Dropdown.OptionData(option.text, option.image));
#endif
			}

			component2.value = value;

		}

		private static void ChangeInputField(GameObject go)
		{
			if (go == null) return;
			var component = go.GetComponent<InputField>();
			if (component == null) return;

			bool interactable = component.interactable;
			Selectable.Transition transition = component.transition;
			Navigation navigation = component.navigation;
			Graphic targetGraphic = component.targetGraphic;
			ColorBlock colors = component.colors;
			SpriteState spriteState = component.spriteState;
			AnimationTriggers animationTriggers = component.animationTriggers;
			Text textComponent = component.textComponent;
			string text = component.text;
			int characterLimit = component.characterLimit;
			InputField.ContentType contentType = component.contentType;
			InputField.LineType lineType = component.lineType;
			Graphic placeHolder = component.placeholder;
			float caretBlinkRate = component.caretBlinkRate;
			int caretWidth = component.caretWidth;
			bool customCaretColor = component.customCaretColor;
			Color caretColor = component.caretColor;
			Color selectionColor = component.selectionColor;
			bool hideMobileInput = component.shouldHideMobileInput;
			bool readOnly = component.readOnly;

			GameObject.DestroyImmediate(component);

			TMP_InputField component2 = go.AddComponent<TMP_InputField>();

			component2.interactable = interactable;
			component2.transition = transition;
			component2.navigation = navigation;
			component2.targetGraphic = targetGraphic;
			component2.colors = colors;
			component2.spriteState = spriteState;
			component2.animationTriggers = animationTriggers;
			GameObject @object = textComponent?.gameObject;
			ChangeText(@object);
			component2.textComponent = @object.GetComponent<TextMeshProUGUI>();
			component2.text = text;
			component2.characterLimit = characterLimit;
			component2.contentType = (TMP_InputField.ContentType)((int)contentType);
			component2.lineType = (TMP_InputField.LineType)((int)lineType);
			if (placeHolder is Text)
			{
				@object = placeHolder?.gameObject;
				ChangeText(@object);
				component2.placeholder = @object.GetComponent<TextMeshProUGUI>();
			}
			else
			{
				component2.placeholder = placeHolder;
			}
			LayoutElement element = component2.placeholder.gameObject.AddComponent<LayoutElement>();
			element.ignoreLayout = true;
			element.layoutPriority = 1;
			component2.caretBlinkRate = caretBlinkRate;
			component2.caretWidth = caretWidth;
			component2.customCaretColor = customCaretColor;
			component2.caretColor = caretColor;
			component2.selectionColor = selectionColor;
			component2.shouldHideMobileInput = hideMobileInput;
			component2.readOnly = readOnly;

			GameObject textArea = new();
			textArea.name = "Text Area";
			RectTransform rect = textArea.AddComponent<RectTransform>();
			rect.SetParent(go.transform);
			rect.localScale = Vector3.one;
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = component2.textComponent.rectTransform.offsetMin;
			rect.offsetMax = component2.textComponent.rectTransform.offsetMax;
			RectMask2D mask = textArea.AddComponent<RectMask2D>();
			mask.padding = new(-8, -8, -5, -5);

			component2.textComponent.rectTransform.SetParent(rect);
			component2.placeholder.rectTransform.SetParent(rect);

		}
	}
}
