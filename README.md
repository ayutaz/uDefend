# uDefend

[English](README.en.md) | **日本語**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](Assets/uDefend/LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)

**Unity 向けセーブデータ暗号化 & アンチチートライブラリ**

uDefend は、ゲームのセーブデータを AES-256 で暗号化し、メモリ上の値をチート対策で保護する Unity パッケージです。ゼロコンフィグで OWASP 推奨のセキュリティ設定が適用されます。

## 主な機能

### 暗号化セーブ/ロード
- **AES-256-CBC + HMAC-SHA256** (Encrypt-then-MAC) 認証付き暗号化
- アトミックファイル書き込み（クラッシュ耐性）
- スロット単位のスレッドセーフな排他制御
- 自動バックアップ & ローテーション
- バージョンベースのセーブデータマイグレーション

### プラットフォーム鍵管理
| プラットフォーム | 鍵ストレージ | 備考 |
|:---:|:---:|---|
| Windows | DPAPI | ユーザースコープ |
| Android | Keystore | TEE/StrongBox ハードウェアバック |
| iOS | Keychain | Secure Enclave 対応 |
| その他 | PBKDF2 フォールバック | SHA-256, 600,000 イテレーション |

### アンチチート（独立モジュール）
- **ObscuredTypes（19型）**: int, float, string, Vector3 等のメモリ保護版
- **Detectors（6種）**: SpeedHack, TimeCheating, WallHack, Injection, ObscuredCheating, AntiDebug
- **ObscuredPrefs / ObscuredFilePrefs**: 暗号化された PlayerPrefs 代替
- 全19型の Inspector 用カスタム PropertyDrawer

## クイックスタート

### セーブ/ロード

```csharp
using uDefend.Core;
using uDefend.KeyManagement;

// プラットフォームに応じた鍵プロバイダを自動選択
IKeyProvider keyProvider = PlatformKeyProvider.Create("com.example.mygame");
var saveManager = new SaveManager(saveSettings, keyProvider);

// 保存（シリアライズ → 暗号化 → アトミック書き込み）
await saveManager.SaveAsync("slot1", playerData);

// 読込（読み取り → HMAC検証 → 復号 → デシリアライズ）
var data = await saveManager.LoadAsync<PlayerData>("slot1");
```

### メモリ保護

```csharp
using uDefend.AntiCheat;

// 通常の型をそのまま置き換え可能
ObscuredInt health = 100;
health -= 25;  // 通常のintと同じように使える

// 改ざん検知
ObscuredInt.OnCheatingDetected += () => Debug.LogWarning("チート検知!");
```

## 動作要件

- Unity 2022.3.12f1 LTS 以降（Unity 6 対応）
- .NET Standard 2.1
- IL2CPP / Mono 両対応

## インストール

### Unity Package Manager（Git URL）

1. **Window > Package Manager** を開く
2. **+** > **Add package from git URL...** をクリック
3. 以下を入力:

```
https://github.com/ayutaz/uDefend.git?path=Assets/uDefend
```

### .unitypackage

[Releases](https://github.com/ayutaz/uDefend/releases) ページから最新の `.unitypackage` をダウンロードしてインポート。

## アーキテクチャ

```
Assets/uDefend/
├── Runtime/                    # コア: Save/Load, 暗号化, 鍵管理, マイグレーション
│   ├── Core/                   # SaveManager, SaveFile, SaveSlot, SaveSettings
│   ├── Encryption/             # AES-CBC-HMAC, CryptoUtility, StringEncryption
│   ├── KeyManagement/          # DPAPI, Android Keystore, iOS Keychain, PBKDF2
│   ├── Serialization/          # JSON, MessagePack
│   ├── Migration/              # IMigration, MigrationRunner
│   └── Plugins/                # ネイティブプラグイン (Android/iOS/Windows)
├── Runtime.AntiCheat/          # 独立モジュール: ObscuredTypes, Detectors, Prefs
│   ├── ObscuredTypes/          # 19型 (Int, Float, Vector3, Quaternion 等)
│   ├── Detectors/              # 6種 (SpeedHack, WallHack, AntiDebug 等)
│   ├── ObscuredPrefs.cs
│   └── ObscuredFilePrefs.cs
├── Editor/                     # PropertyDrawer (19型), EditorWindow (2種)
├── Tests/                      # 170+ ユニットテスト
├── Samples~/                   # 4サンプル
└── Documentation~/             # ドキュメント
```

**AntiCheat モジュールは Runtime への依存ゼロ** — どちらか一方だけでも使用可能です。

## セキュリティ設計

- **Secure by Default**: ゼロコンフィグで OWASP 推奨設定
- CSPRNG による IV / 鍵 / ソルト生成（`RandomNumberGenerator.Fill`）
- HMAC 検証 → 復号の順序（パディングオラクル防止）
- 定数時間 HMAC 比較（タイミング攻撃防止）
- 使用後バッファのゼロクリア（`Array.Clear`）
- 非推奨 API 不使用（`RijndaelManaged`, `BinaryFormatter`, `DES`, `MD5`）

## セーブファイルフォーマット

```
| Magic "uSAV" (4B) | Version (2B) | Flags (2B) | IV (16B) | Encrypted Data (可変) | HMAC-SHA256 (32B) |
```

## ドキュメント

詳細は [`Assets/uDefend/Documentation~/`](Assets/uDefend/Documentation~/index.md) を参照:

- [Getting Started](Assets/uDefend/Documentation~/getting-started.md) — 導入ガイド
- [Save/Load API](Assets/uDefend/Documentation~/save-load-api.md) — セーブ/ロード API 詳細
- [Anti-Cheat](Assets/uDefend/Documentation~/anti-cheat.md) — ObscuredTypes & Detectors
- [Migration](Assets/uDefend/Documentation~/migration.md) — セーブデータマイグレーション
- [API Reference](Assets/uDefend/Documentation~/api-reference.md) — 全 API リファレンス

## サンプル

Unity Package Manager > uDefend > Samples からインポート:

| サンプル | 内容 |
|:---:|---|
| Basic Usage | 基本的なセーブ/ロード |
| Advanced Encryption | 低レベル暗号化 API, カスタム IEncryptionProvider |
| Migration | バージョンベースのセーブデータマイグレーション |
| Anti-Cheat | ObscuredTypes とチート検知 Detector のセットアップ |

## テスト

170+ ユニットテストで全モジュールをカバー:
- ObscuredTypes (52+ tests / 全19型)
- Detectors (29 tests / 全6種 + DetectorBase)
- 暗号化, 鍵管理, マイグレーション, シリアライズ

## ライセンス

MIT License — [LICENSE](Assets/uDefend/LICENSE)
