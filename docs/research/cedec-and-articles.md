# CEDEC 講演・技術記事サマリ

## 概要

Unity セーブデータ暗号化に関連する CEDEC（Computer Entertainment Developers Conference）講演および日本語技術記事の調査結果をまとめる。

---

## 1. CEDEC 講演

### 1.1 CEDEC 2018: セキュリティ会社のエンジニアが伝えたい 2018 年のチート事情

| 項目 | 内容 |
|------|------|
| **登壇者** | DNP ハイパーテック |
| **URL** | https://www.hypertech.co.jp/column/event/2018/09/%E3%80%90cedec2018%E3%80%91%E3%82%BB%E3%82%AD%E3%83%A5%E3%83%AA%E3%83%86%E3%82%A3%E4%BC%9A%E7%A4%BE%E3%81%AE%E3%82%A8%E3%83%B3%E3%82%B8%E3%83%8B%E3%82%A2%E3%81%8C-2018%E5%B9%B4%E3%81%AE%E3%83%81/ |
| **所要時間** | 25 分（3 日目） |

**要点:**
- 位置偽装チートと他社 IP 画像の不正抽出の 2 つの脅威事例を紹介
- スマートフォンゲームにおけるチート手法と対策が中心
- 約 2 年間でチーターのスキルレベルが急速に向上している現状を報告
- アプリ保護の重要性を啓発

**教訓:** チート技術は年々高度化しており、「やられてから対策する」では遅い。設計段階からセキュリティを組み込む必要がある。

---

### 1.2 CEDEC 2019: リバースエンジニアリングとチート事例から学ぶセキュリティ対策

| 項目 | 内容 |
|------|------|
| **登壇者** | 國澤佳代（DNP ハイパーテック研究開発部） |
| **URL** | https://cedec.cesa.or.jp/2019/session/detail/s5ce3a91054cde.html |
| **CEDiL** | https://cedil.cesa.or.jp/cedil_sessions/view/2084 |
| **日時** | 2019/9/5（木）11:20〜12:20 |

**要点:**
- 脆弱性把握のためのリバースエンジニアリング技術を共有
- 逆アセンブラを使ったシンプルな攻撃の実例を紹介
- PC・モバイルゲームにおけるチート手口を体系的に整理
- セキュリティ技術と効果的な対策に関する知見を提供
- セッション動画が YouTube の CEDEC チャンネルで公開

**教訓:** IL2CPP であっても逆アセンブルは可能。暗号鍵のハードコードは容易に発見される。

---

### 1.3 CEDEC 2019: チート行為を未然に防ぐゲームセキュリティ診断のススメ

| 項目 | 内容 |
|------|------|
| **登壇者** | ネットエージェント株式会社 |
| **URL** | https://cedec.cesa.or.jp/2019/session/detail/s5ce49e4ce62ce.html |
| **日時** | 2019/9/4〜9/6 |

**要点:**
- クライアントプログラムの保護は「対処療法」であり、適切なサーバーサイド設計が「根本治療」
- ゲームセキュリティ診断の蓄積ノウハウに基づく想定事例を解説
- 実際にゲームをプレイしてチート行為を行い、脆弱性を検出する手法を紹介

**教訓:** クライアントサイドの暗号化だけでは不十分。重要データはサーバーサイドで検証する設計が必要。

---

### 1.4 CEDEC 2020: ゲームエンジニアだからこそ実現できるプロダクト特化型チート対策

| 項目 | 内容 |
|------|------|
| **URL** | https://cedec.cesa.or.jp/2020/session/detail/s5e8315e0756e9.html |
| **日時** | 2020/9/4 18:00〜19:00 |
| **対象** | ネイティブエンジニア（主に Unity）、スマートフォンゲーム開発者 |

**要点:**
- 主要チート手法: メモリチート、スピードハック、タイマーハック、MITM 攻撃、Jailbreak/Cycript 攻撃、MOD 改変
- クライアント側の計算ロジックを最小化し、パラメータをサーバー側（ゲーム外）とクライアント側（ゲーム内）に分割
- 汎用対策より「プロダクト特化型」の検証を重視

