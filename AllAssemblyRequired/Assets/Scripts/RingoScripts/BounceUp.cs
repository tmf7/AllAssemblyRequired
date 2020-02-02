using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BounceUp : MonoBehaviour
{
    [SerializeField] int BounceForce = 10;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject.tag == "sticky" || other.gameObject.layer == 20)
        //{
            var rb = other.gameObject.GetComponentsInParent<Rigidbody>().FirstOrDefault();
            if(rb != null)
            {
                var targetVel = Vector3.up * BounceForce * (rb.mass * 0.5f);
                Debug.Log("fly object fly!" + targetVel.ToString());

            //rb.velocity = targetVel.y > 100000 ? new Vector3(targetVel.x, 500000, targetVel.z) : targetVel;
            rb.velocity = targetVel;
            }
        //}
    }
}
