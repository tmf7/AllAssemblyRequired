
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
    private Dictionary<GameObject, Dictionary<Renderer, Material>> IsTransparent = new Dictionary<GameObject, Dictionary<Renderer, Material>>();

    [SerializeField] float smoothing = 5f;                          //Amount of smoothing to apply to the cameras movement
    [SerializeField] Vector3 offset = new Vector3(0f, 15f, -22f);  //The offset of the camera from the player (how far back and above the player the camera should be)

    private void FixedUpdate()
    {
        if(MyGameManager.Instance.Player != null)
        {
            Vector3 targetCamPos = MyGameManager.Instance.Player.transform.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
        }
    }

    private void Update()
    {
        var hits = (new List<GameObject>() { MyGameManager.Instance.Player }).SelectMany(player =>
        {
            var distance = Vector3.Distance(player.transform.position, transform.position);
            //Debug.DrawRay(transform.position, transform.forward * distance, Color.green);
            var detectedhits = Physics.RaycastAll(transform.position, transform.forward, distance, TransparentLayerMask);
            return detectedhits.Select(h => h.collider.gameObject);
        }).ToList();

        //remove the one that is 
        var keys = IsTransparent.Keys;
        var removal = keys.Except(hits).ToList();
        foreach (var noTransObject in removal)
        {
            var rs = noTransObject.GetComponentsInParent<Renderer>();
            foreach (var r in rs)
            {
                Fade(r, IsTransparent[noTransObject][r]);
            }
            IsTransparent.Remove(noTransObject);
        }

        var newHits = hits.Except(keys).ToList();
        foreach (var newTransObj in newHits)
        {
            var rs = newTransObj.GetComponentsInParent<Renderer>();
            var old = rs.Aggregate(new Dictionary<Renderer, Material>(), (acc, cur) => {
                var oldMat = Fade(cur, TransparentMaterial);
                acc.Add(cur, oldMat);
                return acc; 
            });
            IsTransparent.Add(newTransObj, old);
        }
    }

    public Material Fade(Renderer obj, Material newMat)
    {
        var r = obj.GetComponentInParent<Renderer>();
        var m = r.material;
        r.material = newMat;
        return m;
    }
}