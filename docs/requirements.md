# uSave 要件定義

## 概要

本文書は `docs/research/` 配下の調査結果から導出した、Unity 向けセーブデータ暗号化ライブラリ **uSave** の要件定義である。

**対象プラットフォーム:** Windows / Android / iOS

---

## 1. 機能要件

### FR-1: 暗号化 API

| ID | 要件 | 優先度 | 根拠 |
|----|------|--------|------|
| FR-1.1 | AES-256-CBC + HMAC-SHA256（Encrypt-then-MAC）による認証付き暗号化を提供する | 必須 | [security-analysis.md §1.3] 既存ライブラリの大半が認証なし暗号化のみ |
| FR-1.2 | 暗号化バックエンドを抽象化し、将来的に AES-GCM への切り替えを可能にする | 必須 | [security-analysis.md §1.7] CoreCLR 移行で AES-GCM が利用可能に |
| FR-1.3 | IV / Nonce を毎回ランダム生成し、暗号文に付加して保存する | 必須 | [security-analysis.md §3.2] 静的 IV の脆弱性対策 |
| FR-1.4 | `byte[]` の暗号化・復号化を行う低レベル API を提供する | 必須 | 柔軟性の確保 |
| FR-1.5 | `string` の暗号化・復号化を行うユーティリティ API を提供する | 推奨 | 利便性向上 |
| FR-1.6 | ストリーム暗号化・復号化 API を提供する | 推奨 | [cedec-and-articles.md §4.4] 大容量データのメモリ効率 |

---

### FR-2: シリアライズ API

| ID | 要件 | 優先度 | 根拠 |
|----|------|--------|------|
| FR-2.1 | 任意の C# オブジェクトをシリアライズ → 暗号化 → 保存する統合 API を提供する | 必須 | [existing-libraries.md §4] 暗号化とシリアライズの分離が既存の課題 |
| FR-2.2 | デフォルトシリアライザとして MessagePack for C# v3 をサポートする | 必須 | [existing-libraries.md §3.3] IL2CPP 対応・実績・パフォーマンスで最適 |
| FR-2.3 | シリアライザを差し替え可能なインターフェースを提供する | 必須 | ユーザーが JSON, MemoryPack 等を選択可能に |
| FR-2.4 | `JsonUtility` による JSON シリアライズをビルトインサポートする | 推奨 | 外部依存なしの選択肢を提供 |
| FR-2.5 | スキーマ進化（フィールド追加）に対応する | 必須 | セーブデータの後方互換性維持 |

---

### FR-3: 改ざん検知

| ID | 要件 | 優先度 | 根拠 |
|----|------|--------|------|
| FR-3.1 | HMAC-SHA256 によるファイル完全性検証を行う | 必須 | [security-analysis.md §3.6] 認証なし暗号化の脆弱性対策 |
| FR-3.2 | 改ざんが検出された場合、例外をスローしデータを拒否する | 必須 | 不正データの受理防止 |
| FR-3.3 | HMAC 検証を復号処理の前に実行する（パディングオラクル対策） | 必須 | [security-analysis.md §3.4] |
| FR-3.4 | HMAC の比較を定数時間で行う（タイミング攻撃対策） | 必須 | [security-analysis.md §3.4] |

---

### FR-4: 鍵管理

| ID | 要件 | 優先度 | 根拠 |
|----|------|--------|------|
| FR-4.1 | プラットフォーム固有の鍵ストレージを統合する抽象レイヤーを提供する | 必須 | [security-analysis.md §2] プラットフォーム間の差異を吸収 |
| FR-4.2 | Android Keystore によるマスター鍵保護をサポートする | 必須 | [security-analysis.md §2.1] ハードウェアレベルの鍵保護 |
| FR-4.3 | iOS Keychain によるマスター鍵保護をサポートする | 必須 | [security-analysis.md §2.1] Secure Enclave 連携 |
| FR-4.4 | Windows DPAPI によるマスター鍵保護をサポートする | 必須 | [security-analysis.md §2.1] OS レベルの鍵保護 |
| FR-4.5 | PBKDF2 によるフォールバック鍵導出を提供する。以下の条件で自動発動: (1) プラットフォーム固有 API の検出失敗時、(2) Android Keystore / iOS Keychain が非対応の端末、(3) Unity Editor 上での実行時、(4) ユーザーが明示的にフォールバックを指定した場合 | 必須 | プラットフォーム固有 API が使用不可の場合 |
| FR-4.6 | エンベロープ暗号化パターンを採用する（マスター鍵 → DEK → データ） | 推奨 | [security-analysis.md §2.2] 鍵ローテーションの容易化 |
| FR-4.7 | 鍵ローテーション API を提供する | 推奨 | [cedec-and-articles.md §3.2] 運用時のセキュリティ向上 |
| FR-4.8 | 暗号鍵のハードコードを検出する静的解析ルールまたはドキュメントを提供する | 推奨 | [security-analysis.md §3.1] 最もよくあるミスの防止 |

