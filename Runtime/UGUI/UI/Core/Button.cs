using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using MultiWindow.EventSystems;

namespace MultiWindow.UI
{
	[AddComponentMenu( "MultiWindowUI/Button", 30)]
	public class Button : Selectable, IPointerClickHandler, ISubmitHandler
	{
		protected Button()
		{
		}
		public ButtonClickedEvent onClick
		{
			get { return m_OnClick; }
			set { m_OnClick = value; }
		}
		private void Press()
		{
			if (!IsActive() || !IsInteractable())
			{
				return;
			}
			UISystemProfilerApi.AddMarker("Button.onClick", this);
			m_OnClick.Invoke();
		}
		public virtual void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
			{
				return;
			}
			Press();
		}
		public virtual void OnSubmit(BaseEventData eventData)
		{
			Press();
			
			if (!IsActive() || !IsInteractable())
			{
				return;
			}
			DoStateTransition(SelectionState.Pressed, false);
			StartCoroutine(OnFinishSubmit());
		}
		private IEnumerator OnFinishSubmit()
		{
			var fadeTime = colors.fadeDuration;
			var elapsedTime = 0f;
			
			while (elapsedTime < fadeTime)
			{
				elapsedTime += Time.unscaledDeltaTime;
				yield return null;
			}
			DoStateTransition(currentSelectionState, false);
		}
		[Serializable]
		public class ButtonClickedEvent : UnityEvent {}
		
		[FormerlySerializedAs("onClick")]
		[SerializeField]
		ButtonClickedEvent m_OnClick = new();
	}
}
