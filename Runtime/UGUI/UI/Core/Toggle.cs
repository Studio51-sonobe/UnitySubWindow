using System;
using UnityEngine;
using UnityEngine.Events;
using MultiWindow.EventSystems;

namespace MultiWindow.UI
{
	[AddComponentMenu("MultiWindow/UI/Toggle", 30)]
	[RequireComponent(typeof(RectTransform))]
	public class Toggle : Selectable, IPointerClickHandler, ISubmitHandler, UnityEngine.UI.ICanvasElement
	{
		public enum ToggleTransition
		{
			None,
			Fade
		}
		public bool isOn
		{
			get { return m_IsOn; }
			set { Set(value); }
		}
		public ToggleGroup group
		{
			get { return m_Group; }
			set
			{
				SetToggleGroup(value, true);
				PlayEffect(true);
			}
		}
		protected Toggle()
		{	
		}
	#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			
			if( !UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this) && !Application.isPlaying)
			{
				UnityEngine.UI.CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
			}
		}
	#endif
		public virtual void Rebuild( UnityEngine.UI.CanvasUpdate executing)
		{
		#if UNITY_EDITOR
			if( executing == UnityEngine.UI.CanvasUpdate.Prelayout)
			{
				onValueChanged.Invoke(m_IsOn);
			}
		#endif
		}
		public virtual void LayoutComplete()
		{
		}
		public virtual void GraphicUpdateComplete()
		{
		}
		protected override void OnDestroy()
		{
			if( m_Group != null)
				m_Group.EnsureValidState();
			base.OnDestroy();
		}
		protected override void OnEnable()
		{
			base.OnEnable();
			SetToggleGroup(m_Group, false);
			PlayEffect(true);
		}
		protected override void OnDisable()
		{
			SetToggleGroup(null, false);
			base.OnDisable();
		}
		protected override void OnDidApplyAnimationProperties()
		{
			if( graphic != null)
			{
				bool oldValue = !Mathf.Approximately(graphic.canvasRenderer.GetColor().a, 0);
				if( m_IsOn != oldValue)
				{
					m_IsOn = oldValue;
					Set(!oldValue);
				}
			}
			base.OnDidApplyAnimationProperties();
		}
		private void SetToggleGroup( ToggleGroup newGroup, bool setMemberValue)
		{
			if( m_Group != null)
			{
				m_Group.UnregisterToggle(this);
			}
			if( setMemberValue)
			{
				m_Group = newGroup;
			}
			if( newGroup != null && IsActive())
			{
				newGroup.RegisterToggle(this);
			}
			if( newGroup != null && isOn && IsActive())
			{
				newGroup.NotifyToggleOn(this);
			}
		}
		public void SetIsOnWithoutNotify( bool value)
		{
			Set(value, false);
		}
		void Set( bool value, bool sendCallback = true)
		{
			if( m_IsOn == value)
			{
				return;
			}
			m_IsOn = value;
			
			if( m_Group != null && m_Group.isActiveAndEnabled && IsActive())
			{
				if( m_IsOn ||( !m_Group.AnyTogglesOn() && !m_Group.allowSwitchOff))
				{
					m_IsOn = true;
					m_Group.NotifyToggleOn( this, sendCallback);
				}
			}
			PlayEffect( toggleTransition == ToggleTransition.None);
			
			if( sendCallback)
			{
				UISystemProfilerApi.AddMarker("Toggle.value", this);
				onValueChanged.Invoke(m_IsOn);
			}
		}
		private void PlayEffect(bool instant)
		{
			if( graphic == null)
			{
				return;
			}
		#if UNITY_EDITOR
			if( !Application.isPlaying)
			{
				graphic.canvasRenderer.SetAlpha(m_IsOn ? 1f : 0f);
			}
			else
			{
		#endif
				graphic.CrossFadeAlpha( m_IsOn ? 1f : 0f, instant ? 0f : 0.1f, true);
			}
		}
		protected override void Start()
		{
			PlayEffect( true);
		}
		private void InternalToggle()
		{
			if( !IsActive() || !IsInteractable())
			{
				return;
			}
			isOn = !isOn;
		}
		public virtual void OnPointerClick( PointerEventData eventData)
		{
			if( eventData.button != PointerEventData.InputButton.Left)
			{
				return;
			}
			InternalToggle();
		}
		public virtual void OnSubmit(BaseEventData eventData)
		{
			InternalToggle();
		}
		[Serializable]
		public class ToggleEvent : UnityEvent<bool>{}
		[SerializeField]
		public ToggleTransition toggleTransition = ToggleTransition.Fade;
		[SerializeField]
		public Graphic graphic;
		[SerializeField]
		ToggleGroup m_Group;
		[SerializeField]
		public ToggleEvent onValueChanged = new();
		[Tooltip("Is the toggle currently on or off?")]
		[SerializeField]
		private bool m_IsOn;
	}
}