---

### FR-5: ファイル管理

| ID | 要件 | 優先度 | 根拠 |
|----|------|--------|------|
| FR-5.1 | セーブファイルにバージョン情報を含むヘッダを付加する | 必須 | [security-analysis.md §4.1] マイグレーション対応 |
| FR-5.2 | セーブファイルのマイグレーション機構を提供する。バージョンは符号なし 16bit 整数（0〜65535）の単調増加とし、N → N+1 の順次マイグレーションを実行する。下位互換保証範囲はライブラリ利用者が `MinSupportedVersion` で指定する | 必須 | ゲームアップデート時の互換性維持 |
| FR-5.3 | 複数のセーブスロットをサポートする | 推奨 | ゲームの一般的な要件 |
| FR-5.4 | セーブファイルの存在確認・一覧取得・削除 API を提供する | 必須 | 基本的なファイル操作 |
| FR-5.5 | アトミックな書き込み（一時ファイル → リネーム）を実装する | 必須 | 書き込み中のクラッシュ対策 |
| FR-5.6 | 自動バックアップ機構を提供する | 推奨 | データ損失防止 |
| FR-5.7 | `Application.persistentDataPath` をデフォルトの保存先とする | 必須 | Unity の標準的なデータ保存先 |

---

### FR-6: エディタ対応

| ID | 要件 | 優先度 | 根拠 |
|----|------|--------|------|
| FR-6.1 | Unity Editor 上でセーブデータの閲覧・編集が可能なウィンドウを提供する | 推奨 | [existing-libraries.md §1.1] Easy Save 3 のエディタ機能に匹敵 |
| FR-6.2 | エディタ上で暗号化を無効化するオプションを提供する | 推奨 | 開発時のデバッグ効率向上 |
| FR-6.3 | セーブデータの構造をインスペクタで確認できるようにする | 任意 | 開発者体験の向上 |

---

### FR-7: アンチチート

| ID | 要件 | 優先度 | 根拠 |
|----|------|--------|------|
| FR-7.1 | 基本型のメモリ難読化型（ObscuredInt, ObscuredFloat 等 19 型）を提供する | 必須 | ACTk 代替 |
| FR-7.2 | ObscuredTypes は暗黙の型変換演算子により既存コードへの差し替えを容易にする | 必須 | 導入コスト低減 |
| FR-7.3 | ObscuredTypes の値が外部ツールで改変された場合のチート検知コールバックを提供する | 必須 | ObscuredCheatingDetector 相当 |
| FR-7.4 | ObscuredTypes のうち主要型は Unity Inspector で編集可能にする（CustomPropertyDrawer） | 推奨 | 開発者体験 |
| FR-7.5 | スピードハック検知（Time.time vs 実時刻の乖離検出）を提供する | 必須 | SpeedHackDetector 相当 |
| FR-7.6 | システム時刻改ざん検知（インターネット時刻との比較）を提供する | 推奨 | TimeCheatingDetector 相当 |
| FR-7.7 | 壁抜け検知（壁越し射撃・移動・視認のサンドボックス検出）を提供する | 推奨 | WallHackDetector 相当 |
| FR-7.8 | 不正アセンブリ注入検知（ビルド時アセンブリリスト vs 実行時比較）を提供する | 推奨 | InjectionDetector 相当 |
| FR-7.9 | PlayerPrefs の暗号化保存・読み込み API（ObscuredPrefs）を提供する | 必須 | ObscuredPrefs 相当 |
| FR-7.10 | 全 Detector はコードレスでも Inspector から設定・利用可能にする | 推奨 | ACTk の UX に合わせる |

