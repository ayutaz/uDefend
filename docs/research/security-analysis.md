# 暗号化方式・鍵管理・攻撃手法の分析

## 概要

Unity セーブデータ暗号化ライブラリの設計に必要な、暗号化アルゴリズム・鍵管理戦略・既知の脆弱性パターン・攻撃手法を分析する。対象プラットフォームは Windows / Android / iOS。

---

## 1. 暗号化方式の比較

### 1.1 比較表

| 方式 | 機密性 | 完全性 | 認証 | パフォーマンス | Unity 対応 | 推奨度 |
|------|--------|--------|------|---------------|-----------|--------|
| **AES-GCM** | ◎ | ◎ | ◎ | ○ | △（要検討） | ★★★★★ |
| **AES-CBC + HMAC-SHA256** | ◎ | ◎ | ○ | ○ | ◎ | ★★★★☆ |
| **AES-CBC（単体）** | ◎ | × | × | ◎ | ◎ | ★★☆☆☆ |
| **XOR** | × | × | × | ◎ | ◎ | ★☆☆☆☆ |
| **ChaCha20-Poly1305** | ◎ | ◎ | ◎ | ◎ | ×（要外部ライブラリ） | ★★★★☆ |
| **AES-GCM-SIV** | ◎ | ◎ | ◎ | ○ | ×（.NET 未サポート） | ★★★☆☆ |

### 1.2 AES-GCM（Galois/Counter Mode）

**概要:**
- 認証付き暗号化（AEAD: Authenticated Encryption with Associated Data）
- 暗号化と改ざん検知を 1 つの操作で実現
- 128bit の認証タグを生成

**メリット:**
- 暗号化 + 認証を単一パスで処理（高速）
- NIST 推奨の標準方式
- 追加認証データ（AAD）をサポート（ヘッダ等の非暗号化データの認証）
- AES-NI ハードウェアアクセラレーション対応

**デメリット:**
- Nonce の再利用で壊滅的な安全性低下（鍵 + Nonce の組み合わせは一意でなければならない）
- 96bit nonce の場合、同一鍵で約 2^32 メッセージが上限（wear-out limit）
- `.NET Standard 2.1` の `AesGcm` クラスは Unity での利用に制約あり

**Unity での可用性:**
- `System.Security.Cryptography.AesGcm` は .NET Core 3.0+ で利用可能
- Unity は .NET Standard 2.1 サブセットを使用しており、`AesGcm` クラスは直接利用不可
- 代替策: BouncyCastle C# ライブラリ（`Org.BouncyCastle.Crypto.Modes.GcmBlockCipher`）
- Unity 6 の CoreCLR 移行（6.7 以降予定）で `AesGcm` が利用可能になる見込み

**参考:**
- https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aesgcm
- https://github.com/dotnet/runtime/issues/89718

---

### 1.3 AES-CBC + HMAC-SHA256（Encrypt-then-MAC）

**概要:**
- AES-CBC で暗号化 → HMAC-SHA256 で認証タグを付与
- 「Encrypt-then-MAC」パターン（暗号化してから MAC を計算）

**メリット:**
- Unity の `System.Security.Cryptography` で完全に利用可能
- .NET Standard 2.0/2.1 で問題なく動作
- IL2CPP/AOT 環境でも動作確認済み
- 認証付き暗号化を実現できる
- 実装の透明性が高い

**デメリット:**
- AES-GCM と比較して 2 パス必要（暗号化 + HMAC）
- 実装ミスのリスクが高い（Encrypt-then-MAC の順序を間違えると脆弱に）
- IV は毎回ランダム生成が必須
- パディングオラクル攻撃のリスク（HMAC 検証を暗号化復号より先に行う必要あり）

**実装時の注意点:**
1. 必ず **Encrypt-then-MAC** の順序で実装する
2. HMAC の検証は暗号文の復号 **前** に行う
3. HMAC 検証は定数時間比較（timing-safe comparison）を使用する
4. IV は `RandomNumberGenerator.Fill()` で生成する
5. 暗号鍵と HMAC 鍵は別々に管理する（同じ鍵を使い回さない）

**参考:**
- https://learn.microsoft.com/en-us/dotnet/standard/security/cross-platform-cryptography

---

### 1.4 XOR 難読化

**概要:**
- 平文と鍵の XOR 演算のみ

**なぜ不十分か:**
- 既知平文攻撃に対して即座に鍵が暴露される
- 鍵の長さ以下のパターン繰り返しが統計的に検出可能
- 暗号学的安全性が皆無
- カジュアルな改変すら防げない（構造が推測可能）

**用途:**
- 開発中のプレースホルダーとしてのみ許容
- 本番環境での使用は不可

---

### 1.5 ChaCha20-Poly1305

