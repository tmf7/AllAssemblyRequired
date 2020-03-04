﻿using System.Collections;
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
        IgnoreAttachedColliders(matchJoint.StickyBody, true);
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
        IgnoreAttachedColliders(matchJoint.StickyBody, false);
        ownedJoint.UnlinkFromJoint(matchJoint);
    }
}