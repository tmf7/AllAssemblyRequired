using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager current; 
    public event Action<int> onAnimationStart;
    public event Action<int> onAnimationEnd;
    private void Awake() {
        current = this;
    }

    private void Update() {
        if (Input.GetKeyDown("space")) {
            triggerAnimationStart(1);
        }    
    }

    
    public void triggerAnimationStart(int id) {
        if (onAnimationStart != null) {
            onAnimationStart(id);
        }
    }

    public void triggerAnimationEnd(int id) {
        if (onAnimationEnd != null) {
            onAnimationEnd(id);
        }
    }
}
