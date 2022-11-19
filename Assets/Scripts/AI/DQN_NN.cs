using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System;

[Serializable]
public class NN
{
    // 重み行列などの初期化に使うランダム値の上限と下限
    private float RandomMax { get; set; } = 1;
    private float RandomMin { get; set; } = -1;

    // レイヤーごとの重み行列のリスト
    [SerializeField] private List<Matrix> weights = new List<Matrix>();
    public List<Matrix> Weights { get { return weights; } }

    // レイヤーごとのバイアスのリスト
    [SerializeField] private List<Matrix> biases = new List<Matrix>();
    public List<Matrix> Biases { get { return biases; } }

    // ニューラルネットワークの入力次元
    [SerializeField] private int inputSize = 8;
    public int InputSize { get { return inputSize; } private set { inputSize = value; } }

    // ニューラルネットワークの隠れ層の次元
    [SerializeField] private int hiddenSize = 0;
    public int HiddenSize { get { return hiddenSize; } private set { hiddenSize = value; } }

    // ニューラルネットワークの隠れ層の数（0以上）
    [SerializeField] private int hiddenLayers = 1;
    public int HiddenLayers { get { return hiddenLayers; } private set { hiddenLayers = value; } }

    // ニューラルネットワークの出力次元
    [SerializeField] private int outputSize = 9;
    public int OutputSize { get { return outputSize; } private set { outputSize = value; } }

    // 勾配の逆伝播時に使う，各層の活性化関数を通す前の出力のリスト
    [SerializeField] private List<Matrix> v = new List<Matrix>();
    public List<Matrix> V { get { return v; } }

    // 勾配の逆伝播時に使う，各層の活性化関数を通した後の出力のリスト
    [SerializeField] private List<Matrix> y = new List<Matrix>();
    public List<Matrix> Y { get { return y; } }

    // 勾配の逆伝播時に使う，前回のニューラルネットワークへの入力
    [SerializeField] private double[] lastInputs;

    // 逆伝播された重み行列に関する損失関数の勾配
    [SerializeField] private List<Matrix> dw = new List<Matrix>();
    public List<Matrix> DW { get { return dw; } }

    // 逆伝播されたバイアスに関する損失関数の勾配
    [SerializeField] private List<Matrix> db = new List<Matrix>();
    public List<Matrix> DB { get { return db; } }

    // 逆伝播させた勾配に基づいて重み行列・バイアスを更新する際の学習率
    [SerializeField] private double learningRate = 0.0025;
    public double LearningRate { get { return learningRate; } set { learningRate = value; } }

    public NN(int inputSize, int hiddenSize, int hiddenLayers, int outputSize) {
        InputSize = inputSize;
        OutputSize = outputSize;
        HiddenLayers = hiddenLayers;
        HiddenSize = hiddenSize;
        lastInputs = new double[inputSize];
        CreateMatrix(inputSize, hiddenSize, hiddenLayers, outputSize);
        InitAllMatrix();//行列をランダムに初期化する
    }

    public NN(NN other) {
        InputSize = other.InputSize;
        OutputSize = other.OutputSize;
        HiddenLayers = other.HiddenLayers;
        HiddenSize = other.HiddenSize;
        lastInputs = new double[inputSize];

        for(int i = 0; i < other.Weights.Count; i++) {
            Matrix w = other.Weights[i].Copy();
            Matrix b = other.Biases[i].Copy();
            Weights.Add(w);
            Biases.Add(b);
            Matrix v = other.V[i].Copy();
            Matrix y = other.Y[i].Copy();
            V.Add(v);
            Y.Add(y);
            Matrix dw = other.DW[i].Copy();
            Matrix db = other.DB[i].Copy();
            DW.Add(dw);
            DB.Add(db);
        }
    }

    // 各層の重み行列・バイアスの作成
    private void CreateMatrix(int inputSize, int hiddenSize, int hiddenLayers, int outputSize) {
        for(int i = 0; i < hiddenLayers + 1; i++) {
            int inSize = (i == 0) ? inputSize : hiddenSize;
            int outSize = (i == hiddenLayers) ? outputSize : hiddenSize;
            Weights.Add(new Matrix(inSize, outSize));
            Biases.Add(new Matrix(1, outSize));
            V.Add(new Matrix(1, outSize));
            Y.Add(new Matrix(1, outSize));
            DW.Add(new Matrix(inSize, outSize));
            DB.Add(new Matrix(1, outSize));
        }
    }

    // ニューラルネットワークの順伝播．
    // 入力：ニューラルネットワークへの入力　doubleの配列
    // 出力：ニューラルネットワークの出力　doubleの配列
    public double[] Predict(double[] inputs) {
        var input_array = inputs;
        var input = new Matrix(input_array);
        Array.Copy(input_array, lastInputs, inputSize); // lastInputsの更新
        // var input = new Matrix(inputs);
        // Array.Copy(inputs, lastInputs, inputSize); // lastInputsの更新

        for(int i = 0; i < HiddenLayers + 1; i++) {
            // V[i] = (i == 0) ? input.Mul(Weights[i]) : Y[i-1].Mul(Weights[i]);
            if (i == 0) {
                V[0] = input.Mul(Weights[i]);
            }
            else {
                V[i] = Y[i-1].Mul(Weights[i]);
            }
            var b = Biases[i];
            for(int c = 0; c < b.Colmun; c++) {
                V[i][0, c] += b[0, c];
                Y[i][0, c] = Activation(V[i][0, c]); //V[1]が頭狂ってる→Weights[1]が頭狂ってるから。
            } // ずっと
        }

        return Y[HiddenLayers].ToArray();
    }
    
