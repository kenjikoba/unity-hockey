using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine.UI;

public class DEEnvironment : Environment
{
    /***** DE(Differential Evolution) Parameters *****/
    [Header("Settings"), SerializeField] private int totalPopulation = 100;
    private int TotalPopulation { get { return totalPopulation; } }
    [Header("Settings"), SerializeField] private float mutationScalingFactor = 0.5f;
    private float AmpFactor { get { return mutationScalingFactor; } }
    [Header("Settings"), SerializeField] private float crossRate = 0.45f;
    private float CrossRate { get { return crossRate; } }
    
    /***** NN Model Parameters *****/
    [SerializeField] private int inputSize = 6;
    private int InputSize { get { return inputSize; } }
    [SerializeField] private int hiddenSize = 32;
    private int HiddenSize { get { return hiddenSize; } }
    [SerializeField] private int hiddenLayers = 1;
    private int HiddenLayers { get { return hiddenLayers; } }
    [SerializeField] private int outputSize = 2;
    private int OutputSize { get { return outputSize; } }
    
    /***** Num of Symaltaneously Playing Agents *****/
    [SerializeField] private int nAgents = 2;
    private int NAgents { get { return nAgents; } }
    
    /***** SerializeField for gameObjects *****/
    [Header("Agent Prefab"), SerializeField] private GameObject GObject1;
    [Header("Agent Prefab"), SerializeField] private GameObject GObject2;
    [Header("UI References"), SerializeField] private Text populationText = null;
    
    /***** Property of GameObjects *****/
    private Text PopulationText { get { return populationText; } }
    
    /***** Values for Record *****/
    private float GenBestRecord { get; set; }
    private float GenSumReward { get; set; }
    private float GenAvgReward { get; set; }
    private float BestRecord { get; set; }

    /***** Objects for DE *****/
    private List<NNBrain> childBrains { get; set; } = new List<NNBrain>();
    private List<NNBrain> parentBrains { get; set; } = new List<NNBrain>();
    private List<GameObject> GObjects { get; } = new List<GameObject>();
    private List<HockeyAgent> Agents { get; } = new List<HockeyAgent>();
    private int Generation { get; set; }
    private List<AgentPair> AgentsSet { get; } = new List<AgentPair>();
    private Queue<NNBrain> CurrentBrains { get; set; }

    /***** Bool Objects For Snchronization With Opponent Player *****/
    // Getter and Setter should be written?
    public bool WaitingFlag = false;
    public bool RestartFlag = false;
    public bool ManualModeFlag = false;

    /***** 学習が開始された際に呼ばれる *****/
    void Awake() {
        // 同時にプレイするプレーヤー数. 現在は2で固定.
        if (nAgents != 2) {
            Debug.Log("Now, nAgents must be equal to 2.");
            nAgents = 2;
        }
        
        // NNを作り, 初期値として親と子に同じNNを与える
        for(int i = 0; i < TotalPopulation; i++) {
            childBrains.Add(new NNBrain(InputSize, HiddenSize, HiddenLayers, OutputSize));
            parentBrains.Add(childBrains[i]);
        }

        // Objectの追加
        GObjects.Add(GObject1);
        Agents.Add(GObject1.GetComponent<HockeyAgent>());
        GObjects.Add(GObject2);
        Agents.Add(GObject2.GetComponent<HockeyAgent>());

        // 学習開始
        SetStartAgents();
    }
    
    // 対戦する個体をAgentsSetに追加
    void SetStartAgents() {
        //UnityEngine.Random.InitState( name.Length );
        CurrentBrains = new Queue<NNBrain>(childBrains);
        AgentsSet.Clear();
        var size = Math.Min(NAgents, TotalPopulation);
        for(var i = 0; i < size; i++) {
            AgentsSet.Add(new AgentPair {
                agent = Agents[i],
                brain = CurrentBrains.Dequeue()
            });
        }
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
        AgentsSet.ForEach(p => { p.agent.TimeUp = false; });
    }

    /*************************************/
    /***** 毎フレーム呼ばれ, 学習を進める *****/
    /*************************************/
    void FixedUpdate() {
        // Waiting, Restart, ManualModeであれば学習を進めない
        if (WaitingFlag || RestartFlag || ManualModeFlag) {
            return;
        }

        // AgentsSetの2つのAgentについて更新する
        foreach(var pair in AgentsSet.Where(p => !p.agent.IsDone)) {
            // 観測・計算・実行
            AgentUpdate(pair.agent, pair.brain);
        }

        // 時間切れであればRestart, 時間切れでなければ再開
        AgentsSet.RemoveAll(p => {
            if(p.agent.IsDone) {
                //TODO? (CAUTION) : Getting High Reward will get Difficult As Generation Goes
                //                  => Here, Reward = Reward + Generation * constant(20)
                float r = p.agent.Reward + Generation * 10;
                BestRecord = Mathf.Max(r, BestRecord);
                GenBestRecord = Mathf.Max(r, GenBestRecord);
                p.brain.Reward = r;
                GenSumReward += r;
                WaitingFlag = true;
            }
            if (p.agent.TimeUp) {
                RestartFlag = true;
            }
            return p.agent.IsDone;
        });

        if(CurrentBrains.Count == 0 && AgentsSet.Count == 0) {
            // 全ての個体が対戦済みであれば新しい世代を生成
            SetNextGeneration();
        }
        else {
            // 次の個体をセットする
            SetNextAgents();
        }
    }
    
