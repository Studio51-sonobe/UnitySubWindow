
#include "pch.h"
#include "windows.h"
#include <stdio.h>
#include <windowsx.h>
#pragma comment(lib, "d3d11.lib")
#pragma comment(lib, "dwmapi.lib")

#define CS_DEFAULT_STYLE	(CS_HREDRAW | CS_VREDRAW | CS_NOCLOSE)
#define WS_DEFAULT_STYLE	(WS_VISIBLE | WS_POPUP)
#define WS_DEFAULT_EXSTYLE	(WS_EX_LAYERED/* | WS_EX_TRANSPARENT*/)
#define SUB_WND_CLASS_NAME	L"UnitySubWnd"

struct TMainWindow
{
	HWND hWnd;
	int x;
	int y;
	int width;
	int height;
	WNDPROC defaultWndProc;
};
struct TSubWindow
{
	HWND hWnd;
	int index;
	int x;
	int y;
	int width;
	int height;
	ID3D11Texture2D *pTexture;
	ID3D11Device *pDevice;
	ID3D11DeviceContext *pContext;
	IDXGISwapChain *pSwapChain;
	SubWindowEventCallback pCallback;
	HANDLE pThreadHandle;
	BOOL threadKeep;
};
static LogCallback s_logCallback = NULL;
static TMainWindow s_mainWinodw = {};
static TSubWindow *s_pSubWindows = NULL;
static int s_subWindowMaxCount = 0;

