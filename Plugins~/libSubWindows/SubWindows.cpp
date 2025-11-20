
#include "pch.h"
#include "SubWindows.h"
#include <stdio.h>
#include <windowsx.h>
#pragma comment(lib, "d3d11.lib")

#define CS_DEFAULT_STYLE	(CS_HREDRAW | CS_VREDRAW | CS_NOCLOSE)
#define WS_DEFAULT_STYLE	(WS_VISIBLE | WS_POPUP)
#define WS_DEFAULT_EXSTYLE	(WS_EX_LAYERED/* | WS_EX_TRANSPARENT*/)
#define SUB_WND_CLASS_NAME	L"UnitySubWnd"

struct TSubWindow
{
	int index;
	HWND hWnd;
	ID3D11Texture2D *pTexture;
	ID3D11Device *pDevice;
	ID3D11DeviceContext *pContext;
	IDXGISwapChain *pSwapChain;
	HANDLE pThreadHandle;
	BOOL threadKeep;
	SubWindowEventCallback pCallback;
};

static LogCallback s_LogCallback = NULL;
static HWND s_CurrentWindowHandle = NULL;
static WNDPROC s_DefaultWindowProc = NULL;
static TSubWindow *s_pSubWindows = NULL;
static int s_SubWindowMaxCount = 0;

void Log( const char *p)
{
	if( s_LogCallback != NULL)
	{
		s_LogCallback(p);
	}
	printf(p);
	OutputDebugStringA( p);
}
void DLL_API SetLogCallback( LogCallback logCallback)
{
	s_LogCallback = logCallback;
}
int FindCreatableSubWindowIndex()
{
	TSubWindow *pWindow = NULL;

	for( int i0 = 0; i0 < s_SubWindowMaxCount; ++i0)
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

	for( int i0 = 0; i0 < s_SubWindowMaxCount; ++i0)
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
			break;
		}
		case WM_SIZE:
		{
			break;
		}
	}
	return CallWindowProc( s_DefaultWindowProc, hWnd, msg, wParam, lParam);
}
LRESULT CALLBACK SubWndProc( HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	TSubWindow *pWindow = FindSubWindow( hWnd);
	if( pWindow != NULL && pWindow->pCallback != NULL)
	{
		switch( msg)
		{
			case WM_MOVE:
			{
				break;
			}
			case WM_SIZE:
			{
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
		s_SubWindowMaxCount = subWindowMaxCount;
		s_pSubWindows = (TSubWindow *)malloc( sizeof( TSubWindow) * s_SubWindowMaxCount);

		if( s_pSubWindows != NULL)
		{
			memset( s_pSubWindows, 0, sizeof( TSubWindow) * s_SubWindowMaxCount);

			for( int i0 = 0; i0 < s_SubWindowMaxCount; ++i0)
			{
				TSubWindow *pWindow = &s_pSubWindows[ i0];
				pWindow->index = i0;
			}
		}
	}
	if( s_CurrentWindowHandle == NULL && hWnd != NULL)
	{
		s_CurrentWindowHandle = hWnd;
		//s_DefaultWindowProc = (WNDPROC)SetWindowLongPtrA(
		//	s_CurrentWindowHandle, GWLP_WNDPROC, (LONG_PTR)MainWndProc);

		//COLORREF cref = { 0 };
		//SetLayeredWindowAttributes( s_CurrentWindowHandle, cref, 0xff, LWA_ALPHA);

		//MARGINS margins0 = { 0, 0, 0, 0 };
		//DwmExtendFrameIntoClientArea( s_CurrentWindowHandle, &margins0);

		MARGINS margins1 = { -1 };
		DwmExtendFrameIntoClientArea( s_CurrentWindowHandle, &margins1);

		SetWindowLongA( s_CurrentWindowHandle, GWL_STYLE, WS_DEFAULT_STYLE);

		LONG exstyle = GetWindowLongA( s_CurrentWindowHandle, GWL_EXSTYLE);
		exstyle |= WS_DEFAULT_EXSTYLE;
		SetWindowLongA( s_CurrentWindowHandle, GWL_EXSTYLE, exstyle);

		SetWindowPos( s_CurrentWindowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);

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
		for( int i0 = 0; i0 < s_SubWindowMaxCount; ++i0)
		{
			DisposeSubWindow( i0);
		}
		free( s_pSubWindows);
		s_pSubWindows = NULL;
	}
	if( s_DefaultWindowProc != NULL)
	{
		SetWindowLongPtrA( s_CurrentWindowHandle, 
			GWLP_WNDPROC, (LONG_PTR)s_DefaultWindowProc);
		s_DefaultWindowProc = NULL;
	}
}
int DLL_API CreateSubWindow( ID3D11Texture2D *pTexture, int width, int height, SubWindowEventCallback pCallback)
{
	if( s_CurrentWindowHandle == NULL || s_pSubWindows == NULL || pTexture == NULL)
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
		WS_DEFAULT_STYLE,
		//WS_OVERLAPPEDWINDOW,
		CW_USEDEFAULT, CW_USEDEFAULT,
		width, height,
		s_CurrentWindowHandle, NULL,
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
				ShowWindow( pWindow->hWnd, SW_SHOW);
				pWindow->threadKeep = TRUE;
				pWindow->pTexture = pTexture;
				pWindow->pCallback = pCallback;
				pWindow->pThreadHandle = CreateThread( NULL, 0, RenderThread, pWindow, 0, NULL);
			}
		}
	}
	return windowIndex;
}
void DLL_API DisposeSubWindow( int windowIndex)
{
	if( s_pSubWindows == NULL || windowIndex < 0 || windowIndex >= s_SubWindowMaxCount)
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

