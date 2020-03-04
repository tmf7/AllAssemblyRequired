using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Put this directly on any transform that is intended to snap to another transform via a parent rigidbody</summary>
public class StickyJoint : MonoBehaviour
{
    [SerializeField] private ParticleSystem _hintParticles;
    [SerializeField] private float _hintRadius = 0.5f;

    private FixedJoint _fixedJoint;

    public StickyBody StickyBody { get; private set; }
    public StickyJoint AttachedStickyJoint { get; private set; }
    public StickyBody AttachedStickyBody => (AttachedStickyJoint != null ? AttachedStickyJoint.StickyBody : null);

    private bool IsAttachedToRoot => StickyBody.IsAttachedToRoot;
    private Rigidbody Rigidbody => StickyBody.Rigidbody;
    private Quaternion OpposingRotation => Quaternion.LookRotation(-transform.forward, transform.up);

    /// <summary>  Where this StickyJoint's Rigidbody should position itself in worldspace if there is an AttachedStickyJoint </summary>
    public Pose Anchor
    {
        get
        {
            if (AttachedStickyJoint != null)
            {
                return new Pose(AttachedStickyJoint.transform.position + (Rigidbody.position - transform.position),
                                AttachedStickyJoint.OpposingRotation * Quaternion.Inverse(transform.localRotation));
            }

            return new Pose(Rigidbody.position, Rigidbody.rotation);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (AttachedStickyJoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(Rigidbody.position, 0.3f);
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
        _hintParticles.gameObject.SetActive(!IsAttachedToRoot && IsOpenJointInRange());

        //CheckJointIntegrity();
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

    private void OnTriggerEnter(Collider otherCollider)
    {
        var hitStickyJoint = otherCollider.GetComponent<StickyJoint>();

        if (hitStickyJoint != null)
        {
            AttachedStickyJoint = hitStickyJoint;
            hitStickyJoint.AttachedStickyJoint = this;

            StickyBody.TryCreateJoint(hitStickyJoint);
        }
    }

    // TODO: (FREEHILL 26 FEB 2020) work out the JointIntegrity check (to avoid premature destroy call)
    // TODO: (FREEHILL 26 FEB 2020) Multiple FixedJoints are being created, while simultaneously the AttachedStickyJoints are being nulled
    // TODO: (FREEHILL 26 FEB 2020) work out when the pair should be set kinematic/non-kinematic in the event of a failure to connect
    private void CheckJointIntegrity()
    {
        // dont check if still creating the joint...or if the OTHER StickyJoint is creating the joint...given that only one will be creating the FixedJoint between them
        // and only one should move to meet the other
        bool isDisconnectedFromRoot = _creatingJoint == null && !IsAttachedToRoot;
        // !IsCreatingJoint && (AttachedStickyJoint != null && !AttachedStickyJoint.IsCreatingJoint)

        if (!isDisconnectedFromRoot)
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
}
