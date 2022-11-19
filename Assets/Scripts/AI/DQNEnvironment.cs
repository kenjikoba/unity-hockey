// Q学習、基本的に変わりはEpisode以外あんまない。
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;


public class DQNEnvironment : Environment
{
//     // 加えた。
    /***** Num of Symaltaneously Playing Agents *****/
    [SerializeField] private int nAgents = 2;
    private int NAgents { get { return nAgents; } }

    [SerializeField] private GameObject scoreRecorderGameObject;
    // private GameObject scoreRecorderGameObject;
    private DQNScoreRecorder scoreRecorder;

    private List<double> max_q_list { get; set; } = new List<double>();
    private List<double> diff_list { get; set; } = new List<double>();
    
    // /***** SerializeField for gameObjects *****/
    [Header("Agent Prefab"), SerializeField] private GameObject GObject1;
    [Header("Agent Prefab"), SerializeField] private GameObject GObject2;

    [SerializeField] private int actionSize = 9;
    private int ActionSize { get { return actionSize; } }

    [SerializeField] private int stateSize = 8;
    private int StateSize { get { return stateSize; } }

    private DQNBrain DQNBrain1;
    private DQNBrain DQNBrain2;
    private HockeyAgent LearningAgent1;
    private HockeyAgent LearningAgent2;
    public ReplayMemory replaymemory1;
    public ReplayMemory replaymemory2;

    private List<double> CurrentState1;
    private List<double> CurrentState2;
    private int PrevAction1 { get; set; }
    private int PrevAction2 { get; set; }
    private List<double> PrevState1 { get; set; }
    private List<double> PrevState2 { get; set; }

    /***** Values for Record *****/
    private float BestRecord { get; set; }
    private int gamecount = 1;
    private int gamereset1 = 0;
    private int gamereset2 = 0;
    // private int count;

    /***** Bool Objects For Snchronization With Opponent Player *****/
    // Getter and Setter should be written?
    public bool WaitingFlag = false;
    public bool RestartFlag = false;
    public bool ManualModeFlag = false;

    private int CAPACITY = 1000; // メモリの最大長さ
    private int episodes1 = 0;
    private int episodes2 = 0;
    private int episode = 0;
    private int NUM_EPISODES = 300;  // 最大試行回数

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
        
