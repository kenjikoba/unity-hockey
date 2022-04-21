以下、本プロジェクトのために追加・修正したファイル。

/*********************** <プロジェクトセッティング> *************************/

"ProjectSettings/*" : 反発係数のthresholdや衝突するレイヤーの管理

/*************************************************************************/

/************************** <シーンに関して> ******************************/

"Assets/Scenes/HockeyGame.unity" : この度作成したsceneファイル。Windows10,Ubuntu16.04上では動作確認済み。

/*************************************************************************/

/********************** <ホッケーのモデルに関して> *************************/

"Assets/Materials/*"                       : モデルの色を設定したファイル。
"Assets/Textures/*"                        ： パックの残像用のテクスチャファイル。
"Assets/Prefabs/HockeyPlayer.fbx"          : HockeyPlayerの3Dモデル。
"Assets/Prefabs/HockeyBoard.fbx"           : HockeyBoardの3Dモデル。
"Assets/StreamingAssets/ComputerBrains/*"  : 学習済みエージェントとの対戦用のファイル
/**************************************************************************/

/*************************** <スクリプトに関して> ***************************/

"Assets/Scripts/AI/"以下
  <NEW FILES>
  ・DEEnvironment.cs   : ニューロ進化(差分進化)の管理

  <MODIFIED FILES>
  ・NNBrain.cs                 :　DE(差分進化)の操作を追加

"Assets/Scripts/HockeyController/"以下
  <NEW FILES>
  ・HockeyAgent.cs             : エージェントの入力・ニューラルネットのモデル・報酬などを記述
  ・HockeyPlayer.cs            : HockeyAgentの行動を受け取り、実際にオブジェクトの移動をさせる
  ・ManualPlayer.cs            : WASDキーでのプレイヤーの制御、マウスでの操作.(タッチでの操作)
  ・ComputerPlayer.cs          : 学習済みエージェントのコントロールと強さ切り替え
  ・PackManager.cs             : Packの最大速度の制限や試合時間のコントロール
  ・GameModeManager.cs         : 学習モードなのか、対戦モードなのかをコントロール
  ・CheckBoxManager.cs         : モード切替、難易度切り替え、操作方法切り替え時のチェックボックスをコントロール

/***********************************************************************/
