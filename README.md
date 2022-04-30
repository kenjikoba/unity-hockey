# エアホッケー

## 概要
エアホッケーでは, スマッシャーと呼ばれる器具を用いて盤上のパックを打ち合い, 相手のゴールに入れて得点を競い合います. 今回はAIにスマッシャーのコントールの仕方を学ばせます. \
配布されているプログラムでは, 自分の位置, パックの位置, 敵の位置を入力とし, 自分の動作(速度ベクトル)を出力とし, 差分進化で学習します.

<img src="https://user-images.githubusercontent.com/56354049/165917364-f45a6abc-6c44-466e-93f5-cc0fff05c314.gif" alt="drawing" width="500"/>


## ダウンロード
```shell
$ git clone https://github.com/trombiano1/airhockey.proj.git
```
で適当なフォルダにcloneした後、Unity HubのOpenから`airhockey.proj`を選択し開いてください. GitHubにはUnityのライブラリは載せていませんが, Unityで開く際に自動生成されます.

## アルゴリズムの説明
差分進化(Differential Evolution)は1995年に発表されたメタヒューリスティクスであり, 最適化手法の中でも強力な方法のひとつとして知られています. 差分進化では以下のようなアルゴリズムを用います.

0. 個体をランダムな値で初期化します
1. **Mutation**\
   親となる個体x1をランダムに選びます. また, 個体をもう2つランダムに選んで x2, x3 とします
2. **Crossover**\
   差分ベクトル F * (x3 - x2) を計算し, 親 x1 にこの差分ベクトルを足したものを子供とします(Fはスケーリングパラメータと呼ばれる定数です)

   1.と2.を繰り返し, 決められた個数の子供を生成します
   
3. **Selection**\
   個体の親と子を比較し, 優れている方を残します

4. 1.~3.を繰り返します

スケーリングパラメータFが大きいと大ぶりな探索となり, 探索は早くなりますが収束が不安定になります. 差分進化について詳しくは参考文献などを参照してください.

## ソースコードの概要
このプログラムでは個体それぞれが行動を決定するニューラルネットワーク(NN)を持っています. また, そのNNは1列の実数の配列(DNA)としても保存されています. このDNAをベクトルとして**Crossover**などの演算をしています.

<img src="Pictures/nndna.png" width="300">

また, アルゴリズムの**Selection**では個体同士を比較する必要があります. このプログラムでは世代を生成した後に個体を2つずつ対戦させ, その成績に応じて固体に報酬を与えます. 全ての個体に報酬を与えた後, その報酬の大小で個体の優劣を決めています.


差分進化の管理は`DEEnvironment.cs`が行っています. 

`DEEnvironment.cs`の`FixedUpdate()`はまず100個の個体を生成し, 2個ずつ対戦させます.

### 対戦
`DEEnvironment`は`HockeyAgent`を呼んでボードの状態やプレーヤーの位置などの情報を取得してもらい, 情報を`NNBrain`に送ります.　

`NNBrain`は個体のNNとボードの状況を受け取り, そのNNを使って次に取るべき行動`action`を計算して返します.

`DEEnvironment`は返ってきた`action`を`HockeyAgent`に送り, 実際にその行動を取るように指示します.

`HockeyAgent`は受け取った`action`を`HockeyPlayer`に送り, 実際にUnity上でプレーヤーの位置を移動させます.

最後に, `HockeyAgent`はボードの状態をもとに個体に対して報酬を与えます. 報酬の値は現在以下のように決まっています.

- 1フレームごとに, パックの正面にいればいるほど報酬を追加します
- ゴールを決めると大きな報酬(1000)を足します
- ゴールを決められると大きな報酬(1000)を引きます

この操作を繰り返すことで対戦が進んでいきます. 時間切れになると対戦が止まり, 報酬値が決まります.

<img src="Pictures/vs.png" width="600">

- `NNBrain.cs`\
  **Crossover**で


NNBrainがその個体のNNと現在のボードの状態をもとに次の行動を決めます. ゲームの勝敗などに応じて個体に与える報酬が定められ, その値に基づき4.の評価が行われて次の世代が生成されます.

交叉率`crossrate`が定められており, その割合の子供が2.で計算された新しい遺伝子を持ちます. その他の子供は親の遺伝子をそのまま引き継ぎます.

2.の計算と4.の比較は`NNBrain`が行っており, 残りの操作は`DEEnvironment`が担当しています. また, スケーリングパラメータは`mutationScalingFactor`という値として`DEEnvironment.cs`内で定義されています.

Sceneフォルダ内のHockeyGameを開き, 画面上部の再生ボタンを押すと学習が始まります. 再生中にはGame画面に表示されるスライダでプログラムの実行速度を調整できます. コンピュータへの負荷を少なくしたい場合は、描画をオフにすることもできます。

<img src="Pictures/gameinterface.png" alt="gameinterface" width="400"/>

ManualPlayがオフであればPlayer1とPlayer2が対決しそれぞれが学習します(Populationが2ずつ増えるのはこれが理由です). ManualPlayをオンにすると, その時点までに学習したBrainが制御するComputerPlayerとプレーヤーがキーボードなどで操作するManualPlayerが対決します.

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
