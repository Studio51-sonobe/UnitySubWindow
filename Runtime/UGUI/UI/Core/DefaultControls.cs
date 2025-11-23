using System;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiWindow.UI
{
	public static class DefaultControls
	{
		static IFactoryControls m_CurrentFactory = DefaultRuntimeFactory.Default;
		public static IFactoryControls factory
		{
			get { return m_CurrentFactory; }
		#if UNITY_EDITOR
			set { m_CurrentFactory = value; }
		#endif
		}
		public interface IFactoryControls
		{
			GameObject CreateGameObject(string name, params Type[] components);
		}
		class DefaultRuntimeFactory : IFactoryControls
		{
			public static IFactoryControls Default = new DefaultRuntimeFactory();
			
			public GameObject CreateGameObject(string name, params Type[] components)
			{
				return new GameObject(name, components);
			}
		}
		public struct Resources
		{
			public Sprite standard;
			public Sprite background;
			public Sprite inputField;
			public Sprite knob;
			public Sprite checkmark;
			public Sprite dropdown;
			public Sprite mask;
		}
		const float  kWidth       = 160f;
		const float  kThickHeight = 30f;
		const float  kThinHeight  = 20f;
		static Vector2 s_ThickElementSize       = new Vector2(kWidth, kThickHeight);
		static Vector2 s_ThinElementSize        = new Vector2(kWidth, kThinHeight);
		static Vector2 s_ImageElementSize       = new Vector2(100f, 100f);
		static Color   s_DefaultSelectableColor = new Color(1f, 1f, 1f, 1f);
		static Color   s_PanelColor             = new Color(1f, 1f, 1f, 0.392f);
		static Color   s_TextColor              = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);
		
		static GameObject CreateUIElementRoot(string name, Vector2 size, params Type[] components)
		{
			GameObject child = factory.CreateGameObject(name, components);
			RectTransform rectTransform = child.GetComponent<RectTransform>();
			rectTransform.sizeDelta = size;
			return child;
		}
		static GameObject CreateUIObject(string name, GameObject parent, params Type[] components)
		{
			GameObject go = factory.CreateGameObject(name, components);
			SetParentAndAlign(go, parent);
			return go;
		}
		static void SetDefaultTextValues( UnityEngine.UI.Text lbl)
		{
			lbl.color = s_TextColor;
			
			if (lbl.font == null)
			{
				lbl.font = UnityEngine.Resources.GetBuiltinResource<Font>( "LegacyRuntime.ttf");
			}
		}
		static void SetDefaultTextValues( TMPro.TMP_Text lbl)
		{
			lbl.color = s_TextColor;
			lbl.fontSize = 14;
		}
		static void SetDefaultColorTransitionValues(Selectable slider)
		{
			UnityEngine.UI.ColorBlock colors = slider.colors;
			colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
			colors.pressedColor     = new Color(0.698f, 0.698f, 0.698f);
			colors.disabledColor    = new Color(0.521f, 0.521f, 0.521f);
		}
		static void SetParentAndAlign(GameObject child, GameObject parent)
		{
			if (parent == null)
			{
				return;
			}
		#if UNITY_EDITOR
			Undo.SetTransformParent(child.transform, parent.transform, "");
		#else
			child.transform.SetParent(parent.transform, false);
		#endif
			SetLayerRecursively(child, parent.layer);
		}
		static void SetLayerRecursively( GameObject go, int layer)
		{
			go.layer = layer;
			Transform t = go.transform;
			for (int i = 0; i < t.childCount; i++)
				SetLayerRecursively(t.GetChild(i).gameObject, layer);
		}
		public static GameObject CreatePanel(Resources resources)
		{
			GameObject panelRoot = CreateUIElementRoot("Panel", s_ThickElementSize, typeof(Image));
			
			RectTransform rectTransform = panelRoot.GetComponent<RectTransform>();
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform.sizeDelta = Vector2.zero;
			
			Image image = panelRoot.GetComponent<Image>();
			image.sprite = resources.background;
			image.type = Image.Type.Sliced;
			image.color = s_PanelColor;
			
			return panelRoot;
		}
		public static GameObject CreateButtonLegacy(Resources resources)
		{
			GameObject buttonRoot = CreateUIElementRoot("Button (Legacy)", s_ThickElementSize, typeof(Image), typeof(Button));
			GameObject childText = CreateUIObject("Text (Legacy)", buttonRoot, typeof(UnityEngine.UI.Text));
			
			Image image = buttonRoot.GetComponent<Image>();
			image.sprite = resources.standard;
			image.type = Image.Type.Sliced;
			image.color = s_DefaultSelectableColor;
			
			Button bt = buttonRoot.GetComponent<Button>();
			SetDefaultColorTransitionValues(bt);
			
			UnityEngine.UI.Text text = childText.GetComponent<UnityEngine.UI.Text>();
			text.text = "Button";
			text.alignment = TextAnchor.MiddleCenter;
			SetDefaultTextValues( text);
			
			RectTransform textRectTransform = childText.GetComponent<RectTransform>();
			textRectTransform.anchorMin = Vector2.zero;
			textRectTransform.anchorMax = Vector2.one;
			textRectTransform.sizeDelta = Vector2.zero;
			return buttonRoot;
		}
		public static GameObject CreateButtonPro(Resources resources)
		{
			GameObject buttonRoot = CreateUIElementRoot( "Button", s_ThickElementSize, typeof(Image), typeof(Button));
			GameObject childText = CreateUIObject("Text (TMP)", buttonRoot, typeof(TMPro.TextMeshProUGUI));
			
			Image image = buttonRoot.GetComponent<Image>();
			image.sprite = resources.standard;
			image.type = Image.Type.Sliced;
			image.color = s_DefaultSelectableColor;
			
			Button bt = buttonRoot.GetComponent<Button>();
			SetDefaultColorTransitionValues(bt);
			
			TMPro.TextMeshProUGUI text = childText.GetComponent<TMPro.TextMeshProUGUI>();
			text.text = "Button";
			text.alignment = TMPro.TextAlignmentOptions.Center;
			SetDefaultTextValues( text);
			
			RectTransform textRectTransform = childText.GetComponent<RectTransform>();
			textRectTransform.anchorMin = Vector2.zero;
			textRectTransform.anchorMax = Vector2.one;
			textRectTransform.sizeDelta = Vector2.zero;
			
			return buttonRoot;
		}
		public static GameObject CreateImage(Resources resources)
		{
			GameObject go = CreateUIElementRoot("Image", s_ImageElementSize, typeof(Image));
			return go;
		}
/*
		public static GameObject CreateRawImage(Resources resources)
		{
			GameObject go = CreateUIElementRoot("RawImage", s_ImageElementSize, typeof(RawImage));
			return go;
		}
*/
		public static GameObject CreateSlider(Resources resources)
		{
			GameObject root = CreateUIElementRoot("Slider", s_ThinElementSize, typeof(Slider));	
			GameObject background = CreateUIObject("Background", root, typeof(Image));
			GameObject fillArea = CreateUIObject("Fill Area", root, typeof(RectTransform));
			GameObject fill = CreateUIObject("Fill", fillArea, typeof(Image));
			GameObject handleArea = CreateUIObject("Handle Slide Area", root, typeof(RectTransform));
			GameObject handle = CreateUIObject("Handle", handleArea, typeof(Image));
			
			Image backgroundImage = background.GetComponent<Image>();
			backgroundImage.sprite = resources.background;
			backgroundImage.type = Image.Type.Sliced;
			backgroundImage.color = s_DefaultSelectableColor;
			RectTransform backgroundRect = background.GetComponent<RectTransform>();
			backgroundRect.anchorMin = new Vector2(0, 0.25f);
			backgroundRect.anchorMax = new Vector2(1, 0.75f);
			backgroundRect.sizeDelta = new Vector2(0, 0);
			
			RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
			fillAreaRect.anchorMin = new Vector2(0, 0.25f);
			fillAreaRect.anchorMax = new Vector2(1, 0.75f);
			fillAreaRect.anchoredPosition = new Vector2(-5, 0);
			fillAreaRect.sizeDelta = new Vector2(-20, 0);
			
			Image fillImage = fill.GetComponent<Image>();
			fillImage.sprite = resources.standard;
			fillImage.type = Image.Type.Sliced;
			fillImage.color = s_DefaultSelectableColor;
			
			RectTransform fillRect = fill.GetComponent<RectTransform>();
			fillRect.sizeDelta = new Vector2(10, 0);
			
			RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
			handleAreaRect.sizeDelta = new Vector2(-20, 0);
			handleAreaRect.anchorMin = new Vector2(0, 0);
			handleAreaRect.anchorMax = new Vector2(1, 1);
			
			Image handleImage = handle.GetComponent<Image>();
			handleImage.sprite = resources.knob;
			handleImage.color = s_DefaultSelectableColor;
			
			RectTransform handleRect = handle.GetComponent<RectTransform>();
			handleRect.sizeDelta = new Vector2(20, 0);
			
			Slider slider = root.GetComponent<Slider>();
			slider.fillRect = fill.GetComponent<RectTransform>();
			slider.handleRect = handle.GetComponent<RectTransform>();
			slider.targetGraphic = handleImage;
			slider.direction = Slider.Direction.LeftToRight;
			SetDefaultColorTransitionValues(slider);
			return root;
		}
		public static GameObject CreateScrollbar(Resources resources)
		{
			GameObject scrollbarRoot = CreateUIElementRoot("Scrollbar", s_ThinElementSize, typeof(Image), typeof(Scrollbar));
			GameObject sliderArea = CreateUIObject("Sliding Area", scrollbarRoot, typeof(RectTransform));
			GameObject handle = CreateUIObject("Handle", sliderArea, typeof(Image));
			
			Image bgImage = scrollbarRoot.GetComponent<Image>();
			bgImage.sprite = resources.background;
			bgImage.type = Image.Type.Sliced;
			bgImage.color = s_DefaultSelectableColor;
			
			Image handleImage = handle.GetComponent<Image>();
			handleImage.sprite = resources.standard;
			handleImage.type = Image.Type.Sliced;
			handleImage.color = s_DefaultSelectableColor;
			
			RectTransform sliderAreaRect = sliderArea.GetComponent<RectTransform>();
			sliderAreaRect.sizeDelta = new Vector2(-20, -20);
			sliderAreaRect.anchorMin = Vector2.zero;
			sliderAreaRect.anchorMax = Vector2.one;
			
			RectTransform handleRect = handle.GetComponent<RectTransform>();
			handleRect.sizeDelta = new Vector2(20, 20);
			
			Scrollbar scrollbar = scrollbarRoot.GetComponent<Scrollbar>();
			scrollbar.handleRect = handleRect;
			scrollbar.targetGraphic = handleImage;
			SetDefaultColorTransitionValues(scrollbar);
			
			return scrollbarRoot;
		}
		public static GameObject CreateToggle(Resources resources)
		{
			GameObject toggleRoot = CreateUIElementRoot("Toggle", s_ThinElementSize, typeof(Toggle));
			GameObject background = CreateUIObject("Background", toggleRoot, typeof(Image));
			GameObject checkmark = CreateUIObject("Checkmark", background, typeof(Image));
			GameObject childLabel = CreateUIObject("Label", toggleRoot, typeof(UnityEngine.UI.Text));
			
			Toggle toggle = toggleRoot.GetComponent<Toggle>();
			toggle.isOn = true;
			
			Image bgImage = background.GetComponent<Image>();
			bgImage.sprite = resources.standard;
			bgImage.type = Image.Type.Sliced;
			bgImage.color = s_DefaultSelectableColor;
			
			Image checkmarkImage = checkmark.GetComponent<Image>();
			checkmarkImage.sprite = resources.checkmark;
			
			UnityEngine.UI.Text label = childLabel.GetComponent<UnityEngine.UI.Text>();
			label.text = "Toggle";
			SetDefaultTextValues(label);
			
			toggle.graphic = checkmarkImage;
			toggle.targetGraphic = bgImage;
			SetDefaultColorTransitionValues(toggle);
			
			RectTransform bgRect = background.GetComponent<RectTransform>();
			bgRect.anchorMin        = new Vector2(0f, 1f);
			bgRect.anchorMax        = new Vector2(0f, 1f);
			bgRect.anchoredPosition = new Vector2(10f, -10f);
			bgRect.sizeDelta        = new Vector2(kThinHeight, kThinHeight);
			
			RectTransform checkmarkRect = checkmark.GetComponent<RectTransform>();
			checkmarkRect.anchorMin = new Vector2(0.5f, 0.5f);
			checkmarkRect.anchorMax = new Vector2(0.5f, 0.5f);
			checkmarkRect.anchoredPosition = Vector2.zero;
			checkmarkRect.sizeDelta = new Vector2(20f, 20f);
			
			RectTransform labelRect = childLabel.GetComponent<RectTransform>();
			labelRect.anchorMin     = new Vector2(0f, 0f);
			labelRect.anchorMax     = new Vector2(1f, 1f);
			labelRect.offsetMin     = new Vector2(23f, 1f);
			labelRect.offsetMax     = new Vector2(-5f, -2f);
			return toggleRoot;
		}
		public static GameObject CreateInputFieldLegacy(Resources resources)
		{
			GameObject root = CreateUIElementRoot("InputField (Legacy)", s_ThickElementSize, typeof(Image), typeof(InputField));

			GameObject childPlaceholder = CreateUIObject("Placeholder", root, typeof(UnityEngine.UI.Text));
			GameObject childText = CreateUIObject("Text (Legacy)", root, typeof(UnityEngine.UI.Text));

			Image image = root.GetComponent<Image>();
			image.sprite = resources.inputField;
			image.type = Image.Type.Sliced;
			image.color = s_DefaultSelectableColor;

			InputField inputField = root.GetComponent<InputField>();
			SetDefaultColorTransitionValues(inputField);

			UnityEngine.UI.Text text = childText.GetComponent<UnityEngine.UI.Text>();
			text.text = "";
			text.supportRichText = false;
			SetDefaultTextValues(text);

			UnityEngine.UI.Text placeholder = childPlaceholder.GetComponent<UnityEngine.UI.Text>();
			placeholder.text = "Enter text...";
			placeholder.fontStyle = FontStyle.Italic;
			// Make placeholder color half as opaque as normal text color.
			Color placeholderColor = text.color;
			placeholderColor.a *= 0.5f;
			placeholder.color = placeholderColor;

			RectTransform textRectTransform = childText.GetComponent<RectTransform>();
			textRectTransform.anchorMin = Vector2.zero;
			textRectTransform.anchorMax = Vector2.one;
			textRectTransform.sizeDelta = Vector2.zero;
			textRectTransform.offsetMin = new Vector2(10, 6);
			textRectTransform.offsetMax = new Vector2(-10, -7);

			RectTransform placeholderRectTransform = childPlaceholder.GetComponent<RectTransform>();
			placeholderRectTransform.anchorMin = Vector2.zero;
			placeholderRectTransform.anchorMax = Vector2.one;
			placeholderRectTransform.sizeDelta = Vector2.zero;
			placeholderRectTransform.offsetMin = new Vector2(10, 6);
			placeholderRectTransform.offsetMax = new Vector2(-10, -7);

			inputField.textComponent = text;
			inputField.placeholder = placeholder;

			return root;
		}
		public static GameObject CreateInputFieldPro(Resources resources)
		{
			GameObject root = CreateUIElementRoot("InputField (TMP)", s_ThickElementSize, typeof(Image), typeof(TMP_InputField));
			GameObject textArea = CreateUIObject("Text Area", root, typeof(RectMask2D));
			GameObject childPlaceholder = CreateUIObject("Placeholder", textArea, typeof(TMPro.TextMeshProUGUI), typeof(UnityEngine.UI.LayoutElement));
			GameObject childText = CreateUIObject("Text", textArea, typeof( TMPro.TextMeshProUGUI));
			
			Image image = root.GetComponent<Image>();
			image.sprite = resources.inputField;
			image.type = Image.Type.Sliced;
			image.color = s_DefaultSelectableColor;
			
			TMP_InputField inputField = root.GetComponent<TMP_InputField>();
			SetDefaultColorTransitionValues(inputField);
			
			RectMask2D rectMask = textArea.GetComponent<RectMask2D>();
			rectMask.padding = new Vector4(-8, -5, -8, -5);
			
			RectTransform textAreaRectTransform = textArea.GetComponent<RectTransform>();
			textAreaRectTransform.anchorMin = Vector2.zero;
			textAreaRectTransform.anchorMax = Vector2.one;
			textAreaRectTransform.sizeDelta = Vector2.zero;
			textAreaRectTransform.offsetMin = new Vector2(10, 6);
			textAreaRectTransform.offsetMax = new Vector2(-10, -7);
			
			TMPro.TextMeshProUGUI text = childText.GetComponent<TMPro.TextMeshProUGUI>();
			text.text = "";
			text.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
			text.extraPadding = true;
			text.richText = true;
			SetDefaultTextValues(text);

			TMPro.TextMeshProUGUI placeholder = childPlaceholder.GetComponent<TMPro.TextMeshProUGUI>();
			placeholder.text = "Enter text...";
			placeholder.fontSize = 14;
			placeholder.fontStyle = TMPro.FontStyles.Italic;
			placeholder.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
			placeholder.extraPadding = true;

			// Make placeholder color half as opaque as normal text color.
			Color placeholderColor = text.color;
			placeholderColor.a *= 0.5f;
			placeholder.color = placeholderColor;

			// Add Layout component to placeholder.
			UnityEngine.UI.LayoutElement placeholderLayout = childPlaceholder.GetComponent<UnityEngine.UI.LayoutElement>();
			placeholderLayout.ignoreLayout = true;

			RectTransform textRectTransform = childText.GetComponent<RectTransform>();
			textRectTransform.anchorMin = Vector2.zero;
			textRectTransform.anchorMax = Vector2.one;
			textRectTransform.sizeDelta = Vector2.zero;
			textRectTransform.offsetMin = new Vector2(0, 0);
			textRectTransform.offsetMax = new Vector2(0, 0);

			RectTransform placeholderRectTransform = childPlaceholder.GetComponent<RectTransform>();
			placeholderRectTransform.anchorMin = Vector2.zero;
			placeholderRectTransform.anchorMax = Vector2.one;
			placeholderRectTransform.sizeDelta = Vector2.zero;
			placeholderRectTransform.offsetMin = new Vector2(0, 0);
			placeholderRectTransform.offsetMax = new Vector2(0, 0);

			inputField.textViewport = textAreaRectTransform;
			inputField.textComponent = text;
			inputField.placeholder = placeholder;
			inputField.fontAsset = text.font;

			return root;
		}
		public static GameObject CreateDropdownLegacy(Resources resources)
		{
			GameObject root = CreateUIElementRoot("Dropdown (Legacy)", s_ThickElementSize, typeof(Image), typeof(Dropdown));
			GameObject label = CreateUIObject("Label", root, typeof(UnityEngine.UI.Text));
			GameObject arrow = CreateUIObject("Arrow", root, typeof(Image));
			GameObject template = CreateUIObject("Template", root, typeof(Image), typeof(ScrollRect));
			GameObject viewport = CreateUIObject("Viewport", template, typeof(Image), typeof(Mask));
			GameObject content = CreateUIObject("Content", viewport, typeof(RectTransform));
			GameObject item = CreateUIObject("Item", content, typeof(Toggle));
			GameObject itemBackground = CreateUIObject("Item Background", item, typeof(Image));
			GameObject itemCheckmark = CreateUIObject("Item Checkmark", item, typeof(Image));
			GameObject itemLabel = CreateUIObject("Item Label", item, typeof(UnityEngine.UI.Text));
			
			GameObject scrollbar = CreateScrollbar(resources);
			scrollbar.name = "Scrollbar";
			SetParentAndAlign(scrollbar, template);
			
			Scrollbar scrollbarScrollbar = scrollbar.GetComponent<Scrollbar>();
			scrollbarScrollbar.SetDirection(Scrollbar.Direction.BottomToTop, true);
			
			RectTransform vScrollbarRT = scrollbar.GetComponent<RectTransform>();
			vScrollbarRT.anchorMin = Vector2.right;
			vScrollbarRT.anchorMax = Vector2.one;
			vScrollbarRT.pivot = Vector2.one;
			vScrollbarRT.sizeDelta = new Vector2(vScrollbarRT.sizeDelta.x, 0);
			
			UnityEngine.UI.Text itemLabelText = itemLabel.GetComponent<UnityEngine.UI.Text>();
			SetDefaultTextValues(itemLabelText);
			itemLabelText.alignment = TextAnchor.MiddleLeft;
			
			Image itemBackgroundImage = itemBackground.GetComponent<Image>();
			itemBackgroundImage.color = new Color32(245, 245, 245, 255);
			
			Image itemCheckmarkImage = itemCheckmark.GetComponent<Image>();
			itemCheckmarkImage.sprite = resources.checkmark;
			
			Toggle itemToggle = item.GetComponent<Toggle>();
			itemToggle.targetGraphic = itemBackgroundImage;
			itemToggle.graphic = itemCheckmarkImage;
			itemToggle.isOn = true;
			
			Image templateImage = template.GetComponent<Image>();
			templateImage.sprite = resources.standard;
			templateImage.type = Image.Type.Sliced;
			
			ScrollRect templateScrollRect = template.GetComponent<ScrollRect>();
			templateScrollRect.content = content.GetComponent<RectTransform>();
			templateScrollRect.viewport = viewport.GetComponent<RectTransform>();
			templateScrollRect.horizontal = false;
			templateScrollRect.movementType = ScrollRect.MovementType.Clamped;
			templateScrollRect.verticalScrollbar = scrollbarScrollbar;
			templateScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
			templateScrollRect.verticalScrollbarSpacing = -3;
			
			Mask scrollRectMask = viewport.GetComponent<Mask>();
			scrollRectMask.showMaskGraphic = false;
			
			Image viewportImage = viewport.GetComponent<Image>();
			viewportImage.sprite = resources.mask;
			viewportImage.type = Image.Type.Sliced;
			
			UnityEngine.UI.Text labelText = label.GetComponent<UnityEngine.UI.Text>();
			SetDefaultTextValues(labelText);
			labelText.alignment = TextAnchor.MiddleLeft;
			
			Image arrowImage = arrow.GetComponent<Image>();
			arrowImage.sprite = resources.dropdown;
			
			Image backgroundImage = root.GetComponent<Image>();
			backgroundImage.sprite = resources.standard;
			backgroundImage.color = s_DefaultSelectableColor;
			backgroundImage.type = Image.Type.Sliced;
			
			Dropdown dropdown = root.GetComponent<Dropdown>();
			dropdown.targetGraphic = backgroundImage;
			SetDefaultColorTransitionValues(dropdown);
			dropdown.template = template.GetComponent<RectTransform>();
			dropdown.captionText = labelText;
			dropdown.itemText = itemLabelText;
			
			itemLabelText.text = "Option A";
			dropdown.options.Add(new Dropdown.OptionData {text = "Option A"});
			dropdown.options.Add(new Dropdown.OptionData {text = "Option B"});
			dropdown.options.Add(new Dropdown.OptionData {text = "Option C"});
			dropdown.RefreshShownValue();
			
			RectTransform labelRT = label.GetComponent<RectTransform>();
			labelRT.anchorMin           = Vector2.zero;
			labelRT.anchorMax           = Vector2.one;
			labelRT.offsetMin           = new Vector2(10, 6);
			labelRT.offsetMax           = new Vector2(-25, -7);
			
			RectTransform arrowRT = arrow.GetComponent<RectTransform>();
			arrowRT.anchorMin           = new Vector2(1, 0.5f);
			arrowRT.anchorMax           = new Vector2(1, 0.5f);
			arrowRT.sizeDelta           = new Vector2(20, 20);
			arrowRT.anchoredPosition    = new Vector2(-15, 0);
			
			RectTransform templateRT = template.GetComponent<RectTransform>();
			templateRT.anchorMin        = new Vector2(0, 0);
			templateRT.anchorMax        = new Vector2(1, 0);
			templateRT.pivot            = new Vector2(0.5f, 1);
			templateRT.anchoredPosition = new Vector2(0, 2);
			templateRT.sizeDelta        = new Vector2(0, 150);
			
			RectTransform viewportRT = viewport.GetComponent<RectTransform>();
			viewportRT.anchorMin        = new Vector2(0, 0);
			viewportRT.anchorMax        = new Vector2(1, 1);
			viewportRT.sizeDelta        = new Vector2(-18, 0);
			viewportRT.pivot            = new Vector2(0, 1);
			
			RectTransform contentRT = content.GetComponent<RectTransform>();
			contentRT.anchorMin         = new Vector2(0f, 1);
			contentRT.anchorMax         = new Vector2(1f, 1);
			contentRT.pivot             = new Vector2(0.5f, 1);
			contentRT.anchoredPosition  = new Vector2(0, 0);
			contentRT.sizeDelta         = new Vector2(0, 28);
			
			RectTransform itemRT = item.GetComponent<RectTransform>();
			itemRT.anchorMin            = new Vector2(0, 0.5f);
			itemRT.anchorMax            = new Vector2(1, 0.5f);
			itemRT.sizeDelta            = new Vector2(0, 20);
			
			RectTransform itemBackgroundRT = itemBackground.GetComponent<RectTransform>();
			itemBackgroundRT.anchorMin  = Vector2.zero;
			itemBackgroundRT.anchorMax  = Vector2.one;
			itemBackgroundRT.sizeDelta  = Vector2.zero;
			
			RectTransform itemCheckmarkRT = itemCheckmark.GetComponent<RectTransform>();
			itemCheckmarkRT.anchorMin   = new Vector2(0, 0.5f);
			itemCheckmarkRT.anchorMax   = new Vector2(0, 0.5f);
			itemCheckmarkRT.sizeDelta   = new Vector2(20, 20);
			itemCheckmarkRT.anchoredPosition = new Vector2(10, 0);
			
			RectTransform itemLabelRT = itemLabel.GetComponent<RectTransform>();
			itemLabelRT.anchorMin       = Vector2.zero;
			itemLabelRT.anchorMax       = Vector2.one;
			itemLabelRT.offsetMin       = new Vector2(20, 1);
			itemLabelRT.offsetMax       = new Vector2(-10, -2);
			
			template.SetActive(false);
			return root;
		}
		public static GameObject CreateDropdownPro(Resources resources)
		{
			GameObject root = CreateUIElementRoot("Dropdown", s_ThickElementSize, typeof(Image), typeof(TMP_Dropdown));
			GameObject label = CreateUIObject("Label", root, typeof(TMPro.TextMeshProUGUI));
			GameObject arrow = CreateUIObject("Arrow", root, typeof(Image));
			GameObject template = CreateUIObject("Template", root, typeof(Image), typeof(ScrollRect));
			GameObject viewport = CreateUIObject("Viewport", template, typeof(Image), typeof(Mask));
			GameObject content = CreateUIObject("Content", viewport, typeof(RectTransform));
			GameObject item = CreateUIObject("Item", content, typeof(Toggle));
			GameObject itemBackground = CreateUIObject("Item Background", item, typeof(Image));
			GameObject itemCheckmark = CreateUIObject("Item Checkmark", item, typeof(Image));
			GameObject itemLabel = CreateUIObject("Item Label", item, typeof(TMPro.TextMeshProUGUI));
			
			GameObject scrollbar = CreateScrollbar(resources);
			scrollbar.name = "Scrollbar";
			SetParentAndAlign(scrollbar, template);
			
			Scrollbar scrollbarScrollbar = scrollbar.GetComponent<Scrollbar>();
			scrollbarScrollbar.SetDirection(Scrollbar.Direction.BottomToTop, true);
			
			RectTransform vScrollbarRT = scrollbar.GetComponent<RectTransform>();
			vScrollbarRT.anchorMin = Vector2.right;
			vScrollbarRT.anchorMax = Vector2.one;
			vScrollbarRT.pivot = Vector2.one;
			vScrollbarRT.sizeDelta = new Vector2(vScrollbarRT.sizeDelta.x, 0);
			
			TMPro.TextMeshProUGUI itemLabelText = itemLabel.GetComponent<TMPro.TextMeshProUGUI>();
			SetDefaultTextValues(itemLabelText);
			itemLabelText.alignment = TMPro.TextAlignmentOptions.Left;
			
			Image itemBackgroundImage = itemBackground.GetComponent<Image>();
			itemBackgroundImage.color = new Color32(245, 245, 245, 255);
			
			Image itemCheckmarkImage = itemCheckmark.GetComponent<Image>();
			itemCheckmarkImage.sprite = resources.checkmark;
			
			Toggle itemToggle = item.GetComponent<Toggle>();
			itemToggle.targetGraphic = itemBackgroundImage;
			itemToggle.graphic = itemCheckmarkImage;
			itemToggle.isOn = true;
			
			Image templateImage = template.GetComponent<Image>();
			templateImage.sprite = resources.standard;
			templateImage.type = Image.Type.Sliced;
			
			ScrollRect templateScrollRect = template.GetComponent<ScrollRect>();
			templateScrollRect.content = content.GetComponent<RectTransform>();
			templateScrollRect.viewport = viewport.GetComponent<RectTransform>();
			templateScrollRect.horizontal = false;
			templateScrollRect.movementType = ScrollRect.MovementType.Clamped;
			templateScrollRect.verticalScrollbar = scrollbarScrollbar;
			templateScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
			templateScrollRect.verticalScrollbarSpacing = -3;
			
			Mask scrollRectMask = viewport.GetComponent<Mask>();
			scrollRectMask.showMaskGraphic = false;
			
			Image viewportImage = viewport.GetComponent<Image>();
			viewportImage.sprite = resources.mask;
			viewportImage.type = Image.Type.Sliced;
			
			TMPro.TextMeshProUGUI labelText = label.GetComponent<TMPro.TextMeshProUGUI>();
			SetDefaultTextValues(labelText);
			labelText.alignment = TMPro.TextAlignmentOptions.Left;
			
			Image arrowImage = arrow.GetComponent<Image>();
			arrowImage.sprite = resources.dropdown;
			
			Image backgroundImage = root.GetComponent<Image>();
			backgroundImage.sprite = resources.standard;
			backgroundImage.color = s_DefaultSelectableColor;
			backgroundImage.type = Image.Type.Sliced;
			
			TMP_Dropdown dropdown = root.GetComponent<TMP_Dropdown>();
			dropdown.targetGraphic = backgroundImage;
			SetDefaultColorTransitionValues(dropdown);
			dropdown.template = template.GetComponent<RectTransform>();
			dropdown.captionText = labelText;
			dropdown.itemText = itemLabelText;
			
			itemLabelText.text = "Option A";
			dropdown.options.Add( new TMP_Dropdown.OptionData {text = "Option A" });
			dropdown.options.Add( new TMP_Dropdown.OptionData {text = "Option B" });
			dropdown.options.Add( new TMP_Dropdown.OptionData {text = "Option C" });
			dropdown.RefreshShownValue();
			
			RectTransform labelRT = label.GetComponent<RectTransform>();
			labelRT.anchorMin = Vector2.zero;
			labelRT.anchorMax = Vector2.one;
			labelRT.offsetMin = new Vector2(10, 6);
			labelRT.offsetMax = new Vector2(-25, -7);
			
			RectTransform arrowRT = arrow.GetComponent<RectTransform>();
			arrowRT.anchorMin = new Vector2(1, 0.5f);
			arrowRT.anchorMax = new Vector2(1, 0.5f);
			arrowRT.sizeDelta = new Vector2(20, 20);
			arrowRT.anchoredPosition = new Vector2(-15, 0);
			
			RectTransform templateRT = template.GetComponent<RectTransform>();
			templateRT.anchorMin = new Vector2(0, 0);
			templateRT.anchorMax = new Vector2(1, 0);
			templateRT.pivot = new Vector2(0.5f, 1);
			templateRT.anchoredPosition = new Vector2(0, 2);
			templateRT.sizeDelta = new Vector2(0, 150);
			
			RectTransform viewportRT = viewport.GetComponent<RectTransform>();
			viewportRT.anchorMin = new Vector2(0, 0);
			viewportRT.anchorMax = new Vector2(1, 1);
			viewportRT.sizeDelta = new Vector2(-18, 0);
			viewportRT.pivot = new Vector2(0, 1);
			
			RectTransform contentRT = content.GetComponent<RectTransform>();
			contentRT.anchorMin = new Vector2(0f, 1);
			contentRT.anchorMax = new Vector2(1f, 1);
			contentRT.pivot = new Vector2(0.5f, 1);
			contentRT.anchoredPosition = new Vector2(0, 0);
			contentRT.sizeDelta = new Vector2(0, 28);
			
			RectTransform itemRT = item.GetComponent<RectTransform>();
			itemRT.anchorMin = new Vector2(0, 0.5f);
			itemRT.anchorMax = new Vector2(1, 0.5f);
			itemRT.sizeDelta = new Vector2(0, 20);
			
			RectTransform itemBackgroundRT = itemBackground.GetComponent<RectTransform>();
			itemBackgroundRT.anchorMin = Vector2.zero;
			itemBackgroundRT.anchorMax = Vector2.one;
			itemBackgroundRT.sizeDelta = Vector2.zero;
			
			RectTransform itemCheckmarkRT = itemCheckmark.GetComponent<RectTransform>();
			itemCheckmarkRT.anchorMin = new Vector2(0, 0.5f);
			itemCheckmarkRT.anchorMax = new Vector2(0, 0.5f);
			itemCheckmarkRT.sizeDelta = new Vector2(20, 20);
			itemCheckmarkRT.anchoredPosition = new Vector2(10, 0);
			
			RectTransform itemLabelRT = itemLabel.GetComponent<RectTransform>();
			itemLabelRT.anchorMin = Vector2.zero;
			itemLabelRT.anchorMax = Vector2.one;
			itemLabelRT.offsetMin = new Vector2(20, 1);
			itemLabelRT.offsetMax = new Vector2(-10, -2);
			template.SetActive(false);
			return root;
		}
		public static GameObject CreateScrollView(Resources resources)
		{
			GameObject root = CreateUIElementRoot("Scroll View", new Vector2(200, 200), typeof(Image), typeof(ScrollRect));
			GameObject viewport = CreateUIObject("Viewport", root, typeof(Image), typeof(Mask));
			GameObject content = CreateUIObject("Content", viewport, typeof(RectTransform));
			GameObject hScrollbar = CreateScrollbar(resources);
			
			hScrollbar.name = "Scrollbar Horizontal";
			SetParentAndAlign(hScrollbar, root);
			RectTransform hScrollbarRT = hScrollbar.GetComponent<RectTransform>();
			hScrollbarRT.anchorMin = Vector2.zero;
			hScrollbarRT.anchorMax = Vector2.right;
			hScrollbarRT.pivot = Vector2.zero;
			hScrollbarRT.sizeDelta = new Vector2(0, hScrollbarRT.sizeDelta.y);
			
			GameObject vScrollbar = CreateScrollbar(resources);
			vScrollbar.name = "Scrollbar Vertical";
			SetParentAndAlign(vScrollbar, root);
			vScrollbar.GetComponent<Scrollbar>().SetDirection(Scrollbar.Direction.BottomToTop, true);
			RectTransform vScrollbarRT = vScrollbar.GetComponent<RectTransform>();
			vScrollbarRT.anchorMin = Vector2.right;
			vScrollbarRT.anchorMax = Vector2.one;
			vScrollbarRT.pivot = Vector2.one;
			vScrollbarRT.sizeDelta = new Vector2(vScrollbarRT.sizeDelta.x, 0);
			
			RectTransform viewportRT = viewport.GetComponent<RectTransform>();
			viewportRT.anchorMin = Vector2.zero;
			viewportRT.anchorMax = Vector2.one;
			viewportRT.sizeDelta = Vector2.zero;
			viewportRT.pivot = Vector2.up;
			
			RectTransform contentRT = content.GetComponent<RectTransform>();
			contentRT.anchorMin = Vector2.up;
			contentRT.anchorMax = Vector2.one;
			contentRT.sizeDelta = new Vector2(0, 300);
			contentRT.pivot = Vector2.up;
			
			ScrollRect scrollRect = root.GetComponent<ScrollRect>();
			scrollRect.content = contentRT;
			scrollRect.viewport = viewportRT;
			scrollRect.horizontalScrollbar = hScrollbar.GetComponent<Scrollbar>();
			scrollRect.verticalScrollbar = vScrollbar.GetComponent<Scrollbar>();
			scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
			scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
			scrollRect.horizontalScrollbarSpacing = -3;
			scrollRect.verticalScrollbarSpacing = -3;
			
			Image rootImage = root.GetComponent<Image>();
			rootImage.sprite = resources.background;
			rootImage.type = Image.Type.Sliced;
			rootImage.color = s_PanelColor;
			
			Mask viewportMask = viewport.GetComponent<Mask>();
			viewportMask.showMaskGraphic = false;
			
			Image viewportImage = viewport.GetComponent<Image>();
			viewportImage.sprite = resources.mask;
			viewportImage.type = Image.Type.Sliced;
			return root;
		}
	}
}
