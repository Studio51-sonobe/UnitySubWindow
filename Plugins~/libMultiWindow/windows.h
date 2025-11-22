#pragma once
#include <d3d11.h>

#ifdef LIBUNIWINC_EXPORTS
#define DLL_API __stdcall
#define DLL_EXPORT extern "C" __declspec(dllexport)
#else
#define DLL_API __stdcall
#define DLL_EXPORT extern "C" __declspec(dllimport)
#endif

#pragma pack(push, 1)
struct TSubWindowEvent
{
    int index;
    UINT msg;
    int x;
    int y;
    int z;
    int w;
};
struct TPoint
{
    int x;
    int y;
};
#pragma pack(pop)

typedef void(* LogCallback)( const char*);
typedef void(* SubWindowEventCallback)( TSubWindowEvent ev);

DLL_EXPORT void DLL_API SetLogCallback( LogCallback logCallback);
DLL_EXPORT void DLL_API InitializeNative( HWND hWnd, int subWindowMaxCount);
DLL_EXPORT void DLL_API TerminateNative();
DLL_EXPORT TPoint DLL_API GetCursorPosition();
DLL_EXPORT void DLL_API MoveMainWindow( int x, int y);
DLL_EXPORT TPoint DLL_API GetMainWindowPoint();
DLL_EXPORT int DLL_API CreateSubWindow( 
    ID3D11Texture2D *pTexture,
    int x, int y, int width, int height, 
    SubWindowEventCallback pCallback);
DLL_EXPORT void DLL_API DisposeSubWindow( int windowIndex);
DLL_EXPORT void DLL_API MoveSubWindow( int windowIndex, int x, int y);
DLL_EXPORT TPoint DLL_API GetSubWindowPoint( int windowIndex);