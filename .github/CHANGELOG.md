# v1.0.3 - ホログラム箔 for YMM4

v1.0.2 に対する保守リリースです。
アトラスモードの合成方式を加算からアルファ合成へ変更し、スリットジッターを除去してフレーム間クロスフェードを平滑化します。
パラメーター・UI・ローカライズ文言に変更はありません。

---

## 修正

### 1. アトラス合成を加算からアルファ合成へ変更

加算合成では元アイテムが透けて明るく飽和するため、`revealMode == 2` の合成を `foil` への加算から、アトラスのアルファチャンネルを被覆率として用いるアルファ合成へ変更します。

`SampleAtlas` の戻り値を `float3` から `float4` へ変更してアルファを伝播し、合成式を以下のとおり改めます。

| 項目 | 変更前 | 変更後 |
|---|---|---|
| `SampleAtlas` 戻り値 | `float3`（RGB のみ） | `float4`（RGBA） |
| 合成式 | `foil += atlas * revealStrength` | `base = source.rgb * (1 - k * atlas.a) + source.a * atlas.rgb * k` |
| `revealStrength` の役割 | foil への加算係数 | 被覆率の上限（`k = saturate(revealStrength) * mask`） |

### 2. スリットジッターの除去とクロスフェードの平滑化

`localUv.x` と `angleValue` の端数を混合するスリットジッター項がフレーム境界に視覚的なノイズを生じさせていました。
`slit` 変数およびそれによる `coordinate` の補正を削除し、フレーム座標に沿った `smoothstep` クロスフェードのみを残します。

合わせて `revealBlend` の上限を `clamp(revealBlend, 0.0001, 1.0)` で 1.0 に制限し、`RevealBlend` をフレーム切り替えの柔らかさとして再定義します。`scale` 引数はジッター計算でのみ使用していたため `SampleAtlas` の仮引数から削除します。

| 変更箇所 | 変更前 | 変更後 |
|---|---|---|
| `slit` 項 | `frac(localUv.x * frames * max(scale, 0.1) * 12.0 + angleValue * 0.015)` | 削除 |
| `width` の上限 | なし（`max(revealBlend, 0.0001)`） | 1.0（`clamp(revealBlend, 0.0001, 1.0)`） |
| `scale` 引数 | `SampleAtlas` に渡していた | 削除 |

### 3. プラグインバージョンの更新

`HolographicFoil.csproj` の `<Version>` を `1.0.2` から `1.0.3` へ更新します。
