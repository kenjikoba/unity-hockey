using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HockeyAgent : Agent
{
    // Playerの動かすオブジェクト、相手Playerのオブジェクト
    public GameObject Pack;
    public GameObject Opponent;
    
    // フィールドの上部にいるか、下部にいるか
    private int ModeSign;
    public float maxSpeed = 2.0f;

    // 時間制限、現在の時間、時間切れ判定変数、ゲームの状態
    public float maxBattleTime = 40;
    public float BattleTime { get; set; }
    public bool TimeUp = false;
    public string gameState = "onPlaying";
    private int GoalCounter { get; set; }

    // パックに当たったか、経過後のカウント
    private bool HitPack = false;
    private int HitPackCounter = 10;
    

    // Playerコントローラ、パックのコントローラの取得
    private HockeyPlayer PlayerController;
    private PackManager PackManager;

    // 初期状態を保持しておくための変数
    private Vector3 StartPosition { get; set; }
    private Vector3 PackStartPosition { get; set; }
    private Vector3 StartInertia { get; set; }
    private Vector3 PackStartInertia { get; set; }

    // PackのVecocity取得用のRigidbody
    public Rigidbody pack_rb { get; set; }
    private int count;

    public float PlayermaxSpeed = 0.1f;

    private void Awake() {
        // Playerの制御コントローラーを取得
        PlayerController = GetComponent<HockeyPlayer>();
        pack_rb = Pack.GetComponent<Rigidbody>();
        count = 0;
        PackManager = Pack.GetComponent<PackManager>();
    }

    // 開始時に呼び出される初期化処理
    private void Start() {
        //初期位置の登録
        StartPosition = transform.position;
        PackStartPosition = Pack.transform.position;
        if (transform.position.z > 0) {
            ModeSign = -1;
        } else {
            ModeSign = 1;
        }
    }

    // 初期状態に戻す
    public override void AgentReset() {
        PlayerController.Reset();
        transform.position = StartPosition;
        BattleTime = 0;
        PackManager.ResetTime();
        TimeUp = false;
    }

    // 変更というか書いた。
    public override int GetState() {
        var stateDivide = 4;
        count += 1;
        var observations = QCollectObservations();
        var r = 0;
        for(int i = 0; i < observations.Count; i++) { // 0とstateDivide-1の何パーのものを返す。
            var v = Mathf.FloorToInt(Mathf.Lerp(0, stateDivide - 1, (float)observations[i]));
            if(observations[i] >= 0.99f) {
                v = stateDivide - 1;
            }
            r += (int)(v * Mathf.Pow(stateDivide, i));
        }
        return r; 
        // throw new NotImplementedException();
    }
    
    // 変更というか書いた。
    public override double[] ActionNumberToVectorAction(int ActionNumber) {
        var action = new double[2]; 
        var xAxis = 0.0d;
        var yAxis = 0.0d;   // 原点と、原点中心で各頂点に距離１の正八角形にした。に0.3かけた
        if(ActionNumber % 6 == 1) {
            xAxis = 1.0d * 0.3;
            yAxis = 0.0d;
        }
        else if(ActionNumber % 6 == 2) {
            xAxis = 0.3d * 0.3;
            yAxis = 0.9d * 0.3;
        }
        else if(ActionNumber % 6 == 3) {
            xAxis = -0.8d * 0.3;
            yAxis = 0.6d * 0.3;
        }
        else if(ActionNumber % 6 == 4) {
            xAxis = -0.8d * 0.3;
            yAxis = -0.6d * 0.3;
        }
        else if(ActionNumber % 6 == 5) {
            xAxis = 0.3d * 0.3;
            yAxis = -0.9d * 0.3;
        }

        action[0] = xAxis;
        action[1] = yAxis;
        return action;
        // throw new NotImplementedException();
    }

    // 変更というか書いた。
    public override double[] DQNActionNumberToVectorAction(int ActionNumber) {
        var action = new double[2]; 
        var xAxis = 0.0d;
        var yAxis = 0.0d;   // 原点と、原点中心で各頂点に距離１の正五角形にした。に0.7かけた
        if(ActionNumber % 9 == 1) {
            xAxis = 1.0d * 0.3;
            yAxis = 0.0d;
        }
        else if(ActionNumber % 9 == 2) {
            xAxis = 0.7d * 0.3;
            yAxis = 0.7d * 0.3;
        }
        else if(ActionNumber % 9 == 3) {
            xAxis = 0.0d;
            yAxis = 1.0d * 0.3;
        }
        else if(ActionNumber % 9 == 4) {
            xAxis = -0.7d * 0.3;
            yAxis = 0.7d * 0.3;
        }
        else if(ActionNumber % 9 == 5) {
            xAxis = -1.0d * 0.3;
            yAxis = 0.0d;
        }
        else if(ActionNumber % 9 == 6) {
            xAxis = -0.7d * 0.3;
            yAxis = -0.7d * 0.3;
        }
        else if(ActionNumber % 9 == 7) {
            xAxis = 0.0d;
            yAxis = -1.0d * 0.3;
        }
        else if(ActionNumber % 9 == 8) {
            xAxis = 0.7d * 0.3;
            yAxis = -0.7d * 0.3;
        }

        action[0] = xAxis;
        action[1] = yAxis;
        return action;
        // throw new NotImplementedException();
    }

    // Agentへの入力を集める
    public override List<double> DECollectObservations() {
        var observations = new List<double>();
        double scalingFactor = 10f;

        var pos = transform.position;
        var pack_pos = Pack.transform.position;
        var opponent_pos = Opponent.transform.position;

        observations.Add(pos.x*scalingFactor);
        observations.Add(pos.z*scalingFactor*ModeSign);
        observations.Add((pos.x-pack_pos.x)*scalingFactor);
        observations.Add((pos.z-pack_pos.z)*scalingFactor*ModeSign);
        observations.Add(opponent_pos.x*scalingFactor);
        observations.Add(opponent_pos.z*scalingFactor*ModeSign);
        return observations;
    }

    // Agentへの入力を集める
    public override List<double> QCollectObservations() {
        var observations = new List<double>();
        // double scalingFactor = 10f; // 10.0のこと。

        var pos = transform.position;
        var pack_pos = Pack.transform.position;
        var opponent_pos = Opponent.transform.position;
        var pack_velocity = pack_rb.velocity;

        observations.Add(pos.x +0.5);  //自分の横の座標,-0.5<x<0.5
        observations.Add((pos.z*ModeSign)*2+1.70); //自分の縦の座標-0.5<y<0.5
        observations.Add(pack_pos.x + 0.5); //パックの横の座標
        observations.Add(pack_pos.z*ModeSign / 2 + 0.5); //パックの縦の座標
        // observations.Add(opponent_pos.x * 2);  //相手の横の座標
        // observations.Add((opponent_pos.z*ModeSign)*4-2.40); //相手の縦の座標
        observations.Add((pack_velocity.x / maxSpeed) / 2 + 0.5);
        observations.Add((pack_velocity.z / maxSpeed) / 2 + 0.5);

        // observations.Add(pos.x+0.5);  //自分の横の座標,-0.5<x<0.5
        // observations.Add((pos.z*ModeSign)*2+1.70); //自分の縦の座標-0.5<y<0.5
        // observations.Add(pack_pos.x+0.5); //パックの横の座標
        // observations.Add(pack_pos.z*ModeSign/2+0.5); //パックの縦の座標
        // observations.Add(opponent_pos.x+0.5);  //相手の横の座標
        // observations.Add((opponent_pos.z*ModeSign)*2+1.70); //相手の縦の座標
        // observations.Add((pack_velocity.x / PlayermaxSpeed) / 2 + 0.5);
        // observations.Add((pack_velocity.z / PlayermaxSpeed) / 2 + 0.5);

        return observations;
    }

    // Agentへの入力を集める
    public override List<double> DQNCollectObservations() {
        var observations = new List<double>();
        // double scalingFactor = 10f; // 10.0のこと。

        var pos = transform.position;
        var pack_pos = Pack.transform.position;
        var opponent_pos = Opponent.transform.position;
        var pack_velocity = pack_rb.velocity;
        // 全て-1~1に正規化しておきたい。
        observations.Add(pos.x * 2);  //自分の横の座標,-0.5<x<0.5
        observations.Add((pos.z*ModeSign)*4+2.40); //自分の縦の座標-0.5<y<0.5
        observations.Add(pack_pos.x * 2); //パックの横の座標
        observations.Add(pack_pos.z*ModeSign); //パックの縦の座標
        observations.Add(opponent_pos.x * 2);  //相手の横の座標
        observations.Add((opponent_pos.z*ModeSign)*4-2.40); //相手の縦の座標
        observations.Add((pack_velocity.x / maxSpeed));
        observations.Add((pack_velocity.z / maxSpeed));

        return observations;
    }

    public override void Stop() {
        PlayerController.Stop();
    }
    
    // actionを受け取り、プレーヤーを動かし、報酬をセットする
    public override void AgentAction(double[] action) {
        // 時間切れなら何もしない
        if (TimeUp) { return; }
        // コントローラーにActionを渡す
        action[1] *= ModeSign;
        PlayerController.Move(action);
        
        // 時間を更新
        BattleTime += Time.fixedDeltaTime;

        // パックの正面にいればいるほど報酬を追加
        AddReward(1-Mathf.Abs(Pack.transform.position.x - transform.position.x));

        // 動かないほど報酬を追加
        if (action[1] == 0.0) {
            if (action[0] == 0.0) {
                if (count <= 5000000) {
                    AddReward(1);
                }
            }
        }
       
        // ゴールを決めるとプラスの報酬、Player1がModeSign=1
        if ((ModeSign == 1 && Pack.transform.position.z > 1.03f) || (ModeSign == -1 && Pack.transform.position.z < -1.03f) ) {
            GoalCounter++;
            AddReward(1000);
            AgentReset();
            TimeUp = true;
            gameState = "GetPoint";
            return;    
        }
        // ゴールを決められるとマイナスの報酬
        if ((ModeSign == 1 && Pack.transform.position.z < -1.03f) || (ModeSign == -1 && Pack.transform.position.z > 1.03f) ) {
            GoalCounter++;
            AddReward(-1000);
            AgentReset();
            TimeUp = true;
            gameState = "LosePoint";
            return;
        }
        
        // パックが自分の後ろにいたらマイナスの報酬
        if ((ModeSign == 1 && Pack.transform.position.z < transform.position.z) || (ModeSign == -1 && Pack.transform.position.z > transform.position.z)) {
            AddReward(-1);
        }

        // パックを押し出すことへの報酬
        if ((HitPack && ModeSign == 1 && Pack.transform.position.z >= transform.position.z) || (HitPack && ModeSign == 1 && Pack.transform.position.z <= transform.position.z)) {
            AddReward(pack_rb.velocity.z * ModeSign * 1);
            HitPackCounter--;
        }
        // //　パックを打ったけど自分の後ろに撃ったらマイナスの報酬
        // if ((HitPack && ModeSign == 1 && Pack.transform.position.z < transform.position.z) || (HitPack && ModeSign == 1 && Pack.transform.position.z > transform.position.z)) {
        //     AddReward(pack_rb.velocity.z * ModeSign * (-1));
        //     HitPackCounter--;
        // }

        if (HitPackCounter == 0) {
            HitPack = false;
            HitPackCounter = 10;
        }

        // 時間切れ判定
        if(BattleTime > maxBattleTime) {
            GoalCounter++;
            AgentReset();
            TimeUp = true;
            return;
        }

        // 試合終了判定
        if (GoalCounter >= 7) {
            AgentReset();
            TimeUp = true;
            Done();
            GoalCounter = 0;
            return;
        }
    }

    // actionを受け取り、プレーヤーを動かし、報酬をセットする
    public override void DQNAgentAction(double[] action) {
        // 時間切れなら何もしない
        if (TimeUp) { return; }
        // コントローラーにActionを渡す
        action[1] *= ModeSign;
        PlayerController.Move(action);
        
        // 時間を更新
        BattleTime += Time.fixedDeltaTime;

        // パックの正面にいればいるほど報酬を追加
        // AddReward(1-Mathf.Abs(Pack.transform.position.x - transform.position.x));

        // // 動かないほど報酬を追加
        // if (action[1] == 0.0) {
        //     if (action[0] == 0.0) {
        //         if (count <= 1000) {
        //             AddReward(1);
        //         }
        //     }
        // }
       
        // ゴールを決めるとプラスの報酬、Player1がModeSign=1
        if ((ModeSign == 1 && Pack.transform.position.z > 1.03f) || (ModeSign == -1 && Pack.transform.position.z < -1.03f) ) {
            GoalCounter++;
            AddReward(1);
            AgentReset();
            TimeUp = true;
            gameState = "GetPoint";
            return;    
        }
        // ゴールを決められるとマイナスの報酬
        if ((ModeSign == 1 && Pack.transform.position.z < -1.03f) || (ModeSign == -1 && Pack.transform.position.z > 1.03f) ) {
            GoalCounter++;
            AddReward(-1);
            AgentReset();
            TimeUp = true;
            gameState = "LosePoint";
            return;
        }
        
        // // パックが自分の後ろにいたらマイナスの報酬
        // if ((ModeSign == 1 && Pack.transform.position.z < transform.position.z) || (ModeSign == -1 && Pack.transform.position.z > transform.position.z)) {
        //     AddReward(-1);
        // }

        // // パックを押し出すことへの報酬
        // if ((HitPack && ModeSign == 1 && Pack.transform.position.z >= transform.position.z) || (HitPack && ModeSign == 1 && Pack.transform.position.z <= transform.position.z)) {
        //     AddReward(pack_rb.velocity.z * ModeSign * 1);
        //     HitPackCounter--;
        // }
        // //　パックを打ったけど自分の後ろに撃ったらマイナスの報酬
        // if ((HitPack && ModeSign == 1 && Pack.transform.position.z < transform.position.z) || (HitPack && ModeSign == 1 && Pack.transform.position.z > transform.position.z)) {
        //     AddReward(pack_rb.velocity.z * ModeSign * (-1));
        //     HitPackCounter--;
        // }

        if (HitPackCounter == 0) {
            HitPack = false;
            HitPackCounter = 10;
        }

        // 時間切れ判定
        if(BattleTime > maxBattleTime) {
            GoalCounter++;
            AgentReset();
            TimeUp = true;
            return;
        }

        // // 試合終了判定
        // if (GoalCounter >= 7) {
        //     AgentReset();
        //     TimeUp = true;
        //     Done();
        //     GoalCounter = 0;
        //     return;
        // }
    }

    public void OnCollisionStay(Collision collision) {
        // 壁との衝突はマイナス報酬
        if (collision.gameObject.tag == "wall") {
            AddReward(-5);
        } else if (collision.gameObject.tag == "Pack") {
            HitPack = true;
        }
    }
}
