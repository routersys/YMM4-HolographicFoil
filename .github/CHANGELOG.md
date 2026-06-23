# v1.0.1 - ホログラム箔 for YMM4

v1.0.0 に対する保守リリースです。
未使用コードの削除・UI 表示条件の統一・ローカライズ文言の整合を行います。
箔合成および潜像生成の描画結果に変更はありません。

---

## 修正

### 1. 角度範囲・切替補間の表示条件をアトラスモード限定に変更

`RevealRange`（角度範囲）と `RevealBlend`（切替補間）はシェーダー上 `ImageAtlas` モードの `SampleAtlas` でのみ参照され、`Guilloche` モードでは無効でした。
表示属性を `AngleRevealVisible`（`RevealMode != None`）から `ImageAtlasVisible`（`RevealMode == ImageAtlas`）へ変更し、アトラス専用パラメーターである `RevealImage`・`RevealFrames` と表示条件を統一します。

| プロパティ | 変更前 | 変更後 |
|---|---|---|
| `RevealRange` | `AngleRevealVisible` | `ImageAtlasVisible` |
| `RevealBlend` | `AngleRevealVisible` | `ImageAtlasVisible` |

`RevealStrength` は `Guilloche`・`ImageAtlas` の両モードで使用するため `AngleRevealVisible` のまま維持します。

---

### 2. 未使用の補助計算メソッドの削除

`HolographicFoilMath` の以下のメソッドは実行時に呼び出されていません。虹彩サイクルとアトラスのフレーム補間はシェーダー（`CycleColor`・`SampleAtlas`）が GPU 上で計算するため、実装の単一化のために削除します。

| 削除したメソッド |
|---|
| `ComputeRevealCoordinate(double, double, int)` |
| `ComputeFrameWeights(double, int, double, Span<float>)` |
| `EvaluateRgbCycle(double)` |
| `SmoothStep(double, double, double)` |

実行時に使用する `NormalizeAngleDegrees`・`ComputeCameraYawDegrees` とそのテストは維持します。削除したメソッドに対応するユニットテストも併せて削除します。

---

### 3. ローカライズ文言の整合（RevealRangeDesc）

`RevealRange` をアトラスモード限定としたため、`RevealRangeDesc` の説明から潜像（ギロシェ）への言及を削除し、アトラス画像のフレーム切り替えを表す文言へ統一します。`Texts.csv` の編集のみで各ロケールのリソースが再生成されます。

---

### 4. プラグインバージョンの更新

`HolographicFoil.csproj` の `<Version>` を `1.0.0` から `1.0.1` へ更新します。
