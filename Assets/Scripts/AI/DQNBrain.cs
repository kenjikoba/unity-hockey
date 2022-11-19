using UnityEngine; // Load関数が少し違うのと、Save関数が本当に絶妙に違う。
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

public class DQNBrain : Brain
{
    public int StateSize { get; private set; }
    public int ActionSize { get; private set; }

    public int BATCH_SIZE = 32; // バッチサイズ
    private int MAX_STEPS = 200;  // 1試行のstep数
    // private int CAPACITY = 10000; // メモリの最大長さ

    // ニューラルネットワーク
    public NN Qnn;
    public NN Targetnn;
    private int inputSize;
    private int outputSize;
    [SerializeField] private int hiddenSize = 40;
    [SerializeField] private int hiddenLayers = 1;
    // ニューラルネットワークの学習率
    [SerializeField] private double learningRate = 0.0025;

    [SerializeField] private float epsilon = 1.0f;
    [SerializeField] private float epsilonMin;

    private float[][] QTable { get; set; }
    // Initial epsilon value for random action selection.}
    private float Epsilon { get; set; } = 1.0f;
    // Lower bound of epsilon.
    private float EpsilonMin { get; set; } = 0.0f;
    // Number of steps to lower e to eMin.
    private int AnnealingSteps { get; set; } = 100000;
    // Discount factor for calculating Q-target.
    private double Gamma { get; set; } = 0.99;
    // The rate at which to update the value estimates given a reward.
    private float ETA { get; set; } = 0.1f;

    public DQNBrain(int stateSize, int actionSize)
	{
        int inputSize = stateSize;
        int outputSize = actionSize;
        // Qニューラルネットワークの作成
        Qnn = new NN(inputSize, hiddenSize, hiddenLayers, outputSize);
        // 学習途中のモデルを使用。
        // var textAssetqnn = Resources.Load("DQNtest1") as TextAsset;
        // Qnn = JsonUtility.FromJson<NN>(textAssetqnn.text); 
        Qnn.LearningRate = learningRate;
        // Target Networkの作成
        Targetnn = new NN(Qnn);
        // 学習途中のモデルを使用。
        // var textAssettargetnn = Resources.Load("DQNtest2") as TextAsset;
        // Targetnn = JsonUtility.FromJson<NN>(textAssettargetnn.text); 
        Targetnn.LearningRate = learningRate;
	}

    public override void Save(string path)
    {
        var json = JsonUtility.ToJson(this);
        File.WriteAllText(path, json);
    }

    public static NN Loadjson(TextAsset asset) {
        return JsonUtility.FromJson<NN>(asset.text);
    }

    public override void Load(string path)
    {
        int size = 60;
        using (var br = new BinaryReader(new FileStream(path, FileMode.Open)))
        {
            for (int i = 0; i < StateSize / size; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    QTable[i][j] = br.ReadSingle();
                }
            }
        }

