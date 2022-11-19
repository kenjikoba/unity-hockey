// Q学習、基本的に変わりはEpisode以外あんまない。
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;


public class QEnvironment : Environment
{
//     // 加えた。
    /***** Num of Symaltaneously Playing Agents *****/
    [SerializeField] private int nAgents = 2;
    private int NAgents { get { return nAgents; } }

    [SerializeField] private GameObject scoreRecorderGameObject;
    // private GameObject scoreRecorderGameObject;
    private QScoreRecorder scoreRecorder;

    private float Gamma { get; set; } = 0.75f;

    private List<double> max_q_list { get; set; } = new List<double>();
    private List<double> diff_list { get; set; } = new List<double>();
    
    // /***** SerializeField for gameObjects *****/
    [Header("Agent Prefab"), SerializeField] private GameObject GObject1;
    [Header("Agent Prefab"), SerializeField] private GameObject GObject2;

    [SerializeField] private int actionSize = 9;
    private int ActionSize { get { return actionSize; } }

    [SerializeField] private int stateSize = 16384;
    private int StateSize { get { return stateSize; } }

    private QBrain QLBrain1;
    private QBrain QLBrain2;
    private HockeyAgent LearningAgent1;
    private HockeyAgent LearningAgent2;

    private int PrevState1;
    private int PrevState2;
    private int PrevAction1;
    private int PrevAction2;
    private int episodes1 = 0;
    private int episodes2 = 0;
    private int gamecount = 0;
    private int gamereset1 = 0;
    private int gamereset2 = 0;
    private int episode = 0;

    /***** Values for Record *****/
    private float BestRecord { get; set; }

    /***** Bool Objects For Snchronization With Opponent Player *****/
    // Getter and Setter should be written?
    public bool WaitingFlag = false;
    public bool RestartFlag = false;
    public bool ManualModeFlag = false;

    [Header("UI References"), SerializeField] private Text Qtext = null;
    
    /***** Property of GameObjects *****/
    private Text QText { get { return Qtext; } }

    /***** 学習が開始された際に呼ばれる *****/
    void Awake() {
        // 同時にプレイするプレーヤー数. 現在は2で固定.
        if (nAgents != 2) {
            Debug.Log("Now, nAgents must be equal to 2.");
            nAgents = 2;
        }    
        QLBrain1 = new QBrain(StateSize, ActionSize);
        QLBrain2 = new QBrain(StateSize, ActionSize);
        LearningAgent1 = GObject1.GetComponent<HockeyAgent>();
        LearningAgent2 = GObject2.GetComponent<HockeyAgent>();
        // scoreRecorder = scoreRecorderGameObject.GetComponent<ScoreRecorder>();
    }

    void Start() {
        PrevState1 = LearningAgent1.GetState();
        PrevState2 = LearningAgent2.GetState();
        scoreRecorder = scoreRecorderGameObject.GetComponent<QScoreRecorder>();
    }

    public void Inactivate() {
        ManualModeFlag = true;
        GObject1.SetActive(false);
        GObject2.SetActive(false);
    }

    public void Activate() {
        ManualModeFlag = false;
        GObject1.SetActive(true);
        GObject2.SetActive(true);
    }

    // Agentが切り替わる際にReset()が呼ばれる
    /***** Reset() Is Used When Agents Change *****/
    public void Reset() {
        WaitingFlag = false;
    }

    // Agentが変わらない場合はRestart()が呼ばれる
    /***** Restart() Is Used When Agents Don't Change *****/
    public void Restart() {
        RestartFlag = false;
        LearningAgent1.TimeUp = false;
        LearningAgent2.TimeUp = false;
        gamereset1 = 1;
        gamereset2 = 1;
        // AgentsSet.ForEach(p => { p.agent.TimeUp = false; });
    }

