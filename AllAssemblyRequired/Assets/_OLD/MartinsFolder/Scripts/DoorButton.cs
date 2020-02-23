using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorButton : MonoBehaviour
{
    private float massOnTop = 0;
    public float massToPress = 100;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody body = other.gameObject.GetComponent<Rigidbody>();

        if (body != null && massOnTop < massToPress) {
            massOnTop += body.mass;

            if (massOnTop >= massToPress) {
                EventManager.current.triggerDoors();
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        Rigidbody body = other.gameObject.GetComponent<Rigidbody>();

        if (body != null && massOnTop < massToPress) {
            massOnTop -= body.mass;
        }
    }
}
