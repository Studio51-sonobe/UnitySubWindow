
using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace MultiWindow
{
	public partial class Window : MonoBehaviour
	{
		[StructLayout( LayoutKind.Sequential)]
		internal struct InputEvent
		{
			internal int index;
			internal uint msg;
			internal int x;
			internal int y;
			internal int z;
			internal int w;
		}
		[StructLayout( LayoutKind.Sequential)]
		internal struct TPoint
		{
			internal int x;
			internal int y;
		}
		[UnmanagedFunctionPointer( CallingConvention.Winapi)]
		delegate void LogCallback( string message);
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		internal delegate void InputEventCallback( InputEvent msg);
		
		[DllImport( "libMultiWindow", CallingConvention = CallingConvention.Winapi)]
		static extern void SetLogCallback( LogCallback logCallback);
		[DllImport( "libMultiWindow", CallingConvention = CallingConvention.Winapi)]
		static extern void InitializeNative( IntPtr hWnd, int subWindowMaxCount);
		[DllImport( "libMultiWindow", CallingConvention = CallingConvention.Winapi)]
		static extern void TerminateNative();
		[DllImport( "libMultiWindow", CallingConvention = CallingConvention.Winapi)]
		static extern TPoint GetCursorPosition();
		[DllImport( "libMultiWindow", CallingConvention = CallingConvention.Winapi)]
		static extern void MoveMainWindow( int x, int y);
		[DllImport( "libMultiWindow", CallingConvention = CallingConvention.Winapi)]
		static extern TPoint GetMainWindowPoint();
		[DllImport( "libMultiWindow", CallingConvention = CallingConvention.Winapi)]
		static extern int CreateSubWindow( IntPtr texturePtr, 
			int x, int y, int width, int height, InputEventCallback callback);
		[DllImport( "libMultiWindow", CallingConvention = CallingConvention.Winapi)]
		static extern void DisposeSubWindow( int windowIndex);
		[DllImport( "libMultiWindow", CallingConvention = CallingConvention.Winapi)]
		static extern void MoveSubWindow( int windowIndex, int x, int y);
		[DllImport( "libMultiWindow", CallingConvention = CallingConvention.Winapi)]
		static extern TPoint GetSubWindowPoint( int windowIndex);
	}
}