**概要:**
- Google が TLS で採用した AEAD 方式
- AES-NI 非搭載デバイスで AES-GCM より高速

**メリット:**
- AES-NI なしのデバイスで優位
- 192bit nonce（XChaCha20）で nonce 衝突リスクが極めて低い
- タイミング攻撃に強い（定数時間演算）

**デメリット:**
- .NET 標準ライブラリに未実装
- BouncyCastle 等の外部ライブラリが必要
- Unity コミュニティでの採用実績が少ない

---

### 1.6 AES-GCM-SIV（Nonce Misuse-Resistant）

**概要:**
- Nonce を再利用しても壊滅的な安全性低下が起きない AEAD
- RFC 8452 で標準化

**メリット:**
- Nonce 再利用時でも同一平文の一致のみが漏洩（機密性は部分的に維持）
- 実装ミスに対する耐性が高い

**デメリット:**
- 2 パス必要で AES-GCM より約 33% 遅い
- .NET 標準ライブラリに未実装
- 実用的な .NET 実装が限られる（https://github.com/Metalnem/aes-gcm-siv は .NET Core 3.0 向け）

---

### 1.7 推奨方針

**短期（Unity 2022〜6.x）:** AES-CBC + HMAC-SHA256（Encrypt-then-MAC）
- Unity 標準 API で完結
- IL2CPP/AOT 対応が確実
- 十分なセキュリティ強度

**中長期（Unity 6.7+ CoreCLR 移行後）:** AES-GCM への移行
- `System.Security.Cryptography.AesGcm` が利用可能に
- 単一パスで高速
- よりシンプルな実装

**ライブラリ設計:** 暗号化バックエンドを抽象化し、将来的な方式切り替えを容易にする

---

## 2. 鍵管理戦略

### 2.1 プラットフォーム別鍵ストレージ

#### Android Keystore

| 項目 | 内容 |
|------|------|
| **URL** | https://developer.android.com/privacy-and-security/keystore |
| **方式** | ハードウェアセキュリティモジュール（TEE / StrongBox） |
| **対応** | Android 6.0 (API 23)+ |

**特徴:**
- 鍵がアプリケーションプロセスの外部（TEE/StrongBox）に保存される
- 鍵素材がアプリ空間に読み込まれない（鍵を使った演算は TEE 内で実行）
- 鍵の使用条件（認証要件、有効期限等）を設定可能
- root 化されたデバイスでも鍵の抽出が困難

**Unity からの利用:**
- Unity のネイティブプラグイン（Android Java/Kotlin → C# ブリッジ）が必要
- `AndroidJavaClass` / `AndroidJavaObject` を経由して Keystore API を呼び出す
- 直接的な Unity API サポートはない

**制約:**
- API 23 未満では利用不可
- StrongBox 対応は API 28+ かつハードウェア依存
- Keystore への鍵格納・取得にはオーバーヘッドがある

---

#### iOS Keychain Services

| 項目 | 内容 |
|------|------|
| **URL** | https://developer.apple.com/documentation/security/keychain_services |
| **方式** | Secure Enclave 連携の暗号化キーチェーン |
| **対応** | iOS 2.0+ |

**特徴:**
- パスワード・鍵・証明書を安全に保存
- Secure Enclave 対応デバイスではハードウェアレベルの保護
- アプリ削除後も Keychain データは残存可能（設定次第）
- iCloud Keychain による同期も可能

**Unity からの利用:**
- ネイティブプラグイン（Objective-C/Swift → C# ブリッジ）が必要
- `[DllImport("__Internal")]` でネイティブ関数を呼び出す
- 既存の Unity Keychain プラグインが複数存在

**制約:**
- Keychain アイテムのアクセス制御設定が複雑
- アプリ間共有の設定（Keychain Access Groups）に注意

---

#### Windows DPAPI（Data Protection API）

| 項目 | 内容 |
|------|------|
| **URL** | https://learn.microsoft.com/en-us/dotnet/standard/security/how-to-use-data-protection |
| **方式** | ユーザー/マシン単位の暗号化 |
| **対応** | Windows 2000+ |

**特徴:**
- `ProtectedData.Protect()` / `ProtectedData.Unprotect()` の簡潔な API
- ユーザーアカウント単位 (`DataProtectionScope.CurrentUser`) またはマシン単位で保護
- 鍵管理が OS に委譲される（開発者が鍵を管理する必要がない）
- .NET Framework / .NET Core 両方で利用可能

**Unity からの利用:**
- `System.Security.Cryptography.ProtectedData` クラスを使用
- NuGet パッケージ `System.Security.Cryptography.ProtectedData` のインストールが必要
- Windows 以外のプラットフォームでは利用不可

