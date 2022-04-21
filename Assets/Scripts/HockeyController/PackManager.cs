using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackManager : MonoBehaviour
{
    //Packの最大速度
    public float maxSpeed = 2.0f;
    
    public bool Goal = false;
    public GameObject Player1;
    public GameObject Player2;

    private HockeyAgent Player1Agent;
    private HockeyAgent Player2Agent;
    private Rigidbody rb;
    
    private int HitByPlayerCounter = 0;
    private bool ExtendTime = false;
    private float ExtendTimeLength = 100f;
    private Vector3 StartPosition;
    
    // Start is called before the first frame update
    void Start()
    {
        Player1Agent = Player1.GetComponent<HockeyAgent>();
        Player2Agent = Player2.GetComponent<HockeyAgent>();
        rb = GetComponent<Rigidbody>();
        StartPosition = transform.position;
    }


    public void ResetTime()
    {
        ExtendTimeLength = 10f;
    }

    public void Reset()
    {
        //ランダムな初期速度を持たせて、初期位置に戻す
        Vector3 rand_diff = Vector3.zero;
        transform.position = StartPosition + rand_diff;
        float rand_value = UnityEngine.Random.Range(0,1.0f);
        if (rand_value < 0.5f) {
            rb.velocity = new Vector3(UnityEngine.Random.Range(-0.01f,0.01f),0,UnityEngine.Random.Range(0.3f,0.5f));
        } else {
            rb.velocity = new Vector3(UnityEngine.Random.Range(-0.01f,0.01f),0,UnityEngine.Random.Range(-0.5f,-0.3f));
        } 
        rb.ResetInertiaTensor();
        Goal = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //ゴール判定
        if (transform.position.z > 1.03f || transform.position.z < -1.03f) {
            Goal = true;
            return;
        }
        //パックの速度を最大速度によって制限
        if (rb.velocity.magnitude >= maxSpeed) {
            rb.velocity = (rb.velocity / rb.velocity.magnitude) * maxSpeed;
        }

        //時間延長
        if (HitByPlayerCounter > 0) {
            if (rb.velocity.z > 0.1f || rb.velocity.z < -0.1f) {
                ExtendTime = true;
            }
            HitByPlayerCounter--;
        }

        if (ExtendTime) { 
            Player1Agent.BattleTime -= ExtendTimeLength;
            Player2Agent.BattleTime -= ExtendTimeLength;
            ExtendTimeLength /= 2;
            ExtendTime = false;
        }
        
        //ゴールに向かって無聊な力で吸い込まれる
        int ForceDirection = transform.position.z > 0 ? 1 : -1;
        Vector3 force = new Vector3(-transform.position.x, 0, transform.position.z * ForceDirection);
        rb.AddForce(force * 0.000001f);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player") {
            HitByPlayerCounter = 10;
        }
    }
}   
