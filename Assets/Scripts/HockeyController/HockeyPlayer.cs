using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HockeyPlayer : MonoBehaviour
{
    public float maxSpeed = 0.1f;
    private Rigidbody rb;
    private Vector3 move_direction;
    private int ModeSign;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        move_direction = new Vector3(0,0,0);
        ModeSign = (transform.position.z < 0) ? 1 : -1;
    }

    public void Stop()
    {
        Reset();
    }
    
    //Reset Position and Velocity
    public void Reset()
    {
        move_direction = new Vector3(0,0,0);
    }

    //Limitation of Movement
    Vector3 clippingPos(Vector3 pos) {
        if (pos[0] > 0.52f) {
            pos[0] = 0.52f;
        } else if (pos[0] < -0.52f) {
            pos[0] = -0.52f;
        }
        if (ModeSign == 1) {
            if (pos[2] < -0.85f) {
                pos[2] = -0.85f;
            } else if (pos[2] > -0.1f) {
                pos[2] = -0.1f;
            }
        } else if (ModeSign == -1) {
            if (pos[2] > 0.85f) {
                pos[2] = 0.85f;
            } else if (pos[2] < 0.1f) {
                pos[2] = 0.1f;
            }
        }
        return pos;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 target_diff = new Vector3(move_direction[0], 0, move_direction[2]);
        if (target_diff.magnitude > maxSpeed) {
            target_diff = (target_diff / target_diff.magnitude) * maxSpeed;
        }
        rb.MovePosition(clippingPos(Vector3.Lerp(transform.position, transform.position + target_diff, 0.1f)));
    }

    //Use This Function To Move Player From Outside of This Script
    public void Move(double[] action) {
        move_direction[0] = (float)action[0];
        move_direction[2] = (float)action[1];
    }

}
