using System.Collections;
using UnityEngine;

/// <summary>Put this directly on any transform that is intended to snap to another transform via a parent rigidbody</summary>
public class StickyJoint : MonoBehaviour
{
    [SerializeField, Range(0.0f, 360.0f)] private float _rotationSpeed = 360.0f;
    [SerializeField, Range(0.0f, 50.0f)] private float _linearSpeed = 20.0f;
    [SerializeField, Range(0.001f, 1.0f)] private float _snapThreshold = 0.1f;

    private FixedJoint _fixedJoint;
    private IEnumerator _creatingJoint = null;

    public StickyBody StickyBody { get; private set; }
    public StickyJoint AttachedStickyJoint { get; private set; }
    public StickyBody AttachedStickyBody => (AttachedStickyJoint != null ? AttachedStickyJoint.StickyBody : null);
    private bool IsAttachedToRoot => StickyBody.IsAttachedToRoot;

    private Rigidbody Rigidbody => StickyBody.Rigidbody;
    private Quaternion OpposingRotation => Quaternion.LookRotation(-transform.forward, transform.up);

    private Pose Anchor
    {
        get
        {
            if (AttachedStickyJoint != null)
            {
                return new Pose(AttachedStickyJoint.transform.position + (AttachedStickyJoint.transform.forward * transform.localPosition.magnitude),
                                AttachedStickyJoint.OpposingRotation * Quaternion.Inverse(transform.localRotation));
            }

            return new Pose(RigidbodyPosition, RigidbodyRotation);
        }
    }

    private Quaternion RigidbodyRotation
    {
        get { return Rigidbody.rotation; }
        set { Rigidbody.rotation = value; }
    }

    private Vector3 RigidbodyPosition
    {
        get { return Rigidbody.position; }
        set { Rigidbody.position = value; }
    }

    private void OnDrawGizmos()
    {
        if (AttachedStickyJoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(RigidbodyPosition, 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Anchor.position, 0.1f);
        }
    }

    private void Awake()
    {
        StickyBody = GetComponentInParent<StickyBody>();
    }

    private void FixedUpdate()
    {
        CheckJointIntegrity();
    }

    private void CheckJointIntegrity()
    {
        bool isDisconnectedFromRoot = _creatingJoint == null && !IsAttachedToRoot;

        if (isDisconnectedFromRoot)
        {
            if (_fixedJoint != null && _fixedJoint.connectedBody == null) // the joint is intact, but the connected rigidbody is destroyed
            {
                Destroy(_fixedJoint);
                _fixedJoint = null;
                AttachedStickyJoint = null;
            }
            else if (_fixedJoint == null && AttachedStickyJoint != null) // the joint is destroyed, but the attached body is intact
            {
                IgnoreAttachedColliders(false);
                AttachedStickyJoint.AttachedStickyJoint = null;
                AttachedStickyJoint = null;
            }
        }
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        var hitObject = otherCollider.GetComponent<StickyJoint>();

        if (hitObject != null && 
            !IsAttachedToRoot &&
            AttachedStickyJoint == null &&
            hitObject.AttachedStickyJoint == null)
        {
            AttachedStickyJoint = hitObject;
            hitObject.AttachedStickyJoint = this;

            _creatingJoint = CreateJoint();
            StartCoroutine(_creatingJoint);
        }
    }

    /// <summary>
    /// Ignores collision between this StickyBody's colliders and the AttachedStickyBody's colliders
    /// while the two are joined, when the joint is broken, collision detection resumes.
    /// NOTE: Its irrelevant to fix the collision if the AttachedStickyBody is Destroyed, but relevant if only the FixedJoint is broken.
    /// </summary>
    private void IgnoreAttachedColliders(bool ignore)
    {
        foreach (var collider in StickyBody.GetComponentsInChildren<Collider>())
        {
            foreach (var attachedCollider in AttachedStickyBody.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(collider, attachedCollider, ignore);
            }
        }
    }

    // TODO: possibly add a timeout where this gives up trying to match positions and falls away
    /// <summary>Ignores collision to move the joint into position.</summary>
    private IEnumerator MatchAttachmentPoints()
    {
        do
        {
            RigidbodyRotation = Quaternion.RotateTowards(RigidbodyRotation, Anchor.rotation, _rotationSpeed * Time.fixedDeltaTime);
            RigidbodyPosition = Vector3.MoveTowards(RigidbodyPosition, Anchor.position, _linearSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        } while (Vector3.Distance(RigidbodyPosition, Anchor.position) > _snapThreshold &&
                 Quaternion.Angle(RigidbodyRotation, Anchor.rotation) > _snapThreshold &&
                 _creatingJoint != null);

    }

    private IEnumerator CreateJoint()
    {
        IgnoreAttachedColliders(true);
        yield return StartCoroutine(MatchAttachmentPoints());

        _fixedJoint = Rigidbody.gameObject.AddComponent<FixedJoint>();
        _fixedJoint.enablePreprocessing = true;
        _fixedJoint.connectedBody = AttachedStickyJoint.Rigidbody;
        _creatingJoint = null;
    }
}
