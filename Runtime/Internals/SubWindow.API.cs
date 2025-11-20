
using UnityEngine;

namespace SubWindows
{
	public partial class SubWindow : MonoBehaviour
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
	#if !UNITY_EDITOR && UNITY_STANDALONE_WIN && DEVELOPMENT_BUILD
		static LogCallback m_LogCallback;
	#endif
	}
}
