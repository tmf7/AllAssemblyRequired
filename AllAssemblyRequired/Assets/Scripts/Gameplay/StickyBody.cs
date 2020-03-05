using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Rigidbody))]
public class StickyBody : MonoBehaviour
{
    [SerializeField, Range(0.0f, 360.0f)] private float _rotationSpeed = 360.0f;
    [SerializeField, Range(0.0f, 50.0f)] private float _linearSpeed = 20.0f;
    [SerializeField, Range(0.001f, 1.0f)] private float _snapThreshold = 0.1f;
    [SerializeField, Range(0.5f, 500.0f)] private float _jointCreationTimeout = 4.0f;

    private class JointSetupCoroutine
    {
        public StickyJoint MatchJoint { get; private set; }
        public Coroutine Coroutine { get; private set; }

        public JointSetupCoroutine(StickyJoint matchJoint, Coroutine coroutine)
        {
            MatchJoint = matchJoint;
            Coroutine = coroutine;
        }
    }

    private JointSetupCoroutine _jointSetupCoroutine = null;
    private HashSet<StickyBody> _ignoredCollisionBodies = new HashSet<StickyBody>();
    private List<FixedJoint> _fixedJoints = new List<FixedJoint>();
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
            return allStickyBodies.Any(stickyBody => stickyBody.IsRoot);
        }
    }

    /// <summary> Returns the sum total mass of all StickyBodies attached to this StickyBody </summary>
    public float TotalStickyMass
    {
        get
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
    }

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        _stickyJoints = GetComponentsInChildren<StickyJoint>();
    }

    private void Update()
    {
        CleanupFixedJoints();
    }

    /// <summary> Populates the given set with this StickyBody and all StickyBodies attached it via all of its StickyJoints </summary>
    public void GetAllStickyBodies(HashSet<StickyBody> allStickyBodies)
    {
        if (!allStickyBodies.Contains(this))
        {
            allStickyBodies.Add(this);

            if (_stickyJoints != null)
            {
                for (int i = 0; i < _stickyJoints.Length; ++i)
                {
                    var attachedStickyBody = _stickyJoints[i].AttachedStickyBody;

                    if (attachedStickyBody != null)
                    {
                        attachedStickyBody.GetAllStickyBodies(allStickyBodies);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Returns true if there is any StickyBody moving to occupy the given StickyJoint, returns false otherwise
    /// </summary>
    /// <param name="ownedJoint"> The joint to check if there's StickyBody moving to occupy with one if its joints </param>
    public bool IsCreatingJointFor(StickyJoint ownedJoint)
    {
        if (ownedJoint != null && _stickyJoints.Contains(ownedJoint))
        {
            return _ignoredCollisionBodies.Any(body => (body._jointSetupCoroutine != null && body._jointSetupCoroutine.MatchJoint == ownedJoint));
        }

        return false;
    }

    /// <summary>
    /// Attempts to move a non-root StickyBody to align with the given StickJoint's Anchor position and rotation
    /// Then creates a FixedJoint to permanently join this StickyBody with the given StickyJoint's StickyBody
    /// </summary>
    public void TryCreateJoint(StickyJoint ownedJoint, StickyJoint matchJoint)
    {
        if (matchJoint != null &&                                 // do not form a joint with nothingness
            _stickyJoints.Contains(ownedJoint) &&                 // confirm this body owns the ownedJoint so the correct Anchor position can be calulated
            _jointSetupCoroutine == null &&                       // this body is not already creating a joint
            !IsAttachedToRoot &&                                  // this body is not part of the root assembly
            matchJoint.IsAttachedToRoot &&                        // the match joint is part of the root assembly
            matchJoint.AttachedStickyJoint == null &&             // nothing is already attached to the match joint
            !matchJoint.IsCreatingJoint)                          // nothing moving to occpuy the same match joint
        {
            _jointSetupCoroutine = new JointSetupCoroutine(matchJoint, StartCoroutine(MoveToAnchor(ownedJoint, matchJoint)));
        }
    }

    /// <summary>
    /// Ignores collision between this, the given StickyBody, and any StickyBodies attached to this and the given StickyBody
    /// This avoids chaotic collision during joint creation, and resumes collision when a joint is broken.
    /// </summary>
    public void IgnoreAllColliders(StickyBody other, bool ignore)
    {
        var allStickyBodies = new HashSet<StickyBody>();
        var allOtherStickyBodies = new HashSet<StickyBody>();

        GetAllStickyBodies(allStickyBodies);
        other.GetAllStickyBodies(allOtherStickyBodies);

        // account for prior calls to IgnoreAllColliders by other StickyBodies
        // for example: another StickyBody that is still moving to form a joint but not officially attached
        // to avoid a three-stooges-in-the-doorway situation
        foreach (var body in _ignoredCollisionBodies)
        {
            allStickyBodies.Add(body);
        }

        foreach (var otherBody in other._ignoredCollisionBodies)
        {
            allOtherStickyBodies.Add(otherBody);
        }

        UpdateIgnoredCollisionBodies(allOtherStickyBodies, ignore);
        other.UpdateIgnoredCollisionBodies(allStickyBodies, ignore);

        // stop ignoring collision between the this StickyBody and all other incoming StickyBodies, NOT amongst my own attached/attaching StickyBodies
        var allColliders = allStickyBodies.SelectMany(body => body.GetComponentsInChildren<Collider>(true)).ToArray();
        var allOtherColliders = allOtherStickyBodies.SelectMany(otherBody => otherBody.GetComponentsInChildren<Collider>(true)).ToArray();

        foreach (var collider in allColliders)
        {
            foreach (var otherCollider in allOtherColliders)
            {
                Physics.IgnoreCollision(collider, otherCollider, ignore);
            }
        }
    }

    private void UpdateIgnoredCollisionBodies(HashSet<StickyBody> otherStickyBodies, bool ignore)
    {
        if (ignore)
        {
            foreach (var otherBody in otherStickyBodies)
            {
                _ignoredCollisionBodies.Add(otherBody);
            }
        }
        else
        {
            _ignoredCollisionBodies.RemoveWhere(ignoredBody => otherStickyBodies.Contains(ignoredBody));
        }
    }

    /// <summary> Destroy all FixedJoints whose connectedBodies have been destroyed </summary>
    private void CleanupFixedJoints()
    {
        for (int i = 0; i < _fixedJoints.Count; ++i)
        {
            if (_fixedJoints[i] != null && _fixedJoints[i].connectedBody == null)
            {
                Destroy(_fixedJoints[i]);
                _fixedJoints[i] = null;
            }
        }

        _fixedJoints.RemoveAll(joint => joint == null);
    }

    /// <summary>Ignores collision to move the joint into position.</summary>
    // TODO: (FREEHILL 4 MAR 2020) this doesn't account for the attachmentPoint being destroyed while this is moving into place
    private IEnumerator MoveToAnchor(StickyJoint ownedJoint, StickyJoint matchJoint)
    {
        IgnoreAllColliders(matchJoint.StickyBody, true);
        matchJoint.StickyBody.SetKinematic(true);
        SetKinematic(true);

        float timeRemaining = _jointCreationTimeout;

        do
        {
            Rigidbody.rotation = Quaternion.RotateTowards(Rigidbody.rotation, ownedJoint.AnchorOn(matchJoint).rotation, _rotationSpeed * Time.fixedDeltaTime);
            Rigidbody.position = Vector3.MoveTowards(Rigidbody.position, ownedJoint.AnchorOn(matchJoint).position, _linearSpeed * Time.fixedDeltaTime);

            yield return new WaitForFixedUpdate();
            timeRemaining -= Time.fixedDeltaTime;

        } while ((Vector3.Distance(Rigidbody.position, ownedJoint.AnchorOn(matchJoint).position) > _snapThreshold ||
                 Quaternion.Angle(Rigidbody.rotation, ownedJoint.AnchorOn(matchJoint).rotation) > _snapThreshold) &&
                 timeRemaining > 0.0f);

        if (timeRemaining > 0.0f)
        {
            CreateJoint(ownedJoint, matchJoint);
        }
        else
        {
            DontCreateJoint(ownedJoint, matchJoint);
        }

        SetKinematic(false);
        matchJoint.StickyBody.SetKinematic(false);
        _jointSetupCoroutine = null;
    }

    private void CreateJoint(StickyJoint ownedJoint, StickyJoint matchJoint)
    {
        var fixedJoint = Rigidbody.gameObject.AddComponent<FixedJoint>();
        fixedJoint.enablePreprocessing = true;
        fixedJoint.connectedBody = matchJoint.StickyBody.Rigidbody;
        _fixedJoints.Add(fixedJoint);
        ownedJoint.LinkToJoint(matchJoint);
    }

    // TODO: (FREEHILL 5 MAR 2020) possibly destroy or teleport this StickyBody so it doesn't linger alongside the matchJoint
    private void DontCreateJoint(StickyJoint ownedJoint, StickyJoint matchJoint)
    {
        IgnoreAllColliders(matchJoint.StickyBody, false);
        ownedJoint.UnlinkFromJoint(matchJoint);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(StickyBody))]
    public class StickyBodyEditor : Editor
    {
        private StickyBody _stickyBody;

        void OnEnable()
        {
            _stickyBody = target as StickyBody;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            GUI.enabled = false;
            EditorGUILayout.Toggle(nameof(IsAttachedToRoot), _stickyBody.IsAttachedToRoot);
            GUI.enabled = true;
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}