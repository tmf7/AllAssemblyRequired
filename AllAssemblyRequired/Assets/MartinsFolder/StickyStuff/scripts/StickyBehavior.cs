using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    public string animationID = "move";
    public bool isRoot = false;
    public int jointBreakForce = 200000;
    public float maxSpeed = 5000000f;
    public AudioClip attachmentAudioClip;
    public float maxHeight = 1000f;

    private bool connectedToRoot = false;
    protected bool isConnected = false;
    public int id = 0;
    public float forceStrength = 1.0f;
    private Rigidbody rigidBodyComp;
    public int stickyLayer = 20;
    private Vector3 currentForce;
    private void Awake() {
        gameObject.layer = stickyLayer;
        rigidBodyComp = gameObject.GetComponent<Rigidbody>();

        if (rigidBodyComp == null) {
            rigidBodyComp =gameObject.AddComponent<Rigidbody>();
        }
        rigidBodyComp.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (isRoot) {
            isConnected = true;
        }
    }
    void Start()
    {
        EventManager.current.onAnimationStart += playAnimation;
        EventManager.current.onAddForce += addForce;
        EventManager.current.onStopForce += stopForce;
        EventManager.current.onForceBreak += ForceBreak;
    }

    // Update is called once per frame
    void Update()
    {
        rigidBodyComp.velocity += (currentForce * forceStrength / 2000);
        //if(rigidBodyComp.velocity.magnitude > maxSpeed){
         //   Debug.Log("max speed reached");
         //    rigidBodyComp.velocity = Vector3.ClampMagnitude(rigidBodyComp.velocity, maxSpeed);
         //}

        if(gameObject.transform.position.y > maxHeight)
        {
            //max height reached...
            //gameObject.transform.position = new Vector3 (gameObject.transform.position.x, , gameObject.transform.position.z);
            //rigidBodyComp.velocity = currentForce * 0;
        }
    }

    private void OnJointBreak(float breakForce) {
        breakConnection();
    }

    private void ForceBreak(int id)
    {
        breakConnection();
    }

    void breakConnection() {
        FixedJoint joint = gameObject.GetComponent<FixedJoint>();
        isConnected = false;
        connectedToRoot = false;

        if (joint != null) {
            Rigidbody childBody = joint.connectedBody;
            
            if (childBody != null) {
                GameObject child = childBody.gameObject;

                if (child != null) {
                    StickyBehavior sticky = child.GetComponent<StickyBehavior>();
                    sticky.breakConnection();
                }
            }
            
            
            print("destroying connection: " + gameObject.name);
            Destroy(joint);
        }
    }

    void addForce(int requestId, string forceDirection) {
        if (isRoot == true) {
            switch(forceDirection) {
                case "forward":
                    currentForce.z += 1;
                    break;
                case "backward":
                    currentForce.z -= 1;
                    break;
                case "right":
                    currentForce.x += 1;
                    break;
                case "left":
                    currentForce.x -= 1;
                    break;
                default:
                    print("unknown force direction " + forceDirection);
                    break;
            }
        } 
    }

    void stopForce(int requestId, string forceDirection) {
        if (isRoot == false) {
            return;
        } else if (id == requestId) {
            switch(forceDirection) {
                case "forward":
                    currentForce.z -= 1;
                    break;
                case "backward":
                    currentForce.z += 1;
                    break;
                case "right":
                    currentForce.x -= 1;
                    break;
                case "left":
                    currentForce.x += 1;
                    break;
                default:
                    print("unknown force direction " + forceDirection);
                    break;
            }
        } 
    }

    void playAnimation(int requestId) {
        if (isConnected && id == requestId) {
           Animator animator = gameObject.GetComponent<Animator>();
            if (animator != null) {
                animator.Play(animationID, -1, 0f);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isRoot == false && connectedToRoot == false) {
            return;
        }

        GameObject collidingObject = collision.gameObject;
        StickyBehavior state = collidingObject.GetComponent<StickyBehavior>();

        if (collidingObject.layer == stickyLayer && state != null && state.isConnected == false) {
            FixedJoint joint = collidingObject.AddComponent<FixedJoint>();
            if (joint != null && state != null) {
                joint.connectedBody = rigidBodyComp;
                joint.breakForce = jointBreakForce;
                isConnected = true;
                state.isConnected = true;
                state.connectedToRoot = true;
                state.playAttachmentSound();
            }
        }
    }

    void playAttachmentSound() {
        if (attachmentAudioClip != null) {
            SoundManager.Instance.PlaySoundFX(attachmentAudioClip, gameObject);
        }
    }
}
