using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Put this directly on any transform that is intended to snap to another transform via a parent rigidbody</summary>
public class StickyJoint : MonoBehaviour
{

#if UNITY_EDITOR
    [CustomEditor(typeof(StickyJoint))]
    [CanEditMultipleObjects]
    public class StickyJointEditor : Editor
    {
        private StickyJoint stickyJoint;

        void OnEnable()
        {
            stickyJoint = target as StickyJoint;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            GUI.enabled = false;
            EditorGUILayout.ObjectField("StickyBody", stickyJoint.StickyBody, typeof(StickyBody), true);
            EditorGUILayout.ObjectField("AttachedStickyJoint", stickyJoint.AttachedStickyJoint, typeof(StickyJoint), true);
            GUI.enabled = true;
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif



    [SerializeField, Range(0.0f, 360.0f)] private float _rotationSpeed = 360.0f;
    [SerializeField, Range(0.0f, 50.0f)] private float _linearSpeed = 20.0f;
    [SerializeField, Range(0.001f, 1.0f)] private float _snapThreshold = 0.1f;
    [SerializeField, Range(0.5f, 500.0f)] private float _jointCreationTimeout = 400.0f;
    [SerializeField] private ParticleSystem _hintParticles;
    [SerializeField] private float _hintRadius = 0.5f;

    private FixedJoint _fixedJoint;
    private IEnumerator _creatingJoint = null;

    public StickyBody StickyBody { get; private set; }
    public StickyJoint AttachedStickyJoint { get; private set; }
    public StickyBody AttachedStickyBody => (AttachedStickyJoint != null ? AttachedStickyJoint.StickyBody : null);

    private bool IsCreatingJoint => _creatingJoint != null;
    private bool IsAttachedToRoot => StickyBody.IsAttachedToRoot;
    private Rigidbody Rigidbody => StickyBody.Rigidbody;
    private Quaternion OpposingRotation => Quaternion.LookRotation(-transform.forward, transform.up);

    /// <summary>  Where this StickyJoint's Rigidbody should position itself in worldspace if there is an AttachedStickyJoint </summary>
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

    private void OnDrawGizmosSelected()
    {
        if (AttachedStickyJoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(RigidbodyPosition, 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Anchor.position, 0.1f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _hintRadius);
    }

    private void Awake()
    {
        StickyBody = GetComponentInParent<StickyBody>();
    }

    private void FixedUpdate()
    {
        _hintParticles.gameObject.SetActive(IsOpenJointInRange());

        CheckJointIntegrity();
    }

    private bool IsOpenJointInRange()
    {
        var currentOverlapping = Physics.OverlapSphere(transform.position, _hintRadius);

        foreach (var possibleRoot in currentOverlapping)
        {
            var stickyJoint = possibleRoot.GetComponent<StickyJoint>();
            if (stickyJoint != null && 
                stickyJoint.StickyBody != StickyBody &&
                stickyJoint.AttachedStickyJoint == null)
            {
                return true;
            }
        }

        return false;
    }

    private void CheckJointIntegrity()
    {
        // dont check if still creating the joint...or if the OTHER StickyJoint is creating the joint...given that only one will be creating the FixedJoint between them
        // and only one should move to meet the other
        //bool isDisconnectedFromRoot = _creatingJoint == null && !IsAttachedToRoot; 

        // TODO: (FREEHILL 26 FEB 2020)  Multiple FixedJoints are being created, while simultaneously the AttachedStickyJoints are being nulled

        if (!IsCreatingJoint && (AttachedStickyJoint != null && !AttachedStickyJoint.IsCreatingJoint))
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
        var hitStickyJoint = otherCollider.GetComponent<StickyJoint>();

        if (hitStickyJoint != null && 
            !IsAttachedToRoot && // only allow one StickyJoint to create their FixedJoint bond
            AttachedStickyJoint == null &&
            hitStickyJoint.AttachedStickyJoint == null &&
            !IsCreatingJoint)
        {
            AttachedStickyJoint = hitStickyJoint;
            hitStickyJoint.AttachedStickyJoint = this;

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

    /// <summary>Ignores collision to move the joint into position.</summary>
    private IEnumerator MatchAttachmentPoints()
    {
        float timeRemaining = _jointCreationTimeout;
        do
        {
            RigidbodyRotation = Quaternion.RotateTowards(RigidbodyRotation, Anchor.rotation, _rotationSpeed * Time.fixedDeltaTime);
            RigidbodyPosition = Vector3.MoveTowards(RigidbodyPosition, Anchor.position, _linearSpeed * Time.fixedDeltaTime);

            yield return new WaitForFixedUpdate();
            timeRemaining -= Time.fixedDeltaTime;

        } while (Vector3.Distance(RigidbodyPosition, Anchor.position) > _snapThreshold &&
                 Quaternion.Angle(RigidbodyRotation, Anchor.rotation) > _snapThreshold &&
                 timeRemaining > 0.0f &&
                 _creatingJoint != null);

        if (timeRemaining <= 0.0f)
        {
            StopCreatingJoint();
        }
    }

    private void StopCreatingJoint()
    {
        IgnoreAttachedColliders(false);
        AttachedStickyJoint.AttachedStickyJoint = null;
        AttachedStickyJoint = null;

        if (_creatingJoint != null)
        {
            StopCoroutine(_creatingJoint);
            _creatingJoint = null;
        }
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
