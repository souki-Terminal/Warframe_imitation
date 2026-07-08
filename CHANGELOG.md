# 更新履歴 (CHANGELOG)

このファイルは、プロジェクト内のコード変更や新機能追加、修正などの履歴を記録するためのものです。
AIアシスタントがコードを変更した際は、必ずこのファイルに変更内容を追記します。

## [2026-07-06] - スクリプトの最適化と機能追加

### 追加・変更内容
* **WeaponAnimationEvent.cs**
  * `Update()` 内で毎フレーム実行されていた文字列比較（`IsName`, `IsTag`）を、`Animator.StringToHash` を用いたハッシュ値比較に変更し、パフォーマンスを大幅に改善。
* **PlayerStatus.cs**
  * 被弾時の負荷を減らすため、`TakeDamage`内で都度取得していた `Animator`, `Rigidbody`, `CharacterCore`, `PlayerControllerReal` を `Start()` 時にキャッシュする構造へリファクタリング。
* **TitleManager.cs**
  * シーン遷移先をハードコード（`"GameScene"`）から `[SerializeField] private string gameSceneName;` に変更し、インスペクターから設定可能に。
  * ボタンのハイライト色をインスペクターから設定できるように変数を公開。
  * タイトル画面からゲームを終了するためのメソッド `OnClickQuitButton()` を追加。
* **DamageReceiver.cs**
  * `public int attackDamage` を `[SerializeField]` とプロパティ `AttackDamage` にカプセル化し、安全性を向上。
* **EnemySpawner.cs**
  * `DamageReceiver` のカプセル化に伴い、プロパティ `AttackDamage` 経由でダメージ量を設定するように参照を修正。

---

## 2026-07-08
- EnemySpawner.cs, GameManager.cs: 画面上のUI通知（ウェーブ開始、クリア、強化）を英語から日本語に変更。

## 2026-07-08
- DamageText.cs, DamageTextManager.cs: 敵およびプレイヤーがダメージを受けた際に数値がポップアップ表示されるシステムを追加。
- damage.cs: クリティカルヒットの確率と倍率のステータスを追加し、クリティカル判定を実装。
- PlayerStatus.cs: 被ダメージ軽減ステータスを追加し、ダメージ計算に反映。
- PowerUpManager.cs: パワーアップアイテムに「致命の刃（クリティカル率アップ）」と「鉄壁の守り（被ダメージ軽減）」を追加。

## 2026-07-08
- PlayerPowerUps.cs: 新規追加。プレイヤーが取得したレベル制パワーアップの現在レベルを管理。GameManagerにて自動付与。
- PowerUpManager.cs: 既存のアイテムを刷新。メモに記載されたレベルアップ上限付きのスキルセット（切断、爆発、遅延、吸収、ステータス強化など）を追加。
- EnemyStatus.cs: 属性攻撃（切断の継続ダメージ、遅延の速度低下）を付与する機能とコルーチンを追加。
- damage.cs (Damager): 攻撃ヒット時にレベルに応じて、吸収（HP回復）、切断・遅延の付与、爆発の範囲ダメージ判定を適用。
- PlayerStatus.cs / CharacterCore.cs: 体力上限アップと移動速度アップのレベル比例計算を追加。
- PowerUpManager.cs: すべてのパワーアップの最大レベルを3に変更。

## 2026-07-08
- PowerUpManager.cs: パワーアップ抽選時に「+Lv2」「+Lv3」が低確率で出現する仕組みを追加（通常時は2割/1割、タイピングゲーム後は4割/3割）。タイピング完全成功時は必ず1つはLv+3が出現するように変更。
- TypingManager.cs: タイピングゲーム終了時に完全成功かどうかを判定し、PowerUpManagerへフラグを渡す処理を追加。
- PlayerPowerUps.cs: 取得済みパワーアップとその現在レベルを画面左上にリスト表示するUIの自動生成機能を追加。

## 2026-07-08
- damage.cs: 近接武器のレベルアップによるダメージスケーリングが指数関数的（1, 5, 25, 125倍）だったものを、線形加算（+100%ごと。40, 80, 120, 160）になるように修正。
- EnemySpawner.cs: 敵のHP設定を、想定されるプレイヤーのレベル別攻撃力に合わせ、どのウェーブ（レベル）でも「プレイヤーの最大火力攻撃を4回当てれば倒せるHP」になるようロジックを調整。
- TypingManager.cs: 前半2文字と後半の文字パーツをランダムに組み合わせて2500種類の単語を生成するロジックを追加し、タイピングバリエーションを大幅に拡張。また、タイピングパネルの背景に暗幕（半透明ブラック）を自動で追加する処理を追加。
- PowerUpManager.cs: 未実装の「ライフルダメージ上昇」を選択肢から削除。
- DamageTextManager.cs, DamageText.cs: 毎フレームのInstantiate/Destroyによる負荷を避けるため、オブジェクトプールパターン（Object Pooling）を実装しパフォーマンスを最適化。
- CameraController.cs: ロックオン時の敵検索ロジックを `FindObjectsByType` から `Physics.OverlapSphere` に変更し、無駄な全探索を避けて軽量化。
- PlayerPowerUps.cs: レベルが3（上限）に達したスキルについて、「Lv3」の表記を赤い「MAX」の表記に変更するUI改修を実施。