void Log( const char *p)
{
	if( s_logCallback != NULL)
	{
		s_logCallback(p);
	}
	printf(p);
	OutputDebugStringA( p);
}
void DLL_API SetLogCallback( LogCallback logCallback)
{
	s_logCallback = logCallback;
}
int FindCreatableSubWindowIndex()
{
	TSubWindow *pWindow = NULL;

	for( int i0 = 0; i0 < s_subWindowMaxCount; ++i0)
	{
		pWindow = &s_pSubWindows[ i0];

		if( pWindow->hWnd == NULL)
		{
			return i0;
		}
	}
	return -1;
}
TSubWindow *FindSubWindow( HWND hWnd)
{
	TSubWindow *pWindow = NULL;

	for( int i0 = 0; i0 < s_subWindowMaxCount; ++i0)
	{
		pWindow = &s_pSubWindows[ i0];

		if( pWindow->hWnd == hWnd)
		{
			return pWindow;
		}
	}
	return NULL;
}
LRESULT CALLBACK MainWndProc( HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch( msg)
	{
		case WM_MOVE:
		{
			s_mainWinodw.x = LOWORD( lParam);
			s_mainWinodw.y = HIWORD( lParam);
			break;
		}
		case WM_SIZE:
		{
			s_mainWinodw.width = LOWORD( lParam);
			s_mainWinodw.height = HIWORD( lParam);
			break;
		}
	}
	return CallWindowProc( s_mainWinodw.defaultWndProc, hWnd, msg, wParam, lParam);
}
LRESULT CALLBACK SubWndProc( HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	TSubWindow *pWindow = FindSubWindow( hWnd);
	if( pWindow != NULL && pWindow->pCallback != NULL)
	{
		switch( msg)
		{
			case WM_CREATE:
			{
				break;
			}
			case WM_DESTROY:
			{
				break;
			}
			case WM_MOVE:
			{
				pWindow->x = LOWORD( lParam);
				pWindow->y = HIWORD( lParam);
				break;
			}
			case WM_SIZE:
			{
				pWindow->width = LOWORD( lParam);
				pWindow->height = HIWORD( lParam);
				break;
			}
			case WM_ENABLE:
			{
				break;
			}
			case WM_SETFOCUS:
			{
				break;
			}
			case WM_KILLFOCUS:
			{
				break;
			}
			case WM_NCHITTEST:
			{
				//return HTTRANSPARENT;
				//return HTCLIENT;
				break;
			}
			case WM_KEYDOWN:
			case WM_KEYUP:
			case WM_CHAR:
			case WM_IME_CHAR:
			{
				TSubWindowEvent ev;
				ev.index = pWindow->index;
				ev.msg = msg;
				ev.x = wParam;
				ev.y = 0;
				ev.z = 0;
				ev.w = 0;
				pWindow->pCallback( ev);
				break;
			}
			case WM_MOUSEMOVE:
			case WM_LBUTTONDOWN:
			case WM_LBUTTONUP:
			case WM_RBUTTONDOWN:
			case WM_RBUTTONUP:
			case WM_MBUTTONDOWN:
			case WM_MBUTTONUP:
			{
				TSubWindowEvent ev;
				ev.index = pWindow->index;
				ev.msg = msg;
				ev.x = GET_X_LPARAM( lParam);
				ev.y = GET_Y_LPARAM( lParam);
				ev.z = 0;
				ev.w = 0;
				pWindow->pCallback( ev);
				break;
			}
			case WM_MOUSEWHEEL:
			{
				TSubWindowEvent ev;
				ev.index = pWindow->index;
				ev.msg = msg;
				ev.x = GET_X_LPARAM( lParam);
				ev.y = GET_Y_LPARAM( lParam);
				ev.z = GET_KEYSTATE_WPARAM( wParam);
				ev.w = GET_WHEEL_DELTA_WPARAM( wParam);
				pWindow->pCallback( ev);
				break;
			}
		}
	}
	return DefWindowProc( hWnd, msg, wParam, lParam);
}
DWORD WINAPI RenderThread( void *pArgs)
{
	TSubWindow *pWindow = (TSubWindow *)pArgs;
	ID3D11Texture2D *pBackBuffer;
	HRESULT hr;

	while( pWindow->threadKeep != FALSE)
	{
		hr = pWindow->pSwapChain->GetBuffer( 0, __uuidof( ID3D11Texture2D), (void **)&pBackBuffer);
		if ( SUCCEEDED( hr) && pBackBuffer != NULL)
		{
			pWindow->pContext->CopyResource( pBackBuffer, pWindow->pTexture);
			pWindow->pSwapChain->Present( 1, 0);
			pBackBuffer->Release();
		}
		Sleep( 16);
	}
	return 0;
}
void DLL_API InitializeNative( HWND hWnd, int subWindowMaxCount)
{
	if( s_pSubWindows == NULL || subWindowMaxCount > 0)
	{
		s_subWindowMaxCount = subWindowMaxCount;
		s_pSubWindows = (TSubWindow *)malloc( sizeof( TSubWindow) * s_subWindowMaxCount);

		if( s_pSubWindows != NULL)
		{
			memset( s_pSubWindows, 0, sizeof( TSubWindow) * s_subWindowMaxCount);

			for( int i0 = 0; i0 < s_subWindowMaxCount; ++i0)
			{
				TSubWindow *pWindow = &s_pSubWindows[ i0];
				pWindow->index = i0;
			}
		}
	}
	if( s_mainWinodw.hWnd == NULL && hWnd != NULL)
	{
		s_mainWinodw.hWnd = hWnd;
		s_mainWinodw.defaultWndProc = (WNDPROC)SetWindowLongPtrA(
			s_mainWinodw.hWnd, GWLP_WNDPROC, (LONG_PTR)MainWndProc);

		SetWindowLongA( s_mainWinodw.hWnd, GWL_STYLE, WS_DEFAULT_STYLE);

		LONG exstyle = GetWindowLongA( s_mainWinodw.hWnd, GWL_EXSTYLE);
		exstyle |= WS_DEFAULT_EXSTYLE;
		SetWindowLongA( s_mainWinodw.hWnd, GWL_EXSTYLE, exstyle);
		SetWindowPos( s_mainWinodw.hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
		//SetWindowPos( s_mainWinodw.hWnd, HWND_TOPMOST, 0, 0, 512, 512, SWP_NOMOVE);

		//COLORREF cref = { 0 };
		//SetLayeredWindowAttributes( s_mainWinodw.hWnd, cref, 0, LWA_COLORKEY);
		//SetLayeredWindowAttributes( s_mainWinodw.hWnd, cref, 0xff, LWA_COLORKEY | LWA_ALPHA);

		//MARGINS margins0 = { 0, 0, 0, 0 };
		//DwmExtendFrameIntoClientArea( s_mainWinodw.hWnd, &margins0);

		MARGINS margins1 = { -1, 0, 0, 0 };
		DwmExtendFrameIntoClientArea( s_mainWinodw.hWnd, &margins1);

		RECT rect;
		GetWindowRect( s_mainWinodw.hWnd, &rect);
		s_mainWinodw.x = rect.left;
		s_mainWinodw.y = rect.top;
		s_mainWinodw.width = rect.right - rect.left;
		s_mainWinodw.height = rect.bottom - rect.top;

		WNDCLASSEXW wcex;
		memset( &wcex, 0, sizeof(wcex));
		wcex.cbSize = sizeof(WNDCLASSEXW);
		wcex.style = CS_DEFAULT_STYLE;
		wcex.lpfnWndProc = SubWndProc;
		wcex.hInstance = GetModuleHandle( NULL);
		wcex.lpszClassName = SUB_WND_CLASS_NAME;
		wcex.hbrBackground = reinterpret_cast<HBRUSH>( COLOR_WINDOW + 1);
		RegisterClassExW( &wcex);
	}
}
void DLL_API TerminateNative()
{
	if( s_pSubWindows != NULL)
	{
		for( int i0 = 0; i0 < s_subWindowMaxCount; ++i0)
		{
			DisposeSubWindow( i0);
		}
		free( s_pSubWindows);
		s_pSubWindows = NULL;
	}
	if( s_mainWinodw.hWnd != NULL && s_mainWinodw.defaultWndProc != NULL)
	{
		SetWindowLongPtrA( s_mainWinodw.hWnd, 
			GWLP_WNDPROC, (LONG_PTR)s_mainWinodw.defaultWndProc);
		s_mainWinodw.defaultWndProc = NULL;
	}
}
TPoint DLL_API GetCursorPosition()
{
	TPoint position = { 0, 0 };
	POINT point;
	
	if( GetCursorPos( &point) != FALSE)
	{
		position.x = point.x;
		position.y = point.y;
	}
	return position;
}
void DLL_API MoveMainWindow( int x, int y)
{
	if( s_mainWinodw.hWnd != NULL)
	{
		MoveWindow( s_mainWinodw.hWnd, x, y, s_mainWinodw.width, s_mainWinodw.height, TRUE);
	}
}
TPoint DLL_API GetMainWindowPoint()
{
	TPoint point = 
	{
		s_mainWinodw.x, 
		s_mainWinodw.y
	};
	return point;
}
int DLL_API CreateSubWindow( ID3D11Texture2D *pTexture, int x, int y, int width, int height, SubWindowEventCallback pCallback)
{
	if( s_mainWinodw.hWnd == NULL || s_pSubWindows == NULL || pTexture == NULL)
	{
		return -1;
	}
	int windowIndex = FindCreatableSubWindowIndex();

	if( windowIndex < 0)
	{
		return -1;
	}
	TSubWindow *pWindow = &s_pSubWindows[ windowIndex];
	HMODULE hInstance = ::GetModuleHandleW( NULL);

	if( hInstance == NULL)
	{
		return -1;
	}
	pWindow->hWnd = ::CreateWindowExW(
		0,
		SUB_WND_CLASS_NAME,
		L"",
		WS_DEFAULT_STYLE, /* WS_OVERLAPPEDWINDOW, */
		x, y, width, height,
		s_mainWinodw.hWnd, NULL,
		hInstance,
		NULL);

	if( pWindow->hWnd == NULL)
	{
		return -1;
	}
	else
	{
		HRESULT hr;
		MARGINS margins1 = { -1 };

		hr = DwmExtendFrameIntoClientArea( pWindow->hWnd, &margins1);
		if FAILED( hr)
		{
			DestroyWindow( pWindow->hWnd);
			pWindow->hWnd = NULL;
			return -1;
		}
		else
		{
			DXGI_SWAP_CHAIN_DESC sd = {};
			sd.BufferCount = 1;
			sd.BufferDesc.Width = width;
			sd.BufferDesc.Height = height;
			sd.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
			sd.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
			sd.OutputWindow = pWindow->hWnd;
			sd.SampleDesc.Count = 1;
			sd.Windowed = TRUE;

			hr = D3D11CreateDeviceAndSwapChain(
				NULL,
				D3D_DRIVER_TYPE_HARDWARE,
				NULL,
				0,
				NULL, 0,
				D3D11_SDK_VERSION,
				&sd,
				&pWindow->pSwapChain,
				&pWindow->pDevice,
				NULL,
				&pWindow->pContext);

			if FAILED( hr)
			{
				DestroyWindow( pWindow->hWnd);
				pWindow->hWnd = NULL;
				return -1;
			}
			else
			{
				SetWindowLongA( pWindow->hWnd, GWL_STYLE, WS_DEFAULT_STYLE);

				LONG exstyle = GetWindowLongA( pWindow->hWnd, GWL_EXSTYLE);
				exstyle |= WS_DEFAULT_EXSTYLE;
				SetWindowLongA( pWindow->hWnd, GWL_EXSTYLE, exstyle);
				SetWindowPos( pWindow->hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);

				ShowWindow( pWindow->hWnd, SW_SHOW);
				pWindow->threadKeep = TRUE;
				pWindow->pTexture = pTexture;
				pWindow->pCallback = pCallback;
				pWindow->x = x;
				pWindow->y = y;
				pWindow->width = width;
				pWindow->height = height;
				pWindow->pThreadHandle = CreateThread( NULL, 0, RenderThread, pWindow, 0, NULL);
			}
		}
	}
	return windowIndex;
}
void DLL_API DisposeSubWindow( int windowIndex)
{
	if( s_pSubWindows == NULL || windowIndex < 0 || windowIndex >= s_subWindowMaxCount)
	{
		return;
	}
	TSubWindow *pWindow = &s_pSubWindows[ windowIndex];

	if( pWindow->pThreadHandle != NULL)
	{
		pWindow->threadKeep = FALSE;
		WaitForSingleObject( pWindow->pThreadHandle, INFINITE);
		pWindow->pThreadHandle = NULL;
	}
	if( pWindow->pContext != NULL)
	{
		pWindow->pContext->ClearState();
		pWindow->pContext->Flush();
		pWindow->pContext->Release();
		pWindow->pContext = NULL;
	}
	if( pWindow->pSwapChain != NULL)
	{
		pWindow->pSwapChain->Release();
		pWindow->pSwapChain = NULL;
	}
	if( pWindow->pDevice != NULL)
	{
		pWindow->pDevice->Release();
		pWindow->pDevice = NULL;
	}
	if( pWindow->hWnd != NULL)
	{
		DestroyWindow( pWindow->hWnd);
		pWindow->hWnd = NULL;
	}
}
void DLL_API MoveSubWindow( int windowIndex, int x, int y)
{
	if( s_pSubWindows == NULL || windowIndex < 0 || windowIndex >= s_subWindowMaxCount)
	{
		return;
	}
	TSubWindow *pWindow = &s_pSubWindows[ windowIndex];

	if( pWindow != NULL)
	{
		MoveWindow( pWindow->hWnd, x, y, pWindow->width, pWindow->height, TRUE);
	}
}
TPoint DLL_API GetSubWindowPoint( int windowIndex)
{
	TPoint point = { 0, 0 };

	if( s_pSubWindows != NULL && windowIndex >= 0 && windowIndex < s_subWindowMaxCount)
	{
		TSubWindow *pWindow = &s_pSubWindows[ windowIndex];

		if( pWindow != NULL)
		{
			point.x = pWindow->x;
			point.y = pWindow->y;
		}
	}
	return point;
}