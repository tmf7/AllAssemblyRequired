using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    public string animationID;
    public bool isRoot = false;
    protected bool isConnected = false;
    public int id = 0;
    public float forceStrength = 1.0f;
    private Rigidbody rigidBodyComp;
    private void Awake() {
        gameObject.tag = "sticky";
        rigidBodyComp = gameObject.GetComponent<Rigidbody>();

        if (isRoot == true) {
            rigidBodyComp.mass = 2.5f;
        }
    }
    void Start()
    {
        EventManager.current.onAnimationStart += playAnimation;
        EventManager.current.onAddForce += addForce;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void addForce(int requestId, string forceDirection) {
        if (isRoot == false) {
            return;
        } else if (id == requestId) {
            switch(forceDirection) {
                case "forward":
                    rigidBodyComp.AddForce(Vector3.forward * forceStrength);
                    break;
                case "backward":
                    rigidBodyComp.AddForce(Vector3.forward * -1 * forceStrength);
                    break;
                case "right":
                    rigidBodyComp.AddForce(Vector3.right * forceStrength);
                    break;
                case "left":
                    rigidBodyComp.AddForce(Vector3.right * -1 * forceStrength);
                    break;
                default:
                    print("unknown force direction " + forceDirection);
                    break;
            }
        } 
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
                joint.connectedBody = rigidBodyComp;
                state.isConnected = true;
                isConnected = true;
            }
        }
    }
}
