using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Put this directly on any transform that is intended to snap to another transform via a parent rigidbody</summary>
public class StickyJoint : MonoBehaviour
{
    [SerializeField] private ParticleSystem _hintParticles;
    [SerializeField] private float _hintRadius = 0.5f;

    public StickyBody StickyBody { get; private set; }
    public StickyJoint AttachedStickyJoint { get; private set; }
    public StickyBody AttachedStickyBody => (AttachedStickyJoint != null ? AttachedStickyJoint.StickyBody : null);

    /// <summary> Returns true if this joint's StickyBody is directly, or indirectly attached to the root StickyBody via any of their joints </summary>
    public bool IsAttachedToRoot => StickyBody.IsAttachedToRoot;

    private Rigidbody Rigidbody => StickyBody.Rigidbody;
    private Quaternion OpposingRotation => Quaternion.LookRotation(-transform.forward, transform.up);

    /// <summary> Returns where this StickyJoint's Rigidbody should position itself in worldspace relative to the matchJoint, if any </summary>
    public Pose AnchorOn(StickyJoint matchJoint)
    {
        if (matchJoint != null)
        {
            return new Pose(matchJoint.transform.position + (Rigidbody.position - transform.position),
                            matchJoint.OpposingRotation * Quaternion.Inverse(transform.localRotation));
        }

        return new Pose(Rigidbody.position, Rigidbody.rotation);
    }

    private void OnDrawGizmosSelected()
    {
        if (AttachedStickyJoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(Rigidbody.position, 0.3f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(AnchorOn(AttachedStickyJoint).position, 0.1f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _hintRadius);
    }

    private void Awake()
    {
        StickyBody = GetComponentInParent<StickyBody>();
    }

    private void Update()
    {
        CheckJointIntegrity();
        _hintParticles.gameObject.SetActive(!IsAttachedToRoot && IsOpenJointInRange());
    }

    private void CheckJointIntegrity()
    {
        if (!IsAttachedToRoot && AttachedStickyJoint != null)
        {
            StickyBody.IgnoreAttachedColliders(AttachedStickyJoint.StickyBody, false);
            UnlinkFromJoint(AttachedStickyJoint);
        }
    }

    /// <summary> Returns true if there is an non-attached StickyJoint within the hint radius </summary>
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
        StickyBody.TryCreateJoint(this, otherCollider.GetComponent<StickyJoint>());
    }

    public void LinkToJoint(StickyJoint other)
    {
        if (other != null)
        {
            other.AttachedStickyJoint = this;
            AttachedStickyJoint = other;
        }
    }

    public void UnlinkFromJoint(StickyJoint other)
    {
        if (other != null)
        {
            other.AttachedStickyJoint = null;
            AttachedStickyJoint = null;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(StickyJoint))]
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
