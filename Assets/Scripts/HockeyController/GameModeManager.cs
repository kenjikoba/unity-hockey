using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    // public DEEnvironment de;
    public QEnvironment qe;
    // public DQNEnvironment dqne;
    public ManualPlayer mp;
    public ComputerPlayer ap;
    public PackManager pm;
    public CheckBoxManager cbm;
    private string Mode;

    // Start is called before the first frame update
    void Start()
    {
       Mode = cbm.GetMode();
       pm.Reset();
    }

    // Update is called once per frame
    void Update()
    {
        //学習モードの時、得点が入るとプレイヤー・パックの位置をリセット
        if (Mode == "Auto") {
            // if (de.WaitingFlag) {
            //     pm.Reset();
            //     de.Reset();
            // }
            if (qe.WaitingFlag) {
                pm.Reset();
                qe.Reset();
            }
            // if (dqne.WaitingFlag) {
            //     pm.Reset();
            //     dqne.Reset();
            // }
            // if (de.RestartFlag) {
            //    pm.Reset();
            //    de.Restart();
            // }
            if (qe.RestartFlag) {
               pm.Reset();
               qe.Restart();
            }
            // if (dqne.RestartFlag) {
            //     pm.Reset();
            //     dqne.Restart();
            // }
        }

        //対戦モードの時は得点が入るとパックのみリセット
        if (Mode == "Manual") {
            if (pm.Goal) {
                pm.Reset();
            }
        }

        //モード切り替え時
        string CurrentMode = cbm.GetMode();
        if (Mode != CurrentMode) {
            Mode = CurrentMode;
            //手動プレイヤー、敵エージェントを非活性化
            //学習エージェントを活性化
            if (Mode == "Auto") {
                mp.Inactivate();
                ap.Inactivate();
                // de.Activate();
                // dqne.Activate();
                qe.Activate();
            }
            //学習エージェントを非活性化
            //手動プレイヤー、敵エージェントを活性化
            if (Mode == "Manual") {
                // de.Inactivate();
                // dqne.Inactivate();
                qe.Inactivate();
                mp.Activate();
                ap.Activate();
            }
        }
    }
}
