using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System;

[Serializable]
public class ReplayMemory
{
    public int capacity;
    public int index;
    public List<object[]> memory;
}