**教訓:** プロダクトの仕様を理解した上で、重要なパラメータはサーバーサイドで管理する設計が必要。

---

### 1.5 CEDEC 2022: チート行為とオンラインゲームセキュリティ

| 項目 | 内容 |
|------|------|
| **登壇者** | 松田和樹（株式会社ラック） |
| **URL** | https://cedec.cesa.or.jp/2022/session/detail/237.html |
| **CEDiL** | https://cedil.cesa.or.jp/cedil_sessions/view/2519 |
| **日時** | 2022/8/23（60 分） |

**要点:**
- サーバープログラムの設計・実装によるチート対策
- 不正課金、有料アイテム窃取、パラメータ改ざん、RMT、DoS 攻撃を網羅
- 講演者は書籍「オンラインゲームセキュリティ」（2022 年 5 月、データハウス刊）の著者
- 参考レポート: https://dev.classmethod.jp/articles/cedec2022-cheat-and-onlinegamesecurity/

**教訓:** オンラインゲームのセキュリティは設計段階から組み込む必要がある。

---

### 1.6 CEDEC 2024: ゲームアプリへのチート対策の必要性

| 項目 | 内容 |
|------|------|
| **登壇者** | DNP ハイパーテック（CrackProof チーム） |
| **URL** | https://cedec.cesa.or.jp/2024/session/detail/s6661648e54155/ |
| **CEDiL** | https://cedil.cesa.or.jp/cedil_sessions/view/2905 |

**要点:**
- Android アプリでのチート対策を Q&A 形式で解説
- CrackProof 等の商用ソリューションの紹介

**教訓:** チート対策は汎用ツールだけでは不十分であり、アプリ固有の保護（バイナリ保護・改ざん検知）を組み合わせる必要がある。

---

### 1.7 CEDEC 2025: ゲームにおけるチート、海賊版対策の必要性

| 項目 | 内容 |
|------|------|
| **登壇者** | DNP ハイパーテック |
| **URL** | https://www.hypertech.co.jp/column/developer/2025/07/cedec2025-lecture/ |

**要点:**
- Windows PC ゲームでのチート・海賊版対策を解説
- デスクトップ環境特有のセキュリティ課題を整理

**教訓:** PC ゲームはモバイルと異なり、ユーザーが管理者権限を持つため、メモリ改ざん・ファイル改変が容易。暗号化に加えてバイナリ保護・整合性チェックの多層防御が不可欠。

---

## 2. 企業カンファレンス講演（CEDEC 以外）

### 2.1 DeNA TechCon 2019: スマホゲームのチート手法とその対策

| 項目 | 内容 |
|------|------|
| **登壇者** | 舟久保貴彦（DeNA セキュリティ技術グループ） |
| **URL** | https://www.slideshare.net/dena_tech/dena-techcon-2019-132194701 |

**要点:**
- チート = 「開発者の意図しない動作を引き起こし、有利にゲームを進める不正行為」
- 3 種類のチート手法: メモリ改変、パケット改変、コード改変
- パケット改変: プログラムコードから暗号化アルゴリズムと鍵を抽出 → 復号 → 改変 → 再暗号化して送信
- 暗号鍵のバイナリ内ハードコードが根本的な脆弱性

---

## 3. Qiita 記事

### 3.1 Unity の資産を死守するための暗号化と難読化のやりすぎテクニック

| 項目 | 内容 |
|------|------|
| **著者** | waiwaiunity |
| **URL** | https://qiita.com/waiwaiunity/items/776e1e60ac4183afb75c |
| **プラットフォーム** | Qiita |

**要点:**
- AssetBundle, ScriptableObject, テクスチャ等の暗号化手法を網羅的に解説
- IL2CPP 環境での難読化テクニック
- 暗号化と難読化を組み合わせた多層防御
- 「やりすぎ」のレベル感で実践的なテクニックを紹介

