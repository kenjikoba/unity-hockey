using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckBoxManager : MonoBehaviour
{
    [SerializeField] private string Mode = "Auto";
    private float prevTimeScale = 1.0f;
    public GameObject EasyCheckBox;
    public GameObject MediumCheckBox;
    public GameObject HardCheckBox;
    public GameObject MouseCheckBox;
    public GameObject KeyCheckBox;
    public GameObject QLearningBox;


    public string GetMode() {
        return Mode;
    }
    void Start() {
        //デフォルトは自動学習モード
        Mode = "Auto";
        EasyCheckBox.SetActive(false);
        MediumCheckBox.SetActive(false);
        HardCheckBox.SetActive(false);
        MouseCheckBox.SetActive(false);
        KeyCheckBox.SetActive(false);
        QLearningBox.SetActive(false);
    }


    public void OnToggle(bool check) {
        //学習モードの切り替え
        if (check) {
            //チェックボックスを見せる
            prevTimeScale = Time.timeScale;
            Mode = "Manual";
            Time.timeScale = 1.0f;
            EasyCheckBox.SetActive(true);
            MediumCheckBox.SetActive(true);
            HardCheckBox.SetActive(true);
            MouseCheckBox.SetActive(true);
            KeyCheckBox.SetActive(true);
            QLearningBox.SetActive(true);
        } else {
            //チェックボックスを隠す
            Mode = "Auto";
            Time.timeScale = prevTimeScale;
            EasyCheckBox.SetActive(false);
            MediumCheckBox.SetActive(false);
            HardCheckBox.SetActive(false);
            MouseCheckBox.SetActive(false);
            KeyCheckBox.SetActive(false);
            QLearningBox.SetActive(false);
        }
    }
}
