using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StickyBody : MonoBehaviour
{
    private StickyJoint[] _stickyJoints;

    public Rigidbody Rigidbody { get; private set; }
    public float Mass => Rigidbody.mass;
    public bool IsRoot => GetComponent<RootMovement>() != null;

    public bool IsAttachedToRoot
    {
        get
        {
            var allStickyBodies = new HashSet<StickyBody>();
            GetAllStickyBodies(allStickyBodies);
            return allStickyBodies.Count(stickyBody => stickyBody.IsRoot) > 0;
        }
    }

    public float GetTotalStickyMass()
    {
        float totalStickyMass = 0.0f;
        var allStickyBodies = new HashSet<StickyBody>();

        GetAllStickyBodies(allStickyBodies);

        foreach (var stickyBody in allStickyBodies)
        {
            totalStickyMass += stickyBody.Mass;
        }

        return totalStickyMass;
    }

    public void GetAllStickyBodies(HashSet<StickyBody> allStickyBodies)
    {
        if (!allStickyBodies.Contains(this))
        {
            allStickyBodies.Add(this);

            foreach (var joint in _stickyJoints)
            {
                var attachedStickyBody = joint.AttachedStickyBody;

                if (attachedStickyBody != null)
                {
                    attachedStickyBody.GetAllStickyBodies(allStickyBodies);
                }
            }
        }
    }

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        _stickyJoints = GetComponentsInChildren<StickyJoint>();
    }
}
