
//This script allows a camera to follow the player smoothly and without rotation

using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class MyCameraFollow : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> Players;
    public LayerMask TransparentLayerMask;
    public float goalAlpha;
    public Material TranspMaterial;

    private void Update()
    {
        foreach(var player in Players) 
        {
            var distance = Vector3.Distance(player.transform.position, transform.position);
            Debug.DrawRay(transform.position, transform.forward * distance, Color.green);
            var hits = Physics.RaycastAll(transform.position, transform.forward, distance, TransparentLayerMask);
            if (hits.Length > 0)
            {
                Debug.Log(string.Join(",", hits.Select(hit => hit.collider.gameObject.name)));
            }
        }
    }

    public void FadeOut(GameObject obj, float alpha)
    {
        var color = obj.gameObject.GetComponent<Renderer>().material.color;
        color.a = 0.5f;

    }
}