    private void FixedUpdate()
    {
        // Waiting, Restart, ManualModeのいずれかがtrueであれば学習を進めない
        if (WaitingFlag || RestartFlag || ManualModeFlag) {
            return;
        }

        // count += 1;

        // if (LearningAgent1.IsDone) {
        //     int r = (int)LearningAgent1.Reward;
        //     BestRecord = Mathf.Max(r, BestRecord);
        //     System.Console.WriteLine("Hello, World! ={0} \n",BestRecord.ToString());
        //     File.WriteAllText("./Assets/BestRecord.txt", BestRecord.ToString());
        //     QLBrain1.Save("./Assets/Q.txt");
        //     // WaitingFlag = true;
        // }
        if (LearningAgent1.TimeUp) {
            int r = (int)LearningAgent1.Reward;
            BestRecord = Mathf.Max(r, BestRecord);

            double average_max_q = max_q_list.Average();
            double average_diff = diff_list.Average();
            // 記録。
            scoreRecorder.QUpdateRecord(gamecount, episode, average_max_q, r, average_diff);
            max_q_list.Clear();
            diff_list.Clear();
            episode = 0;
            gamecount += 1;
            RestartFlag = true;

            // File.WriteAllText("./Assets/Q_BestRecord.txt", BestRecord.ToString());
            // QLBrain1.Save("./Assets/StreamingAssets/ComputerBrains/Q.txt");
            // int pastRecord = 0;
            // // インスタンスを作成，パスをコンストラクタに渡す
            // using (var objReader = new StreamReader("./Assets/BestRecord.txt")) {
            //     string pastRead = objReader.ReadToEnd();
            //     pastRecord = int.Parse(pastRead);
            // }
            // if (pastRecord < BestRecord) {
            //     File.WriteAllText("./Assets/BestRecord.txt", BestRecord.ToString());
            //     QLBrain1.Save("./Assets/StreamingAssets/ComputerBrains/Q.txt");
            // }
            UpdateText();
            // LearningAgent1.SetReward(0);
        }
        // if (LearningAgent2.IsDone) {
        //     int r = (int)LearningAgent2.Reward;
        //     BestRecord = Mathf.Max(r, BestRecord);
        //     System.Console.WriteLine("Hello, World! ={0} \n",BestRecord.ToString());
        //     File.WriteAllText("./Assets/BestRecord.txt", BestRecord.ToString());
        //     QLBrain2.Save("./Assets/Q.txt");
        //     // WaitingFlag = true;
        // }
        if (LearningAgent2.TimeUp) {
            RestartFlag = true;
            int r = (int)LearningAgent2.Reward;
            BestRecord = Mathf.Max(r, BestRecord);
            
            // File.WriteAllText("./Assets/Q_BestRecord.txt", BestRecord.ToString());
            // QLBrain2.Save("./Assets/StreamingAssets/ComputerBrains/Q.txt");
            // int pastRecord = 0;
            // // インスタンスを作成，パスをコンストラクタに渡す
            // using (var objReader = new StreamReader("./Assets/BestRecord.txt")) {
            //     string pastRead = objReader.ReadToEnd();
            //     pastRecord = int.Parse(pastRead);
            // }
            // if (pastRecord < BestRecord) {
            //     File.WriteAllText("./Assets/BestRecord.txt", BestRecord.ToString());
            //     QLBrain2.Save("./Assets/StreamingAssets/ComputerBrains/Q.txt");
            // }
            UpdateText();
            // LearningAgent2.SetReward(0);
        }

        AgentUpdate(LearningAgent1, QLBrain1,1);
        if (LearningAgent1.IsDone)
        {
            // gamecount += 1;
            LearningAgent1.Reset();
        }
        AgentUpdate(LearningAgent2, QLBrain2,2);
        if (LearningAgent2.IsDone)
        {
            // gamecount += 1;
            LearningAgent2.Reset();
        }
    }

    private void AgentUpdate(Agent a, QBrain b, int num)
    {
        if (num == 1) {
            episodes1 += 1;
            episode += 1;
            // 1ステップ前の行動によって得た結果の取得．
            int currentState1 = a.GetState();
            float reward = a.Reward;
            bool isDone = a.IsDone;

            // 1ステップ前の行動とその結果を用いてQテーブルを更新する．
            // リセット直後は直前の行動が使えないため更新しない．
            if(gamereset1 != 1 && episodes1 != 1)
            {
                b.UpdateTable(PrevState1, currentState1, PrevAction1, reward, isDone);
            }
            if (LearningAgent1.TimeUp) {
                LearningAgent1.SetReward(0);
            }
            // 現在の状態からとる行動を決定する．
            int actionNo = b.GetAction(currentState1);
            double[] action = a.DQNActionNumberToVectorAction(actionNo);
            a.AgentAction(action);

            double max_q = b.GetMaxQ(currentState1);
            max_q_list.Add(max_q);
            double q = b.GetQ(PrevState1,PrevAction1);
            double diff = reward + Gamma * max_q - q;
            diff_list.Add(diff);


            // 状態と行動を記録する．
            gamereset1 = 0;

            PrevState1 = currentState1;
            PrevAction1 = actionNo;
        }
        else {
            episodes2 += 1;
            // 1ステップ前の行動によって得た結果の取得．
            int currentState2 = a.GetState();
            float reward = a.Reward;
            bool isDone = a.IsDone;

            // 1ステップ前の行動とその結果を用いてQテーブルを更新する．
            // リセット直後は直前の行動が使えないため更新しない．
            if(gamereset2 != 1 && episodes2 != 1)
            {
                b.UpdateTable(PrevState2, currentState2, PrevAction2, reward, isDone);
            }
            if (LearningAgent2.TimeUp) {
                LearningAgent2.SetReward(0);
            }
            // 現在の状態からとる行動を決定する．
            int actionNo = b.GetAction(currentState2);
            double[] action = a.DQNActionNumberToVectorAction(actionNo);
            a.AgentAction(action);

            // 状態と行動を記録する．
            gamereset2 = 0;

            PrevState2 = currentState2;
            PrevAction2 = actionNo;

        }
    }

    private void UpdateText() {
        QText.text = "Best Record: " + BestRecord +
        "\nEpisode: " + episodes1 + "\nGamecount: " + gamecount; 
    }
}