**教訓:** 暗号化だけでなく難読化も組み合わせることで、攻撃コストを引き上げる多層防御が有効。

---

### 3.2 Unity でセーブデータを暗号化して Serialize 保存

| 項目 | 内容 |
|------|------|
| **著者** | tempura |
| **URL（その 1）** | https://qiita.com/tempura/items/a9b63ce8a32def6a69b1 |
| **URL（その 2）** | https://qiita.com/tempura/items/84143f36160a66519c5e |
| **シリーズ** | 2 記事 |

**要点:**
- Object → JSON 変換 → 暗号化 → Base64 → byte 保存の流れを実装
- `StringEncryptor` クラスを作成して暗号化・復号化を分離
- `JsonUtility.ToJson` でシリアライズ後に暗号化するパターン
- 連載記事で段階的に実装を解説

**教訓:** JSON + 暗号化の組み合わせはシンプルだが、JSON のオーバーヘッドとセキュリティのトレードオフがある。

---

### 3.3 Unity でセーブデータを Serialize 保存する 〜現状〜

| 項目 | 内容 |
|------|------|
| **著者** | tempura |
| **URL** | https://qiita.com/tempura/items/7e63f39990620189c332 |

**要点:**
- シリアライザの推奨順序: ZeroFormatter > ScriptableObject > JsonUtility > Easy Save > FlatBuffers > MessagePack > その他
- 各シリアライザのメリット・デメリットを比較
- パフォーマンスと使いやすさの観点から評価

---

### 3.4 Unity のモバイルゲーム向けセキュリティ関連覚書

| 項目 | 内容 |
|------|------|
| **著者** | s_ryuuki |
| **URL（Qiita）** | https://qiita.com/s_ryuuki/items/04e136cf08328a835654 |
| **URL（Zenn 移行版）** | https://zenn.dev/s_ryuuki/articles/e65af482a789cf |

**要点:**
- モバイルゲーム向けセキュリティの包括的な覚書
- Qiita 版は Zenn に移行済み
- チート手法の分類と対策を体系的に整理

**教訓:** セキュリティ対策の全体像を把握する上で非常に参考になる資料。

---

### 3.5 Unity のモバイルゲーム向けクラッキングが行われるポイントを整理してみた

| 項目 | 内容 |
|------|------|
| **著者** | s_ryuuki |
| **URL（Qiita）** | https://qiita.com/s_ryuuki/items/c6b63d108959582a2b2e |
| **URL（Zenn）** | https://zenn.dev/s_ryuuki/articles/7fa34a75da220e |

**要点:**
- クラッキングの目的（チート、海賊版、データ解析）を分類
- 攻撃ポイント: ローカルデータ改変、メモリ改変、通信傍受、アプリ改変
- クライアント脅威: デバッガアタッチ、インジェクション攻撃
- ネットワーク脅威: HTTPS でも証明書検証の不備で脆弱に
- サーバー脅威: DoS 攻撃、SQL インジェクション、ブルートフォース
- 暗号化・サーバーサイド検証・鍵ローテーション・行動監視を推奨

**教訓:** セーブデータ暗号化はセキュリティ対策の一部に過ぎず、全体像を理解した上で設計する必要がある。

---

### 3.6 Unity 製アプリに対するよくある不正行為とその対策

| 項目 | 内容 |
|------|------|
| **著者** | hanaaaaaachiru |
| **URL** | https://qiita.com/hanaaaaaachiru/items/04760d8c6cbda167cb27 |

**要点:**
- メモリ改変、セーブデータ改変、通信改変の 3 つの攻撃ベクトルを解説
- それぞれに対する具体的な対策手法を紹介
- Unity 固有の脆弱性と対策に焦点

---

### 3.7 JsonUtility で複数ファイルに簡易暗号化データを保存する

| 項目 | 内容 |
|------|------|
| **著者** | kiku09020 |
| **URL** | https://qiita.com/kiku09020/items/cbb480916b39e39a01b6 |

