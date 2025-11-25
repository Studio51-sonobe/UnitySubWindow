using UnityEngine;
using MultiWindow.EventSystems;

namespace MultiWindow.UI
{
	internal static class MultipleDisplayUtilities
	{
		public static bool GetRelativeMousePositionForDrag( PointerEventData eventData, ref Vector2 position)
		{
		#if UNITY_EDITOR
			position = eventData.position;
		#else
			int pressDisplayIndex = eventData.pointerPressRaycast.displayIndex;
			var relativePosition = RelativeMouseAtScaled( eventData.position, eventData.displayIndex);
			int currentDisplayIndex = (int)relativePosition.z;
			
			if (currentDisplayIndex != pressDisplayIndex)
			{
				return false;
			}
			position = (pressDisplayIndex != 0)? (Vector2)relativePosition : eventData.position;
		#endif
			return true;
		}
		internal static Vector3 GetRelativeMousePositionForRaycast( PointerEventData eventData)
		{
			Vector3 eventPosition = RelativeMouseAtScaled(eventData.position, eventData.displayIndex);
			
			if (eventPosition == Vector3.zero)
			{
				eventPosition = eventData.position;
			#if UNITY_EDITOR
				eventPosition.z = Display.activeEditorGameViewTarget;
			#endif
			}
		#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM
			if (eventData.displayIndex > 0)
			{
				eventPosition.z = eventData.displayIndex;
			}
		#endif
			return eventPosition;
		}
		public static Vector3 RelativeMouseAtScaled( Vector2 position, int displayIndex)
		{
	#if !UNITY_EDITOR && !UNITY_WSA
			var display = Display.main;
		#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM
			if (displayIndex >= Display.displays.Length)
			{
				displayIndex = 0;
			}
			display = Display.displays[displayIndex];
			
			if (!Screen.fullScreen)
			{
				return new Vector3(position.x, position.y, displayIndex);
			}
		#endif
			if (display.renderingWidth != display.systemWidth || display.renderingHeight != display.systemHeight)
			{
				var systemAspectRatio = display.systemWidth / (float)display.systemHeight;
				var sizePlusPadding = new Vector2(display.renderingWidth, display.renderingHeight);
				var padding = Vector2.zero;
				
				if (Screen.fullScreen)
				{
					var aspectRatio = Screen.width / (float)Screen.height; // This assumes aspectRatio is the same for all displays
					if (display.systemHeight * aspectRatio < display.systemWidth)
					{
						// Horizontal padding
						sizePlusPadding.x = display.renderingHeight * systemAspectRatio;
						padding.x = (sizePlusPadding.x - display.renderingWidth) * 0.5f;
					}
					else
					{
						// Vertical padding
						sizePlusPadding.y = display.renderingWidth / systemAspectRatio;
						padding.y = (sizePlusPadding.y - display.renderingHeight) * 0.5f;
					}
				}
				var sizePlusPositivePadding = sizePlusPadding - padding;
				
				if( position.y < -padding.y
				||	position.y > sizePlusPositivePadding.y
				||	position.x < -padding.x
				||	position.x > sizePlusPositivePadding.x)
				{
					var adjustedPosition = position;
					
					if (!Screen.fullScreen)
					{
						adjustedPosition.x -= (display.renderingWidth - display.systemWidth) * 0.5f;
						adjustedPosition.y -= (display.renderingHeight - display.systemHeight) * 0.5f;
					}
					else
					{
						adjustedPosition += padding;
						adjustedPosition.x *= display.systemWidth / sizePlusPadding.x;
						adjustedPosition.y *= display.systemHeight / sizePlusPadding.y;
					}
				#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_ANDROID || UNITY_EMBEDDED_LINUX || UNITY_QNX)
					var relativePos = new Vector3(adjustedPosition.x, adjustedPosition.y, displayIndex);
				#else
					var relativePos = Display.RelativeMouseAt(adjustedPosition);
				#endif
					if (relativePos.z != 0)
					{
						return relativePos;
					}
				}
			#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM && (UNITY_ANDROID || UNITY_EMBEDDED_LINUX || UNITY_QNX)
				return new Vector3(position.x, position.y, displayIndex);
			#else
				return new Vector3(position.x, position.y, 0);
			#endif
			}
	#endif
		#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_ANDROID || UNITY_EMBEDDED_LINUX || UNITY_QNX)
			return new Vector3(position.x, position.y, displayIndex);
		#else
			return Display.RelativeMouseAt(position);
		#endif
		}
	}
}
