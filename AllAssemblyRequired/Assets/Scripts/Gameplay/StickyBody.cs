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

    private Dictionary<StickyJoint, Coroutine> _jointSetupCoroutines = new Dictionary<StickyJoint, Coroutine>();
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

    /// <summary>
    /// Ignores collision between this, the given StickyBody, and any StickyBodies attached to this and the given StickyBody
    /// This avoids chaotic collision during joint creation, and resumes collision when a joint is broken.
    /// </summary>
    public void IgnoreAllColliders(StickyBody other, bool ignore)
    {
        var allStickyBodies = new HashSet<StickyBody>();

        GetAllStickyBodies(allStickyBodies);
        other.GetAllStickyBodies(allStickyBodies);

        var allColliders = allStickyBodies.SelectMany(body => body.GetComponentsInChildren<Collider>(true)).ToArray();

        if (allColliders.Length >= 2)
        {
            for (int i = 0; i < allColliders.Length - 1; ++i)
            {
                for (int j = i + 1; j < allColliders.Length; ++j)
                {
                    Physics.IgnoreCollision(allColliders[i], allColliders[j], ignore);
                }
            }
        }
    }

    /// <summary>
    /// Attempts to move a non-root StickyBody to align with the given StickJoint's Anchor position and rotation
    /// Then creates a FixedJoint to permanently join this StickyBody with the given StickyJoint's StickyBody
    /// </summary>
    public void TryCreateJoint(StickyJoint ownedJoint, StickyJoint matchJoint)
    {
        if (matchJoint != null &&
            _stickyJoints.Contains(ownedJoint) &&
            !_jointSetupCoroutines.ContainsKey(matchJoint) &&
            !IsAttachedToRoot &&
            matchJoint.IsAttachedToRoot)
        {
            _jointSetupCoroutines[matchJoint] = StartCoroutine(MoveToAnchor(ownedJoint, matchJoint));
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
        _jointSetupCoroutines.Remove(matchJoint);
    }

    private void CreateJoint(StickyJoint ownedJoint, StickyJoint matchJoint)
    {
        var fixedJoint = Rigidbody.gameObject.AddComponent<FixedJoint>();
        fixedJoint.enablePreprocessing = true;
        fixedJoint.connectedBody = matchJoint.StickyBody.Rigidbody;
        _fixedJoints.Add(fixedJoint);
        ownedJoint.LinkToJoint(matchJoint);
    }

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

        // TODO: (FREEHILL 4 MAR 2020) something is making IsAttachedToRoot return false AFTER IsAttachedToRoot becomes true for both legs
        // SOLUTION(?): FixedUpdate/OnTriggerEnter gets called and does something odd?
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