        for (int i = 1; i < size; i++)
        {
            for (int j = 0; j < StateSize / size; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    QTable[StateSize / size * i + j][k] = QTable[j][k] * 0.1f;
                }
            }
        }
    }

    public int GetAction(double[] q)
    {
        int actionnum;

        if (Epsilon <= UnityEngine.Random.Range(0.0f, 1.0f))
        {
            actionnum = Array.IndexOf(q, q.Max());
        }
        else
        {
            actionnum = UnityEngine.Random.Range(0, q.Length);
        }

        if (Epsilon > EpsilonMin)
        {
            Epsilon = Epsilon - ((1f - EpsilonMin) / AnnealingSteps);
        }

        return actionnum;
    }

    public void PushToMemory(ReplayMemory replaymemory, List<double> currentstate, int action, List<double> nextstate, float reward, bool timeup) {// , double[] q
        // currentstate, action, nextstate, rewardをメモリに保存します
        System.Random rand = new System.Random();

        object [] onememory = new object[5];
        onememory[0] = currentstate;
        onememory[1] = action;
        onememory[2] = nextstate;
        onememory[3] = reward;
        onememory[4] = timeup; 

        // メモリに保存。
        if (replaymemory.capacity == replaymemory.memory.Count) {
            int randomindex = rand.Next(0, replaymemory.memory.Count);
            replaymemory.memory[randomindex] = onememory;
        }
        else {
            replaymemory.memory.Add(onememory);
        }
    }

    private List<object[]> GetSampleBatch(ReplayMemory replaymemory, int batch_size) {
        // batch_size分だけ、ランダムに保存内容を取り出し、返す。
        System.Random rand = new System.Random();
        int randomindex = 0;
        List<object[]> samplememory = new List<object[]>();
        List<int> index_array = new List<int>();

        // ダブりの無いように取ってくるように、乱数生成→その乱数のインデックスの要素を取得&削除を繰り返している。
        for(int r = 0; r < batch_size; r++)
        {
            randomindex = rand.Next(0, replaymemory.memory.Count);
            while (index_array.Contains(randomindex) == true) {
                index_array.Add(randomindex);
                randomindex = rand.Next(0, replaymemory.memory.Count);
            }
            samplememory.Add(replaymemory.memory[randomindex]);
        }
        return samplememory;
    }

    public List<object []> Replay(ReplayMemory replaymemory) {
        // Experience Replayでネットワークの重みを学習
        // メモリからミニバッチ分のデータを取り出す
        List<object[]> batchdata = GetSampleBatch(replaymemory, BATCH_SIZE);
        return batchdata;
    }

    // 次の状態を入力としてTargetQNetworkから算出したQ値と、今とったQ値との損失関数
    private double[] GetLoss(double[] y, double[] qsa) { // Huber関数で実装。
        double[] loss = new double[9];
        for (int i = 0; i < 9; i++) {
            if (Math.Abs(y[i] - qsa[i]) < 1) {
                loss[i] = Math.Pow(y[i] - qsa[i],2);
            }
            else {
                loss[i] = Math.Abs(y[i] - qsa[i]);
            }
        }
        return loss;
    }

    public double[] GetQusingQnn(double[] CurrentState)
    {
        // double[] action;
        return Qnn.Predict(CurrentState);
    }

    public double[] GetQusingTargetnn(double[] CurrentState)
    {
        // double[] action;
        return Targetnn.Predict(CurrentState);
    }

    public double Learn(ReplayMemory replaymemory) {
        List<object[]> minibatch = Replay(replaymemory);
        List<double> diff_batch_list = new List<double>();
        for (int i = 0; i < BATCH_SIZE; i++) {
            object[] onebatch = minibatch[i];
            double[] currentstate = new double[8];
            // int action = new int();
            double[] nextstate = new double[8];
            double reward = new double();
            // double[] q_batch = new double[8];
            bool done = (bool)onebatch[4];

            List<double> current = (List<double>)onebatch[0];
            for (int j = 0; j < 8; j++) {
                currentstate[j] = (double)current[j];
            }
            // action = (int)(double)onebatch[0];
            List<double> next = (List<double>)onebatch[2];
            for (int j = 0; j < 8; j++) {
                nextstate[j] = (double)next[j];
            }
            reward = (double)(float)onebatch[3];
            // double[] re = (double[])onebatch[0];
            // for (int j = 0; j < 8; j++) {
            //     q_batch[j] = (double)re[j];
            // }
            double[] q_sa = GetQusingTargetnn(nextstate);
            double[] qsa = GetQusingQnn(currentstate);
            double[] y = new double[9];
            for (int j = 0; j < 9; j++) {
                if (done) {
                    y[j] = reward;
                }
                else {
                    y[j] = reward + Gamma * q_sa[j];
                }
            }
            double[] loss = GetLoss(y,qsa);
            // Qnn.BackPropagate(loss); // 問題はlossが大きすぎること。1つ1つ0~1に抑えたい。
            Qnn.BackPropagate(y); // 引数は教師データ。
            // 一回目の更新でbiasとDB,DWの値が爆発してる。
            diff_batch_list.Add(loss.ToList().Average());
        }
        return diff_batch_list.Average();
    }
}
