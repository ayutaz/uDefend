# 既存ライブラリ調査・比較レポート

## 概要

Unity向けセーブデータ暗号化・シリアライズに関連する既存ライブラリを、商用・OSSの両面から調査し、暗号化方式・特徴・課題を比較する。

---

## 1. 商用ライブラリ（Unity Asset Store）

### 1.1 Easy Save 3

| 項目 | 内容 |
|------|------|
| **URL** | https://assetstore.unity.com/packages/tools/utilities/easy-save-the-complete-save-data-serializer-system-768 |
| **価格** | 約 €54.28（USD $55〜60 相当、税別） |
| **暗号化方式** | AES-128（CBC モード） |
| **シリアライズ** | 独自 JSON ベース（ES3 形式） |
| **対応プラットフォーム** | Windows, macOS, Linux, iOS, Android, WebGL, コンソール |

**特徴:**
- Unity で最も広く使用されているセーブソリューション（★4.8, 3,200+ レビュー）
- `ES3.Save()` / `ES3.Load()` の 1 行 API
- PlayerPrefs, File, PlayerPrefs+File のストレージモード
- 暗号化・圧縮のオン/オフ切り替え可能
- エディタ上でセーブデータを閲覧・編集可能
- コレクション型（List, Dictionary, Array 等）を自動シリアライズ
- スプレッドシート連携、クラウドセーブ対応

**課題:**
- AES-128 のみで AES-256 未対応
- 暗号化モードの選択不可（CBC 固定）
- 認証付き暗号化（AEAD）非対応のため、改ざん検知は別途必要
- 鍵管理機能なし（パスワード文字列をハードコード前提）
- ソースコードが難読化されており、セキュリティ監査が困難
- 商用ライセンスのため OSS プロジェクトへの組み込み制限あり
- サードパーティ製の復号ツール（`es3-modifier`）が GitHub に存在し、パスワードが既知の場合に復号可能
- WebGL でのテクスチャ保存が遅くプロセスを阻害する場合がある

---

### 1.2 Anti-Cheat Toolkit（Code Stage）

| 項目 | 内容 |
|------|------|
| **URL** | https://assetstore.unity.com/packages/tools/utilities/anti-cheat-toolkit-2024-202695 |
| **価格** | $55.00 |
| **暗号化方式** | 独自難読化 + AES |
| **主要機能** | メモリ保護・PlayerPrefs 暗号化・改ざん検知 |
| **対応プラットフォーム** | 全プラットフォーム |

**特徴:**
- `ObscuredPrefs`: PlayerPrefs を暗号化して保存
- `ObscuredInt` / `ObscuredFloat` 等: ランタイムメモリ上の値を難読化（Cheat Engine 対策）
- `ObscuredFile`: ファイルの暗号化保存
- スピードハック検知、壁透過検知等のアンチチート機能
- メモリスキャンツール（GameGuardian, Cheat Engine）への耐性

**課題:**
- セーブデータ暗号化は主機能ではなく、チート防止が主目的
- 暗号化の鍵管理はユーザー任せ
- シリアライズ機能は含まれない（JSON/Binary 変換は別途必要）
- メモリ保護のオーバーヘッドがある（頻繁なアクセスでパフォーマンス低下）

---

### 1.3 Quick Save

| 項目 | 内容 |
|------|------|
| **URL** | https://assetstore.unity.com/packages/tools/integration/quick-save-228743 |
| **価格** | 無料 |
| **暗号化方式** | AES |
| **シリアライズ** | JSON ベース |

**特徴:**
- 無料で使用可能
- 圧縮 + 暗号化に対応
- セーブデータの軽量化が可能
- シンプルな API

**課題:**
- 暗号化の詳細設定（モード、鍵長等）の記述が少ない
- メンテナンス状況が不透明
- 認証付き暗号化（AEAD）非対応
- プラットフォーム固有の鍵管理非対応

---

### 1.4 Smart Save System Lite

| 項目 | 内容 |
|------|------|
| **URL** | https://assetstore.unity.com/packages/tools/utilities/smart-save-system-lite-205498 |
| **価格** | 無料（Lite 版） |
| **暗号化方式** | AES |
| **シリアライズ** | Binary / JSON |

**特徴:**
- Lite 版は無料で基本機能が利用可能
- Binary / JSON の切り替えが可能
- Scene 単位でのセーブ・ロード対応

**課題:**
- Lite 版は機能制限あり
- 暗号化の詳細仕様が不明確
- 更新頻度が低い

---