    // 勾配の逆伝搬
    // 各層の重み行列・バイアスに関する損失関数の勾配を計算して重み行列・バイアスを更新する
    // 損失関数には二乗誤差を用いる
    // 入力：前回のニューラルネットワークへの入力に対する真の出力　doubleの配列
    // 出力：なし　（重み行列・バイアスが更新される）
    public void BackPropagate(double[] y) {// これ今損失だけど間違い。ここに教師データのq_saを入れて、まずY[hidden_layers]=qsaかどうか確かめたい。だったらおけ。違ったら原因考える。
        // 出力層での勾配計算
        for(int c = 0; c < outputSize; c++) { // Huber関数に変更。
            if ((Y[HiddenLayers][0, c] - y[c] >= -1) && (Y[HiddenLayers][0, c] - y[c] <= 1)) {
                DB[HiddenLayers][0, c] = ActivationPrime(V[HiddenLayers][0, c]) * (Y[HiddenLayers][0, c] - y[c]); 
            }
            else if (Y[HiddenLayers][0, c] - y[c] <= -1) {
                DB[HiddenLayers][0, c] = ActivationPrime(V[HiddenLayers][0, c]) * -1; 
            }
            else {
                DB[HiddenLayers][0, c] = ActivationPrime(V[HiddenLayers][0, c]) * 1; 
            }
        }
        for(int r = 0; r < Weights[HiddenLayers].Row; r++) {
            for(int c = 0; c < outputSize; c++) { 
                DW[HiddenLayers][r, c] = DB[HiddenLayers][0, c] * Y[HiddenLayers - 1][0, r];
            }
        }

        // 隠れ層での勾配計算
        for(int i = HiddenLayers - 1; i >= 0; i--) {
            for(int r = 0; r < Weights[i+1].Row; r++) {
                DB[i][0, r] = 0;
                for(int c = 0; c < Weights[i+1].Colmun; c++) {
                    DB[i][0, r] += DB[i+1][0, c] * Weights[i+1][r, c];
                }
                DB[i][0, r] *= ActivationPrime(V[i][0, r]);
            }

            if(i == 0){
                for(int r = 0; r < inputSize; r++) {
                    for(int c = 0; c < hiddenSize; c++) {
                        DW[0][r, c] = DB[0][0, c] * lastInputs[r];// このlastInputsはcurrentstate。大丈夫そう。
                    }
                }
            }else{
                for(int r = 0; r < hiddenSize; r++) {
                    for(int c = 0; c < hiddenSize; c++) {
                        DW[i][r, c] = DB[i][0, c] * Y[i - 1][0, r];
                    }
                }
            }
        }

        // 各層の重み行列・バイアスの更新
        for(int i=0; i < hiddenLayers + 1; i++){
            int inSize = (i == 0) ? inputSize : hiddenSize;
            int outSize = (i == hiddenLayers) ? outputSize : hiddenSize;
            for(int c=0; c<outSize; c++){
                for(int r=0; r<inSize; r++){
                    Weights[i][r, c] -= learningRate * DW[i][r, c]; // 重みが爆発（勾配爆発？）
                }
                Biases[i][0, c] -= learningRate * DB[i][0, c];
            }
        }
    }

    // ニューラルネットワークへの入力と真の出力の組から重み行列・バイアスを学習する
    public void BackPropagate(double[] inputs, double[] trueOutputs) {
        Predict(inputs);// これを使おう、けど損失関数の部分を変えよう。
        BackPropagate(trueOutputs);
    }

    // 活性化関数
    private double Activation(double x) {
        return Tanh(x);
        // return Sigmoid(x);
        // return Relu(x);
    }

    // 活性化関数の微分
    private double ActivationPrime(double x) {
        return TanhPrime(x);
        // return SigmoidPrime(x);
        // return ReluPrime(x);
    }

    private double Sigmoid(double x) {
        return 1 / (1 + Mathf.Exp(-1 * (float)x));
    }

    private double SigmoidPrime(double x) {
        double t = Sigmoid(x); 
        return (1 - t) * t;
    }

    private double Tanh(double x) {
        return Math.Tanh(x);
    }

    private double TanhPrime(double x) {
        double t = Math.Tanh(x);
        return 1.0 -  t * t;
    }

    private double Relu(double x) {
        if (x > 0) {
            return x;
        }
        else {
            return 0;
        }
    }

    private double ReluPrime(double x) {
        if (x > 0) {
            return 1;
        }
        else {
            return 0;
        }
    }

    // 重み行列・バイアスのランダム初期化
    private void InitAllMatrix() {
        foreach(Matrix m in Biases) {
            InitMatrix(m);
        }
        foreach(Matrix m in Weights) {
            InitMatrix(m);
        }
    }

    private void InitMatrix(Matrix m) {
        for(int r = 0; r < m.Row; r++) {
            for(int c = 0; c < m.Colmun; c++) {
                m[r, c] = UnityEngine.Random.Range(RandomMin, RandomMax);
            }
        }
    }

    public void Save(string path) {
        var json = JsonUtility.ToJson(this);
        File.WriteAllText(path, json);
    }

    public static NN Load(TextAsset asset) {
        return JsonUtility.FromJson<NN>(asset.text);
    }
}
