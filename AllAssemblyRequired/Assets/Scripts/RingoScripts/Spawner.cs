using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public bool isSpawn = false;

    // Update is called once per frame
    void Update()
    {
        if(isSpawn) {
            transform.Rotate(0, 0, 50 * Time.deltaTime);
        }
    }
}