## 2. OSS ライブラリ（GitHub）

### 2.1 USaveSerializer（TinyPlay）

| 項目 | 内容 |
|------|------|
| **URL** | https://github.com/TinyPlay/USaveSerializer |
| **Stars** | ~25 |
| **暗号化方式** | AES / RSA |
| **シリアライズ** | Binary, JSON, XML |
| **ライセンス** | MIT |

**特徴:**
- シリアライズと暗号化を統合した汎用ソリューション
- Binary / JSON / XML の 3 形式に対応
- AES と RSA の両方をサポート
- クラスを丸ごとシリアライズ可能

**課題:**
- Star 数が少なく、コミュニティが小さい
- 鍵管理機能なし
- GCM モード未対応（CBC のみと推測）
- テストコードが不十分
- メンテナンス状況が不透明

---

### 2.2 UnityCipher

| 項目 | 内容 |
|------|------|
| **URL** | https://github.com/TakuKobayashi/UnityCipher |
| **Stars** | ~80 |
| **暗号化方式** | AES（RijndaelManaged）, RSA |
| **ライセンス** | MIT |

**特徴:**
- AES と RSA の暗号化・復号化を簡潔な API で提供
- UPM (Unity Package Manager) 対応
- 文字列・バイト配列の暗号化に対応

**課題:**
- `RijndaelManaged` を使用（.NET 新版では非推奨）
- セーブデータのシリアライズ機能なし（暗号化のみ）
- 鍵管理・改ざん検知の仕組みなし
- IV の生成方法が不明確

---

### 2.3 GameShield

| 項目 | 内容 |
|------|------|
| **URL** | https://github.com/nicloay/GameShield |
| **Stars** | ~30 |
| **暗号化方式** | 難読化ベース |
| **ライセンス** | MIT |

**特徴:**
- ランタイムメモリの値保護（ObscuredInt 等）
- PlayerPrefs の難読化保存
- Anti-Cheat Toolkit の OSS 代替を目指す

**課題:**
- 暗号化ではなく難読化が中心（セキュリティ強度が低い）
- ファイルベースの暗号化セーブ非対応
- メンテナンスが活発でない

---

### 2.4 Unity-Saver

| 項目 | 内容 |
|------|------|
| **URL** | https://github.com/nicloay/Unity-Saver （推定） |
| **Stars** | ~15 |
| **暗号化方式** | 基本的な暗号化 |
| **ライセンス** | MIT |

**特徴:**
- シンプルなセーブ・ロード機能
- JSON シリアライズベース

**課題:**
- 暗号化機能が限定的
- 実用レベルのセキュリティには不十分
- ドキュメントが少ない

---

### 2.5 SafeValues

| 項目 | 内容 |
|------|------|
| **URL** | https://github.com/pitt500/SafeValues （類似リポジトリ複数） |
| **Stars** | ~10 |
| **暗号化方式** | XOR / 簡易暗号化 |
| **ライセンス** | MIT |

**特徴:**
- メモリ上の値を保護する型ラッパー（SafeInt, SafeFloat 等）
- Cheat Engine 等のメモリスキャナ対策

**課題:**
- XOR ベースのため暗号学的に脆弱
- セーブデータ暗号化ではなくメモリ保護が主目的
- ファイル暗号化機能なし

---

### 2.6 UnityCrypto（DevsDaddy）

| 項目 | 内容 |
|------|------|
| **URL** | https://github.com/DevsDaddy/UnityCrypto |
| **Stars** | ~19 |
| **暗号化方式** | AES, RSA, DES, TripleDES, SHA, MD5 等多数 |
| **ライセンス** | MIT |

**特徴:**
- 暗号化関数・ハッシュ関数を多数収録した包括的ライブラリ
- 複数の暗号方式を単一ライブラリで利用可能

**課題:**
- 暗号方式が多すぎて、推奨設定が不明確
- DES/TripleDES/MD5 など非推奨アルゴリズムも含む
- セーブデータ特化の設計ではない
- 鍵管理・改ざん検知なし

---

### 2.7 unity-crypto（Dubit）

| 項目 | 内容 |
|------|------|
| **URL** | https://github.com/dubit/unity-crypto |
| **Stars** | ~40 |
| **暗号化方式** | AES |
| **ライセンス** | MIT |

**特徴:**
- シンプルな AES 暗号化・復号化関数を提供
- Unity 向けに最適化

**課題:**
- 最小限の機能のみ
- 鍵管理・AEAD・改ざん検知なし
- 長期間メンテナンスされていない