---

## 2. 非機能要件

### NFR-1: パフォーマンス

| ID | 要件 | 目標値 | 根拠 |
|----|------|--------|------|
| NFR-1.1 | 1MB のセーブデータの暗号化・復号化を 50ms 以内で完了する（モバイル） | 50ms | ロード画面のない暗号化を目標 |
| NFR-1.2 | GC Allocation を最小限に抑える（暗号化操作あたり 1KB 未満のアロケーション） | 1KB 未満 | モバイルでの GC スパイク防止 |
| NFR-1.3 | 暗号化処理を非同期（async/await）で実行可能にする | - | メインスレッドのブロッキング防止 |
| NFR-1.4 | バッファの再利用を可能にする API を提供する | - | 頻繁なセーブ操作の最適化 |
| NFR-1.5 | ObscuredInt の get/set オーバーヘッドが通常 int の 10 倍以内 | 10x 以内 | ゲームループ内での使用を想定 |
| NFR-1.6 | アンチチートモジュール未使用時、コアモジュールのパフォーマンスに影響しない | 0 オーバーヘッド | モジュラー設計（asmdef 分離） |

---

### NFR-2: 互換性

| ID | 要件 | 内容 | 根拠 |
|----|------|------|------|
| NFR-2.1 | Unity 2022.3.12f1 LTS 以降をサポートする | - | MessagePack v3 Source Generator の最小対応バージョン |
| NFR-2.2 | Unity 6.x をサポートする | - | 最新版サポート |
| NFR-2.3 | IL2CPP / Mono 両バックエンドで動作する | - | [security-analysis.md §5.1] |
| NFR-2.4 | .NET Standard 2.1 互換の API のみを使用する | - | [security-analysis.md §5.2] |
| NFR-2.5 | Windows, Android, iOS で動作する | - | 対象プラットフォーム |
| NFR-2.6 | macOS, Linux, WebGL での動作を考慮する（ベストエフォート） | - | 将来的な拡張 |

---

### NFR-3: 配布形式

| ID | 要件 | 内容 | 根拠 |
|----|------|------|------|
| NFR-3.1 | UPM（Unity Package Manager）パッケージとして配布する | - | [existing-libraries.md §3.2] 最新の配布形式 |
| NFR-3.2 | OpenUPM への登録を行う | - | 発見性の向上 |
| NFR-3.3 | .unitypackage でも配布する | - | UPM 非対応プロジェクトへの対応 |
| NFR-3.4 | MIT ライセンスで公開する | - | OSS として広く利用可能に |

---

### NFR-4: セキュリティ

| ID | 要件 | 内容 | 根拠 |
|----|------|------|------|
| NFR-4.1 | デフォルト設定が安全である（Secure by Default） | - | [existing-libraries.md §4] 安全なデフォルトが不在 |
| NFR-4.2 | 非推奨 API を使用しない（RijndaelManaged, BinaryFormatter 等） | - | [cedec-and-articles.md §6] |
| NFR-4.3 | 暗号鍵をアセンブリ内にハードコードしない | - | [security-analysis.md §3.1] |
| NFR-4.4 | すべての暗号化操作で CSPRNG（暗号論的擬似乱数生成器）を使用する | - | 予測不可能な IV/ソルトの生成 |
| NFR-4.5 | 復号後のバッファを使用後にゼロクリアする | - | [security-analysis.md §3.5] メモリダンプ対策 |
| NFR-4.6 | エラーメッセージで暗号化の内部状態を漏洩しない | - | [security-analysis.md §3.4] オラクル攻撃対策 |

---

### NFR-5: スレッドセーフティ

| ID | 要件 | 内容 | 根拠 |
|----|------|------|------|
| NFR-5.1 | 同一スロットへの `SaveAsync` / `LoadAsync` の並行呼び出しをシリアライズ（排他制御）する | 同一スロットへの同時書き込みによるデータ破損を防止 | データ整合性の保証 |
| NFR-5.2 | 異なるスロットへの `SaveAsync` / `LoadAsync` は並行実行を許可する | スロット間の独立性を保ち、スループットを確保 | パフォーマンス要件 |
| NFR-5.3 | 内部のバッファプール・鍵キャッシュ等の共有リソースはスレッドセーフに管理する | - | 非同期 API の安全性保証 |

