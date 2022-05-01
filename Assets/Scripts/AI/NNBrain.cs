using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System;

[Serializable]
public class NNBrain : Brain
{
    private float MutationRate { get; set; } = 0.20f;
    private float RandomMin { get; set; } = -1;
    private float RandomMax { get; set; } = 1;

    private int DNALength = 0;
    // List of biases and weights of each layer
    [SerializeField] private List<Matrix> weights = new List<Matrix>();
    public List<Matrix> Weights { get { return weights; } }

    [SerializeField] private List<Matrix> biases = new List<Matrix>();
    public List<Matrix> Biases { get { return biases; } }

    [SerializeField] private int inputSize = 0;
    public int InputSize { get { return inputSize; } private set { inputSize = value; } }

    [SerializeField] private int hiddenSize = 0;
    public int HiddenSize { get { return hiddenSize; } private set { hiddenSize = value; } }

    [SerializeField] private int hiddenLayers = 0;
    public int HiddenLayers { get { return hiddenLayers; } private set { hiddenLayers = value; } } // may be equal to 0

    [SerializeField] private int outputSize = 0;
    public int OutputSize { get { return outputSize; } private set { outputSize = value; } }

    // DEEnvironmentから行動を取得する際に呼ばれる.
    // actionを返す.
    public double[] GetAction(List<double> observation) {
        var action = Predict(observation.ToArray());
        return action;
    }

    public NNBrain(int inputSize, int hiddenSize, int hiddenLayers, int outputSize) {
        InputSize = inputSize;
        OutputSize = outputSize;
        HiddenLayers = hiddenLayers;
        HiddenSize = hiddenSize;
        CreateMatrix(inputSize, hiddenSize, hiddenLayers, outputSize);
        InitAllMatrix();//行列をランダムに初期化する
    }

    public NNBrain(NNBrain other) {
        InputSize = other.InputSize;
        OutputSize = other.OutputSize;
        HiddenLayers = other.HiddenLayers;
        HiddenSize = other.HiddenSize;

        for(int i = 0; i < other.Weights.Count; i++) {
            Matrix w = other.Weights[i].Copy();
            Matrix b = other.Biases[i].Copy();
            Weights.Add(w);
            Biases.Add(b);
        }
    }

    private void CreateMatrix(int inputSize, int hiddenSize, int hiddenLayers, int outputSize) {
        
        for(int i = 0; i < hiddenLayers + 1; i++) {
            int inSize = (i == 0) ? inputSize : hiddenSize;
            int outSize = (i == hiddenLayers) ? outputSize : hiddenSize;
            Weights.Add(new Matrix(inSize, outSize));
            Biases.Add(new Matrix(1, outSize));
        }
    }

    // ボードの状態をinputとしてNNに入力し, 出力となる行動を計算する
    public double[] Predict(double[] inputs) {
        var output = new Matrix(inputs);
        var result = new double[OutputSize];
        for(int i = 0; i < HiddenLayers + 1; i++) {
            output = output.Mul(Weights[i]);
            var b = Biases[i];
            if(i != HiddenLayers) {
                for(int c = 0; c < b.Colmun; c++) {
                    output[0, c] = Tanh(output[0, c] + b[0, c]);
                }
            }
            else {
                for(int c = 0; c < b.Colmun; c++) {
                    output[0, c] = output[0, c] + b[0, c];
                }
            }
        }
        for(int c = 0; c < OutputSize; c++) {
            result[c] = output[0, c];
        }
        return result;
    }

    private double Sigmoid(double x) {
        return 1 / (1 - Mathf.Exp(-1 * (float)x));
    }

    private double Tanh(double x) {
        return Math.Tanh(x);
    }

    private void SetDNA(double[] dna, bool mutation = true) {//DNAの形にしたものをもとの意味を持つ行列群に戻す。
        var index = 0;
        foreach(var b in Biases) {
            index = SetDNA(b, dna, index);
        }
        foreach(var w in Weights) {
            index = SetDNA(w, dna, index);
        }
    }

    public double[] ToDNA() {
        var dna = new List<double>();
        foreach(var b in Biases) {
            dna.AddRange(b.ToArray());
        }
        foreach(var w in Weights) {
            dna.AddRange(w.ToArray());
        }
        return dna.ToArray();
    }

    private int SetDNA(Matrix m, double[] dna, int index) {
        for(int r = 0; r < m.Row; r++) {
            for(int c = 0; c < m.Colmun; c++) {
                m[r, c] = dna[index];
                index++;
            }
        }

        return index;
    }

    public override void Save(string path) {
        using(var bw = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write))) {
            bw.Write(InputSize);
            bw.Write(HiddenSize);
            bw.Write(HiddenLayers);
            bw.Write(OutputSize);
            var dna = ToDNA();
            bw.Write(dna.Length);

            for(int i = 0; i < dna.Length; i++) {
                bw.Write(dna[i]);
            }
        }
    }

    public override void Load(string path) {
        using(var br = new BinaryReader(new FileStream(path, FileMode.Open))) {
            int inputSize = br.ReadInt32();
            int hiddenSize = br.ReadInt32();
            int hiddenLayers = br.ReadInt32();
            int outputSize = br.ReadInt32();

            var length = br.ReadInt32();
            var dna = new double[length];
            
            for(int i = 0; i < length; i++) {
                dna[i] = br.ReadDouble();
            }

            SetDNA(dna);
        }
    }

    private void InitAllMatrix() {
        DNALength = 0;
        foreach(Matrix m in Biases) {
            InitMatrix(m);
        }
        foreach(Matrix m in Weights) {
            InitMatrix(m);
        }
    }

    private void InitMatrix(Matrix m) {//行列をランダムに初期化
        for(int r = 0; r < m.Row; r++) {
            for(int c = 0; c < m.Colmun; c++) {
                m[r, c] = UnityEngine.Random.Range(RandomMin, RandomMax);
                DNALength++;
            }
        }
        
    }

    // 差分進化のx1 + F(x3 - x2)を計算する関数.
    // [0,1]で生成した乱数が交叉率を下回っていた場合は新しい遺伝子は破棄する.
    // 世代生成中にDEEnvironmentから呼ばれる.
    public NNBrain DE(NNBrain ind1, NNBrain ind2, NNBrain ind3, double ampFactor, double crossRate) {
        double [] currentDNA = ToDNA();
        double [] newDNA = ToDNA();
        double [] ind1DNA = ind1.ToDNA();
        double [] ind2DNA = ind2.ToDNA();
        double [] ind3DNA = ind3.ToDNA();
        for (int i = 0; i < DNALength; i++) {
            newDNA[i] = ind1DNA[i] + ampFactor * (ind2DNA[i] - ind3DNA[i]);
            if (UnityEngine.Random.Range(0,1) > crossRate) {
                newDNA[i] = currentDNA[i];
            }
        }
        NNBrain newBrain = new NNBrain(this);
        newBrain.SetDNA(newDNA);
        return newBrain;
    }
}
