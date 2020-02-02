﻿using System;
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
    public event Action<int> onForceBreak;
    public event Action onButtonPress;

    [SerializeField]
    private float force = 1f;

    private void Awake() {
        if (current == null) {
            current = this;
        }
         else if (current != this)
            Destroy(this);
    }

    private void Start() {
        Cursor.visible = true;
    }

    private void Update() {
        // index 2 mapped to robot's right arm
        if (Input.GetKeyDown("space")) {
            triggerAnimationStart(2);
        } 

        // index 3 mapped to robot's left arm
        if (Input.GetKeyDown("r")) {
            triggerAnimationStart(3);
        }

        // index 4 mapped to octopus legs
        if (Input.GetKeyDown("t")) {
            triggerAnimationStart(4);
        }

        if (Input.GetKeyDown("y")) {
            triggerAnimationStart(5);
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

        if(Input.GetKeyDown("q"))
        {
            onForceBreak(1);
        }
    }

    public void triggerDoors() {
        if (onButtonPress != null) {
            onButtonPress();
            MyGameManager.Instance.EndGame();
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
