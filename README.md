# SubWindows

Unity のメインウィンドウ以外に別のウィンドウを生成、運用することができる `サブウィンドウ` 提供するパッケージです。

１つのサブウィンドウにつき SubWindow コンポーネントが１つ必要になります。

※現状では URP 環境のみの対応で、Built-in、HDRP、独自のSRP拡張環境では動作しません。

※メインウィンドウを半透明化する機能も提供されていますが、今後強制適用ではなくなる可能性があります。

# 環境構築

サブウィンドウを実現するためには以下の環境を構築する必要があります。

## Project Settings

- Graphics/Render Graph Compatibility Mode: OFF
- Player/Resolution and Presentation/Standalone Player Options/Use DXGI flip model swapchain for D3D11: OFF ※半透明化に必要
- Player/Other Settings/Rendering/Auto Graphics API for Windows: OFF
- Player/Other Settings/Rendering/Graphics API for Windows: Direct3D11
- Tags and Layers/Layers: サブウィンドウ分のレイヤーを定義

## URP

サブウィンドウ分 + その他 + シーンビュー用の Universal Renderer Data を用意し、Filtering を設定する

以下、アセット用意例

- RendererDebug: シーンビュー用
	- Prepass Layer Mask: Everything
	- Opaque Layer Mask: Everything
	- Transparent Layer Mask: Everything
- RendererDefault
	- Prepass Layer Mask: Default
	- Opaque Layer Mask: Default
	- Transparent Layer Mask: Default
- RendererWindow1
	- Prepass Layer Mask: Window1
	- Opaque Layer Mask: Window1
	- Transparent Layer Mask: Window1
- RendererWindow2
	- Prepass Layer Mask: Window2
	- Opaque Layer Mask: Window2
	- Transparent Layer Mask: Window2

サブウィンドウ用に用意した Universal Renderer Data には Add Renderer Feature で `Flip Vertical Sync` を追加してください

用意した Universal Renderer Data を Universal Render Pipeline Asset に登録し、シーンビュー用をデフォルトとする

## RenderTextureの用意

運用するサブウィンドウの数分の RenderTexture アセットを事前に用意する

アセットは以下の設定にしてください

- Dimension: 2D
- Size: 任意 ※サブウィンドウのクライアント領域の幅高さとなります
- Anti-aliasing: None
- Enable Compatible Format : OFF
- Color Format: R8G8B8A8_UNORM
- Depth Stencil Format: D24_UNORM_S8_UNIT
- Mipmap: OFF
- Dynamic Scaling: OFF
- Random Write: OFF
- Warp Mode: Clamp
- Filter Mode: Bilinear
- Shadow Sampling Mode: None

## Hiearchy での構成

サブウィンドウ毎に Camera、Canvas、GraphicRaycaster、WindowsInputModule をそれぞれ用意する必要があります。

以下、GameObject構成例

- EventSystem<EventSystem, WindowsInputModule, WindowsInputModule>
	- Window1<SubWindow, Camera>
		- Canvas1<Canvas, CanvasScaler, GraphicRaycaster>
	- Window2<SubWindow, Camera>
		- Canvas2<Canvas, CanvasScaler, GraphicRaycaster>
	
各コンポーネントでは以下の設定が必要になります

### SubWindow

Camera、Raycaster、InputModule を設定する。

InputModule のみ Hierachy、Inspector で操作するのが難しいので、`Create Input Module` ボタンを押して設定してください。

ボタンを押すことで、Hierarchy 上に存在する EventSystem に専用の WindowsInputModule を付与してシリアライズします。

### Camera

- Rendering/Renderer: 専用に用意した Universal Renderer Data を選択
- Enviroment/Volumes/Volume Mask: 専用に用意したレイヤーのみを選択
- Output/Output Texture: 専用に用意した RenderTexture を選択
- Output/TargetDisplay: Display1 ※Output Texture を設定すると Inspector 上では見えなくなるので注意

### Canvas

- RenderMode: Screen Space - Camera
- Render Camera: 専用に用意したカメラコンポーネントを選択

### Canvas Scaler

- UI Scale Mode: Scale With Screen Size
- Reference Resolution: 用意した RenderTexture と同じ値を指定

# 組み込み

SubWindow.Initialize() をアプリ起動時に１度だけ呼び出すようにしてください。

現状では Initialize() の中でメインウィンドウの半透明化が行われます。

理想的なタイミングは [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSplashScreen)] になります。

アプリを終了する際には SubWindow.Terminate() を１度だけ呼び出すようにしてください。

サブウィンドウを開くと場合には、事前に インスタンス化してある SubWindow コンポーネントの Create() を呼び出してください。

閉じる場合は同じコンポーネントインスタンスに対して Dispose() を呼び出してください。

既に任意のサブウィンドウが開かれているか確認する場合は IsCreated プロパティを参照してください。




