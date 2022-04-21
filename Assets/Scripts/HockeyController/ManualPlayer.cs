using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualPlayer : MonoBehaviour
{
    public Rigidbody rb;
    public Camera mainCamera;
    public float moveSpeed = 1.5f;
    private string inputMode = "Key";


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        inputMode = "Key";
    }

    //移動範囲制限
    Vector3 clippingPos(Vector3 pos) {
        if (pos[0] > 0.52f) {
            pos[0] = 0.52f;
        } else if (pos[0] < -0.52f) {
            pos[0] = -0.52f;
        }
        if (pos[2] > 0.85f) {
            pos[2] = 0.85f;
        } else if (pos[2] < 0.1f) {
            pos[2] = 0.1f;
        }
        return pos;
    }

    //入力モードに切り替え
    public void MouseInput()
    {
        inputMode = "Mouse";
    }
    public void KeyInput()
    {
        inputMode = "Key";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.E)) {
            UnityEngine.Application.Quit();
        }
        if (!this.gameObject.activeSelf) {
            return;
        }
        Time.timeScale = 1.0f;
        //UnityEditor以外での起動の場合
        //タッチでその位置に移動
        //現状の実装では、カメラ位置によって位置がずれるかも
        if (!Application.isEditor) {
            if (Input.touchCount > 0) {
                Touch touch = Input.GetTouch(0);
                Vector3 touchScreenPosition = touch.position;
                touchScreenPosition.x = 0.7f + touchScreenPosition.x;
                touchScreenPosition.z = 0.7f + touchScreenPosition.y / 600.0f;
                Vector3 touchWorldPosition  = mainCamera.ScreenToWorldPoint( touchScreenPosition );
                touchWorldPosition.y = transform.position.y;
                Vector3 new_pos = Vector3.Lerp(transform.position, touchWorldPosition, 0.4f);
                new_pos = clippingPos(new_pos);
                rb.MovePosition(new_pos);
            }
        }

        //キーボードでのプレイヤーの位置操作
        if (inputMode == "Key") {
            Vector3 now_pos = transform.position;
            Vector3 next_pos = transform.position;
            if (Input.GetKey(KeyCode.W))  {
                next_pos = now_pos - transform.forward * Time.deltaTime * moveSpeed;
                next_pos = clippingPos(next_pos);
                rb.MovePosition(next_pos);
            }

            if (Input.GetKey(KeyCode.S)) {
                next_pos = next_pos + transform.forward * Time.deltaTime * moveSpeed;
                next_pos = clippingPos(next_pos);
                rb.MovePosition(next_pos);
            }
            if (Input.GetKey(KeyCode.A))  {
                next_pos = next_pos + transform.right * Time.deltaTime * moveSpeed;
                next_pos = clippingPos(next_pos);
                rb.MovePosition(next_pos);
            }
            if (Input.GetKey(KeyCode.D)) {
                next_pos = next_pos - transform.right * Time.deltaTime * moveSpeed;
                next_pos = clippingPos(next_pos);
                rb.MovePosition(next_pos);
            }
        }
        
        //マウスでのプレイヤーの位置操作
        if (inputMode == "Mouse") {
            Vector3 touchScreenPosition = Input.mousePosition;
            touchScreenPosition.x = 0.7f + touchScreenPosition.x;
            touchScreenPosition.z = 0.7f + touchScreenPosition.y / 600.0f;
            Vector3 touchWorldPosition  = mainCamera.ScreenToWorldPoint( touchScreenPosition );
            touchWorldPosition.y = transform.position.y;
            Vector3 new_pos = Vector3.Lerp(transform.position, touchWorldPosition, 0.4f);
            new_pos = clippingPos(new_pos);
            rb.MovePosition(new_pos);
        }

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