    /***************************/
    /***** プレーヤーを動かす *****/
    /***************************/
    private void AgentUpdate(Agent a, NNBrain b) {
        // ボードの状態を取得
        var observation = a.CollectObservations();
        // NNBrainからactionを取得
        var action = b.GetAction(observation);
        // Agentにactionを送る
        a.AgentAction(action); 
    }

    // 次の個体をセットする
    private void SetNextAgents() {
        int size = Math.Min(NAgents - AgentsSet.Count, CurrentBrains.Count);
        for(var i = 0; i < size; i++) {
            var nextAgent = Agents.First(a => a.IsDone);
            var nextBrain = CurrentBrains.Dequeue();
            nextAgent.Reset();
            nextAgent.SetReward(0);
            AgentsSet.Add(new AgentPair {
                agent = nextAgent,
                brain = nextBrain
            });
            nextBrain.Save("./Assets/StreamingAssets/ComputerBrains/CURRENT.txt");
        }
        UpdateText();
    }

    // 新しい世代をセットする
    private void SetNextGeneration() {
        GenAvgReward = GenSumReward / TotalPopulation;
        
        GenPopulation();
        GenSumReward = 0;
        GenBestRecord = 0;
        Agents.ForEach(a => a.Reset());
        Agents.ForEach(a => a.SetReward(0));
        SetStartAgents();
        UpdateText();
    }

    private static int CompareBrains(Brain a, Brain b) {
        if(a.Reward > b.Reward) return -1;
        if(b.Reward > a.Reward) return 1;
        return 0;
    }

    /*******************************************************/
    /***** DE(Differential Evolution) + EliteSelection *****/
    /*****           新しい世代をDEの手法で生成する         *****/
    /*******************************************************/
    private void GenPopulation() {
        var children = new List<NNBrain>();
        for (int i = 0; i < TotalPopulation; i++) {
            /***** 親と子とを比較し, 報酬が高い方を残す *****/
            /***** Keep Child-Indiv When It Is Better Than Parent *****/
            if (parentBrains[i].Reward <= childBrains[i].Reward) {
                 parentBrains[i] = childBrains[i];
            }
         }

        /****** Sort Parents *****/
        // 親を報酬が高い順にソートする
        parentBrains = parentBrains.ToList();
        parentBrains.Sort(CompareBrains);
        //File.WriteAllText("BestBrain.json", JsonUtility.ToJson(parentBrains[0]));
        parentBrains[0].Save("./Assets/BestBrain.txt");

        /***** EliteSelection *****/
        // 最も成績が良かった親の子は親と同じNNを持つ
        int ElitePop = 1;
        for (int i = 0; i < ElitePop; i++) {
            children.Add(parentBrains[i]);
        }

        /*****************************/
        /***** Main DE Operation *****/
        /*****    差分進化計算     *****/
        /*****************************/
        for (int i = 0; i < TotalPopulation-ElitePop; i++) {
            // 親となる個体を選ぶ
            int ind1 = UnityEngine.Random.Range(0, TotalPopulation);
            // 探索点となる2つの個体を選ぶ
            int ind2 = UnityEngine.Random.Range(0, TotalPopulation);
            int ind3 = UnityEngine.Random.Range(0, TotalPopulation);
            // x_child = x1 + F ( x3 - x2 )を計算する
            NNBrain child = parentBrains[i].DE(parentBrains[ind1], parentBrains[ind2], parentBrains[ind3], mutationScalingFactor, crossRate);
            // x_childをDNAとして持たせる
            children.Add(child);
        }
        childBrains = children;
        for (int i = 0; i < TotalPopulation; i++) {
            childBrains[i].Reward = 0;
        }

        Generation++;
    }

    private void UpdateText() {
        PopulationText.text = "Population: " + (TotalPopulation - CurrentBrains.Count) 
            + "/" + TotalPopulation
            + "\nGeneration: " + (Generation + 1)
            + "\nBest Record: " + BestRecord
            + "\nBest this gen: " + GenBestRecord
            + "\nAverage: " + GenAvgReward;
    }

    private struct AgentPair
    {
        public NNBrain brain;
        public HockeyAgent agent;
    }
}
