# v1.0.0 - ホログラム箔 for YMM4

YukkuriMovieMaker4 向けのホログラフィック箔エフェクトプラグインの初回リリースです。
Direct2D カスタムピクセルシェーダーで角度反応型の箔効果を映像に合成する映像エフェクトプラグインです。
カメラ角度・アイテム回転・手動指定の視線角度に応じて回折虹彩・疑似凹凸・細線反射・きらめきが変化し、
角度別潜像としてギロシェパターンまたは画像アトラスに対応します。
9 言語対応 UI を備えます。

---

## 新機能

### 1. マスク選択（HolographicFoilAlphaSource）

箔を合成する対象領域の基準を表す列挙体です。

| 値 | 意味 |
|---|---|
| `Alpha` | チャンネルアルファ |
| `Luminance` | BT.709 輝度（0.2126R + 0.7152G + 0.0722B） |
| `AlphaLuminance` | アルファ × 輝度の積 |

---

### 2. 潜像モード（HolographicFoilRevealMode）

角度で現れる潜像の種類を表す列挙体です。

| 値 | 意味 |
|---|---|
| `None` | 潜像なし |
| `Guilloche` | 回転と格子の干渉によるギロシェパターンをシェーダーが動的に生成 |
| `ImageAtlas` | 水平方向に並べた角度フレーム画像からフレームを選択して合成 |

---

### 3. 角度の取得元（HolographicFoilViewSource）

視線角度の算出方法を表す列挙体です。

| 値 | 計算方法 |
|---|---|
| `Camera` | カメラのヨー角度 + アイテム Y 回転 |
| `ItemRotation` | アイテムの Y 回転 |
| `Manual` | 手動角度パラメータの値をそのまま使用 |

---

### 4. 補助計算（HolographicFoilMath）

`internal static class HolographicFoilMath` は角度計算とフレーム混合の補助メソッドを提供します。

| メソッド | 説明 |
|---|---|
| `NormalizeAngleDegrees(double)` | 角度を [-180, 180] の主値に正規化。非有限値は 0 を返す |
| `ComputeRevealCoordinate(double, double, int)` | 角度からアトラスのフレーム座標を計算。`(angle + range/2) / range` を [0, frameCount−1] にクリップ |
| `ComputeFrameWeights(double, int, double, Span<float>)` | フレーム座標から隣接フレームの混合重みを SmoothStep で計算。重みの合計は常に 1 |
| `EvaluateRgbCycle(double)` | 位相から RGB 虹彩サイクルを計算。R/G/B をそれぞれ 0°/120°/240° ずらした余弦波を返す |
| `ComputeCameraYawDegrees(Matrix4x4)` | カメラ行列の forward ベクトルから atan2(x, z) × 180/π でヨー角度を計算 |

#### SmoothStep

`SmoothStep(edge0, edge1, value)` は `x = clamp((value − edge0) / (edge1 − edge0), 0, 1)` を求め、`x² × (3 − 2x)` を返します。

#### RGB 虹彩サイクル

`EvaluateRgbCycle(phase)` は以下を返します。

| チャンネル | 式 |
|---|---|
| R | `0.5 + 0.5 × cos(2π × p)` |
| G | `0.5 + 0.5 × cos(2π × (p − 1/3))` |
| B | `0.5 + 0.5 × cos(2π × (p − 2/3))` |

`p = phase − floor(phase)`（小数部）で 1.0 周期の波形になります。

---

### 5. カスタムシェーダーエフェクト（HolographicFoilCustomEffect）

`internal sealed class HolographicFoilCustomEffect(IGraphicsDevicesAndContext) : D2D1CustomShaderEffectBase` は `[CustomEffect(2)]` の 2 入力エフェクトとして宣言されます（入力 0: ソース画像、入力 1: アトラス画像）。

公開プロパティは `GetFloatValue`・`GetIntValue`・`SetValue` を介して `EffectImpl` へ転送します。