**要点:**
- XOR 暗号を使用した簡易暗号化の実装例
- JsonUtility でのデータ保存と組み合わせ

**教訓:** XOR は「暗号化」ではなく「難読化」。実用的なセキュリティには不十分。

---

### 3.8 AssetBundle の解析手法とその対策について

| 項目 | 内容 |
|------|------|
| **著者** | k7a |
| **URL** | https://qiita.com/k7a/items/b64d6f7859f4332220ba |

**要点:**
- AssetStudio 等のツールによる AssetBundle 解析手法
- 暗号化による保護手法
- アセット保護の限界と現実的な対策

---

## 4. Zenn 記事

### 4.1 Unity のモバイルゲーム向けセキュリティ関連覚書（Zenn 版）

| 項目 | 内容 |
|------|------|
| **著者** | s_ryuuki |
| **URL** | https://zenn.dev/s_ryuuki/articles/e65af482a789cf |

**要点:**
- Qiita 版からの移行・更新版
- モバイルゲームのセキュリティ対策を包括的に整理
- 暗号化、難読化、サーバーサイド検証を含む多層防御を推奨

---

### 4.2 Unity のモバイルゲーム向けクラッキングが行われるポイント（Zenn 版）

| 項目 | 内容 |
|------|------|
| **著者** | s_ryuuki |
| **URL** | https://zenn.dev/s_ryuuki/articles/7fa34a75da220e |

**要点:**
- クラッキングポイントの体系的整理
- 攻撃目的・手法・対策の対応表
- 実務的な対策の優先度付け

---

## 5. 企業技術ブログ

### 5.1 QualiArts: Unity 製アプリにおいてアセットを暗号化する手法

| 項目 | 内容 |
|------|------|
| **URL** | https://technote.qualiarts.jp/article/32/ |
| **発行元** | QualiArts エンジニアブログ |

**要点:**
- TextAsset と SpriteAtlas の暗号化手法を具体的に解説
- 暗号化タイミングの選択肢:
  1. Unity インポート前のテキストファイル段階
  2. TextAsset になるタイミング
  3. アプリビルドのタイミング
- SpriteAtlas の暗号化: `Include in Build` を OFF → AssetBundle 化 → 暗号化 → StreamingAssets 化
- AssetStudio 等のツールによるカジュアルハック対策として有効
- ただし暗号化・復号化処理自体が逆コンパイルで解読されるリスクは残る

**教訓:** 「アプリを公開した時点でアプリ内のアセットは全て解読される前提」を持つべき。

---

### 5.2 WonderPlanet: Unity で AES によるデータ暗号化

| 項目 | 内容 |
|------|------|
| **URL** | https://developers.wonderpla.net/entry/2016/08/02/141000 |
| **発行元** | WonderPlanet Developers' Blog |

**要点:**
- PlayerPrefs やファイル保存時のデータ暗号化の必要性
- AES 暗号化の基本: ブロック長 128bit 固定、鍵長 128/192/256bit
- `RijndaelManaged` クラスを使用した実装
- 暗号化・復号化には同じ鍵長・ブロック長・パスワード・ソルトが必要

**教訓:** 基本的な AES 実装だが、RijndaelManaged は現在非推奨。Aes クラスへの移行が必要。

---

### 5.3 DNP ハイパーテック: リソース解析について学ぶ – Unity AssetBundle を例に

| 項目 | 内容 |
|------|------|
| **URL** | https://www.hypertech.co.jp/column/developer/2021/10/%E3%83%AA%E3%82%BD%E3%83%BC%E3%82%B9%E8%A7%A3%E6%9E%90%E3%81%AB%E3%81%A4%E3%81%84%E3%81%A6%E5%AD%A6%E3%81%B6%E3%80%80-unity-assetbundle%E3%82%92%E4%BE%8B%E3%81%AB/ |
| **発行元** | DNP ハイパーテック |

