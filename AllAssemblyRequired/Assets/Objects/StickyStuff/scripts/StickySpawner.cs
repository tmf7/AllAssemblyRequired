using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickySpawner : MonoBehaviour
{

    public GameObject[] Spawnables;
    void Start()
    {
        int index = Random.Range(0, Spawnables.Length);
        GameObject newStickyObject = Instantiate(Spawnables[index]);
        newStickyObject.transform.position = gameObject.transform.position;
    }

}