---

## 3. BinaryFormatter 代替シリアライザ比較

Unity で `BinaryFormatter` はセキュリティリスク（任意コード実行の脆弱性）により非推奨となった。以下は代替シリアライザの比較。

### 3.1 比較表

| 項目 | MessagePack-CSharp v3 | MemoryPack | protobuf-net |
|------|----------------------|------------|-------------|
| **GitHub Stars** | ~5,700 | ~3,800 | ~4,900 |
| **最新バージョン** | v3.0.x（2024/12） | v1.x | v3.0.x |
| **パフォーマンス** | 非常に高速 | 最速（ゼロエンコーディング） | 高速 |
| **GC Allocation** | 低 | 最小 | 中 |
| **Unity 対応** | 2022.3.12f1 以降 | 2021.3 以降 | 要検証（制約あり） |
| **IL2CPP/AOT** | Source Generator で完全対応（mpc 不要） | Source Generator 対応 | JIT 依存のため問題あり |
| **クロス言語** | 多言語対応（msgpack 仕様） | C# + TypeScript のみ | 多言語対応（protobuf 仕様） |
| **スキーマ進化** | Key 属性による柔軟な対応 | VersionTolerant モード | フィールド番号による対応 |
| **暗号化統合** | なし（別途必要） | なし（別途必要） | なし（別途必要） |
| **配布形式** | NuGet + UPM | NuGet + UPM | NuGet |

### 3.2 各シリアライザの詳細

#### MessagePack for C# v3

- **URL**: https://github.com/MessagePack-CSharp/MessagePack-CSharp
- v3 で Source Generator ベースに移行し、mpc（MessagePackCompiler）が不要に
- Unity IL2CPP を完全サポート
- NuGet と UPM の両方で配布
- `[MessagePackObject]` / `[Key]` 属性でシリアライズ対象を指定
- バイナリフォーマットのため JSON より高速・コンパクト
- 最も実績が豊富で、日本のゲーム開発コミュニティでの採用例が多い

#### MemoryPack

- **URL**: https://github.com/Cysharp/MemoryPack
- Cysharp 製（neuecc 氏）。.NET 7 / C# 11 を活用した超高速シリアライザ
- ゼロエンコーディング設計で、メモリコピーに近い速度
- Unity では旧 Source Generator を使用（言語バージョンの制約）
- `GenerateType.VersionTolerant` で完全なバージョン互換を提供
- C# と TypeScript のみ対応（独自フォーマット）
- デフォルトモードではメンバーの削除・順序変更不可

#### protobuf-net

- **URL**: https://github.com/protobuf-net/protobuf-net
- Google Protocol Buffers の .NET 実装
- JIT コンパイルに依存するため、IL2CPP/AOT 環境（iOS, コンソール）で問題が発生
- 依存関係（System.Memory, System.Collections.Immutable）の解決が必要
- Unity 公式サポートなし（コミュニティベース）
- クロスプラットフォーム互換性は高いが、Unity との統合に手間がかかる

### 3.3 推奨

**MessagePack for C# v3** を第一候補とする。理由:
1. Source Generator による IL2CPP/AOT 完全対応
2. Unity コミュニティでの実績が最も豊富
3. パフォーマンスと機能のバランスが良い
4. UPM 対応で導入が容易
5. スキーマ進化への対応が柔軟

---

## 4. 総合評価

### 4.1 既存ソリューションに共通する課題

1. **認証付き暗号化（AEAD）の欠如**: ほとんどのライブラリが AES-CBC のみで、GCM 等の認証付きモードを未サポート
2. **鍵管理の不在**: 暗号化鍵のハードコードや文字列渡しが前提で、プラットフォーム固有の安全な鍵ストレージ連携がない
3. **改ざん検知の欠如**: 暗号化はあっても HMAC/署名による完全性検証がない
4. **セキュリティとシリアライズの分離**: 暗号化ライブラリとシリアライズライブラリが別々で、統合的なソリューションがない
5. **非推奨 API の使用**: `RijndaelManaged`, `BinaryFormatter` 等の非推奨クラスを使い続けている例が多い

---

### 4.2 競合比較マトリクス

§1〜§2 で調査した主要ライブラリと uDefend（計画）を以下の軸で比較する。