**要点:**
- AssetBundle の構造と解析手法を解説
- 暗号化されていない AssetBundle がいかに容易に解析されるかを実演
- 暗号化とアプリ保護の組み合わせを推奨

---

### 5.4 テラシュールブログ: 暗号化した AssetBundle は LoadFromStream でロード

| 項目 | 内容 |
|------|------|
| **URL** | https://tsubakit1.hateblo.jp/entry/2019/03/16/162138 |
| **著者** | tsubaki（テラシュールブログ） |

**要点:**
- `AssetBundle.LoadFromStream` で暗号化 AssetBundle をストリーム復号可能
- 全データをメモリに展開せずに復号→ロードできるためメモリ効率が良い
- 暗号化 AssetBundle の生成 Gist も公開

---

### 5.5 その他の参考記事

| タイトル | URL | 要点 |
|---------|-----|------|
| AES を用いて暗号化・復号化する | https://fineworks-fine.hatenablog.com/entry/2023/01/09/073000 | AES 実装の基本的な解説 |
| Unity セーブデータの使い方を徹底解説 | https://wamutai-tech.com/unity-savedata/ | セーブデータの保存・ロード・暗号化を網羅 |
| AES でガチガチに暗号化したい | https://www.create-forever.games/unity-aes-encrypt/ | AES-CBC + HMAC の実装例 |
| セーブ機能の作り方 - データ改造ができないシステム | https://alicia-ing.com/programming/unity/savedata/ | 改ざん防止を意識したセーブシステム設計 |
| セーブデータを暗号化して保存する | https://edom18.hateblo.jp/entry/2018/10/07/174003 | AES 暗号化セーブの実装例 |
| Anti-Cheat Toolkit でセーブデータを暗号化 | https://tedenglish.site/unity-asset-anticheat-toolkit/ | Anti-Cheat Toolkit の ObscuredPrefs の使い方 |
| データをゲームからサーバーへ AES 暗号化して送信 | https://www.petitmonte.com/unity/aes128.html | AES-128 による通信暗号化 |

---

## 6. Unity 公式・関連リソース

### 6.1 Unity Manual: Protecting Content

| 項目 | 内容 |
|------|------|
| **URL** | https://docs.unity3d.com/540/Documentation/Manual/protectingcontent.html |

**要点:**
- Unity 公式のコンテンツ保護ガイドライン
- AssetBundle の暗号化に関する基本的な推奨事項

### 6.2 Unity SSDLC（Secure Software Development Lifecycle）

| 項目 | 内容 |
|------|------|
| **URL** | https://unity.com/resources/unity-ssdlc |

**要点:**
- Unity のセキュアソフトウェア開発ライフサイクル
- 暗号化に関するガイドライン

### 6.3 Unity Discussions: セーブデータ暗号化関連スレッド

| タイトル | URL |
|---------|-----|
| How to secure save data? | https://discussions.unity.com/t/how-to-secure-save-data/883690 |
| Encrypt Savegames | https://discussions.unity.com/t/encrypt-savegames/721322 |
| Data encryption | https://discussions.unity.com/t/data-encryption/584178 |

---

## 7. 調査から得られた教訓のまとめ

1. **クライアントサイド暗号化の限界**: アプリを公開した時点でリバースエンジニアリングは可能。暗号化は「コストを上げる」ための手段
2. **多層防御の重要性**: 暗号化 + 難読化 + サーバーサイド検証の組み合わせが有効
3. **鍵管理が最重要**: 暗号化アルゴリズムよりも鍵の保護が重要。ハードコードは論外
4. **非推奨 API からの脱却**: `RijndaelManaged`, `BinaryFormatter` の使用を避け、現代的な API を採用すべき
5. **パフォーマンスとのバランス**: 暗号化のオーバーヘッドを最小限に抑える設計が必要（ストリーム処理等）
6. **プラットフォーム固有の対策**: Android, iOS, Windows それぞれのセキュリティ機構を活用すべき
