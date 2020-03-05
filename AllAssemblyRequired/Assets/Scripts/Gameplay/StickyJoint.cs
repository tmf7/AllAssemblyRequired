using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Put this directly on any transform that is intended to snap to another transform via a parent rigidbody</summary>
public class StickyJoint : MonoBehaviour
{
    private StickyJointHint _hint;

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
    }

    private void Awake()
    {
        StickyBody = GetComponentInParent<StickyBody>();
        _hint = GetComponentInChildren<StickyJointHint>(true);
    }

    private void Update()
    {
        CheckJointIntegrity();
        _hint.IsVisible = (AttachedStickyJoint == null && IsOpenJointInRange(true) || (IsOpenJointInRange(false) && IsAttachedToRoot));
    }

    private void CheckJointIntegrity()
    {
        if (!IsAttachedToRoot && AttachedStickyJoint != null)
        {
            StickyBody.IgnoreAllColliders(AttachedStickyJoint.StickyBody, false);
            UnlinkFromJoint(AttachedStickyJoint);
        }
    }

    /// <summary> Returns true if there is an non-attached StickyJoint within the hint radius </summary>
    private bool IsOpenJointInRange(bool checkForRoot)
    {
        foreach (var stickyJoint in _hint.GetAllJointsInRange())
        {
            if (stickyJoint.StickyBody != StickyBody &&
                ((checkForRoot && stickyJoint.IsAttachedToRoot) || !checkForRoot) &&
                stickyJoint.AttachedStickyJoint == null)
            {
                return true;
            }
        }

        return false;
    }

    public void TryCreateJointWith(StickyJoint other)
    {
        StickyBody.TryCreateJoint(this, other);
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
        private StickyJoint _stickyJoint;

        void OnEnable()
        {
            _stickyJoint = target as StickyJoint;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            GUI.enabled = false;
            EditorGUILayout.ObjectField("StickyBody", _stickyJoint.StickyBody, typeof(StickyBody), true);
            EditorGUILayout.ObjectField("AttachedStickyJoint", _stickyJoint.AttachedStickyJoint, typeof(StickyJoint), true);
            GUI.enabled = true;
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