| 比較軸 | Easy Save 3 | Anti-Cheat Toolkit | Quick Save | UnityCipher | UnityCrypto | unity-crypto | **uDefend（計画）** |
|--------|-------------|-------------------|------------|-------------|-------------|--------------|------------------|
| **暗号化方式** | AES-128-CBC | 独自難読化 + AES | AES（詳細不明） | AES（RijndaelManaged） | AES, RSA, DES 等多数 | AES | AES-256-CBC + HMAC-SHA256 |
| **AEAD / 認証付き暗号化** | なし | なし | なし | なし | なし | なし | **Encrypt-then-MAC（デフォルト）** |
| **鍵管理** | パスワード文字列渡し | ユーザー任せ | 不明 | ハードコード前提 | ハードコード前提 | ハードコード前提 | **Android Keystore / iOS Keychain / Windows DPAPI 自動選択** |
| **改ざん検知** | なし | チート検知（メモリ） | なし | なし | なし | なし | **HMAC-SHA256（定数時間比較）** |
| **シリアライザ統合** | 内蔵（ES3 形式） | なし（別途必要） | JSON ベース | なし（暗号化のみ） | なし（暗号化のみ） | なし（暗号化のみ） | **MessagePack v3 統合（差し替え可）** |
| **エディタツール** | あり（閲覧・編集） | あり（設定 UI） | なし | なし | なし | なし | あり（閲覧・編集）※予定 |
| **async/await 対応** | 部分的 | なし | なし | なし | なし | なし | **完全対応** |
| **マイグレーション機構** | バージョン管理あり | なし | なし | なし | なし | なし | **バージョンヘッダ + 順次マイグレーション** |
| **ライセンス** | 商用（€54.28） | 商用（$55） | 無料 | MIT | MIT | MIT | **MIT** |
| **メモリ保護（ObscuredTypes）** | なし | ObscuredInt/Float 等（19 型以上） | なし | なし | なし | なし | **ObscuredTypes 19 型（Int/Float/String/Vector3 等）** |
| **スピードハック検知** | なし | SpeedHackDetector | なし | なし | なし | なし | **SpeedHackDetector（Time.time vs 実時刻の乖離検出）** |
| **チート検知コールバック** | なし | ObscuredCheatingDetector 等（5 種） | なし | なし | なし | なし | **5 種（メモリ改変/スピードハック/時刻改ざん/壁抜け/注入）** |
| **メンテナンス状態** | 活発 | 活発 | 不透明 | 低頻度 | 低頻度 | 長期停止 | 新規開発中 |

> **注記**: 各セルの情報は §1〜§2 の調査結果に基づく。「詳細不明」はドキュメントやソースから確認できなかった項目。

---

### 4.3 uDefend の価値提案

uDefend が既存ソリューション群に対して持つ固有の差別化ポイントを以下に整理する。

1. **認証付き暗号化がデフォルト**
   AES-256-CBC + HMAC-SHA256（Encrypt-then-MAC）が初期設定。他のライブラリはいずれも認証なし暗号化がデフォルトであり、改ざん検知には追加実装が必要。uDefend はゼロコンフィグで暗号化と完全性検証の両方を提供する。

2. **プラットフォームネイティブ鍵管理の統合**
   Android Keystore / iOS Keychain / Windows DPAPI を自動選択し、暗号鍵をプラットフォームの安全な領域に保管する。既存ライブラリはすべて鍵のハードコードまたは文字列渡しが前提であり、鍵の保護はユーザーの責任に委ねられている。

3. **暗号化 + シリアライズの一体化**
   MessagePack v3 をデフォルトシリアライザとして、シリアライズ → 暗号化 → 保存をワンコールで実行。既存ライブラリでは暗号化とシリアライザが分離しており、統合は開発者自身が行う必要がある。

4. **Secure by Default**
   ゼロコンフィグで OWASP 推奨の設定が適用される:
   - PBKDF2 600,000 回反復（OWASP 2023 推奨値）
   - CSPRNG による IV 生成
   - 定数時間 HMAC 比較（タイミング攻撃対策）
   - 復号後バッファのゼロクリア

5. **現代的な実装基盤**
   非推奨 API を一切使用しない（`Aes` クラス使用、`RijndaelManaged` / `BinaryFormatter` 不使用）。Source Generator ベースの IL2CPP 完全対応、async/await による非同期 API、スレッドセーフティ保証を備える。

6. **将来対応設計**
   暗号化バックエンドを `IEncryptionProvider` で抽象化しており、Unity CoreCLR 移行時（Unity 6.7+ 想定）に AES-GCM へシームレスに切替可能。既存のセーブデータとの後方互換性はファイルヘッダのバージョン情報で管理する。

