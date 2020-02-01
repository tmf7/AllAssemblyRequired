
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
    private List<GameObject> IsTransparent = new List<GameObject>();

    private void Update()
    {
        var hits = Players.SelectMany(player =>
        {
            var distance = Vector3.Distance(player.transform.position, transform.position);
            Debug.DrawRay(transform.position, transform.forward * distance, Color.green);
            var detectedhits = Physics.RaycastAll(transform.position, transform.forward, distance, TransparentLayerMask);
            return detectedhits.Select(h => h.collider.gameObject);
        }).ToList();

        var removal = IsTransparent.Except(hits).ToList();
        foreach (var noTransObject in removal)
        {
            Fade(noTransObject, 1f);
            IsTransparent.Remove(noTransObject);
        }

        var newHits = hits.Except(IsTransparent).ToList();
        foreach (var newTransObj in newHits) 
        {
            Fade(newTransObj, goalAlpha);
            IsTransparent.Add(newTransObj);
        }
    }

    public void Fade(GameObject obj, float alpha)
    {
        var color = obj.GetComponent<Renderer>().material.color;
        color.a = alpha;
        obj.GetComponent<Renderer>().material.color = color;
    }
}