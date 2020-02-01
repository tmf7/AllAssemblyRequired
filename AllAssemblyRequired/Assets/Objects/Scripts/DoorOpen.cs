using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    private void Awake() {
        EventManager.current.onButtonPress += openDoor;
    }
    // Start is called before the first frame update
    private void openDoor() {
        // todo open the doors!
    }
}
