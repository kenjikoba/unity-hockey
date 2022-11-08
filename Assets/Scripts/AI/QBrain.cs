using UnityEngine; // Load関数が少し違うのと、Save関数が本当に絶妙に違う。
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

public class QBrain : Brain
{
    public int StateSize { get; private set; }
    public int ActionSize { get; private set; }

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
    private float Gamma { get; set; } = 0.75f;
    // The rate at which to update the value estimates given a reward.
    private float ETA { get; set; } = 0.1f;

    public void CreateTable()
    {
        Epsilon = epsilon;
        EpsilonMin = epsilonMin;

        QTable = new float[StateSize][];
        for (int i = 0; i < StateSize; i++)
        {
            QTable[i] = new float[ActionSize]; // 初期化で最初は0が入ってるっぽい。
        }
        //Load("QT01.txt");
    }

    public QBrain(int stateSize, int actionSize)
	{
		StateSize = stateSize;
		ActionSize = actionSize;
		CreateTable();
	}

    public int GetAction(int state)
    {
        int action;

        if (Epsilon <= UnityEngine.Random.Range(0.0f, 1.0f))
        {
            action = QTable[state].ToList().IndexOf(QTable[state].Max());
        }
        else
        {
            action = UnityEngine.Random.Range(0, ActionSize);
        }

        if (Epsilon > EpsilonMin)
        {
            Epsilon = Epsilon - ((1f - EpsilonMin) / AnnealingSteps);
        }

        return action;
    }

    public float[] GetValue()
    {
        float[] value_table = new float[QTable.Length];
        for (int i = 0; i < QTable.Length; i++)
        {
            value_table[i] = QTable[i].Average();
        }
        return value_table;
    }

    public void UpdateTable(int lastState, int nextState, int action, float reward, bool done)
    {
        if (action != -1)
        {
            if (done == true)
            {
                QTable[lastState][action] += ETA * (reward - QTable[lastState][action]);
            }
            else
            {
                if (nextState != lastState)
                {
                    QTable[lastState][action] += ETA * (reward + Gamma * QTable[nextState].Max() - QTable[lastState][action]);
                }
                else
                {
                    QTable[lastState][action] += ETA * (reward - QTable[lastState][action]); //?
                }
            }
        }
    }

// path, FileMode.OpenOrCreate
// path, FileMode.Create, FileAccess.Write
    public override void Save(string path)
    {
        using (var bw = new BinaryWriter(new FileStream(path, FileMode.OpenOrCreate)))
        {
            for (int i = 0; i < StateSize; i++)
            {
                for (int j = 0; j < ActionSize; j++)
                {
                    bw.Write(QTable[i][j]);
                }
            }
        }
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
}
