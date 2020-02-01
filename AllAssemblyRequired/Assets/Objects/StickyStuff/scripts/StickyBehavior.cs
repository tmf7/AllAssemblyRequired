using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isConnected = false;

    private void Awake() {
        gameObject.tag = "sticky";
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject collidingObject = collision.gameObject;
        StickyBehavior state = collidingObject.GetComponent<StickyBehavior>();

        if (collidingObject.tag == "sticky" && state != null && state.isConnected == false) {
            FixedJoint joint = collidingObject.AddComponent<FixedJoint>();
            if (joint != null && state != null) {
                joint.connectedBody = gameObject.GetComponent<Rigidbody>();
                state.isConnected = true;
            }
        }
    }
}
