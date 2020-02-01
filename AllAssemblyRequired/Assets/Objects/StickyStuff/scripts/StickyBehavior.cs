using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    public string animationID;
    public bool isConnected = false;
    public int id = 0;
    private void Awake() {
        gameObject.tag = "sticky";
    }
    void Start()
    {
        EventManager.current.onAnimationStart += playAnimation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void playAnimation(int requestId) {
        if (id == requestId) {
           Animator animator = gameObject.GetComponent<Animator>();
            if (animator != null) {
                animator.Play(animationID, -1, 0f);
            }
        }
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
                isConnected = true;
            }
        }
    }
}
