﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerPlayer : MonoBehaviour
{
    public Rigidbody rb;
    private string computer_level;
    private string prev_computer_level;
    private int InputSize = 6;
    private int HiddenSize = 32;
    private int HiddenLayers = 1;
    private int OutputSize = 2;

    // Q学習
    private int ActionSize = 6;
    private int StateSize = 1024;

    private NNBrain brain;
    private QBrain QLBrain;
    private HockeyAgent agent;

    void Awake() {
        brain = new NNBrain(InputSize, HiddenSize, HiddenLayers, OutputSize);
        agent = GetComponent<HockeyAgent>();
        computer_level = "EASY";
        string brain_txt = System.IO.Path.Combine(Application.streamingAssetsPath, "ComputerBrains/"+computer_level+".txt");
        brain.Load(brain_txt);
        prev_computer_level = computer_level;
    }


    void SetComputerLevel(string next_level) {
        computer_level = next_level;
    }

    //敵エージェントのレベルをEasyにセット
    public void SetEasyBrain() {
        SetComputerLevel("BestBrain");
    }
    //敵エージェントのレベルをMediumにセット
    public void SetMediumBrain() {
        SetComputerLevel("MEDIUM");
    }
    //敵エージェントのレベルをHardにセット
    public void SetHardBrain() {
        SetComputerLevel("HARD");
    }
    //敵エージェントのQ学習にセット
    public void SetQBrain() {
        SetComputerLevel("Q");
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (!this.gameObject.activeSelf) {
            return;
        }
        //レベルに変更がある場合には、そのBrainをLoadする
        if (prev_computer_level != computer_level) {
            if (computer_level != "Q") {
                string brain_txt = System.IO.Path.Combine(Application.streamingAssetsPath, "ComputerBrains/"+computer_level+".txt");
                brain.Load(brain_txt);
            }
            else {
                QLBrain = new QBrain(StateSize, ActionSize);
                string brain_txt = System.IO.Path.Combine(Application.streamingAssetsPath, "ComputerBrains/"+computer_level+".txt");
                QLBrain.Load(brain_txt);
            }
        }
        if (computer_level != "Q") {
            //現在の状態を取得
            var observation = agent.CollectObservations();
            //行動を決定
            var action = brain.GetAction(observation);
            //行動の実施
            agent.AgentAction(action);

            agent.BattleTime = 0;
            if (agent.TimeUp) {
                agent.AgentReset();
            }
        }
        else {
            var CurrentState = agent.GetState();
            int ActionNo = QLBrain.GetAction(CurrentState); 
            var action = agent.ActionNumberToVectorAction(ActionNo);
            agent.AgentAction(action);
            if (agent.TimeUp) {
                agent.AgentReset();
            }
        }
        
        prev_computer_level = computer_level;
    }

    //対戦モードに入るときに外から呼ばれる関数
    public void Activate() {
        this.gameObject.SetActive(true);
    }
    //対戦モードから出るときに外から呼ばれる関数
    public void Inactivate() {
        this.gameObject.SetActive(false);
    }
}