7. **ランタイムメモリ保護**
   ObscuredTypes（19 型: Bool/Byte/SByte/Short/UShort/Int/UInt/Long/ULong/Float/Double/Decimal/Char/String/Vector2/Vector3/Vector2Int/Vector3Int/Quaternion）によるメモリスキャナ対策。ACTk と同等のメモリ保護を OSS・MIT ライセンスで提供する。暗黙の型変換演算子により、既存コードへの差し替えコストを最小化する。

8. **チート検知の統合**
   スピードハック・時刻改ざん・壁抜け・アセンブリ注入・ObscuredTypes 改変の 5 種の検知を提供する。メモリ保護 → シリアライズ → 暗号化 → ファイル保存の一貫した保護チェーンにより、ゲームデータのライフサイクル全体をカバーする。

---

### 4.4 ターゲットユーザー

uDefend が最も価値を発揮するユーザー像を以下に整理する。

- **インディー〜中規模スタジオ**
  専任セキュリティエンジニアがいないチームで、安全なセーブシステムを短期間で導入したい。Secure by Default の設計により、暗号化の専門知識なしで OWASP 推奨レベルのセキュリティを実現できる。

- **オフライン型 / ローカルセーブ重視のゲーム**
  サーバーサイド検証に頼れないタイトルで、クライアント側の暗号化が最終防衛線となるケース。認証付き暗号化 + プラットフォーム鍵管理により、オフライン環境でも最大限の保護を提供する。

- **OSS 志向の開発者**
  商用アセット（Easy Save 3 等）のブラックボックスに不満があり、ソースコードを監査・カスタマイズしたい開発者。MIT ライセンスで完全なソースコードを公開する。

- **セキュリティ意識の高いプロジェクト**
  認証付き暗号化・プラットフォーム鍵管理を要件として持つプロジェクト。コンプライアンスやセキュリティレビューで暗号化方式の根拠を求められる場合にも対応可能。

---

### 4.5 トレードオフの開示

uDefend が既存ライブラリに劣る点を以下に正直に開示する。ライブラリ選定時の判断材料として提示する。

- **Easy Save 3 との比較**
  ES3 は 3,200 件超のレビュー、豊富なチュートリアル、コンソール対応、クラウドセーブ、スプレッドシート連携など、セーブデータ管理の全機能を網羅するエコシステムを持つ。uDefend は「暗号化セキュリティへの特化」を設計方針としており、ES3 のような包括的なセーブ管理ソリューションではない。セキュリティ要件が低いプロジェクトでは ES3 の方が適切な選択肢となりうる。

- **Anti-Cheat Toolkit との比較**
  ACTk のカテゴリ 4（CodeHash 生成・Android インストール元検証・画面録画防止）は uDefend のスコープ外である。これらが必要な場合は ACTk との併用を推奨する。ただし ObscuredTypes・Detectors・データ保護については uDefend で代替可能である。

- **ACTk の成熟度**
  ACTk は 10 年以上の実績・継続的メンテナンス・大規模プロジェクトでの採用実績がある。uDefend のアンチチートモジュールは新規実装のため、エッジケースや特定デバイスでの挙動に未知の問題がある可能性がある。プロダクション投入前には十分なテストを推奨する。

- **導入の手軽さ**
  デフォルトシリアライザとして MessagePack v3 への依存があり、既存の JSON ベースプロジェクトではシリアライザ移行コストが発生する。`JsonUtility` によるフォールバックは提供するが、MessagePack の型定義（`[MessagePackObject]` / `[Key]` 属性）への移行が推奨パスとなる。

- **コミュニティ規模**
  新規 OSS プロジェクトのため、ドキュメント・サンプルコード・コミュニティサポートの量は商用製品（ES3, ACTk）に比べ限定的である。初期段階では Issue 対応やドキュメント充実に時間を要する。

---

### 4.6 uDefend が埋めるべきギャップ

- 認証付き暗号化（AES-GCM / AES-CBC+HMAC）の標準提供
- プラットフォーム固有鍵管理（Android Keystore / iOS Keychain / Windows DPAPI）の統合
- 高速シリアライザ（MessagePack 等）との統合
- エディタツールによるデバッグ・閲覧機能
- ゼロコンフィグで安全なデフォルト設定
- ランタイムメモリ保護（ObscuredTypes）の OSS 実装
- チート検知（スピードハック・時刻改ざん・壁抜け・注入）の統合
- メモリ保護 → シリアライズ → 暗号化 → ファイル保存の一貫した保護チェーン
