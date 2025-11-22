
using UnityEngine;

namespace MultiWindow.EventSystems
{
	public class AxisEventData : BaseEventData
	{
		public Vector2 moveVector { get; set; }
		public UnityEngine.EventSystems.MoveDirection moveDir { get; set; }
		
		public AxisEventData( EventSystem eventSystem) : base( eventSystem)
		{
			moveVector = Vector2.zero;
			moveDir = UnityEngine.EventSystems.MoveDirection.None;
		}
	}
}
