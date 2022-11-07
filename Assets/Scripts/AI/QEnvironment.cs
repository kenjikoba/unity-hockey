// Q学習、基本的に変わりはEpisode以外あんまない。
using System;
using UnityEngine;
using UnityEngine.UI;

public class QEnvironment : Environment
{
    // [SerializeField] private GameObject GObject; 消した
    [SerializeField] private GameObject gObject = null;
    private GameObject GObject => gObject;

    [SerializeField] private int actionSize = 6;
    private int ActionSize { get { return actionSize; } }

    [SerializeField] private int stateSize = 4;
    private int StateSize { get { return stateSize; } }

    private QBrain QLBrain;
    private Agent LearningAgent;

    private int PrevState;

    void Start()
    {
        QLBrain = new QBrain(StateSize, ActionSize);
        LearningAgent = GObject.GetComponent<Agent>();
        PrevState = LearningAgent.GetState();
    }

    private void FixedUpdate()
    {
        AgentUpdate(LearningAgent, QLBrain);
        if (LearningAgent.IsDone)
        {
            LearningAgent.Reset();
        }
    }

    private void AgentUpdate(Agent a, QBrain b)
    {
        int ActionNo = b.GetAction(PrevState);
        var action = a.ActionNumberToVectorAction(ActionNo);
        a.AgentAction(action);
        var NewState = a.GetState();
        b.UpdateTable(PrevState, NewState, ActionNo, a.Reward, a.IsDone);
        PrevState = NewState;
    }
}
