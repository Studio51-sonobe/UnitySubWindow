using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MultiWindow.EventSystems;
using MultiWindow.UI.CoroutineTween;

namespace MultiWindow.UI
{
	[RequireComponent( typeof( RectTransform))]
	[AddComponentMenu("MultiWindowUI/Dropdown - TextMeshPro", 35)]
	public class TMP_Dropdown : Selectable, IPointerClickHandler, ISubmitHandler, ICancelHandler
	{
		protected internal class DropdownItem : MonoBehaviour, IPointerEnterHandler, ICancelHandler
		{
			public TMPro.TMP_Text text
			{
				get { return m_Text; }
				set { m_Text = value; }
			}
			public Image image
			{
				get { return m_Image; }
				set { m_Image = value; }
			}
			public RectTransform rectTransform
			{
				get { return m_RectTransform; }
				set { m_RectTransform = value; }
			}
			public Toggle toggle
			{
				get { return m_Toggle; }
				set { m_Toggle = value; }
			}
			public virtual void OnPointerEnter( PointerEventData eventData)
			{
				eventData.EventSystem.SetSelectedGameObject( gameObject);
			}
			public virtual void OnCancel( BaseEventData eventData)
			{
				TMP_Dropdown dropdown = GetComponentInParent<TMP_Dropdown>();
				if (dropdown)
				{
					dropdown.Hide();
				}
			}
			[SerializeField]
			TMPro.TMP_Text m_Text;
			[SerializeField]
			Image m_Image;
			[SerializeField]
			RectTransform m_RectTransform;
			[SerializeField]
			Toggle m_Toggle;
		}
		[Serializable]
		public class OptionData
		{
			[SerializeField]
			private string m_Text;
			[SerializeField]
			private Sprite m_Image;
			[SerializeField]
			private Color m_Color = Color.white;
			
			public string text { get { return m_Text; } set { m_Text = value; } }
			public Sprite image { get { return m_Image; } set { m_Image = value; } }
			public Color color { get { return m_Color; } set { m_Color = value; } }
			
			public OptionData() { }
			
			public OptionData(string text)
			{
				this.text = text;
			}
			public OptionData(Sprite image)
			{
				this.image = image;
			}
			public OptionData(string text, Sprite image, Color color)
			{
				this.text = text;
				this.image = image;
				this.color = color;
			}
		}
		[Serializable]
		public class OptionDataList
		{
			[SerializeField]
			private List<OptionData> m_Options;
			public List<OptionData> options { get { return m_Options; } set { m_Options = value; } }
			public OptionDataList()
			{
				options = new List<OptionData>();
			}
		}
		[Serializable]
		public class DropdownEvent : UnityEvent<int> { }
		
		static readonly OptionData k_NothingOption = new OptionData { text = "Nothing" };
		static readonly OptionData k_EverythingOption = new OptionData { text = "Everything" };
		static readonly OptionData k_MixedOption = new OptionData { text = "Mixed..." };
		
		[SerializeField]
		private RectTransform m_Template;
		public RectTransform template { get { return m_Template; } set { m_Template = value; RefreshShownValue(); } }
		[SerializeField]
		private TMPro.TMP_Text m_CaptionText;
		public TMPro.TMP_Text captionText { get { return m_CaptionText; } set { m_CaptionText = value; RefreshShownValue(); } }
		[SerializeField]
		private Image m_CaptionImage;
		public Image captionImage { get { return m_CaptionImage; } set { m_CaptionImage = value; RefreshShownValue(); } }
		[SerializeField]
		private Graphic m_Placeholder;
		public Graphic placeholder { get { return m_Placeholder; } set { m_Placeholder = value; RefreshShownValue(); } }
		
		[Space]
		[SerializeField]
		private TMPro.TMP_Text m_ItemText;
		public TMPro.TMP_Text itemText { get { return m_ItemText; } set { m_ItemText = value; RefreshShownValue(); } }
		[SerializeField]
		private Image m_ItemImage;
		public Image itemImage { get { return m_ItemImage; } set { m_ItemImage = value; RefreshShownValue(); } }
		
