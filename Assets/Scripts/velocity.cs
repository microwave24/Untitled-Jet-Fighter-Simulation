using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class velocity : MonoBehaviour
{
    public Rigidbody rb;
    public float velo;
    // Start is called before the first frame update
    void Start()
    {
        rb.velocity = transform.forward * velo;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
