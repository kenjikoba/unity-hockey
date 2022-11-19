using UnityEngine;

public class NERuntime : MonoBehaviour
{
    [SerializeField] private TextAsset learningData = null;
    private TextAsset LearningData { get { return learningData; } }

    [SerializeField] private Agent agentPrefab = null;
    private Agent AgentPrefab { get { return agentPrefab; } }

    [SerializeField] private bool resetOnDone = false;
    private bool ResetOnDone { get { return resetOnDone; } }

    private Agent CurrentAgent { get; set; }
    private NNBrain CurrentBrain { get; set; }

    private void Start() {
        if(AgentPrefab == null || LearningData == null) {
            return;
        }

        CurrentAgent = Instantiate<Agent>(AgentPrefab);
        CurrentAgent.gameObject.SetActive(true);
        CurrentBrain = JsonUtility.FromJson<NNBrain>(LearningData.text);
    }

    private void FixedUpdate() {
        if(CurrentAgent == null) {
            return;
        }

        if(CurrentAgent.IsDone) {
            if(ResetOnDone) {
                CurrentAgent.Reset();
            }
        }
        else {
            var observations = CurrentAgent.DECollectObservations();
            CurrentAgent.AgentAction(CurrentBrain.GetAction(observations));
        }
    }
}
