
using System;
using UnityEditor.SceneManagement;
using UnityEditor.EventSystems;
using UnityEngine;
using UnityEditor;
using MultiWindow.EventSystems;

namespace MultiWindow.UI.Eidtor
{
	static internal class MenuOptions
	{
		enum MenuOptionsPriorityOrder
		{
			// 2000 - Text (TMP)
			Image = 2001,
			RawImage = 2002,
			Panel = 2003,
			// 2020 - Button (TMP)
			Toggle = 2021,
			// 2022 - Dropdown (TMP)
			// 2023 - Input Field (TMP)
			Slider = 2024,
			Scrollbar = 2025,
			ScrollView = 2026,
			ButtonPro = 2031,
			Canvas = 2060,
			EventSystem = 2061,
			Text = 2080,
			ButtonLegacy = 2081,
			Dropdown = 2082,
			InputField = 2083,
		};
		const string kUILayerName = "UI";
		const string kStandardSpritePath       = "UI/Skin/UISprite.psd";
		const string kBackgroundSpritePath     = "UI/Skin/Background.psd";
		const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
		const string kKnobPath                 = "UI/Skin/Knob.psd";
		const string kCheckmarkPath            = "UI/Skin/Checkmark.psd";
		const string kDropdownArrowPath        = "UI/Skin/DropdownArrow.psd";
		const string kMaskPath                 = "UI/Skin/UIMask.psd";
		
		static DefaultControls.Resources s_StandardResources;
		
		static DefaultControls.Resources GetStandardResources()
		{
			if( s_StandardResources.standard == null)
			{
				s_StandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>( kStandardSpritePath);
				s_StandardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>( kBackgroundSpritePath);
				s_StandardResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>( kInputFieldBackgroundPath);
				s_StandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>( kKnobPath);
				s_StandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>( kCheckmarkPath);
				s_StandardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>( kDropdownArrowPath);
				s_StandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>( kMaskPath);
			}
			return s_StandardResources;
		}
		class DefaultEditorFactory : DefaultControls.IFactoryControls
		{
			public static DefaultEditorFactory Default = new();
			
			public GameObject CreateGameObject( string name, params Type[] components)
			{
				return ObjectFactory.CreateGameObject(name, components);
			}
		}
		class FactorySwapToEditor : IDisposable
		{
			DefaultControls.IFactoryControls factory;
			
			public FactorySwapToEditor()
			{
				factory = DefaultControls.factory;
				DefaultControls.factory = DefaultEditorFactory.Default;
			}
			public void Dispose()
			{
				DefaultControls.factory = factory;
			}
		}
		static void SetPositionVisibleinSceneView( RectTransform canvasRTransform, RectTransform itemTransform)
		{
			SceneView sceneView = SceneView.lastActiveSceneView;
			
			if (sceneView == null || sceneView.camera == null)
			{
				return;
			}
			Vector2 localPlanePosition;
			Camera camera = sceneView.camera;
			Vector3 position = Vector3.zero;
			
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
				canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
			{
				localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
				localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;
				localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
				localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);
				
				position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
				position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;
				
				Vector3 minLocalPosition;
				minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
				minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;
				
				Vector3 maxLocalPosition;
				maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
				maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;
				
				position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
				position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
			}
			itemTransform.anchoredPosition = position;
			itemTransform.localRotation = Quaternion.identity;
			itemTransform.localScale = Vector3.one;
		}
		static void PlaceUIElementRoot( GameObject element, MenuCommand menuCommand)
		{
			GameObject parent = menuCommand.context as GameObject;
			bool explicitParentChoice = true;
			
			if( parent == null)
			{
				parent = GetOrCreateCanvasGameObject();
				explicitParentChoice = false;
				
				PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
				
				if( prefabStage != null && !prefabStage.IsPartOfPrefabContents( parent))
				{
					parent = prefabStage.prefabContentsRoot;
				}
			}
			if (parent.GetComponentsInParent<Canvas>(true).Length == 0)
			{
				GameObject canvas = MenuOptions.CreateNewUI();
				Undo.SetTransformParent(canvas.transform, parent.transform, "");
				parent = canvas;
			}
			GameObjectUtility.EnsureUniqueNameForSibling(element);
			
			SetParentAndAlign(element, parent);
			
			if (!explicitParentChoice)
			{
				SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());
			}
			Undo.RegisterFullObjectHierarchyUndo( parent == null ? element : parent, "");
			Undo.SetCurrentGroupName( "Create " + element.name);
			Selection.activeGameObject = element;
		}
		static void SetParentAndAlign( GameObject child, GameObject parent)
		{
			if (parent == null)
			{
				return;
			}
			Undo.SetTransformParent( child.transform, parent.transform, "");
			RectTransform rectTransform = child.transform as RectTransform;
			
			if( rectTransform)
			{
				rectTransform.anchoredPosition = Vector2.zero;
				Vector3 localPosition = rectTransform.localPosition;
				localPosition.z = 0;
				rectTransform.localPosition = localPosition;
			}
			else
			{
				child.transform.localPosition = Vector3.zero;
			}
			child.transform.localRotation = Quaternion.identity;
			child.transform.localScale = Vector3.one;
			SetLayerRecursively( child, parent.layer);
		}
		static void SetLayerRecursively(GameObject go, int layer)
		{
			go.layer = layer;
			Transform t = go.transform;
			
			for (int i = 0; i < t.childCount; i++)
			{
				SetLayerRecursively( t.GetChild( i).gameObject, layer);
			}
		}
		[MenuItem("GameObject/MultiWindowUI/Image", false, (int)MenuOptionsPriorityOrder.Image)]
		static public void AddImage(MenuCommand menuCommand)
		{
			GameObject go;
			
			using (new FactorySwapToEditor())
			{
				go = DefaultControls.CreateImage( GetStandardResources());
			}
			PlaceUIElementRoot( go, menuCommand);
		}