		[Space]
		[SerializeField]
		private int m_Value;
		[SerializeField]
		private bool m_MultiSelect;
		
		[Space]
		[SerializeField]
		private OptionDataList m_Options = new OptionDataList();
		public List<OptionData> options
		{
			get { return m_Options.options; }
			set { m_Options.options = value; RefreshShownValue(); }
		}
		[Space]
		[SerializeField]
		private DropdownEvent m_OnValueChanged = new DropdownEvent();
		public DropdownEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }
		
		[SerializeField]
		private float m_AlphaFadeSpeed = 0.15f;
		public float alphaFadeSpeed { get { return m_AlphaFadeSpeed; } set { m_AlphaFadeSpeed = value; } }
		
		GameObject m_Dropdown;
		GameObject m_Blocker;
		readonly List<DropdownItem> m_Items = new();
		TweenRunner<FloatTween> m_AlphaTweenRunner;
		bool validTemplate = false;
		Coroutine m_Coroutine = null;
		
		static OptionData s_NoOptionData = new();
		
		public int value
		{
			get
			{
				return m_Value;
			}
			set
			{
				SetValue(value);
			}
		}
		public void SetValueWithoutNotify(int input)
		{
			SetValue(input, false);
		}
		void SetValue(int value, bool sendCallback = true)
		{
			if (Application.isPlaying && (value == m_Value || options.Count == 0))
			{
				return;
			}
			if (m_MultiSelect)
			{
				m_Value = value;
			}
			else
			{
				m_Value = Mathf.Clamp(value, m_Placeholder ? -1 : 0, options.Count - 1);
			}
			RefreshShownValue();
			
			if (sendCallback)
			{
				UISystemProfilerApi.AddMarker("Dropdown.value", this);
				m_OnValueChanged.Invoke(m_Value);
			}
		}
		public bool IsExpanded { get { return m_Dropdown != null; } }
		public bool MultiSelect { get { return m_MultiSelect; } set { m_MultiSelect = value; } }
		
		protected TMP_Dropdown() { }
		
