using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StickyBody : MonoBehaviour
{
    [SerializeField, Range(0.0f, 360.0f)] private float _rotationSpeed = 360.0f;
    [SerializeField, Range(0.0f, 50.0f)] private float _linearSpeed = 20.0f;
    [SerializeField, Range(0.001f, 1.0f)] private float _snapThreshold = 0.1f;
    [SerializeField, Range(0.5f, 500.0f)] private float _jointCreationTimeout = 4.0f;

    private StickyJoint[] _stickyJoints;

    public Rigidbody Rigidbody { get; private set; }
    public float Mass => Rigidbody.mass;
    public bool IsRoot => GetComponent<RootMovement>() != null;

    /// <summary> Ensures this won't generate chaotic forces when a joint is being created </summary>
    public void SetKinematic(bool isKinematic) => Rigidbody.isKinematic = isKinematic;

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

    /// <summary>
    /// Ignores collision between two StickyBodys' colliders.
    /// This is useful for creating their joint, and when their joint is broken, so collision detection can resume.
    /// </summary>
    public void IgnoreAttachedColliders(StickyBody other, bool ignore)
    {
        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            foreach (var attachedCollider in other.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(collider, attachedCollider, ignore);
            }
        }
    }

    /// <summary>Ignores collision to move the joint into position.</summary>
    private IEnumerator MatchAttachmentPoints(StickyJoint attachmentPoint)
    {
        float timeRemaining = _jointCreationTimeout;

        do
        {
            Rigidbody.rotation = Quaternion.RotateTowards(Rigidbody.rotation, attachmentPoint.Anchor.rotation, _rotationSpeed * Time.fixedDeltaTime);
            Rigidbody.position = Vector3.MoveTowards(Rigidbody.position, attachmentPoint.Anchor.position, _linearSpeed * Time.fixedDeltaTime);

            yield return new WaitForFixedUpdate();
            timeRemaining -= Time.fixedDeltaTime;

        } while ((Vector3.Distance(Rigidbody.position, attachmentPoint.Anchor.position) > _snapThreshold ||
                 Quaternion.Angle(Rigidbody.rotation, attachmentPoint.Anchor.rotation) > _snapThreshold) &&
                 timeRemaining > 0.0f);

        if (timeRemaining <= 0.0f)
        {
            //StopCreatingJoint(attachmentPoint);
        }
        else
        {
            SetKinematic(false);
            attachmentPoint.StickyBody.SetKinematic(false);

            _fixedJoint = Rigidbody.gameObject.AddComponent<FixedJoint>();
            _fixedJoint.enablePreprocessing = true;
            _fixedJoint.connectedBody = AttachedStickyJoint.Rigidbody;

            // TODO: (FREEHILL 3 MAR 2020) clean up the dictionary from any lingering Coroutines (kv pairs)
            // ...and on failure ensure the attachmentPoint returns to non-kinematic (and possibly gets destroyed/respawns elsewhere)
            _jointBuilders.Remove(attachmentPoint);
        }
    }

    // TODO: (FREEHILL 3 MAR 2020) keep a StickyBody kinematic so long as a new joint is being created
    // ...in which case the MatchAttachmentPoints coroutine should live on the StickyBody script and handle IsKinematic calls
    //private void StopCreatingJoint(StickyJoint attachmentPoint)
    //{
    //    IgnoreAttachedColliders(attachmentPoint.StickyBody, false);
    //    SetKinematic(false);
    //    attachmentPoint.StickyBody.SetKinematic(false);

    //    AttachedStickyJoint.AttachedStickyJoint = null;
    //    AttachedStickyJoint = null;

    //    if (_creatingJoint != null)
    //    {
    //        StopCoroutine(_creatingJoint);
    //        _creatingJoint = null;
    //    }
    //}

    private Dictionary<StickyJoint, Coroutine> _jointBuilders = new Dictionary<StickyJoint, Coroutine>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="attachmentPoint"></param>
    public void TryCreateJoint(StickyJoint attachmentPoint)
    {
        if (IsAttachedToRoot || 
            _jointBuilders.ContainsKey(attachmentPoint))
        {
            return;
        }

        IgnoreAttachedColliders(attachmentPoint.StickyBody, true);
        attachmentPoint.StickyBody.SetKinematic(true);
        SetKinematic(true);
        
        _jointBuilders[attachmentPoint] = StartCoroutine(MatchAttachmentPoints(attachmentPoint));
    }
}
