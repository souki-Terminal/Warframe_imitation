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

## 2026-07-09
- TypingManager.cs: `kanaToRomaji` ディクショナリに「ん」を追加し、KeyNotFoundExceptionを解消。
- EnemySpawner.cs: スポナーの子オブジェクトから自動的に敵を Instantiate する処理を削除し、手動で配置された子オブジェクトをそのまま使用するように変更。ウェーブクリア時のプレイヤー強化・回復処理を GameManager 側に移行。
- GameManager.cs: 最後のウェーブ終了時にタイピング選択画面をスキップしてクリア画面に遷移する処理を追加。また、ウェーブ移行時（ProceedToNextWave）にプレイヤーへの強化（回復、最大体力・攻撃力アップ）と通知表示を行うよう処理を修正。
- TypingManager.cs: `kanaToRomaji` ディクショナリに「ゃ」「ゅ」「ょ」「っ」などの捨て仮名（小文字）と長音符「ー」を追加し、出題単語にこれらが含まれる際のエラーを修正。
- EnemyStatus.cs: 切断ダメージなどノックバック方向ベクトルが 0 の攻撃を受けた際に、エラーログ（`Debug.LogError`）を出力しないように修正。

## 2026-07-09 (オープンキャンパス向け改修)
- GameManager.cs, TypingManager.cs: タイピング問題数を5問に削減。正解数に応じたパワーアップ選択肢増加機能を追加。Qキーによるパワーアップストック制を導入。
- OpenCampusSpawner.cs: 新規作成。練習ウェーブ（5体）と本番ウェーブ（時間差で計50体）を動的生成で進行するロジックを実装。

## 2026-07-13
- TypingManager.cs: タイピングゲームの出題内容をすべて魚の名前に変更。
- TypingWordProvider.cs: 新規作成。タイピングの単語リストを管理する専用クラス。動物や植物などの別カテゴリも追加しやすい設計にした。
- TypingManager.cs: 単語の生成を TypingWordProvider に委譲し、インスペクターから出題カテゴリを選択できるように修正。
- CharacterCore.cs: ノックバック時の「床抜け・壁抜け」とそれに伴うエラー（Failed to create agent）を防ぐため、NavMeshAgentを無効化せず `agent.Move()` を使用してノックバックするように修正。

## 2026-07-14
- OpenCampusSpawner.cs: `SpawnSingleEnemy` コルーチン内で、待機中に敵オブジェクトが破棄された場合に発生する `MissingReferenceException` を防ぐための `null` チェックを追加。

