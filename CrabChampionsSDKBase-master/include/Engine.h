#pragma once
#include "helper.h"
#include "SDK.h"

namespace SDK {
	class UWorld;
	class ACrabCharacter;
	class ACrabPS;
	class UCrabHC;
	class UCharacterMovementComponent;
	struct FVector;
}

namespace DX11Base
{
	class Engine
	{
	public:

		//	PROCESS & WINDOW
		int									mGamePID{ 0 };
		HMODULE								pGameModule{ 0 };
		__int64								pGameBaseAddr{ 0 };
		HANDLE								pGameHandle{ 0 };
		HWND								pGameWindow{ 0 };
		int									mGameWidth{ 0 };
		int									mGameHeight{ 0 };
		ImVec2								mWindowSize{ 0.f , 0.f };
		LPCSTR								pGameTitle{ 0 };
		LPCSTR								pClassName{ 0 };
		LPCSTR								pGamePath{ 0 };

		//	MENU
		bool								bShowMenu{ false };
		bool								bShowHUD{ false };   // fixed type and name
		bool								bShowDemo{ false };
		bool								bShowESP{ false };
		bool								bShowDemoWindow{ false };
		bool								bShowStyleEditor{ false };
		bool bGodMode = false; // defaults to OFF
		bool bInfiniteAmmo = false;   // Infinite clip / ammo
		bool bNoReload = false;
		bool bNoCooldowns = false;
		float g_overridePlayerScale = 1.f;
		float g_overrideMeleeRange = 5.f; // default value
		bool g_bNoSpread = false;



		SDK::UWorld* pWorld{ nullptr };   // use pointer, NOT value
		SDK::ACrabPS* pLocalPS{ nullptr };     // pointer to local player's ACrabPS
		SDK::UCrabHC* pLocalHC{ nullptr };


	public:
		void								Init();

	public:
		static bool							GetKeyState(WORD vKey, SHORT delta);
		static bool							GamePadGetKeyState(WORD vButton);

		//	constructor
		Engine();
		~Engine();
	};
	inline std::unique_ptr<Engine> g_Engine;
	inline std::vector<SDK::ACrabEnemyC*> gCachedEnemies;

	class Console
	{
	public:
		enum EColors : int
		{
			EColor_dark_blue = 1,
			EColor_dark_green,
			EColor_dark_teal,
			EColor_dark_red,
			EColor_dark_pink,
			EColor_dark_yellow,
			EColor_dark_white,
			EColor_dark_gray,
			EColor_blue,
			EColor_green,
			EColor_teal,
			EColor_red,
			EColor_pink,
			EColor_yellow,
			EColor_white,
			EColor_DEFAULT = EColor_white
		};

	public:
		static FILE* pOutStream;
		static bool							bInit;
		bool								bShow{ false };
		bool								bVerbose{ false };

	public:
		void								InitializeConsole(const char* ConsoleName, bool bShowWindow = true);
		void								cLog(const char* fmt, const EColors Color = EColor_DEFAULT, ...);
		void								LogError(const char* fmt, ...);
		void								DestroyConsole();
		void								SetConsoleVisibility(bool bShow);
		HANDLE								GetHandle();
		HWND								GetWindowHandle();

	public:
		static void							Log(const char* fmt, ...);
		static void							Clear();

	public:
		explicit Console();
		Console(const char* title);
		Console(const char* title, bool bShow);
		~Console();

	private:
		HANDLE								pHandle{};
		HWND								pHwnd{};
	};
	inline std::unique_ptr<Console> g_Console;

	class D3D11Window
	{
	public:
		enum DXGI : int
		{
			IDXGI_PRESENT = 8,
			IDXGI_DRAW_INDEXED = 12,
			IDXGI_RESIZE_BUFFERS = 13,
		};

	public:
		bool								bInit{ false };
		bool								bInitImGui{ false };
		WNDPROC								m_OldWndProc{};
		ImGuiContext* pImGui;
		ImGuiViewport* pViewport;

	public:
		bool								GetD3DContext();
		bool								HookD3D();
		void								UnhookD3D();
		bool								InitWindow();
		bool								DeleteWindow();
		bool								InitImGui(IDXGISwapChain* swapChain);
		void								Overlay(IDXGISwapChain* pSwapChain);

	public:
		explicit D3D11Window();
		~D3D11Window() noexcept;

	private:
		typedef HRESULT(WINAPI* IDXGISwapChainPresent)(IDXGISwapChain* pSwapChain, UINT SyncInterval, UINT Flags);
		typedef void(APIENTRY* ID3D11DeviceContextDrawIndexed)(ID3D11DeviceContext* pContext, UINT IndexCount, UINT StartIndexLocation, INT BaseVertexLocation);
		typedef HRESULT(WINAPI* IDXGISwapChainResizeBuffers)(IDXGISwapChain* pSwapChain, UINT BufferCount, UINT Width, UINT Height, DXGI_FORMAT NewFormat, UINT SwapChainFlags);
		IDXGISwapChainPresent				IDXGISwapChain_Present_stub = 0;
		ID3D11DeviceContextDrawIndexed		ID3D11DeviceContext_DrawIndexed_stub = 0;
		IDXGISwapChainResizeBuffers			IDXGISwapChain_ResizeBuffers_stub = 0;
		WNDCLASSEX							WindowClass;
		HWND								WindowHwnd;
		ID3D11Device* m_Device{};
		ID3D11DeviceContext* m_DeviceContext{};
		ID3D11RenderTargetView* m_RenderTargetView{};
		IDXGISwapChain* m_pSwapChain{};


	private:
		static LRESULT APIENTRY				WndProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
		static HRESULT APIENTRY				SwapChain_Present_hook(IDXGISwapChain* pSwapChain, UINT SyncInterval, UINT Flags);
		static void APIENTRY				DeviceContext_DrawIndexed_hook(ID3D11DeviceContext* pContext, UINT IndexCount, UINT StartIndexLocation, INT BaseVertexLocation);
		static HRESULT WINAPI				SwapChain_ResizeBuffers_hook(IDXGISwapChain* pSwapChain, UINT BufferCount, UINT Width, UINT Height, DXGI_FORMAT NewFormat, UINT SwapChainFlags);
	};
	inline std::unique_ptr<D3D11Window> g_D3D11Window;

	class Hooking
	{
	public:
		void								Initialize();
		void								Shutdown();
		static bool							CreateHook(LPVOID pTarget, LPVOID pDetour, LPVOID* pOrig);
		static void							EnableHook(LPVOID pTarget);
		static void							EnableAllHooks();
		static void							DisableHook(LPVOID pTarget);
		static void							RemoveHook(LPVOID pTarget);
		static void							DisableAllHooks();
		static void							RemoveAllHooks();


	public:
		explicit Hooking();
		~Hooking() noexcept;
	};
	inline std::unique_ptr<Hooking> g_Hooking;
}
