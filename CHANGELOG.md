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