## 2026-07-16
- AudioManager.cs: �V�K�쐬�BBGM�ASE�A�{�C�X���ꌳ�Ǘ�����V���O���g���N���X�B
- TitleManager.cs, GameManager.cs: BGM�̍Đ��E��~�E�ꎞ��~�ASE�̍Đ�������ǉ��B
- PlayerControllerReal.cs, WeaponAnimationEvent.cs, PlayerStatus.cs, damage.cs: �v���C���[�̃W�����v�{�C�X�A���U��SE�A��e�E�U���q�b�gSE�A��e�{�C�X��ǉ��B
- TypingManager.cs: �^�C�s���O�����A�~�X�A���Ԑ؂ꎞ��SE��ǉ��B
- PowerUpManager.cs: �p���[�A�b�v�����l��������ю擾����SE��ǉ��B
- EnemyStatus.cs: �G���j����SE��ǉ��B
- CharacterCore.cs: �U�����ɃW�����v�{�C�X�i�A�N�V�����{�C�X�j���Đ����鏈����ǉ��B
- AudioManager.cs: BGM��1�b�ԃt�F�[�h�A�E�g�@�\��ǉ��B
- GameManager.cs, OpenCampusSpawner.cs: ���K�E�F�[�u���̓G���f�B���OBGM���Đ����A�{�ԃE�F�[�u�ڍs����1�b�����ăt�F�[�h�A�E�g�����Ă���{��BGM���Đ�����悤���W�b�N��ύX�B
- AudioSetupAuto.cs: �쐬�B���X�g�Ɋ�Â���BGM�ESE�E�{�C�X�����ׂĎ����ݒ肳�ꂽAudioManager��Prefab�𐶐�����G�f�B�^�g����ǉ��B
- AudioManager.cs: �Q�[���J�n���A�V�[�����AudioManager�����݂��Ȃ��ꍇ�Ɏ�����Prefab��ǂݍ���Ő������鏈����ǉ��B
- AudioManager.cs: �V�[������AudioListener�����݂��Ȃ��ꍇ�A�������g�Ɏ����ǉ����ăR���\�[���ւ̌x���X�p����h�~���鏈����ǉ��B
- AudioManager.cs: �V�[���ڍs����AudioListener���������������x����h�����߁A�V�[�����[�h����Listener�̐����Ď����Ď����������鏈����ǉ��B
- WeaponAnimationEvent.cs: �U���X�e�[�g�ȊO�ŃR���C�_�[��OFF�ɂȂ�ۂ̃��O�o�͂��R�����g�A�E�g���A�R���\�[���̃X�p����h�~�B
- EnemySpawner.cs, GameManager.cs: OpenCampusSpawner��EnemySpawner�������ɓ��삵�A�G�̈Ӑ}���Ȃ����ł�\�����ʃ^�C�~���O�ł̃E�F�[�u�N���A�i�����L���O�X�V�{�C�X�̍Đ��Ȃǁj�������N�����Ă��������o�O���C���BOpenCampusSpawner���ݎ��͌Â��X�|�i�[�������~�E��������悤�ɕύX�B
- TypingManager.cs: �^�C�s���O�̌��ʉ���1�������Ƃł͂Ȃ��A1�P����N���A�����ہA����ю��Ԑ؂�̍ۂɖ�悤�Ɏd�l�ύX�B

- 2026-07-17: 攻撃や爆発が同じ敵に多段ヒットし一度に消える現象を修正 (damage.cs, WeaponAnimationEvent.cs)
- 2026-07-17: インスペクターからBGM/SE/ボイスの音量を調節できるようAudioManagerを修正 (AudioManager.cs)
- 2026-07-17: 被ダメージ時のボイスが1秒以内に連続再生されないようクールダウンを追加 (AudioManager.cs)

- 2026-07-17: 攻撃も強化もしていないのに敵が消える現象を修正。ランダム配置時にNavMesh外へスポーンし、マップ外へ落下して即死ダメージ判定を受けていた不具合を修正 (EnemyStatus.cs, OpenCampusSpawner.cs)

- 2026-07-17: ユニティちゃんのジャンプ(univ0001,0002)、被ダメージ(univ1091-1095)、攻撃(univ1101,1102)の音声を指定のものに変更 (AudioManager.cs, WeaponAnimationEvent.cs, AudioSetupAuto.cs, UpdateVoicePrefabOnce.cs)

- 2026-07-17: 攻撃時のボイスが連続再生されないように被ダメージ時と同様に1秒間のクールダウンを追加 (AudioManager.cs)
- 2026-07-17: 練習ウェーブ開始時のBGMを本番ウェーブと同じものに変更し、本番移行時のBGM途切れを解消 (OpenCampusSpawner.cs)

- 2026-07-17: タイピングゲームの入力仕様を改善し、拗音（しゃ→sha）や促音（っか→kka）など、xやlを使用しない自然な2文字入力（ローマ字入力）に対応 (TypingManager.cs)

- 2026-07-17: �G���m�̕����Փ˂ɂ��Ӑ}���Ȃ��������ł̒e����΂��i���ŁE�e���|�[�g�o�O�j��h�����߁AEnemyStatus.cs �ɂēG���m�̓����蔻��𖳎����鏈����ǉ��B

- 2026-07-17: OpenCampusSpawner.cs �̃C���X�y�N�^�[�Ɂi�O���[�v�ł͂Ȃ��j�P�̂̓G�I�u�W�F�N�g�𒼐ڐݒ肵���ۂɁA�G�̒��g�i���b�V����{�[�����̎q�I�u�W�F�N�g�j�������E�o���o���ɂȂ��ăe���|�[�g���Ă��܂��v���I�ȃo�O���C���B

- 2026-07-17: OpenCampusSpawner.cs �������̃E�F�[�u�œ����G�I�u�W�F�N�g���Q�Ƃ��Ă����ꍇ�A���̃E�F�[�u�������ɖڂ̑O�̓G���˔@���[�v���āi�����āj�g���񂳂��s����C���B�C���X�y�N�^�[����w�肳�ꂽ�G��K�������iInstantiate�j���ďo��������悤�ɕύX�B

- 2026-07-17: �^�C�s���O�Q�[���̊g���B�J�e�S���Ɂu��؁v�u�����v�u��蕨�v��ǉ����A�v���C���ƂɃ����_���Ƀ��[�e�[�V��������@�\��ǉ��B�܂��A�v���C���[�̌��݂̋����񐔁i�S�p���[�A�b�v���x���̍��v�j�ɉ����āA�o�肳���P��̕��������X�P�[���i���x��1�F2?3�����A���x��2�F2?5�����A���x��3�F�����Ȃ��j�����Փx�����@�\�������B

- 2026-07-17: �W�����v���̃{�C�X���󒆂ŘA���Đ�����Ȃ��悤�ɏC���B��x�W�����v�{�C�X���Đ�����ƁA���n����܂ł͍ēx�Đ�����Ȃ��悤�ɐ����ǉ��i�����ɍU�����̃{�C�X��Ɨ������A�󒆍U���ł������o��悤�ɏC���j�B

- 2026-07-17: �^�C�s���O�Q�[���ŐL�΂��_�i�[ / �n�C�t���j�����͂Ƃ��Ď󂯕t����ꂸ�������Ȃ��o�O���C���B

- 2026-07-17: �v���C���[���m�b�N�o�b�N���ɕǂ𔲂��Ă��܂��s����C���B�ړ���ɕǂ����邩Raycast�Ŕ��肵�A�ǔ�����h��������ǉ��B
- 2026-07-17: �v���C���[�������iY���W��-10�ȉ��j���Ă��܂����ꍇ�̃t�F�C���Z�[�t�Ƃ��āA���W(0, 0, 10)�֎����I�Ƀ��[�v����@�\��ǉ��B