/*
		[MenuItem("GameObject/MultiWindowUI/Raw Image", false, (int)MenuOptionsPriorityOrder.RawImage)]
		static public void AddRawImage(MenuCommand menuCommand)
		{
			GameObject go;
			using (new FactorySwapToEditor())
				go = DefaultControls.CreateRawImage(GetStandardResources());
			PlaceUIElementRoot(go, menuCommand);
		}
*/
		[MenuItem("GameObject/MultiWindowUI/Panel", false, (int)MenuOptionsPriorityOrder.Panel)]
		static public void AddPanel(MenuCommand menuCommand)
		{
			GameObject go;
			
			using (new FactorySwapToEditor())
			{
				go = DefaultControls.CreatePanel( GetStandardResources());
			}
			PlaceUIElementRoot( go, menuCommand);
			
			RectTransform rectTransform = go.GetComponent<RectTransform>();
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform.sizeDelta = Vector2.zero;
		}
		[MenuItem("GameObject/MultiWindowUI/Toggle", false, (int)MenuOptionsPriorityOrder.Toggle)]
		static public void AddToggle(MenuCommand menuCommand)
		{
			GameObject go;
			
			using (new FactorySwapToEditor())
			{
				go = DefaultControls.CreateToggle( GetStandardResources());
			}
			PlaceUIElementRoot( go, menuCommand);
		}
		[MenuItem("GameObject/MultiWindowUI/Slider", false, (int)MenuOptionsPriorityOrder.Slider)]
		static public void AddSlider(MenuCommand menuCommand)
		{
			GameObject go;
			
			using( new FactorySwapToEditor())
			{
				go = DefaultControls.CreateSlider( GetStandardResources());
			}
			PlaceUIElementRoot( go, menuCommand);
		}
/*
		[MenuItem("GameObject/MultiWindowUI/Scrollbar", false, (int)MenuOptionsPriorityOrder.Scrollbar)]
		static public void AddScrollbar(MenuCommand menuCommand)
		{
			GameObject go;
			using (new FactorySwapToEditor())
				go = DefaultControls.CreateScrollbar(GetStandardResources());
			PlaceUIElementRoot(go, menuCommand);
		}
*/
/*
		[MenuItem("GameObject/MultiWindowUI/Scroll View", false, (int)MenuOptionsPriorityOrder.ScrollView)]
		static public void AddScrollView(MenuCommand menuCommand)
		{
			GameObject go;
			using (new FactorySwapToEditor())
				go = DefaultControls.CreateScrollView(GetStandardResources());
			PlaceUIElementRoot(go, menuCommand);
		}
*/
		[MenuItem("GameObject/MultiWindowUI/Canvas", false, (int)MenuOptionsPriorityOrder.Canvas)]
		static public void AddCanvas(MenuCommand menuCommand)
		{
			var go = CreateNewUI();
			SetParentAndAlign(go, menuCommand.context as GameObject);
			
			if( go.transform.parent as RectTransform)
			{
				RectTransform rect = go.transform as RectTransform;
				rect.anchorMin = Vector2.zero;
				rect.anchorMax = Vector2.one;
				rect.anchoredPosition = Vector2.zero;
				rect.sizeDelta = Vector2.zero;
			}
			Selection.activeGameObject = go;
		}
		[MenuItem("GameObject/MultiWindowUI/Legacy/Button", false, (int)MenuOptionsPriorityOrder.ButtonLegacy)]
		static public void AddButtonLegacy(MenuCommand menuCommand)
		{
			GameObject go;
			
			using (new FactorySwapToEditor())
			{
				go = DefaultControls.CreateButtonLegacy( GetStandardResources());
			}
			PlaceUIElementRoot( go, menuCommand);
		}
		[MenuItem("GameObject/MultiWindowUI/Button - TextMeshPro", false, (int)MenuOptionsPriorityOrder.ButtonPro)]
		static public void AddButtonPro(MenuCommand menuCommand)
		{
			GameObject go;
			
			using (new FactorySwapToEditor())
			{
				go = DefaultControls.CreateButtonPro( GetStandardResources());
			}
			PlaceUIElementRoot( go, menuCommand);
		}
