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
    
    // /***** SerializeField for gameObjects *****/
    [Header("Agent Prefab"), SerializeField] private GameObject GObject1;
    [Header("Agent Prefab"), SerializeField] private GameObject GObject2;

    [SerializeField] private int actionSize = 6;
    private int ActionSize { get { return actionSize; } }

    [SerializeField] private int stateSize = 1024;
    private int StateSize { get { return stateSize; } }

    private DQNBrain DQNBrain1;
    private DQNBrain DQNBrain2;
    private HockeyAgent LearningAgent1;
    private HockeyAgent LearningAgent2;

    private int PrevState1;
    private int PrevState2;

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
        
        DQNBrain1 = new DQNBrain(StateSize, ActionSize);
        DQNBrain2 = new DQNBrain(StateSize, ActionSize);
        LearningAgent1 = GObject1.GetComponent<HockeyAgent>();
        LearningAgent2 = GObject2.GetComponent<HockeyAgent>();
        PrevState1 = LearningAgent1.GetState();
        PrevState2 = LearningAgent2.GetState();
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
        // AgentsSet.ForEach(p => { p.agent.TimeUp = false; });
    }

    private void FixedUpdate()
    {
        // Waiting, Restart, ManualModeのいずれかがtrueであれば学習を進めない
        if (WaitingFlag || RestartFlag || ManualModeFlag) {
            return;
        }

        // if (LearningAgent1.IsDone) {
        //     int r = (int)LearningAgent1.Reward;
        //     BestRecord = Mathf.Max(r, BestRecord);
        //     System.Console.WriteLine("Hello, World! ={0} \n",BestRecord.ToString());
        //     File.WriteAllText("./Assets/BestRecord.txt", BestRecord.ToString());
        //     QLBrain1.Save("./Assets/Q.txt");
        //     // WaitingFlag = true;
        // }
        if (LearningAgent1.TimeUp) {
            RestartFlag = true;
            int r = (int)LearningAgent1.Reward;
            BestRecord = Mathf.Max(r, BestRecord);
            int pastRecord = 0;
            // インスタンスを作成，パスをコンストラクタに渡す
            using (var objReader = new StreamReader("./Assets/BestRecord.txt")) {
                string pastRead = objReader.ReadToEnd();
                pastRecord = int.Parse(pastRead);
            }
            if (pastRecord < BestRecord) {
                File.WriteAllText("./Assets/BestRecord.txt", BestRecord.ToString());
                DQNBrain1.Save("./Assets/StreamingAssets/ComputerBrains/Q.txt");
            }
            UpdateText();
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
            int pastRecord = 0;
            // インスタンスを作成，パスをコンストラクタに渡す
            using (var objReader = new StreamReader("./Assets/BestRecord.txt")) {
                string pastRead = objReader.ReadToEnd();
                pastRecord = int.Parse(pastRead);
            }
            if (pastRecord < BestRecord) {
                File.WriteAllText("./Assets/BestRecord.txt", BestRecord.ToString());
                DQNBrain2.Save("./Assets/StreamingAssets/ComputerBrains/Q.txt");
            }
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
            int ActionNo = b.GetAction(PrevState1); 
            var action = a.ActionNumberToVectorAction(ActionNo);
            a.AgentAction(action);
            var NewState = a.GetState();
            b.UpdateTable(PrevState1, NewState, ActionNo, a.Reward, a.IsDone);
            PrevState1 = NewState;
        }
        else {
            int ActionNo = b.GetAction(PrevState2); 
            var action = a.ActionNumberToVectorAction(ActionNo);
            a.AgentAction(action);
            var NewState = a.GetState();
            b.UpdateTable(PrevState2, NewState, ActionNo, a.Reward, a.IsDone);
            PrevState2 = NewState;
        }
    }

    private void UpdateText() {
        QText.text = "Best Record: " + BestRecord; 
    }
}