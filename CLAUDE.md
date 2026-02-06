# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## プロジェクト概要

**uDefend** は Unity 向けのセーブデータ暗号化・アンチチートライブラリ（OSS / MIT ライセンス）。ドキュメント上の旧名称は **uSave**。

主要機能:
- AES-256-CBC + HMAC-SHA256（Encrypt-then-MAC）による認証付き暗号化
- MessagePack v3 をデフォルトシリアライザとした統合 Save/Load API
- プラットフォーム固有の鍵管理（Android Keystore / iOS Keychain / Windows DPAPI / PBKDF2 フォールバック）
- ObscuredTypes（19型）によるメモリ保護 + 5種のチート検知 Detector
- バージョン付きセーブファイル形式とマイグレーション機構

対象プラットフォーム: Windows / Android / iOS（macOS / Linux / WebGL はベストエフォート）

## Unity 環境

- **Unity バージョン**: 6000.3.6f1（Unity 6）
- **対応範囲**: Unity 2022.3.12f1 LTS 以降
- **スクリプティングバックエンド**: IL2CPP + Mono 両対応
- **API レベル**: .NET Standard 2.1

## アーキテクチャ

### モジュール構成（asmdef 分離）

```
uDefend/
├── Runtime/                    # uDefend.Runtime.asmdef（コア）
│   ├── Core/                   # SaveManager, SaveFile, SaveSlot, SaveSettings
│   ├── Encryption/             # IEncryptionProvider → AesCbcHmacProvider, AesGcmProvider, NullEncryptionProvider
│   ├── KeyManagement/          # IKeyProvider → PlatformKeyProvider, Pbkdf2KeyProvider, EnvelopeEncryption
│   ├── Serialization/          # ISerializer → MessagePackSerializer, JsonSerializer
│   ├── Migration/              # IMigration, MigrationRunner
│   └── Plugins/                # ネイティブプラグイン（Android/iOS/Windows）
├── Runtime.AntiCheat/          # uDefend.AntiCheat.asmdef（アンチチート、コアと分離）
│   ├── ObscuredTypes/          # ObscuredInt, ObscuredFloat 等 19型
│   ├── Detectors/              # SpeedHack, TimeCheating, WallHack, Injection, ObscuredCheating
│   ├── ObscuredPrefs.cs
│   └── ObscuredFilePrefs.cs
├── Editor/                     # uDefend.Editor.asmdef
├── Tests/                      # Runtime/ と Editor/ に分離
└── Samples~/
```

### 設計パターン

- **Provider パターン**: `IEncryptionProvider`, `IKeyProvider`, `ISerializer` で暗号化/鍵管理/シリアライズを差し替え可能
- **エンベロープ暗号化**: マスター鍵 → DEK → データの 2段構成。鍵ローテーション時は DEK のみ再生成
- **Factory パターン**: `PlatformKeyProvider` がプラットフォームに応じて Android Keystore / iOS Keychain / DPAPI / PBKDF2 を自動選択
- **Encrypt-then-MAC**: 暗号化 → HMAC 計算の順。復号時は HMAC 検証を先に行いパディングオラクルを防止

### セーブファイルフォーマット

```
Magic Number "uSAV" (4B) | Version uint16 (2B) | Flags (2B) | IV (16B) | Encrypted Data (可変) | HMAC-SHA256 (32B)
```

処理フロー（保存）: オブジェクト → MessagePack シリアライズ → (圧縮) → ランダム IV 生成 → AES-CBC 暗号化 → HMAC-SHA256 計算 → ファイル書き込み

## セキュリティ上の設計原則

- **Secure by Default**: ゼロコンフィグで OWASP 推奨設定が適用される
- IV/Nonce は毎回 `RandomNumberGenerator.Fill()` で生成。静的 IV 禁止
- HMAC 比較は定数時間（タイミング攻撃対策）
- 復号後バッファは使用後に `Array.Clear()` でゼロクリア
- 暗号鍵と HMAC 鍵は別々に管理
- PBKDF2 のイテレーション回数: 600,000（OWASP 2024 推奨、SHA-256）
- CSPRNG のみ使用。`System.Random` を暗号目的に使用しない
- 非推奨 API 禁止: `RijndaelManaged`, `BinaryFormatter`, `DES`, `TripleDES`, `MD5`
- エラーメッセージで暗号化の内部状態を漏洩しない

## パフォーマンス要件

- 1MB 暗号化/復号: モバイルで 50ms 以内
- GC Allocation: 暗号化操作あたり 1KB 未満
- ObscuredInt の get/set オーバーヘッド: 通常 int の 10 倍以内
- アンチチートモジュール未使用時のコアへの影響: ゼロオーバーヘッド（asmdef 分離で保証）

## 配布形式

- UPM パッケージ（第一優先、OpenUPM 登録予定）
- .unitypackage（UPM 非対応プロジェクト向け）

## 開発上の注意

- ObscuredTypes は暗黙の型変換演算子を実装し、既存コードからの差し替えコストを最小化する
- MessagePack v3 は Source Generator ベース（mpc 不要）。IL2CPP/AOT 完全対応
- 将来の AES-GCM 移行（Unity 6.7+ CoreCLR）に備え、暗号化バックエンドは `IEncryptionProvider` で抽象化
- スレッドセーフティ: 同一スロットへの Save/Load は排他制御、異なるスロット間は並行実行可
- ファイル書き込みはアトミック（一時ファイル → リネーム）でクラッシュ対策
- 全 Detector は Inspector からコードレスで設定可能にする

## 要件定義の参照先

詳細な要件定義とリサーチは `docs/` 配下を参照:
- `docs/requirements.md` - 機能要件・非機能要件・API 設計・プロジェクト構成
- `docs/research/security-analysis.md` - 暗号化方式・鍵管理・脆弱性パターンの分析
- `docs/research/existing-libraries.md` - 既存ライブラリ比較と差別化ポイント
- `docs/research/cedec-and-articles.md` - CEDEC 講演・技術記事からの知見