| プロパティ | 型 | 説明 |
|---|---|---|
| `Amount` | `float` | 合成強度（0〜1） |
| `Angle` | `float` | 視線角度（-180〜180°） |
| `FoilColor` | `Vector4` | 箔の基準色（RGBA 0〜1） |
| `Rainbow` | `float` | 虹色強度（0〜2） |
| `Relief` | `float` | 凹凸強度（0〜3） |
| `PatternScale` | `float` | 細線密度（0.1〜5） |
| `PatternAngle` | `float` | 細線角度（-180〜180°） |
| `Sparkle` | `float` | きらめき強度（0〜2） |
| `Motion` | `float` | 時間変化量（-2〜2） |
| `Time` | `float` | 経過時間（秒） |
| `AlphaSource` | `int` | マスク種別（0=Alpha, 1=Luminance, 2=AlphaLuminance） |
| `RevealMode` | `int` | 潜像モード（0=None, 1=Guilloche, 2=ImageAtlas） |
| `RevealStrength` | `float` | 潜像強度（0〜2） |
| `RevealRange` | `float` | 角度範囲（1〜180°） |
| `RevealBlend` | `float` | 切替補間幅（0〜1） |
| `AtlasWidth` | `float` | アトラス画像の幅（px） |
| `AtlasHeight` | `float` | アトラス画像の高さ（px） |
| `AtlasFrameCount` | `int` | アトラスのフレーム数（2〜16） |

`SetSourceInput(ID2D1Image?)` は入力 0 を設定します。`SetAtlasInput(ID2D1Image?)` は入力 1 を設定します。

#### EffectImpl（内部 sealed クラス）

`ConstantBuffer` 構造体（`LayoutKind.Sequential`）のレイアウトは以下のとおりです。

| フィールド | 型 | 説明 |
|---|---|---|
| `FoilColor` | `Vector4` | 箔の基準色（16 バイト） |
| `InputBounds` | `Vector4` | 入力矩形（Left, Top, Right, Bottom） |
| `AtlasInfo` | `Vector4` | アトラス幅（X）・高さ（Y）・フレーム数（Z） |
| `Amount` | `float` | 合成強度 |
| `Angle` | `float` | 視線角度 |
| `Rainbow` | `float` | 虹色強度 |
| `Relief` | `float` | 凹凸強度 |
| `PatternScale` | `float` | 細線密度 |
| `PatternAngle` | `float` | 細線角度 |
| `Sparkle` | `float` | きらめき強度 |
| `Motion` | `float` | 時間変化量 |
| `Time` | `float` | 経過時間 |
| `RevealStrength` | `float` | 潜像強度 |
| `RevealBlend` | `float` | 切替補間幅 |
| `RevealRange` | `float` | 角度範囲 |
| `AlphaSource` | `int` | マスク種別 |
| `RevealMode` | `int` | 潜像モード |
| `Pad0`, `Pad1` | `float` | アライメント用 |

入力 1（アトラス）のサンプラーは `Filter.MinMagMipLinear`（線形補間）を使用します。

`MapInputRectsToOutputRect` は入力 0 の矩形をそのまま出力矩形に設定し、`InputBounds` を定数バッファーに書き込みます。`MapOutputRectToInputRects` で入力 1 に `AtlasInfo.X × AtlasInfo.Y` のアトラス全域を要求します。

シェーダーリソース: `pack://application:,,,/HolographicFoil;component/Shaders/HolographicFoil.cso`（ps_5_0）

---

### 6. エフェクト定義（HolographicFoilEffect）

`public sealed class HolographicFoilEffect : VideoEffectBase, IFileItem, IResourceItem` を継承・実装します。

`[VideoEffect]` 属性は以下のパラメーターで宣言されます。

- 表示名：`Texts.EffectName`（ローカライズキー）
- カテゴリー：`VideoEffectCategories.Filtering`・`VideoEffectCategories.Decoration`
- 検索タグ：`TagHologram`・`TagFoil`・`TagSecurity`・`TagReveal`（「ホログラム」・「箔」・「セキュリティ」・「潜像」）
- `IsAviUtlSupported = false` により AviUtl 向け EXO 出力は非対応
- `ResourceType = typeof(Texts)` でローカライズリソースを指定