---

## 3. 推奨プロジェクト構成案

```
uSave/
├── package.json                      # UPM パッケージ定義
├── README.md
├── LICENSE                           # MIT License
├── CHANGELOG.md
├── Runtime/
│   ├── uSave.Runtime.asmdef
│   ├── Core/
│   │   ├── SaveManager.cs            # メインエントリポイント（Save/Load API）
│   │   ├── SaveFile.cs               # セーブファイルの読み書き（ヘッダ・ペイロード管理）
│   │   ├── SaveSlot.cs               # セーブスロット管理
│   │   └── SaveSettings.cs           # 設定（ScriptableObject）
│   ├── Encryption/
│   │   ├── IEncryptionProvider.cs     # 暗号化プロバイダーインターフェース
│   │   ├── AesCbcHmacProvider.cs      # AES-CBC + HMAC-SHA256 実装
│   │   ├── AesGcmProvider.cs          # AES-GCM 実装（CoreCLR 対応時）
│   │   └── NullEncryptionProvider.cs  # 暗号化なし（開発用）
│   ├── KeyManagement/
│   │   ├── IKeyProvider.cs            # 鍵プロバイダーインターフェース
│   │   ├── PlatformKeyProvider.cs     # プラットフォーム振り分け
│   │   ├── Pbkdf2KeyProvider.cs       # PBKDF2 フォールバック
│   │   └── EnvelopeEncryption.cs      # エンベロープ暗号化
│   ├── Serialization/
│   │   ├── ISerializer.cs             # シリアライザインターフェース
│   │   ├── MessagePackSerializer.cs   # MessagePack 実装
│   │   └── JsonSerializer.cs          # JsonUtility 実装
│   ├── Migration/
│   │   ├── IMigration.cs              # マイグレーションインターフェース
│   │   └── MigrationRunner.cs         # バージョン管理・マイグレーション実行
│   └── Plugins/
│       ├── Android/                   # Android Keystore ネイティブプラグイン
│       ├── iOS/                       # iOS Keychain ネイティブプラグイン
│       └── Windows/                   # DPAPI ラッパー
├── Runtime.AntiCheat/                 # ★ アンチチートモジュール（asmdef 分離）
│   ├── uSave.AntiCheat.asmdef
│   ├── ObscuredTypes/
│   │   ├── ObscuredBool.cs
│   │   ├── ObscuredByte.cs
│   │   ├── ObscuredSByte.cs
│   │   ├── ObscuredShort.cs
│   │   ├── ObscuredUShort.cs
│   │   ├── ObscuredInt.cs
│   │   ├── ObscuredUInt.cs
│   │   ├── ObscuredLong.cs
│   │   ├── ObscuredULong.cs
│   │   ├── ObscuredFloat.cs
│   │   ├── ObscuredDouble.cs
│   │   ├── ObscuredDecimal.cs
│   │   ├── ObscuredChar.cs
│   │   ├── ObscuredString.cs
│   │   ├── ObscuredVector2.cs
│   │   ├── ObscuredVector3.cs
│   │   ├── ObscuredVector2Int.cs
│   │   ├── ObscuredVector3Int.cs
│   │   └── ObscuredQuaternion.cs
│   ├── Detectors/
│   │   ├── ObscuredCheatingDetector.cs
│   │   ├── SpeedHackDetector.cs
│   │   ├── TimeCheatingDetector.cs
│   │   ├── WallHackDetector.cs
│   │   └── InjectionDetector.cs
│   ├── ObscuredPrefs.cs
│   └── ObscuredFilePrefs.cs
├── Editor/
│   ├── uSave.Editor.asmdef
│   ├── SaveDataEditorWindow.cs        # セーブデータ閲覧・編集ウィンドウ
│   ├── SaveSettingsEditor.cs          # 設定カスタムインスペクタ
│   ├── ObscuredPrefsEditorWindow.cs   # ★ ObscuredPrefs 閲覧・編集
│   └── PropertyDrawers/              # ★ ObscuredTypes の Inspector 対応
│       ├── ObscuredIntDrawer.cs
│       ├── ObscuredFloatDrawer.cs
│       └── ...
├── Tests/
│   ├── Runtime/
│   │   ├── uSave.Tests.Runtime.asmdef
│   │   ├── EncryptionTests.cs
│   │   ├── SerializationTests.cs
│   │   ├── KeyManagementTests.cs
│   │   ├── SaveManagerTests.cs
│   │   ├── MigrationTests.cs
│   │   └── AntiCheat/                # ★ アンチチートテスト
│   │       ├── ObscuredTypesTests.cs
│   │       └── DetectorTests.cs
│   └── Editor/
│       ├── uSave.Tests.Editor.asmdef
│       └── EditorToolTests.cs
├── Samples~/
│   ├── BasicUsage/                    # 基本的なセーブ・ロード
│   ├── AdvancedEncryption/            # 暗号化設定のカスタマイズ
│   ├── Migration/                     # マイグレーション実装例
│   └── AntiCheat/                     # ★ アンチチート使用例
└── Documentation~/
    ├── index.md
    ├── getting-started.md
    ├── encryption-guide.md
    ├── key-management.md
    ├── migration-guide.md
    └── anti-cheat-guide.md            # ★ アンチチートガイド
```

