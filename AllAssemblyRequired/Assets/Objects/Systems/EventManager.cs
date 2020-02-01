using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager current; 
    public event Action<int> onAnimationStart;
    public event Action<int> onAnimationEnd;
    public event Action<int, string> onAddForce;
    public event Action<int, string> onStopForce;
    
    private void Awake() {
        current = this;
    }

    private void Start() {
        Cursor.visible = true;
    }

    private void Update() {
        if (Input.GetKeyDown("space")) {
            triggerAnimationStart(2);
        }    

        if (Input.GetKeyDown("w")) {
            triggerForce(1, "forward");
        }

        if (Input.GetKeyDown("s")) {
            triggerForce(1, "backward");
        }

        if (Input.GetKeyDown("a")) {
            triggerForce(1, "left");
        }

        if (Input.GetKeyDown("d")) {
            triggerForce(1, "right");
        }

        if (Input.GetKeyUp("w")) {
            stopForce(1, "forward");
        }

        if (Input.GetKeyUp("s")) {
            stopForce(1, "backward");
        }

        if (Input.GetKeyUp("a")) {
            stopForce(1, "left");
        }

        if (Input.GetKeyUp("d")) {
            stopForce(1, "right");
        }
    }

    public void triggerForce(int objectId, string forceDirection) {
        if (onAddForce != null) {
            onAddForce(objectId, forceDirection);
        }
    }

    public void stopForce(int objectId, string forceDirection) {
        if (onStopForce != null) {
            onStopForce(objectId, forceDirection);
        }
    }
    
    public void triggerAnimationStart(int objectId) {
        if (onAnimationStart != null) {
            onAnimationStart(objectId);
        }
    }

    public void triggerAnimationEnd(int objectId) {
        if (onAnimationEnd != null) {
            onAnimationEnd(objectId);
        }
    }
}
