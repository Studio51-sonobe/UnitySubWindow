
using UnityEngine;

namespace MultiWindow
{
	public partial class Window : MonoBehaviour
	{
		public static void Initialize( int subWindowMaxCount)
		{
	#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
		#if DEVELOPMENT_BUILD
			m_LogCallback = msg => UnityEngine.Debug.LogError( "[Native] " + msg);
			SetLogCallback( m_LogCallback);
		#endif
			InitializeNative( System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle, subWindowMaxCount);
	#endif
		}
		public static void Terminate()
		{
		#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
			TerminateNative();
		#endif
		}
		public static Vector2Int GetCursorPos()
		{
			TPoint point = GetCursorPosition();
			return new Vector2Int( point.x, point.y);
		}
		public static Vector2Int GetMainPosition()
		{
			TPoint point = GetMainWindowPoint();
			return new Vector2Int( point.x, point.y);
		}
		public static void MoveMain( int x, int y)
		{
			MoveMainWindow( x, y);
		}
	#if !UNITY_EDITOR && UNITY_STANDALONE_WIN && DEVELOPMENT_BUILD
		static LogCallback m_LogCallback;
	#endif
	}
}
