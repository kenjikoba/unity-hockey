using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualController : MonoBehaviour
{
    public Rigidbody rb;
    public float moveSpeed = 1.5f;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //There is much room for improvement
        Vector3 now_pos = transform.position;
        Vector3 next_pos = transform.position;
        if (Input.GetKey(KeyCode.W))  {
            next_pos = now_pos + transform.forward * Time.deltaTime * moveSpeed;
            rb.MovePosition(next_pos);
        }

        if (Input.GetKey(KeyCode.S)) {
            next_pos = next_pos - transform.forward * Time.deltaTime * moveSpeed;
            rb.MovePosition(next_pos);
        }
        if (Input.GetKey(KeyCode.A))  {
            next_pos = next_pos - transform.right * Time.deltaTime * moveSpeed;
            rb.MovePosition(next_pos);
        }
        if (Input.GetKey(KeyCode.D)) {
            next_pos = next_pos + transform.right * Time.deltaTime * moveSpeed;
            rb.MovePosition(next_pos);
        }
    }
}
