using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Matrix
{
    public double this[int r, int c] {
        set {
            Elements[GetIndex(r, c)] = value;
        }
        get {
            return Elements[GetIndex(r, c)];
        }
    }

    [SerializeField] private int row = 0;
    public int Row { get { return row; } private set { row = value; } }

    [SerializeField] private int colmun = 0;
    public int Colmun { get { return colmun; } private set { colmun = value; } }

    [SerializeField] private double[] elements = null;
    private double[] Elements { get { return elements; } set { elements = value; } }

    public Matrix(int row, int colmun) {
        Init(row, colmun);
    }

    public Matrix(double[] elements) {
        Init(1, elements.Length);
        for (int c = 0; c < Colmun; c++) {
            this[0, c] = elements[c];
        }
    }

    public void Init(int row, int colmun) {
        Row = row;
        Colmun = colmun;
        Elements = new double[row * colmun];
    }

    public int GetIndex(int row, int colmun) {
        return row * Colmun + colmun;
    }

    public Matrix Mul(Matrix m) {
        if (Colmun != m.Row) {
            throw new ArgumentException("colmun does not match row");
        }

        var newM = new Matrix(Row, m.Colmun);
        for (int r = 0; r < newM.Row; r++) {
            for (int c = 0; c < newM.Colmun; c++) {
                newM[r, c] = MulElement(m, r, c);
            }
        }
        return newM;
    }

    public Matrix Copy() {
        var m = new Matrix(Row, Colmun);
        for (int r = 0; r < Row; r++) {
            for (int c = 0; c < Colmun; c++) {
                m[r, c] = this[r, c];
            }
        }

        return m;
    }

    public double[] ToArray() {
        return Elements;
        //return Elements.ToArray();
    }

    private double MulElement(Matrix m, int index1, int index2) {
        var v = 0.0d;
        for (int c = 0; c < Colmun; c++) {
            v += this[index1, c] * m[c, index2];
        }

        return v;
    }

    public override string ToString() {
        var str = "";
        for (int r = 0; r < Row; r++) {
            for (int c = 0; c < Colmun; c++) {
                str += this[r, c] + ",";
            }
            str += "\n";
        }

        return str;
    }

    private Matrix Hadamard(Matrix m)
    {
        var newM = new Matrix(Row, Colmun);
        for (int r = 0; r < Row; r++) {
            for (int c = 0; c < Colmun; c++) {
                newM[r, c] = this[r, c] * m[r, c];
            }
        }
        return newM;
    }

    private Matrix Add(Matrix m)//must be same size
    {
        if (Row != m.Row | Colmun != m.Colmun)
        {
            throw new ArgumentException("rows and column must match");
        }
        var newM = new Matrix(Row, Colmun);
        for (int r = 0; r < Row; r++)
        {
            for (int c = 0; c < Colmun; c++)
            {
                newM[r, c] = this[r, c] + m[r, c];
            }
        }
        return newM;
    }
}
