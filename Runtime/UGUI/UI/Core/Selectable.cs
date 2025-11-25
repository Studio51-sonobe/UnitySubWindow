using System;
using System.Collections.Generic;
using UnityEngine;
using MultiWindow.EventSystems;
using UnityEngine.Serialization;

namespace MultiWindow.UI
{
	[AddComponentMenu( "MultiWindowUI/Selectable", 35)]
	[ExecuteAlways]
	[SelectionBase]
	[DisallowMultipleComponent]
	public class Selectable
		: UnityEngine.EventSystems.UIBehaviour
		, IMoveHandler
		, IPointerDownHandler
		, IPointerUpHandler
		, IPointerEnterHandler
		, IPointerExitHandler
		, ISelectHandler
		, IDeselectHandler
	{
		protected static Selectable[] s_Selectables = new Selectable[10];
		protected static int s_SelectableCount = 0;
		bool m_EnableCalled = false;
		
		public static Selectable[] allSelectablesArray
		{
			get
			{
				Selectable[] temp = new Selectable[s_SelectableCount];
				Array.Copy(s_Selectables, temp, s_SelectableCount);
				return temp;
			}
		}
		public static int allSelectableCount { get { return s_SelectableCount; } }
		
		[Obsolete("Replaced with allSelectablesArray to have better performance when disabling a element", false)]
		public static List<Selectable> allSelectables
		{
			get
			{
				return new List<Selectable>(allSelectablesArray);
			}
		}
		public static int AllSelectablesNoAlloc(Selectable[] selectables)
		{
			int copyCount = (selectables.Length < s_SelectableCount)? selectables.Length : s_SelectableCount;
			Array.Copy(s_Selectables, selectables, copyCount);
			return copyCount;
		}
		[FormerlySerializedAs( "navigation")]
		[SerializeField]
		Navigation m_Navigation = Navigation.defaultNavigation;
		
		public enum Transition
		{
			None,
			ColorTint,
			SpriteSwap,
			Animation
		}
		[FormerlySerializedAs( "transition")]
		[SerializeField]
		Transition m_Transition = Transition.ColorTint;
		[FormerlySerializedAs( "colors")]
		[SerializeField]
		UnityEngine.UI.ColorBlock m_Colors = UnityEngine.UI.ColorBlock.defaultColorBlock;
		[FormerlySerializedAs( "spriteState")]
		[SerializeField]
		UnityEngine.UI.SpriteState m_SpriteState;
		[FormerlySerializedAs( "animationTriggers")]
		[SerializeField]
		UnityEngine.UI.AnimationTriggers m_AnimationTriggers = new();
		[Tooltip( "Can the Selectable be interacted with?")]
		[SerializeField]
		bool m_Interactable = true;
		[FormerlySerializedAs( "highlightGraphic")]
		[FormerlySerializedAs( "m_HighlightGraphic")]
		[SerializeField]
		Graphic m_TargetGraphic;
		
		bool m_GroupsAllowInteraction = true;
		protected int m_CurrentIndex = -1;
		
		public Navigation navigation
		{
			get { return m_Navigation; }
			set
			{
				if( SetPropertyUtility.SetStruct( ref m_Navigation, value))
				{
					OnSetProperty();
				}
			}
		}
		public Transition transition
		{
			get { return m_Transition; }
			set
			{
				if( SetPropertyUtility.SetStruct( ref m_Transition, value))
				{
					OnSetProperty();
				}
			}
		}
		public UnityEngine.UI.ColorBlock colors
		{
			get { return m_Colors; }
			set
			{
				if( SetPropertyUtility.SetStruct( ref m_Colors, value))
				{
					OnSetProperty();
				}
			}
		}
		public UnityEngine.UI.SpriteState spriteState
		{
			get { return m_SpriteState; }
			set
			{
				if( SetPropertyUtility.SetStruct( ref m_SpriteState, value))
				{
					OnSetProperty();
				}
			}
		}
		public UnityEngine.UI.AnimationTriggers animationTriggers
		{
			get { return m_AnimationTriggers; }
			set
			{
				if( SetPropertyUtility.SetClass( ref m_AnimationTriggers, value))
				{
					OnSetProperty();
				}
			}
		}
		public Graphic targetGraphic
		{
			get { return m_TargetGraphic; }
			set
			{
				if( SetPropertyUtility.SetClass( ref m_TargetGraphic, value))
				{
					OnSetProperty();
				}
			}
		}
		public bool interactable
		{
			get { return m_Interactable; }
			set
			{
				if (SetPropertyUtility.SetStruct(ref m_Interactable, value))
				{
					if( !m_Interactable && eventSystem != null && eventSystem.currentSelectedGameObject == gameObject)
					{
						eventSystem.SetSelectedGameObject( null);
					}
					OnSetProperty();
				}
			}
		}
		bool isPointerInside{ get; set; }
		bool isPointerDown{ get; set; }
		bool hasSelection{ get; set; }
		
		protected EventSystem eventSystem
		{
			get
			{
				if( m_EventSystem == null)
				{
					m_EventSystem = GetComponentInParent<EventSystem>();
				}
				return m_EventSystem;
			}
		}
		EventSystem m_EventSystem;
		
		protected Selectable()
		{
		}
		public Image image
		{
			get { return m_TargetGraphic as Image; }
			set { m_TargetGraphic = value; }
		}
#if PACKAGE_ANIMATION
		public Animator animator
		{
			get { return GetComponent<Animator>(); }
		}
#endif
		protected override void Awake()
		{
			if( m_TargetGraphic == null)
			{
				m_TargetGraphic = GetComponent<Graphic>();
			}
		}
		readonly List<CanvasGroup> m_CanvasGroupCache = new();
		protected override void OnCanvasGroupChanged()
		{
			var parentGroupAllowsInteraction = ParentGroupAllowsInteraction();
			
			if( parentGroupAllowsInteraction != m_GroupsAllowInteraction)
			{
				m_GroupsAllowInteraction = parentGroupAllowsInteraction;
				OnSetProperty();
			}
		}
		bool ParentGroupAllowsInteraction()
		{
			Transform t = transform;
			
			while (t != null)
			{
				t.GetComponents( m_CanvasGroupCache);
				
				for( var i0 = 0; i0 < m_CanvasGroupCache.Count; i0++)
				{
					if (m_CanvasGroupCache[i0].enabled && !m_CanvasGroupCache[i0].interactable)
					{
						return false;
					}
					if (m_CanvasGroupCache[i0].ignoreParentGroups)
					{
						return true;
					}
				}
				t = t.parent;
			}
			return true;
		}
		public virtual bool IsInteractable()
		{
			return m_GroupsAllowInteraction && m_Interactable;
		}
		protected override void OnDidApplyAnimationProperties()
		{
			OnSetProperty();
		}
		protected override void OnEnable()
		{
			if( m_EnableCalled != false)
			{
				return;
			}
			base.OnEnable();
			
			if( s_SelectableCount == s_Selectables.Length)
			{
				var temp = new Selectable[ s_Selectables.Length * 2];
				Array.Copy( s_Selectables, temp, s_Selectables.Length);
				s_Selectables = temp;
			}
			if( eventSystem && eventSystem.currentSelectedGameObject == gameObject)
			{
				hasSelection = true;
			}
			m_CurrentIndex = s_SelectableCount;
			s_Selectables[ m_CurrentIndex] = this;
			s_SelectableCount++;
			isPointerDown = false;
			m_GroupsAllowInteraction = ParentGroupAllowsInteraction();
			DoStateTransition(currentSelectionState, true);
			m_EnableCalled = true;
		}
		protected override void OnTransformParentChanged()
		{
			base.OnTransformParentChanged();
			OnCanvasGroupChanged();
		}
		void OnSetProperty()
		{
		#if UNITY_EDITOR
			if( Application.isPlaying == false)
			{
				DoStateTransition( currentSelectionState, true);
			}
			else
		#endif
			{
				DoStateTransition( currentSelectionState, false);
			}
		}
		protected override void OnDisable()
		{
			if( m_EnableCalled == false)
			{
				return;
			}
			s_SelectableCount--;
			
			s_Selectables[ s_SelectableCount].m_CurrentIndex = m_CurrentIndex;
			s_Selectables[ m_CurrentIndex] = s_Selectables[ s_SelectableCount];
			s_Selectables[ s_SelectableCount] = null;
			
			InstantClearState();
			base.OnDisable();
			m_EnableCalled = false;
		}
		void OnApplicationFocus( bool hasFocus)
		{
			if( hasFocus && IsPressed() == false)
			{
				InstantClearState();
			}
		}
	#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			m_Colors.fadeDuration = Mathf.Max( m_Colors.fadeDuration, 0.0f);
			
			if( isActiveAndEnabled != false)
			{
				if( interactable == false && eventSystem != null
				&&	eventSystem.currentSelectedGameObject == gameObject)
				{
					eventSystem.SetSelectedGameObject(null);
				}
				DoSpriteSwap(null);
				StartColorTween(Color.white, true);
				TriggerAnimation(m_AnimationTriggers.normalTrigger);
				DoStateTransition(currentSelectionState, true);
			}
		}
		protected override void Reset()
		{
			m_TargetGraphic = GetComponent<Graphic>();
		}
	#endif
		protected SelectionState currentSelectionState
		{
			get
			{
				if( IsInteractable() == false)
				{
					return SelectionState.Disabled;
				}
				if( isPointerDown != false)
				{
					return SelectionState.Pressed;
				}
				if( hasSelection != false)
				{
					return SelectionState.Selected;
				}
				if( isPointerInside != false)
				{
					return SelectionState.Highlighted;
				}
				return SelectionState.Normal;
			}
		}
		protected virtual void InstantClearState()
		{
			string triggerName = m_AnimationTriggers.normalTrigger;
			
			isPointerInside = false;
			isPointerDown = false;
			hasSelection = false;
			
			switch( m_Transition)
			{
				case Transition.ColorTint:
				{
					StartColorTween( Color.white, true);
					break;
				}
				case Transition.SpriteSwap:
				{
					DoSpriteSwap( null);
					break;
				}
				case Transition.Animation:
				{
					TriggerAnimation( triggerName);
					break;
				}
			}
		}
		protected virtual void DoStateTransition(SelectionState state, bool instant)
		{
			if( gameObject.activeInHierarchy == false)
			{
				return;
			}
			Color tintColor;
			Sprite transitionSprite;
			string triggerName;
			
			switch( state)
			{
				case SelectionState.Normal:
				{
					tintColor = m_Colors.normalColor;
					transitionSprite = null;
					triggerName = m_AnimationTriggers.normalTrigger;
					break;
				}
				case SelectionState.Highlighted:
				{
					tintColor = m_Colors.highlightedColor;
					transitionSprite = m_SpriteState.highlightedSprite;
					triggerName = m_AnimationTriggers.highlightedTrigger;
					break;
				}
				case SelectionState.Pressed:
				{
					tintColor = m_Colors.pressedColor;
					transitionSprite = m_SpriteState.pressedSprite;
					triggerName = m_AnimationTriggers.pressedTrigger;
					break;
				}
				case SelectionState.Selected:
				{
					tintColor = m_Colors.selectedColor;
					transitionSprite = m_SpriteState.selectedSprite;
					triggerName = m_AnimationTriggers.selectedTrigger;
					break;
				}
				case SelectionState.Disabled:
				{
					tintColor = m_Colors.disabledColor;
					transitionSprite = m_SpriteState.disabledSprite;
					triggerName = m_AnimationTriggers.disabledTrigger;
					break;
				}
				default:
				{
					tintColor = Color.black;
					transitionSprite = null;
					triggerName = string.Empty;
					break;
				}
			}
			switch( m_Transition)
			{
				case Transition.ColorTint:
				{
					StartColorTween( tintColor * m_Colors.colorMultiplier, instant);
					break;
				}
				case Transition.SpriteSwap:
				{
					DoSpriteSwap( transitionSprite);
					break;
				}
				case Transition.Animation:
				{
					TriggerAnimation( triggerName);
					break;
				}
			}
		}
		protected enum SelectionState
		{
			Normal,
			Highlighted,
			Pressed,
			Selected,
			Disabled,
		}
		public Selectable FindSelectable( Vector3 dir)
		{
			dir = dir.normalized;
			Vector3 localDir = Quaternion.Inverse(transform.rotation) * dir;
			Vector3 pos = transform.TransformPoint(GetPointOnRectEdge(transform as RectTransform, localDir));
			float maxScore = Mathf.NegativeInfinity;
			float maxFurthestScore = Mathf.NegativeInfinity;
			float score = 0;
			bool wantsWrapAround = navigation.wrapAround && (m_Navigation.mode == Navigation.Mode.Vertical || m_Navigation.mode == Navigation.Mode.Horizontal);
			Selectable bestPick = null;
			Selectable bestFurthestPick = null;
			
			for( int i0 = 0; i0 < s_SelectableCount; ++i0)
			{
				Selectable sel = s_Selectables[ i0];
				
				if( sel == this)
				{
					continue;
				}
				if( sel.IsInteractable() == false || sel.navigation.mode == Navigation.Mode.None)
				{
					continue;
				}
			#if UNITY_EDITOR
				if( Camera.current != null
				&&	UnityEditor.SceneManagement.StageUtility.IsGameObjectRenderedByCamera( sel.gameObject, Camera.current) == false)
				{
					continue;
				}
			#endif
				var selRect = sel.transform as RectTransform;
				Vector3 selCenter = (selRect != null)?
					(Vector3)selRect.rect.center : Vector3.zero;
				Vector3 myVector = sel.transform.TransformPoint( selCenter) - pos;
				
				float dot = Vector3.Dot( dir, myVector);
				
				if( wantsWrapAround && dot < 0)
				{
					score = -dot * myVector.sqrMagnitude;
					
					if( score > maxFurthestScore)
					{
						maxFurthestScore = score;
						bestFurthestPick = sel;
					}
					continue;
				}
				if( dot <= 0)
				{
					continue;
				}
				score = dot / myVector.sqrMagnitude;
				
				if( score > maxScore)
				{
					maxScore = score;
					bestPick = sel;
				}
			}
			if( wantsWrapAround != false && null == bestPick)
			{
				return bestFurthestPick;
			}
			return bestPick;
		}
		static Vector3 GetPointOnRectEdge( RectTransform rect, Vector2 dir)
		{
			if( rect == null)
			{
				return Vector3.zero;
			}
			if( dir != Vector2.zero)
			{
				dir /= Mathf.Max( Mathf.Abs( dir.x), Mathf.Abs( dir.y));
			}
			dir = rect.rect.center + Vector2.Scale( rect.rect.size, dir * 0.5f);
			return dir;
		}
		void Navigate( AxisEventData eventData, Selectable sel)
		{
			if( sel != null && sel.IsActive() != false)
			{
				eventData.selectedObject = sel.gameObject;
			}
		}
		public virtual Selectable FindSelectableOnLeft()
		{
			if( m_Navigation.mode == Navigation.Mode.Explicit)
			{
				return m_Navigation.selectOnLeft;
			}
			if( (m_Navigation.mode & Navigation.Mode.Horizontal) != 0)
			{
				return FindSelectable( transform.rotation * Vector3.left);
			}
			return null;
		}
		public virtual Selectable FindSelectableOnRight()
		{
			if( m_Navigation.mode == Navigation.Mode.Explicit)
			{
				return m_Navigation.selectOnRight;
			}
			if( (m_Navigation.mode & Navigation.Mode.Horizontal) != 0)
			{
				return FindSelectable( transform.rotation * Vector3.right);
			}
			return null;
		}
		public virtual Selectable FindSelectableOnUp()
		{
			if( m_Navigation.mode == Navigation.Mode.Explicit)
			{
				return m_Navigation.selectOnUp;
			}
			if( (m_Navigation.mode & Navigation.Mode.Vertical) != 0)
			{
				return FindSelectable( transform.rotation * Vector3.up);
			}
			return null;
		}
		public virtual Selectable FindSelectableOnDown()
		{
			if( m_Navigation.mode == Navigation.Mode.Explicit)
			{
				return m_Navigation.selectOnDown;
			}
			if( (m_Navigation.mode & Navigation.Mode.Vertical) != 0)
			{
				return FindSelectable( transform.rotation * Vector3.down);
			}
			return null;
		}
		public virtual void OnMove( AxisEventData eventData)
		{
			switch( eventData.moveDir)
			{
				case UnityEngine.EventSystems.MoveDirection.Right:
				{
					Navigate( eventData, FindSelectableOnRight());
					break;
				}
				case UnityEngine.EventSystems.MoveDirection.Up:
				{
					Navigate( eventData, FindSelectableOnUp());
					break;
				}
				case UnityEngine.EventSystems.MoveDirection.Left:
				{
					Navigate( eventData, FindSelectableOnLeft());
					break;
				}
				case UnityEngine.EventSystems.MoveDirection.Down:
				{
					Navigate( eventData, FindSelectableOnDown());
					break;
				}
			}
		}
		void StartColorTween( Color targetColor, bool instant)
		{
			if (m_TargetGraphic == null)
			{
				return;
			}
			m_TargetGraphic.CrossFadeColor( targetColor, instant ? 0f : m_Colors.fadeDuration, true, true);
		}
		void DoSpriteSwap( Sprite newSprite)
		{
			if( image == null)
			{
				return;
			}
			image.overrideSprite = newSprite;
		}
		void TriggerAnimation( string triggername)
		{
		#if PACKAGE_ANIMATION
			if( transition != Transition.Animation
			||	animator == null
			||	animator.isActiveAndEnabled == false
			||	animator.hasBoundPlayables == false
			||	string.IsNullOrEmpty( triggername) != false)
			{
				return;
			}
			animator.ResetTrigger( m_AnimationTriggers.normalTrigger);
			animator.ResetTrigger( m_AnimationTriggers.highlightedTrigger);
			animator.ResetTrigger( m_AnimationTriggers.pressedTrigger);
			animator.ResetTrigger( m_AnimationTriggers.selectedTrigger);
			animator.ResetTrigger( m_AnimationTriggers.disabledTrigger);
			animator.SetTrigger( triggername);
		#endif
		}
		protected bool IsHighlighted()
		{
			if( IsActive() == false || IsInteractable() == false)
			{
				return false;
			}
			return isPointerInside && !isPointerDown && !hasSelection;
		}
		protected bool IsPressed()
		{
			if( IsActive() == false || IsInteractable() == false)
			{
				return false;
			}
			return isPointerDown;
		}
		void EvaluateAndTransitionToSelectionState()
		{
			if( IsActive() == false || IsInteractable() == false)
			{
				return;
			}
			DoStateTransition( currentSelectionState, false);
		}
		public virtual void OnPointerDown( PointerEventData eventData)
		{
			if( eventData.button != PointerEventData.InputButton.Left)
			{
				return;
			}
			if( IsInteractable() != false && navigation.mode != Navigation.Mode.None && eventSystem != null)
			{
				eventSystem.SetSelectedGameObject( gameObject, eventData);
			}
			isPointerDown = true;
			EvaluateAndTransitionToSelectionState();
		}
		public virtual void OnPointerUp( PointerEventData eventData)
		{
			if( eventData.button != PointerEventData.InputButton.Left)
			{
				return;
			}
			isPointerDown = false;
			EvaluateAndTransitionToSelectionState();
		}
		public virtual void OnPointerEnter( PointerEventData eventData)
		{
			isPointerInside = true;
			EvaluateAndTransitionToSelectionState();
		}
		public virtual void OnPointerExit( PointerEventData eventData)
		{
			isPointerInside = false;
			EvaluateAndTransitionToSelectionState();
		}
		public virtual void OnSelect( BaseEventData eventData)
		{
			hasSelection = true;
			EvaluateAndTransitionToSelectionState();
		}
		public virtual void OnDeselect( BaseEventData eventData)
		{
			hasSelection = false;
			EvaluateAndTransitionToSelectionState();
		}
		public virtual void Select()
		{
			if( eventSystem == null || eventSystem.alreadySelecting != false)
			{
				return;
			}
			eventSystem.SetSelectedGameObject( gameObject);
		}
	}
}