		protected override void Awake()
		{
		#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return;
			}
		#endif
			if (m_CaptionImage)
			{
				m_CaptionImage.enabled = m_CaptionImage.sprite != null && m_CaptionImage.color.a > 0;
			}
			if (m_Template)
			{
				m_Template.gameObject.SetActive(false);
			}
		}
		protected override void Start()
		{
			m_AlphaTweenRunner = new TweenRunner<FloatTween>();
			m_AlphaTweenRunner.Init(this);
			base.Start();
			RefreshShownValue();
		}
	#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			
			if (!IsActive())
			{
				return;
			}
			RefreshShownValue();
		}
	#endif
		protected override void OnDisable()
		{
			ImmediateDestroyDropdownList();
			
			if (m_Blocker != null)
			{
				DestroyBlocker(m_Blocker);
			}
			m_Blocker = null;
			base.OnDisable();
		}
		public void RefreshShownValue()
		{
			OptionData data = s_NoOptionData;
			
			if (options.Count > 0)
			{
				if (m_MultiSelect)
				{
					int firstActiveFlag = FirstActiveFlagIndex(m_Value);
					if (m_Value == 0 || firstActiveFlag >= options.Count)
						data = k_NothingOption;
					else if (IsEverythingValue(options.Count, m_Value))
						data = k_EverythingOption;
					else if (Mathf.IsPowerOfTwo(m_Value) && m_Value > 0)
						data = options[firstActiveFlag];
					else
						data = k_MixedOption;
				}
				else if (m_Value >= 0)
				{
					data = options[Mathf.Clamp(m_Value, 0, options.Count - 1)];
				}
			}
			if (m_CaptionText)
			{
				if (data != null && data.text != null)
				{
					m_CaptionText.text = data.text;
				}
				else
				{
					m_CaptionText.text = "";
				}
			}
			if (m_CaptionImage)
			{
				m_CaptionImage.sprite = data.image;
				m_CaptionImage.color = data.color;
				m_CaptionImage.enabled = m_CaptionImage.sprite != null && m_CaptionImage.color.a > 0;
			}
			if (m_Placeholder)
			{
				m_Placeholder.enabled = options.Count == 0 || m_Value == -1;
			}
		}
		public void AddOptions(List<OptionData> options)
		{
			this.options.AddRange(options);
			RefreshShownValue();
		}
		public void AddOptions(List<string> options)
		{
			for (int i = 0; i < options.Count; i++)
			{
				this.options.Add(new OptionData(options[i]));
			}
			RefreshShownValue();
		}
		public void AddOptions(List<Sprite> options)
		{
			for (int i = 0; i < options.Count; i++)
			{
				this.options.Add(new OptionData(options[i]));
			}
			RefreshShownValue();
		}
		public void ClearOptions()
		{
			options.Clear();
			m_Value = m_Placeholder ? -1 : 0;
			RefreshShownValue();
		}
		private void SetupTemplate()
		{
			validTemplate = false;
			
			if (!m_Template)
			{
				Debug.LogError("The dropdown template is not assigned. The template needs to be assigned and must have a child GameObject with a Toggle component serving as the item.", this);
				return;
			}
			GameObject templateGo = m_Template.gameObject;
			templateGo.SetActive(true);
			Toggle itemToggle = m_Template.GetComponentInChildren<Toggle>();
			
			validTemplate = true;
			if (!itemToggle || itemToggle.transform == template)
			{
				validTemplate = false;
				Debug.LogError("The dropdown template is not valid. The template must have a child GameObject with a Toggle component serving as the item.", template);
			}
			else if (!(itemToggle.transform.parent is RectTransform))
			{
				validTemplate = false;
				Debug.LogError("The dropdown template is not valid. The child GameObject with a Toggle component (the item) must have a RectTransform on its parent.", template);
			}
			else if (itemText != null && !itemText.transform.IsChildOf(itemToggle.transform))
			{
				validTemplate = false;
				Debug.LogError("The dropdown template is not valid. The Item Text must be on the item GameObject or children of it.", template);
			}
			else if (itemImage != null && !itemImage.transform.IsChildOf(itemToggle.transform))
			{
				validTemplate = false;
				Debug.LogError("The dropdown template is not valid. The Item Image must be on the item GameObject or children of it.", template);
			}
			if (!validTemplate)
			{
				templateGo.SetActive(false);
				return;
			}
			DropdownItem item = itemToggle.gameObject.AddComponent<DropdownItem>();
			item.text = m_ItemText;
			item.image = m_ItemImage;
			item.toggle = itemToggle;
			item.rectTransform = (RectTransform)itemToggle.transform;
			
			Canvas parentCanvas = null;
			Transform parentTransform = m_Template.parent;
			while (parentTransform != null)
			{
				parentCanvas = parentTransform.GetComponent<Canvas>();
				if (parentCanvas != null)
				{
					break;
				}
				parentTransform = parentTransform.parent;
			}
			Canvas popupCanvas = GetOrAddComponent<Canvas>(templateGo);
			popupCanvas.overrideSorting = true;
			popupCanvas.sortingOrder = 30000;
			
			if (parentCanvas != null)
			{
				Component[] components = parentCanvas.GetComponents<BaseRaycaster>();
				for (int i = 0; i < components.Length; i++)
				{
					Type raycasterType = components[i].GetType();
					if (templateGo.GetComponent(raycasterType) == null)
					{
						templateGo.AddComponent(raycasterType);
					}
				}
			}
			else
			{
				GetOrAddComponent<GraphicRaycaster>(templateGo);
			}
			GetOrAddComponent<CanvasGroup>(templateGo);
			templateGo.SetActive(false);
			validTemplate = true;
		}
		private static T GetOrAddComponent<T>(GameObject go) where T : Component
		{
			T comp = go.GetComponent<T>();
			if (!comp)
			{
				comp = go.AddComponent<T>();
			}
			return comp;
		}
		public virtual void OnPointerClick(PointerEventData eventData)
		{
			Show();
		}
		public virtual void OnSubmit(BaseEventData eventData)
		{
			Show();
		}
		public virtual void OnCancel(BaseEventData eventData)
		{
			Hide();
		}
		public void Show()
		{
			if (m_Coroutine != null)
			{
				StopCoroutine(m_Coroutine);
				ImmediateDestroyDropdownList();
			}
			if (!IsActive() || !IsInteractable() || m_Dropdown != null)
			{
				return;
			}
			var list = TMP_ListPool<Canvas>.Get();
			gameObject.GetComponentsInParent(false, list);
			if (list.Count == 0)
			{
				return;
			}
			Canvas rootCanvas = list[list.Count - 1];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].isRootCanvas)
				{
					rootCanvas = list[i];
					break;
				}
			}
			TMP_ListPool<Canvas>.Release(list);
			
			if (!validTemplate)
			{
				SetupTemplate();
				
				if (!validTemplate)
				{
					return;
				}
			}
			m_Template.gameObject.SetActive(true);
			
			m_Template.GetComponent<Canvas>().sortingLayerID = rootCanvas.sortingLayerID;
			
			m_Dropdown = CreateDropdownList(m_Template.gameObject);
			m_Dropdown.name = "Dropdown List";
			m_Dropdown.SetActive(true);
			
			RectTransform dropdownRectTransform = m_Dropdown.transform as RectTransform;
			dropdownRectTransform.SetParent(m_Template.transform.parent, false);
			
			DropdownItem itemTemplate = m_Dropdown.GetComponentInChildren<DropdownItem>();
			
			GameObject content = itemTemplate.rectTransform.parent.gameObject;
			RectTransform contentRectTransform = content.transform as RectTransform;
			itemTemplate.rectTransform.gameObject.SetActive(true);
			
			Rect dropdownContentRect = contentRectTransform.rect;
			Rect itemTemplateRect = itemTemplate.rectTransform.rect;
			
			Vector2 offsetMin = itemTemplateRect.min - dropdownContentRect.min + (Vector2)itemTemplate.rectTransform.localPosition;
			Vector2 offsetMax = itemTemplateRect.max - dropdownContentRect.max + (Vector2)itemTemplate.rectTransform.localPosition;
			Vector2 itemSize = itemTemplateRect.size;
			
			m_Items.Clear();
			
			Toggle prev = null;
			if (m_MultiSelect && options.Count > 0)
			{
				DropdownItem item = AddItem(k_NothingOption, value == 0, itemTemplate, m_Items);
				if (item.image != null)
				{
					item.image.gameObject.SetActive(false);
				}
				Toggle nothingToggle = item.toggle;
				nothingToggle.isOn = value == 0;
				nothingToggle.onValueChanged.AddListener(x => OnSelectItem(nothingToggle));
				prev = nothingToggle;
				
				bool isEverythingValue = IsEverythingValue(options.Count, value);
				item = AddItem(k_EverythingOption, isEverythingValue, itemTemplate, m_Items);
				if (item.image != null)
				{
					item.image.gameObject.SetActive(false);
				}
				Toggle everythingToggle = item.toggle;
				everythingToggle.isOn = isEverythingValue;
				everythingToggle.onValueChanged.AddListener(x => OnSelectItem(everythingToggle));
				
				if (prev != null)
				{
					Navigation prevNav = prev.navigation;
					Navigation toggleNav = item.toggle.navigation;
					prevNav.mode = Navigation.Mode.Explicit;
					toggleNav.mode = Navigation.Mode.Explicit;
					
					prevNav.selectOnDown = item.toggle;
					prevNav.selectOnRight = item.toggle;
					toggleNav.selectOnLeft = prev;
					toggleNav.selectOnUp = prev;
					
					prev.navigation = prevNav;
					item.toggle.navigation = toggleNav;
				}
			}
			for (int i = 0; i < options.Count; ++i)
			{
				OptionData data = options[i];
				DropdownItem item = AddItem(data, value == i, itemTemplate, m_Items);
				if (item == null)
				{
					continue;
				}
				if (m_MultiSelect)
				{
					item.toggle.isOn = (value & (1 << i)) != 0;
				}
				else
				{
					item.toggle.isOn = value == i;
				}
				item.toggle.onValueChanged.AddListener(x => OnSelectItem(item.toggle));
				
				if (item.toggle.isOn)
				{
					item.toggle.Select();
				}
				if (prev != null)
				{
					Navigation prevNav = prev.navigation;
					Navigation toggleNav = item.toggle.navigation;
					prevNav.mode = Navigation.Mode.Explicit;
					toggleNav.mode = Navigation.Mode.Explicit;
					
					prevNav.selectOnDown = item.toggle;
					prevNav.selectOnRight = item.toggle;
					toggleNav.selectOnLeft = prev;
					toggleNav.selectOnUp = prev;
					
					prev.navigation = prevNav;
					item.toggle.navigation = toggleNav;
				}
				prev = item.toggle;
			}
			Vector2 sizeDelta = contentRectTransform.sizeDelta;
			sizeDelta.y = itemSize.y * m_Items.Count + offsetMin.y - offsetMax.y;
			contentRectTransform.sizeDelta = sizeDelta;
			
			float extraSpace = dropdownRectTransform.rect.height - contentRectTransform.rect.height;
			if (extraSpace > 0)
			{
				dropdownRectTransform.sizeDelta = new Vector2(dropdownRectTransform.sizeDelta.x, dropdownRectTransform.sizeDelta.y - extraSpace);
			}
			Vector3[] corners = new Vector3[4];
			dropdownRectTransform.GetWorldCorners(corners);
			
			RectTransform rootCanvasRectTransform = rootCanvas.transform as RectTransform;
			Rect rootCanvasRect = rootCanvasRectTransform.rect;
			for (int axis = 0; axis < 2; axis++)
			{
				bool outside = false;
				for (int i = 0; i < 4; i++)
				{
					Vector3 corner = rootCanvasRectTransform.InverseTransformPoint(corners[i]);
					if ((corner[axis] < rootCanvasRect.min[axis] && !Mathf.Approximately(corner[axis], rootCanvasRect.min[axis]))
					||	(corner[axis] > rootCanvasRect.max[axis] && !Mathf.Approximately(corner[axis], rootCanvasRect.max[axis])))
					{
						outside = true;
						break;
					}
				}
				if (outside)
				{
					RectTransformUtility.FlipLayoutOnAxis(dropdownRectTransform, axis, false, false);
				}
			}
			for (int i = 0; i < m_Items.Count; i++)
			{
				RectTransform itemRect = m_Items[i].rectTransform;
				itemRect.anchorMin = new Vector2(itemRect.anchorMin.x, 0);
				itemRect.anchorMax = new Vector2(itemRect.anchorMax.x, 0);
				itemRect.anchoredPosition = new Vector2(itemRect.anchoredPosition.x, offsetMin.y + itemSize.y * (m_Items.Count - 1 - i) + itemSize.y * itemRect.pivot.y);
				itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.x, itemSize.y);
			}
			AlphaFadeList(m_AlphaFadeSpeed, 0f, 1f);
			
			m_Template.gameObject.SetActive(false);
			itemTemplate.gameObject.SetActive(false);
			m_Blocker = CreateBlocker(rootCanvas);
		}
		static bool IsEverythingValue(int count, int value)
		{
			var result = true;
			for (var i = 0; i < count; i++)
			{
				if ((value & 1 << i) == 0)
					result = false;
			}
			return result;
		}
		static int EverythingValue(int count)
		{
			int result = 0;
			for (var i = 0; i < count; i++)
			{
				result |= 1 << i;
			}
			return result;
		}
		protected virtual GameObject CreateBlocker(Canvas rootCanvas)
		{
			GameObject blocker = new GameObject("Blocker");
			
			blocker.layer = rootCanvas.gameObject.layer;
			
			RectTransform blockerRect = blocker.AddComponent<RectTransform>();
			blockerRect.SetParent(rootCanvas.transform, false);
			blockerRect.anchorMin = Vector3.zero;
			blockerRect.anchorMax = Vector3.one;
			blockerRect.sizeDelta = Vector2.zero;
			
			Canvas blockerCanvas = blocker.AddComponent<Canvas>();
			blockerCanvas.overrideSorting = true;
			Canvas dropdownCanvas = m_Dropdown.GetComponent<Canvas>();
			blockerCanvas.sortingLayerID = dropdownCanvas.sortingLayerID;
			blockerCanvas.sortingOrder = dropdownCanvas.sortingOrder - 1;
			
			Canvas parentCanvas = null;
			Transform parentTransform = m_Template.parent;
			while (parentTransform != null)
			{
				parentCanvas = parentTransform.GetComponent<Canvas>();
				if (parentCanvas != null)
				{
					break;
				}
				parentTransform = parentTransform.parent;
			}
			if (parentCanvas != null)
			{
				Component[] components = parentCanvas.GetComponents<BaseRaycaster>();
				for (int i = 0; i < components.Length; i++)
				{
					Type raycasterType = components[i].GetType();
					if (blocker.GetComponent(raycasterType) == null)
					{
						blocker.AddComponent(raycasterType);
					}
				}
			}
			else
			{
				GetOrAddComponent<GraphicRaycaster>(blocker);
			}
			Image blockerImage = blocker.AddComponent<Image>();
			blockerImage.color = Color.clear;
			
			Button blockerButton = blocker.AddComponent<Button>();
			blockerButton.onClick.AddListener(Hide);
			
			CanvasGroup blockerCanvasGroup = blocker.AddComponent<CanvasGroup>();
			blockerCanvasGroup.ignoreParentGroups = true;
			return blocker;
		}
		protected virtual void DestroyBlocker(GameObject blocker)
		{
			Destroy(blocker);
		}
		protected virtual GameObject CreateDropdownList(GameObject template)
		{
			return (GameObject)Instantiate(template);
		}
		protected virtual void DestroyDropdownList(GameObject dropdownList)
		{
			Destroy(dropdownList);
		}
		protected virtual DropdownItem CreateItem(DropdownItem itemTemplate)
		{
			return (DropdownItem)Instantiate(itemTemplate);
		}
		protected virtual void DestroyItem(DropdownItem item)
		{
		}
		private DropdownItem AddItem(OptionData data, bool selected, DropdownItem itemTemplate, List<DropdownItem> items)
		{
			DropdownItem item = CreateItem(itemTemplate);
			item.rectTransform.SetParent(itemTemplate.rectTransform.parent, false);
			
			item.gameObject.SetActive(true);
			item.gameObject.name = "Item " + items.Count + (data.text != null ? ": " + data.text : "");
			
			if (item.toggle != null)
			{
				item.toggle.isOn = false;
			}
			if (item.text)
			{
				item.text.text = data.text;
			}
			if (item.image)
			{
				item.image.sprite = data.image;
				item.image.color = data.color;
				item.image.enabled = (item.image.sprite != null && data.color.a > 0);
			}
			items.Add(item);
			return item;
		}
		private void AlphaFadeList(float duration, float alpha)
		{
			CanvasGroup group = m_Dropdown.GetComponent<CanvasGroup>();
			AlphaFadeList(duration, group.alpha, alpha);
		}
		private void AlphaFadeList(float duration, float start, float end)
		{
			if (end.Equals(start))
			{
				return;
			}
			FloatTween tween = new FloatTween { duration = duration, startValue = start, targetValue = end };
			tween.AddOnChangedCallback(SetAlpha);
			tween.ignoreTimeScale = true;
			m_AlphaTweenRunner.StartTween(tween);
		}
		private void SetAlpha(float alpha)
		{
			if (!m_Dropdown)
			{
				return;
			}
			CanvasGroup group = m_Dropdown.GetComponent<CanvasGroup>();
			group.alpha = alpha;
		}
		public void Hide()
		{
			if (m_Coroutine == null)
			{
				if (m_Dropdown != null)
				{
					AlphaFadeList(m_AlphaFadeSpeed, 0f);
					
					if (IsActive())
					{
						m_Coroutine = StartCoroutine(DelayedDestroyDropdownList(m_AlphaFadeSpeed));
					}
				}
				if (m_Blocker != null)
				{
					DestroyBlocker(m_Blocker);
				}
				m_Blocker = null;
				Select();
			}
		}
		private IEnumerator DelayedDestroyDropdownList(float delay)
		{
			yield return new WaitForSecondsRealtime(delay);
			ImmediateDestroyDropdownList();
		}
		private void ImmediateDestroyDropdownList()
		{
			for (int i = 0; i < m_Items.Count; i++)
			{
				if (m_Items[i] != null)
				{
					DestroyItem(m_Items[i]);
				}
			}
			m_Items.Clear();
			
			if (m_Dropdown != null)
			{
				DestroyDropdownList(m_Dropdown);
			}
			if (m_AlphaTweenRunner != null)
			{
				m_AlphaTweenRunner.StopTween();
			}
			m_Dropdown = null;
			m_Coroutine = null;
		}
		private void OnSelectItem(Toggle toggle)
		{
			int selectedIndex = -1;
			Transform tr = toggle.transform;
			Transform parent = tr.parent;
			for (int i = 1; i < parent.childCount; i++)
			{
				if (parent.GetChild(i) == tr)
				{
					selectedIndex = i - 1;
					break;
				}
			}
			if (selectedIndex < 0)
			{
				return;
			}
			if (m_MultiSelect)
			{
				switch (selectedIndex)
				{
					case 0: // Nothing
					{
						value = 0;
						for (var i = 3; i < parent.childCount; i++)
						{
							var toggleComponent = parent.GetChild(i).GetComponentInChildren<Toggle>();
							if (toggleComponent)
							{
								toggleComponent.SetIsOnWithoutNotify(false);
							}
						}
						toggle.isOn = true;
						break;
					}
					case 1: // Everything
					{
						value = EverythingValue(options.Count);
						for (var i = 3; i < parent.childCount; i++)
						{
							var toggleComponent = parent.GetChild(i).GetComponentInChildren<Toggle>();
							if (toggleComponent)
							{
								toggleComponent.SetIsOnWithoutNotify(i > 2);
							}
						}
						break;
					}
					default:
					{
						var flagValue = 1 << (selectedIndex - 2);
						var wasSelected = (value & flagValue) != 0;
						toggle.SetIsOnWithoutNotify(!wasSelected);
						
						if (wasSelected)
						{
							value &= ~flagValue;
						}
						else
						{
							value |= flagValue;
						}
						break;
					}
				}
			}
			else
			{
				if (!toggle.isOn)
				{
					toggle.SetIsOnWithoutNotify(true);
				}
				value = selectedIndex;
			}
			Hide();
		}
		static int FirstActiveFlagIndex(int value)
		{
			if (value == 0)
			{
				return 0;
			}
			const int bits = sizeof(int) * 8;
			
			for (var i = 0; i < bits; i++)
			{
				if ((value & 1 << i) != 0)
				{
					return i;
				}
			}
			return 0;
		}
	}
}