**制約:**
- Windows 専用（クロスプラットフォーム不可）
- マシン間でのデータ移植不可（別 PC では復号できない）
- Unity での .NET Standard 2.1 との互換性に要検証

---

#### PBKDF2（パスワードベース鍵導出）

| 項目 | 内容 |
|------|------|
| **方式** | パスワードからの鍵導出関数 |
| **API** | `Rfc2898DeriveBytes` (.NET) |

**特徴:**
- ユーザーパスワードやデバイス固有情報から暗号鍵を導出
- ソルト + イテレーション回数で計算コストを調整可能
- クロスプラットフォームで利用可能

**推奨設定:**
- イテレーション回数: 最低 600,000 回（OWASP 2024 推奨、SHA-256 使用時）
- ソルト: 最低 128bit のランダム値
- ハッシュアルゴリズム: SHA-256 以上

**制約:**
- 「パスワード」をどこから取得するかが問題
- デバイス固有情報の組み合わせ（デバイスID + ユーザーソルト等）で疑似的なパスワードを生成する手法がある
- イテレーション回数が多いと初回ロードが遅くなる

---

### 2.2 推奨鍵管理アーキテクチャ

```
[マスター鍵の保護]
  Android → Android Keystore (TEE)
  iOS     → iOS Keychain (Secure Enclave)
  Windows → DPAPI (CurrentUser)
  フォールバック → PBKDF2 (デバイスID + アプリ固有ソルト)

[データ暗号鍵 (DEK)]
  マスター鍵で暗号化された DEK をファイルに保存
  DEK でセーブデータを暗号化（エンベロープ暗号化パターン）

[鍵ローテーション]
  新しい DEK を生成 → 古い DEK でデータ復号 → 新しい DEK で再暗号化
  マスター鍵は変更不要（DEK のみローテーション）
```

**エンベロープ暗号化のメリット:**
- マスター鍵の使用頻度を最小化（DEK の暗号化/復号時のみ）
- 鍵ローテーションが高速（マスター鍵の変更不要）
- 複数のセーブファイルに異なる DEK を使用可能

---

## 3. 既知の脆弱性パターンと対策

### 3.1 鍵のハードコード

**脆弱性:**
```csharp
// NG: 鍵がアセンブリ内にハードコード
private const string KEY = "MySecretKey12345";
```
- IL2CPP バイナリでも文字列はリバースエンジニアリングで抽出可能
- dnSpy, ILSpy, Ghidra 等のツールで容易に発見される

**対策:**
- プラットフォーム固有の鍵ストレージを使用
- 鍵を実行時に導出（PBKDF2 + デバイス固有情報）
- 鍵をバイナリ内に直接格納しない

---

### 3.2 静的 IV / Nonce の再利用

**脆弱性:**
```csharp
// NG: IV が固定
byte[] iv = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
```
- AES-CBC: 同一 IV + 同一鍵で暗号化されたデータのブロックパターンから平文が推測可能
- AES-GCM: 同一 Nonce の再利用で認証が完全に破綻（鍵の復元すら可能）

**対策:**
- 毎回 `RandomNumberGenerator.Fill()` で IV/Nonce を生成
- 生成した IV を暗号文の先頭に付加して保存

---

### 3.3 ECB モードの使用

**脆弱性:**
- 同一平文ブロックが同一暗号文ブロックになる（パターンが保存される）
- 有名な「ペンギン画像」の例

**対策:**
- ECB は使用しない
- CBC, CTR, GCM 等のモードを使用する

---

### 3.4 認証なし暗号化（パディングオラクル攻撃）

**脆弱性:**
- AES-CBC を HMAC なしで使用した場合、パディングエラーの有無から平文を特定可能
- .NET のデフォルト AES は CBC + PKCS7 パディング（パディングオラクルに脆弱）

**参考:** https://pulsesecurity.co.nz/articles/dotnet-padding-oracles

**対策:**
- 認証付き暗号化（AEAD）を使用: AES-GCM または AES-CBC + HMAC
- HMAC 検証を復号前に実行
- エラーメッセージでパディングの成否を区別しない

---

### 3.5 メモリダンプ攻撃

**脆弱性:**
- GameGuardian, Cheat Engine 等のツールでプロセスメモリをスキャン
- 復号された平文データ、暗号鍵がメモリ上に残存
- Unity の managed メモリは GC が管理するため、明示的なクリアが困難

**対策:**
- 復号データの使用後速やかにゼロクリア
- `byte[]` の場合は `Array.Clear()` で明示的にクリア
- `SecureString` の使用（ただし Unity での利用は制限的）
- `ObscuredInt` / `ObscuredFloat` 等のメモリ難読化型の併用
- GC による移動前のクリアは保証されないことを認識する

---

### 3.6 ファイル改ざん（認証なし）

