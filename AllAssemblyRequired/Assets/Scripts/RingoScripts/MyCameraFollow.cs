
//This script allows a camera to follow the player smoothly and without rotation

using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class MyCameraFollow : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> Players;
    [SerializeField]
    private LayerMask TransparentLayerMask;
    [SerializeField]
    private Material TransparentMaterial;

    private Dictionary<GameObject, Material> IsTransparent = new Dictionary<GameObject, Material>();

    private void Update()
    {
        var hits = Players.SelectMany(player =>
        {
            var distance = Vector3.Distance(player.transform.position, transform.position);
            Debug.DrawRay(transform.position, transform.forward * distance, Color.green);
            var detectedhits = Physics.RaycastAll(transform.position, transform.forward, distance, TransparentLayerMask);
            return detectedhits.Select(h => h.collider.gameObject);
        }).ToList();


        //remove the one that is 
        var keys = IsTransparent.Keys;
        var removal = keys.Except(hits).ToList();
        foreach (var noTransObject in removal)
        {
            Fade(noTransObject, IsTransparent[noTransObject]);
            IsTransparent.Remove(noTransObject);
        }

        var newHits = hits.Except(keys).ToList();
        foreach (var newTransObj in newHits) 
        {
            var oldMat = Fade(newTransObj, TransparentMaterial);
            IsTransparent.Add(newTransObj, oldMat);
        }
    }

    public Material Fade(GameObject obj, Material newMat)
    {
        var m = obj.GetComponent<Renderer>().material;
        obj.GetComponent<Renderer>().material = newMat;
        return m;
    }
}