---

## 4. API 設計イメージ

### 4.1 基本的な使用例

```csharp
// 初期化（ゲーム起動時に 1 回）
SaveManager.Initialize(new SaveSettings
{
    Encryption = EncryptionType.AesCbcHmac,  // デフォルト
    Serializer = SerializerType.MessagePack,  // デフォルト
    SavePath = Application.persistentDataPath
});

// セーブ
var playerData = new PlayerData { Name = "Hero", Level = 10 };
await SaveManager.SaveAsync("player", playerData);

// ロード
var loaded = await SaveManager.LoadAsync<PlayerData>("player");

// 存在確認
bool exists = SaveManager.Exists("player");

// 削除
SaveManager.Delete("player");
```

### 4.2 カスタム設定

```csharp
// カスタムシリアライザ
SaveManager.Initialize(new SaveSettings
{
    Encryption = EncryptionType.AesCbcHmac,
    Serializer = SerializerType.Json,  // JsonUtility を使用
    KeyProvider = new CustomKeyProvider(),
    AutoBackup = true,
    MaxBackupCount = 3
});
```

---

## 5. 要件のトレーサビリティ

| 要件 ID | 調査結果 | 根拠セクション |
|---------|---------|---------------|
| FR-1.1 | 既存ライブラリの大半が認証なし暗号化 | existing-libraries.md §4 |
| FR-1.2 | Unity CoreCLR 移行で AES-GCM が利用可能に | security-analysis.md §1.7 |
| FR-2.1 | 暗号化とシリアライズの統合が不在 | existing-libraries.md §4 |
| FR-2.2 | MessagePack v3 が最適なシリアライザ | existing-libraries.md §3.3 |
| FR-3.1 | パディングオラクル・ビットフリッピング攻撃の防止 | security-analysis.md §3.4, §3.6 |
| FR-4.1 | プラットフォーム固有の鍵保護機構が活用されていない | security-analysis.md §2 |
| FR-5.1 | セーブデータのマイグレーションが必要 | security-analysis.md §4.1 |
| FR-7.1〜7.4 | ACTk ObscuredTypes の OSS 代替 | existing-libraries.md §1.2, §4.6 |
| FR-7.5〜7.8 | ACTk Detectors の OSS 代替 | existing-libraries.md §1.2, §4.6 |
| FR-7.9 | ACTk ObscuredPrefs の OSS 代替 | existing-libraries.md §1.2 |
| NFR-1.5 | ゲームループ内での ObscuredTypes 使用に必要 | security-analysis.md §5 |
| NFR-1.6 | モジュラー設計によるゼロオーバーヘッド保証 | existing-libraries.md §4.3 |
| NFR-4.1 | 安全なデフォルトの欠如 | existing-libraries.md §4 |
| NFR-4.2 | 非推奨 API の使用が蔓延 | cedec-and-articles.md §6 |