**脆弱性:**
- 暗号化のみでは改ざんを検知できない
- ビットフリッピング攻撃（CBC モードの場合、特定ビットの反転が可能）

**対策:**
- HMAC-SHA256 による完全性検証
- 認証付き暗号化（AES-GCM）の使用
- セーブファイルの構造: `[Version][IV][Encrypted Data][HMAC]`

---

### 3.7 PlayerPrefs の平文保存

**脆弱性:**
- PlayerPrefs は各プラットフォームで以下の場所に平文保存される:
  - Windows: レジストリ `HKCU\Software\[CompanyName]\[ProductName]`
  - macOS: `~/Library/Preferences/[BundleID].plist`
  - Android: `SharedPreferences` (XML ファイル)
  - iOS: `NSUserDefaults` (plist ファイル)
- 全てのプラットフォームで容易に読み取り・改変可能

**対策:**
- PlayerPrefs を使用しない（ファイルベースの暗号化保存を使用）
- やむを得ず使用する場合は暗号化 + HMAC 後に Base64 エンコードして保存

---

### 3.8 BinaryFormatter の脆弱性

**脆弱性:**
- 任意コード実行（RCE）の脆弱性がある
- Microsoft が公式に「安全でない」と宣言
- .NET 9 以降は `BinaryFormatter` が完全に除去

**参考:** https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-security-guide

**対策:**
- MessagePack, MemoryPack, JSON 等の安全なシリアライザを使用
- 信頼できないデータのデシリアライズを行わない

---

## 4. セーブファイル形式の設計

### 4.1 推奨ファイル構造

```
+------------------+
| Magic Number (4) |  ← "uSAV" (ファイル識別子)
+------------------+
| Version (2)      |  ← ファイル形式バージョン
+------------------+
| Flags (2)        |  ← 暗号化方式、圧縮フラグ等
+------------------+
| IV / Nonce (16)  |  ← ランダム生成された IV
+------------------+
| Encrypted Data   |  ← シリアライズ → 暗号化されたペイロード
| (可変長)          |
+------------------+
| Auth Tag (32)    |  ← HMAC-SHA256 (Encrypt-then-MAC)
+------------------+
```

### 4.2 処理フロー

**保存:**
1. オブジェクト → MessagePack でシリアライズ → `byte[]`
2. (オプション) 圧縮
3. ランダム IV 生成
4. AES-CBC で暗号化
5. HMAC-SHA256 を計算（IV + 暗号文に対して）
6. ヘッダ + IV + 暗号文 + HMAC をファイルに書き込み

**読み込み:**
1. ファイルからヘッダ + IV + 暗号文 + HMAC を読み込み
2. HMAC を検証（不一致なら改ざんとして拒否）
3. AES-CBC で復号
4. (オプション) 解凍
5. MessagePack でデシリアライズ → オブジェクト

---

## 5. Unity 固有のセキュリティ考慮事項

### 5.1 IL2CPP vs Mono

| 項目 | Mono | IL2CPP |
|------|------|--------|
| **逆コンパイル容易性** | 非常に容易（IL コードがそのまま残る） | 困難だが可能（C++ コードに変換後コンパイル） |
| **文字列の抽出** | 容易 | 中程度（strings コマンド等で可能） |
| **暗号 API** | .NET Standard 2.1 サブセット | .NET Standard 2.1 サブセット |
| **推奨** | 開発時のみ | リリースビルドは必ず IL2CPP |

### 5.2 .NET Cryptography API の可用性

Unity（.NET Standard 2.1 サブセット）で利用可能な暗号 API:

| API | 利用可否 | 備考 |
|-----|---------|------|
| `Aes` (AES-CBC) | ◎ | 全プラットフォームで利用可能 |
| `HMACSHA256` | ◎ | 全プラットフォームで利用可能 |
| `RandomNumberGenerator` | ◎ | 全プラットフォームで利用可能 |
| `Rfc2898DeriveBytes` (PBKDF2) | ◎ | 全プラットフォームで利用可能 |
| `SHA256` | ◎ | 全プラットフォームで利用可能 |
| `AesGcm` | × | .NET Core 3.0+ 専用。Unity では利用不可 |
| `ProtectedData` (DPAPI) | △ | Windows のみ。NuGet パッケージが必要 |
| `RSA` | ◎ | 利用可能だがセーブデータ暗号化には不向き |

### 5.3 CoreCLR 移行のロードマップ

- **Unity 6.7（予定）**: 実験的デスクトッププレイヤーで CoreCLR 対応
- **その後**: CoreCLR ベースのエディタ
- **影響**: `AesGcm` 等の .NET Core API が利用可能に
- **設計方針**: 暗号化バックエンドを抽象化し、CoreCLR 移行時に切り替え可能にする
