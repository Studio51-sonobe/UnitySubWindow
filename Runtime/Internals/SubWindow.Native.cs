
using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace SubWindows
{
	public partial class SubWindow : MonoBehaviour
	{
		[StructLayout( LayoutKind.Sequential)]
		internal struct InputEvent
		{
			internal int index;
			internal uint msg;
			internal int x;
			internal int y;
		}
		[UnmanagedFunctionPointer( CallingConvention.Winapi)]
		delegate void LogCallback( string message);
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		internal delegate void InputEventCallback( InputEvent msg);
		
		[DllImport( "libSubWindows", CallingConvention = CallingConvention.Winapi)]
		static extern void SetLogCallback( LogCallback logCallback);
		[DllImport( "libSubWindows", CallingConvention = CallingConvention.Winapi)]
		static extern void InitializeNative( IntPtr hWnd, int subWindowMaxCount);
		[DllImport( "libSubWindows", CallingConvention = CallingConvention.Winapi)]
		static extern void TerminateNative();
		[DllImport( "libSubWindows", CallingConvention = CallingConvention.Winapi)]
		static extern int CreateSubWindow( IntPtr texturePtr, int width, int height, InputEventCallback callback);
		[DllImport( "libSubWindows", CallingConvention = CallingConvention.Winapi)]
		static extern void DisposeSubWindow( int windowIndex);
	}
}
