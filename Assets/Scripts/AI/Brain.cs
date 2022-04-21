using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public abstract class Brain
{
    public float Reward { get; set; }

    public abstract void Save(string path);

    public abstract void Load(string path);
}
