# エアホッケー

## 概要
エアホッケーでは, スマッシャーと呼ばれる器具を用いて盤上のパックを打ち合い, 相手のゴールに入れて得点を競い合います. 今回はAIにスマッシャーのコントールの仕方を学ばせます. \
配布されているプログラムでは, 自分の位置, パックの位置, 敵の位置を入力とし, 自分の動作(速度ベクトル)を出力としたニューラルネットワークを学習させています.

https://user-images.githubusercontent.com/56354049/165915991-cee2ef8c-0148-4fc0-a0cc-c30a903256e6.mov

## ダウンロード
```shell
$ git clone https://github.com/trombiano1/airhockey.proj.git
```
で適当なフォルダにcloneした後、Unity HubのOpenから`airhockey.proj`を選択し開いてください. GitHubにはライブラリはのせていませんが, Unityで開く際に自動的に生成されます.

## ソースコードの概要
Sceneフォルダ内のHockeyGameを動かすことで学習が始まります. ManualPlayがオフであればPlayer1とPlayer2が対決し学習します. ManualPlayをオンにすると, その時点までに学習したBrainが制御するComputerPlayerとプレーヤーがキーボードなどで操作するManualPlayerが対決します.

自動運転プログラムなどと同様にAgent, NNBrain, NNEnvironment の3つのクラスで基本的に構成されています. Agentが観測できる状態は自分の位置, 自分とパックの差ベクトル, 敵の位置です．Agentは速度ベクトルで動かし方を指示します. 

時間制限を超えるか, パックがゴールに入るとゲームオーバーとなりリセットされます. 

- `/Assets/Scripts/HockeyController/HockeyAgent.cs` \
  環境を観測し, 必要な情報を取得します. また, actionをBrainから受け取ってHockeyPlayer.csに渡します.
- `/Assets/Scripts/HockeyController/HockeyPlayer.cs`\
actionを受け取り, 実際にパックを移動させます.
- `/Assets/Scripts/AI/NNBrain.cs`, `QBrain.cs`\
Agentの状態を入力として受け取り, 行動を出力します. 
- `/Assets/Scripts/AI/NEEnvironment.cs`, `/Assets/Scripts/AI/QEnvironment.cs`\
AgentとBrainを管理し, 一定期間でAgentとBrainを更新します.
- `/Assets/Scripts/HockeyPlayer/ManualPlayer.cs`\
ManualPlayの時にのみ使われます. キーボードやマウスからの入力に従ってプレーヤーを動かします.
- `/Assets/Scripts/HockeyPlayer/ComputerPlayer.cs`\
ManualPlayの時にのみ使われます. それまでに学習したBrainのデータを使ってプレーヤーと対戦します.

## 困ったら
- 再生ボタンを押しても動かない
  - Agent Speedが0になっているかもしれません. 少し上げてみてください.
- ManualPlayボタンが押せない
  - 学習を始めてしばらく経ってから押してみてください.
- ManualPlayでパックを動かせない
  - 使用できるキーは矢印キーではなくWASDです.
