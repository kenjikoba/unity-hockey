using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HockeyPlayer : MonoBehaviour
{
    public float maxSpeed = 0.1f;
    private Rigidbody rb;
    private Vector3 move_direction;
    private int ModeSign;

    // 最初のフレームの前に呼ばれる
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        move_direction = new Vector3(0,0,0);
        // ModeSign: 1ならz<0側が陣地　-1ならz>0側が陣地
        ModeSign = (transform.position.z < 0) ? 1 : -1;
    }

    public void Stop()
    {
        Reset();
    }
    
    // 速度と位置をリセット
    public void Reset()
    {
        move_direction = new Vector3(0,0,0);
    }

    //　陣地外に行くことができないように動きを制限
    Vector3 clippingPos(Vector3 pos) {
        // 横方向
        if (pos[0] > 0.52f) {
            pos[0] = 0.52f;
        } else if (pos[0] < -0.52f) {
            pos[0] = -0.52f;
        }
        // 縦方向
        if (ModeSign == 1) {
            // z<0 が陣地のとき
            if (pos[2] < -0.85f) {
                pos[2] = -0.85f;
            } else if (pos[2] > -0.35f) {
                pos[2] = -0.35f; // 変えた
            }
        } else if (ModeSign == -1) {
            // z>0 が陣地のとき
            if (pos[2] > 0.85f) {
                pos[2] = 0.85f;
            } else if (pos[2] < 0.35f) {
                pos[2] = 0.35f; //　変えた
            }
        }
        return pos;
    }

    // 1フレームごとに呼ばれ位置が更新される
    void FixedUpdate()
    {
        // 移動方向
        Vector3 target_diff = new Vector3(move_direction[0], 0, move_direction[2]);

        // target_diffの大きさがmaxSpeedより大きい場合はmaxSpeedにする
        if (target_diff.magnitude > maxSpeed) {
            target_diff = (target_diff / target_diff.magnitude) * maxSpeed;
        }

        // プレーヤーの位置を更新 
        //  clippingPosで動きを制限
        //  [現在地]　→ [現在地+(target_diff)] のベクトルの0.1倍だけプレーヤーを動かす
        rb.MovePosition(clippingPos(Vector3.Lerp(transform.position, transform.position + target_diff, 0.1f)));
    }

    //　HockeyPlayer.cs外からplayerを動かす時はこの関数を使う. move_directionはグローバル
    public void Move(double[] action) {
        move_direction[0] = (float)action[0];
        move_direction[2] = (float)action[1];
    }

}