`Label` プロパティは `Texts.EffectName` を返します。

公開プロパティは以下のとおりです。

**基本グループ**

| プロパティ | 型 | デフォルト | 内部範囲 |
|---|---|---|---|
| `Amount` | `Animation` | 100 | 0〜100 |
| `AlphaSource` | `HolographicFoilAlphaSource` | `Alpha` | — |

**角度グループ**

| プロパティ | 型 | デフォルト | 内部範囲 |
|---|---|---|---|
| `ViewSource` | `HolographicFoilViewSource` | `Camera` | — |
| `ViewSensitivity` | `Animation` | 100 | -400〜400 |
| `ManualAngle` | `Animation` | 0 | -180〜180 |

**箔グループ**

| プロパティ | 型 | デフォルト | 内部範囲 |
|---|---|---|---|
| `FoilColor` | `Color` | `#E8F4FF` | — |
| `Rainbow` | `Animation` | 100 | 0〜200 |
| `Relief` | `Animation` | 80 | 0〜300 |
| `PatternScale` | `Animation` | 120 | 10〜500 |
| `PatternAngle` | `Animation` | 35 | -180〜180 |
| `Sparkle` | `Animation` | 45 | 0〜200 |
| `Motion` | `Animation` | 20 | -200〜200 |

**潜像グループ**

| プロパティ | 型 | デフォルト | 内部範囲 |
|---|---|---|---|
| `RevealMode` | `HolographicFoilRevealMode` | `None` | — |
| `RevealStrength` | `Animation` | 100 | 0〜200 |
| `RevealRange` | `Animation` | 90 | 1〜180 |
| `RevealBlend` | `Animation` | 0.35 | 0〜1 |

**画像アトラスグループ**

| プロパティ | 型 | デフォルト | 内部範囲 |
|---|---|---|---|
| `RevealImage` | `string?` | `null` | — |
| `RevealFrames` | `int` | 3 | 2〜16 |

`GetAnimatables` は `Amount`・`ViewSensitivity`・`ManualAngle`・`Rainbow`・`Relief`・`PatternScale`・`PatternAngle`・`Sparkle`・`Motion`・`RevealStrength`・`RevealRange`・`RevealBlend` を yield します。

`GetFiles` は `RevealImage` が空白でない場合にそのパスを yield します。`ReplaceFile` はパスの一致で `RevealImage` を更新します。`GetResources` は `RevealImage` を `TimelineResourceType.Image` として yield します。

---

### 7. エフェクトプロセッサー（HolographicFoilEffectProcessor）

`internal sealed class HolographicFoilEffectProcessor : VideoEffectProcessorBase` を継承します。

#### Update メソッド

`IsPassThroughEffect || effect is null` の場合は `effectDescription.DrawDescription` をそのまま返します。

各フレームで以下の値を計算します。

| パラメーター | 変換 |
|---|---|
| `Amount` | `value / 100` を [0, 1] にクランプ |
| `Rainbow`・`Sparkle`・`RevealStrength` | `value / 100` を [0, 2] にクランプ |
| `Relief` | `value / 100` を [0, 3] にクランプ |
| `PatternScale` | `value / 100` を [0.1, 5] にクランプ |
| `Motion` | `value / 100` を [-2, 2] にクランプ |
| `PatternAngle` | 有限値を [-180, 180] にクランプ、非有限値は 0 |
| `RevealRange` | 有限値を [1, 180] にクランプ、非有限値は 90 |
| `RevealBlend` | 有限値を [0, 1] にクランプ、非有限値は 0.35 |
| `Time` | `frame / fps`（fps > 0 のとき） |

#### ComputeAngle メソッド

`ViewSource` に応じて以下を計算し、`NormalizeAngleDegrees` で [-180, 180] に正規化します。

- `Camera`：`ComputeCameraYawDegrees(camera)` + `drawDescription.Rotation.Y`
- `ItemRotation`：`drawDescription.Rotation.Y`
- `Manual`：`ManualAngle.GetValue(...)` をそのまま使用