        DQNBrain1 = new DQNBrain(StateSize, ActionSize);
        DQNBrain2 = new DQNBrain(StateSize, ActionSize);
        LearningAgent1 = GObject1.GetComponent<HockeyAgent>();
        LearningAgent2 = GObject2.GetComponent<HockeyAgent>();
        replaymemory1.index = 0;
        replaymemory1.capacity = CAPACITY;
        replaymemory1.memory = new List<object[]>();
        replaymemory2.index = 0;
        replaymemory2.capacity = CAPACITY;
        replaymemory2.memory = new List<object[]>();
        // if (episodes1 == 0) {
        //     using (var objReader = new StreamReader("./Assets/Resources/DQNBestRecord.txt")) {
        //         string pastRead = objReader.ReadToEnd();
        //         BestRecord = int.Parse(pastRead);
        //     }
        // }
    }

    void Start() {
        CurrentState1 = LearningAgent1.DQNCollectObservations();
        CurrentState2 = LearningAgent2.DQNCollectObservations();
        scoreRecorder = scoreRecorderGameObject.GetComponent<DQNScoreRecorder>();
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
    
        if (LearningAgent1.TimeUp) {
            float reward1 = LearningAgent1.Reward;
            double average_max_q = max_q_list.Average();
            double average_diff = diff_list.Average();
            // // 記録。
            scoreRecorder.DQNUpdateRecord(gamecount, episode, average_max_q, reward1, average_diff);
            max_q_list.Clear();
            diff_list.Clear();
            episode = 0;

            gamecount += 1;
            RestartFlag = true;
            WaitingFlag = true;
            // float reward1 = LearningAgent1.Reward;
            BestRecord = Mathf.Max(reward1, BestRecord);

            DQNBrain1.Qnn.Save("./Assets/Resources/DQN1.txt"); 
            DQNBrain1.Targetnn.Save("./Assets/Resources/DQN2.txt");
            UpdateText();
        }
        // if (LearningAgent2.IsDone) {
        //     int r = (int)LearningAgent2.Reward;
        //     BestRecord = Mathf.Max(r, BestRecord);
        //     System.Console.WriteLine("Hello, World! ={0} \n",BestRecord.ToString());
        //     File.WriteAllText("./Assets/BestRecord.txt", BestRecord.ToString());
        //     DQNBrain2.Save("./Assets/Q.txt");
        //     // WaitingFlag = true;
        // }
        if (LearningAgent2.TimeUp) {
            RestartFlag = true;
            WaitingFlag = true;
            float reward2 = LearningAgent2.Reward;
            BestRecord = Mathf.Max(reward2, BestRecord);
            
            // int pastRecord = 0;
            // // インスタンスを作成，パスをコンストラクタに渡す
            // using (var objReader = new StreamReader("./Assets/Resources/DQNBestRecord_new.txt")) {
            //     string pastRead = objReader.ReadToEnd();
            //     pastRecord = int.Parse(pastRead);
            // }
            // if (pastRecord < BestRecord) {
            //     File.WriteAllText("./Assets/Resources/DQNBestRecord_new.txt", BestRecord.ToString());
            //     // DQNBrain2.Qnn.Save("./Assets/Resources/DQNtest3.txt"); 
            //     // DQNBrain2.Targetnn.Save("./Assets/Resources/DQNtest4.txt");
            // }
            DQNBrain2.Qnn.Save("./Assets/Resources/DQN3.txt"); 
            DQNBrain2.Targetnn.Save("./Assets/Resources/DQN4.txt");
            UpdateText();
        }

        AgentUpdate(LearningAgent1, DQNBrain1,1);
        if (LearningAgent1.IsDone)
        {
            LearningAgent1.Reset();
        }
        AgentUpdate(LearningAgent2, DQNBrain2,2);
        if (LearningAgent2.IsDone)
        {
            LearningAgent2.Reset();
        }
    }

    private void AgentUpdate(Agent a, DQNBrain b, int num)
    {
        if (num == 1) {
            episode += 1;
            episodes1 += 1;
            // エージェントの状態（座標など）の取得
            CurrentState1 = a.DQNCollectObservations();
            if(gamereset1 != 1 && episodes1 != 1)
            {
                if (LearningAgent1.TimeUp) {
                    b.PushToMemory(replaymemory1, PrevState1, PrevAction1, CurrentState1, a.Reward, LearningAgent1.TimeUp);
                }
                else {
                    b.PushToMemory(replaymemory1, PrevState1, PrevAction1, CurrentState1, 0, LearningAgent1.TimeUp);
                }
                // b.PushToMemory(replaymemory1, PrevState1, PrevAction1, CurrentState1, a.Reward, q);
                // b.PushToMemory(replaymemory1, PrevState1, PrevAction1, CurrentState1, reward1);
            }
            if (LearningAgent1.TimeUp) {
                LearningAgent1.SetReward(0);
            }
            double[] q = b.GetQusingQnn(CurrentState1.ToArray()); 
            double MaxQ = q.Max();

            max_q_list.Add(MaxQ);

            int actionnum = b.GetAction(q);
            var action = a.DQNActionNumberToVectorAction(actionnum);
            a.DQNAgentAction(action);
            if (episodes1 % NUM_EPISODES == 0) { 
                b.Targetnn = new NN(b.Qnn); 
            }
            if (episodes1 > b.BATCH_SIZE) {
                diff_list.Add(b.Learn(replaymemory1));
            }
            gamereset1 = 0;
            PrevState1 = CurrentState1;
            PrevAction1 = actionnum;



            // episodes1 += 1;
            // // エージェントの状態（座標など）の取得
            // double[] q = b.GetQusingQnn(CurrentState1.ToArray()); 
            // double MaxQ = q.Max();
            // int actionnum = b.GetAction(q);
            // var action = a.ActionNumberToVectorAction(actionnum);
            // a.DQNAgentAction(action);
            // var NewState = a.DQNCollectObservations();
            // b.PushToMemory(replaymemory1, CurrentState1, actionnum, NewState, a.Reward, q);
            // if (episodes1 % NUM_EPISODES == 0) { // DBとDWが同じ数値な気がするんだけど、、、
            //     // if (episodes1 > b.BATCH_SIZE) {
            //     //     b.Learn(replaymemory1);
            //     // }
            //     b.Targetnn = new NN(b.Qnn); // ここで連動されてしまっている。
            // }
            // if (episodes1 > b.BATCH_SIZE) {
            //     b.Learn(replaymemory1);
            // }
            // CurrentState1 = NewState;
        }
        else {
            episodes2 += 1;
            // エージェントの状態（座標など）の取得
            CurrentState2 = a.DQNCollectObservations();
            if(gamereset2 != 1 && episodes2 != 1)
            {
                if (LearningAgent2.TimeUp) {
                    b.PushToMemory(replaymemory2, PrevState2, PrevAction2, CurrentState2, a.Reward, LearningAgent2.TimeUp);
                }
                else {
                    b.PushToMemory(replaymemory2, PrevState2, PrevAction2, CurrentState2, 0, LearningAgent2.TimeUp);
                }
                // b.PushToMemory(replaymemory1, PrevState1, PrevAction1, CurrentState1, a.Reward, q);
                // b.PushToMemory(replaymemory2, PrevState2, PrevAction1, CurrentState2, reward2);
            }
            if (LearningAgent2.TimeUp) {
                LearningAgent2.SetReward(0);
            }
            double[] q = b.GetQusingQnn(CurrentState2.ToArray()); 
            double MaxQ = q.Max();
            int actionnum = b.GetAction(q);
            var action = a.DQNActionNumberToVectorAction(actionnum);
            a.DQNAgentAction(action);
            if (episodes2 % NUM_EPISODES == 0) { 
                b.Targetnn = new NN(b.Qnn); 
            }
            if (episodes2 > b.BATCH_SIZE) {
                b.Learn(replaymemory2);
            }
            gamereset2 = 0;
            PrevState2 = CurrentState2;
            PrevAction2 = actionnum;

            // episodes2 += 1;
            // // エージェントの状態（座標など）の取得
            // double[] q = b.GetQusingQnn(CurrentState2.ToArray()); 
            // int actionnum = b.GetAction(q);
            // var action = a.ActionNumberToVectorAction(actionnum);
            // a.DQNAgentAction(action);
            // var NewState = a.DQNCollectObservations();
            // b.PushToMemory(replaymemory2,CurrentState2, actionnum, NewState, a.Reward);
            // // b.PushToMemory(replaymemory2,CurrentState2, actionnum, NewState, a.Reward, q);
            // if (episodes2 % NUM_EPISODES == 0) {
            //     // if (episodes2 > b.BATCH_SIZE) {
            //     //     b.Learn(replaymemory2);
            //     // }
            //     b.Targetnn = new NN(b.Qnn); 
            // }
            // if (episodes2 > b.BATCH_SIZE) {
            //     b.Learn(replaymemory2);
            // }
            // CurrentState2 = NewState;
        }
    }

    private void UpdateText() {
        QText.text = "Best Record: " + BestRecord +  "\nEpisode: " + episodes1 + "\nGamecount: " + gamecount; 
    }
}