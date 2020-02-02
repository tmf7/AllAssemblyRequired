using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum GameState
{
    Spawn, 
    Playing,
    End
}

public class MyGameManager : MonoBehaviour
{
    public static MyGameManager Instance;             //This script, like MouseLocation, has a public static reference to itself to that other scripts
    public List<GameObject> Prefabs;
    public List<GameObject> Players;
    public int PlayerCount = 1;
    public Transform SpawnOrigin;

    public int maxParts = 10;

    //[SerializeField]
    //private string[] PrefabPaths;
    private GameState State;

    void Awake()
    {
        //This is a common approach to handling a class with a reference to itself.
        //If instance variable doesn't exist, assign this object to it
        if (Instance == null)
            Instance = this;
        //Otherwise, if the instance variable does exist, but it isn't this object, destroy this object.
        //This is useful so that we cannot have more than one GameManager object in a scene at a time.
        else if (Instance != this)
            Destroy(this);
    }

    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        switch(this.State)
        {
            case (GameState.Spawn):
            {
                if(this.maxParts == 0) {
                    this.State = GameState.Playing;
                } else if(maxParts == 1) {
                    this.Spawn(true);
                }
                this.Spawn();
                break;
            }
            case (GameState.Playing):
            {
                    //at some point game ends...
                    break;
            }
            case (GameState.End):
            {
                    break;
            }
            default:
            {
                    Debug.Log("current state is not acccounted of... wtf happneed?");
                    break;
            }
        }
    }

    void Spawn(bool isRoot = false)
    {
        //var max = this.Prefabs.Count() - 1;
        //var index = Random.Range(0, max);
        //var chosenPrefab = this.Prefabs[index];

        //var newObj = Instantiate(chosenPrefab);

        //if(isRoot == true)
        //{
        //    newObj.GetComponent<StickyBehavior>().isRoot = true;
        //}
        //create the game object at spawner
        this.maxParts -= 1;
    }

    void SpawnMotion(GameObject obj, GameObject targetSpawn) {
        //fly up
       
    }
}