/*
		[MenuItem("GameObject/MultiWindowUI/Legacy/Dropdown", false, (int)MenuOptionsPriorityOrder.Dropdown)]
		static public void AddDropdown(MenuCommand menuCommand)
		{
			GameObject go;
			using (new FactorySwapToEditor())
				go = DefaultControls.CreateDropdown(GetStandardResources());
			PlaceUIElementRoot(go, menuCommand);
		}
*/
/*
		[MenuItem("GameObject/MultiWindowUI/Legacy/Input Field", false, (int)MenuOptionsPriorityOrder.InputField)]
		public static void AddInputField(MenuCommand menuCommand)
		{
			GameObject go;
			using (new FactorySwapToEditor())
				go = DefaultControls.CreateInputField(GetStandardResources());
			PlaceUIElementRoot(go, menuCommand);
		}
*/
		static public GameObject CreateNewUI()
		{
			var root = ObjectFactory.CreateGameObject( "Canvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(GraphicRaycaster));
			root.layer = LayerMask.NameToLayer( kUILayerName);
			Canvas canvas = root.GetComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			
			StageUtility.PlaceGameObjectInCurrentStage(root);
			bool customScene = false;
			PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
			
			if( prefabStage != null)
			{
				Undo.SetTransformParent( root.transform, prefabStage.prefabContentsRoot.transform, "");
				customScene = true;
			}
			Undo.SetCurrentGroupName("Create " + root.name);
			
			if (!customScene)
			{
				CreateEventSystem(false);
			}
			return root;
		}
		[MenuItem( "GameObject/MultiWindowUI/Event System", false, (int)MenuOptionsPriorityOrder.EventSystem)]
		public static void CreateEventSystem(MenuCommand menuCommand)
		{
			GameObject parent = menuCommand.context as GameObject;
			CreateEventSystem(true, parent);
		}
		static void CreateEventSystem( bool select)
		{
			CreateEventSystem(select, null);
		}
		static void CreateEventSystem( bool select, GameObject parent)
		{
			StageHandle stage = parent == null ? StageUtility.GetCurrentStageHandle() : StageUtility.GetStageHandle(parent);
			var esys = stage.FindComponentOfType<EventSystem>();
			
			if (esys == null)
			{
				var eventSystem = ObjectFactory.CreateGameObject("EventSystem");
				if (parent == null)
				{
					StageUtility.PlaceGameObjectInCurrentStage(eventSystem);
				}
				else
				{
					SetParentAndAlign(eventSystem, parent);
				}
				esys = ObjectFactory.AddComponent<EventSystem>(eventSystem);
				InputModuleComponentFactory.AddInputModule(eventSystem);
				Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
			}
			if (select && esys != null)
			{
				Selection.activeGameObject = esys.gameObject;
			}
		}
		static public GameObject GetOrCreateCanvasGameObject()
		{
			GameObject selectedGo = Selection.activeGameObject;
			
			Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
			if (IsValidCanvas(canvas))
			{
				return canvas.gameObject;
			}
			Canvas[] canvasArray = StageUtility.GetCurrentStageHandle().FindComponentsOfType<Canvas>();
			
			for (int i = 0; i < canvasArray.Length; i++)
			{
				if (IsValidCanvas(canvasArray[i]))
				{
					return canvasArray[i].gameObject;
				}
			}
			return MenuOptions.CreateNewUI();
		}
		static bool IsValidCanvas(Canvas canvas)
		{
			if (canvas == null || !canvas.gameObject.activeInHierarchy)
			{
				return false;
			}
			if (EditorUtility.IsPersistent(canvas) || (canvas.hideFlags & HideFlags.HideInHierarchy) != 0)
			{
				return false;
			}
			return StageUtility.GetStageHandle(canvas.gameObject) == StageUtility.GetCurrentStageHandle();
		}
	}
}
