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
    
    private void Awake() {
        current = this;
    }

    private void Start() {
        Cursor.visible = true;
    }

    private void Update() {
        if (Input.GetKeyDown("space")) {
            triggerAnimationStart(1);
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
    }

    public void triggerForce(int objectId, string forceDirection) {
        if (onAddForce != null) {
            onAddForce(objectId, forceDirection);
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