`Manual` 以外の場合は結果に `ViewSensitivity / 100` を乗算します。

#### RefreshAtlas メソッド

`RevealMode == ImageAtlas` のときのみアトラス画像をロードします。パスが前フレームと同じ場合はロードを省略します。ロードに失敗した場合または `ImageAtlas` モードでない場合は `Flood`（透明単色）をアトラス入力のフォールバックとして使用します。

#### CreateEffect / setInput / ClearEffectChain

`CreateEffect` は `HolographicFoilCustomEffect` を生成し、`IsEnabled` が false の場合は破棄して `null` を返します。有効な場合は透明単色の `Flood` をフォールバックアトラスとして初期化します。

`setInput` は `effect?.SetSourceInput(input)` を呼び出します。

`ClearEffectChain` は `SetSourceInput(null)`・`SetAtlasInput(null)` を呼び出し、`isFirst = true` にリセットします。

---

### 8. UI 表示制御属性（VisibilityAttributes）

`ICustomVisibilityAttribute2` を実装した 4 つの属性が `EnumVisibilityConverter` を介して WPF バインディングを生成します。

| 属性 | 表示条件 |
|---|---|
| `ManualAngleVisible` | `ViewSource == Manual` |
| `AutomaticAngleVisible` | `ViewSource != Manual` |
| `AngleRevealVisible` | `RevealMode != None` |
| `ImageAtlasVisible` | `RevealMode == ImageAtlas` |

`EnumVisibilityConverter(expected, invert)` は `Equals(value, expected) XOR invert` が true のとき `Visibility.Visible` を返します。

---

### 9. ローカライズ（Texts）

`Texts` クラスは `[AutoGenLocalizer]` 属性を持つ `partial` クラスとして宣言されます。
`YukkuriMovieMaker.Generator` のソースジェネレーターが `Texts.csv` を処理し、各ロケールのリソースファイルを自動生成します。

対応言語：日本語（`ja-jp`）・英語（`en-us`）・中国語簡体字（`zh-cn`）・中国語繁体字（`zh-tw`）・韓国語（`ko-kr`）・スペイン語（`es-es`）・アラビア語（`ar-sa`）・インドネシア語（`id-id`）

ローカライズキーの一覧は以下のとおりです。

| キー | ja-jp |
|---|---|
| `EffectName` | ホログラム箔 |
| `BasicGroup` | 基本 |
| `ViewGroup` | 角度 |
| `FoilGroup` | 箔 |
| `RevealGroup` | 潜像 |
| `AtlasGroup` | 画像アトラス |
| `AmountName` | 強さ |
| `AlphaSourceName` | 対象 |
| `AlphaSourceAlpha` | アルファ |
| `AlphaSourceLuminance` | 輝度 |
| `AlphaSourceAlphaLuminance` | アルファ×輝度 |
| `ViewSourceName` | 角度の取得元 |
| `ViewSourceCamera` | カメラ |
| `ViewSourceItemRotation` | アイテム回転 |
| `ViewSourceManual` | 手動 |
| `ViewSensitivityName` | 角度感度 |
| `ManualAngleName` | 手動角度 |
| `FoilColorName` | 箔色 |
| `RainbowName` | 虹色 |
| `ReliefName` | 凹凸 |
| `PatternScaleName` | 細線密度 |
| `PatternAngleName` | 細線角度 |
| `SparkleName` | きらめき |
| `MotionName` | 動き |
| `RevealModeName` | 潜像モード |
| `RevealModeNone` | なし |
| `RevealModeGuilloche` | ギロシェ |
| `RevealModeImageAtlas` | 画像アトラス |
| `RevealStrengthName` | 潜像強度 |
| `RevealRangeName` | 角度範囲 |
| `RevealBlendName` | 切替補間 |
| `RevealImageName` | アトラス画像 |
| `RevealFramesName` | フレーム数 |
| `TagHologram` | ホログラム |
| `TagFoil` | 箔 |
| `TagSecurity` | セキュリティ |
| `TagReveal` | 潜像 |