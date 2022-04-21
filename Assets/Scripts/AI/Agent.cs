using System.Collections.Generic;
using UnityEngine;

public abstract class Agent : MonoBehaviour
{
    public bool IsDone { get; private set; }
    public bool IsFrozen { get; private set; }

    public float Reward { get; private set; }

    public void SetReward(float reward) {
        Reward = reward;
    }

    public void AddReward(float reward) {
        Reward += reward;
    }

    public abstract int GetState();

    public abstract List<double> CollectObservations();

    public abstract void AgentAction(double[] vectorAction);

    public abstract void AgentReset();

    public abstract void Stop();

    public abstract double[] ActionNumberToVectorAction(int ActionNumber);

    public void Done()
    {
        IsDone = true;
    }

    public void Freeze()
    {
        Stop();
        IsFrozen = true;
        //gameObject.SetActive(false);
    }

    public void Reset()
    {
        //gameObject.SetActive(true);
        Stop();
        AgentReset();
        IsDone = false;
        IsFrozen = false;
    }
}
