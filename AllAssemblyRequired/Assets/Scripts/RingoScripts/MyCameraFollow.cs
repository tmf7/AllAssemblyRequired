
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

    [SerializeField] float smoothing = 5f;                          //Amount of smoothing to apply to the cameras movement
    [SerializeField] Vector3 offset = new Vector3(0f, 15f, -22f);  //The offset of the camera from the player (how far back and above the player the camera should be)

    private void FixedUpdate()
    {
        Vector3 targetCamPos = Players.First().transform.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
    }

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
        var r = obj.GetComponentInParent<Renderer>();
        var m = r.material;
        r.material = newMat;
        return m;
    }
}