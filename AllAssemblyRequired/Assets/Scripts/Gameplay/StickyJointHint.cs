using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class StickyJointHint : MonoBehaviour
{
    [SerializeField] private float _hintRadius = 0.5f;

    private StickyJoint _stickyJoint;
    private ParticleSystem _hintParticles;

    public bool IsVisible
    {
        get
        {
            return _hintParticles.isEmitting;
        }
        set
        {
            if (value)
            {
                if (!IsVisible)
                {
                    _hintParticles.Play(true);
                }
            }
            else
            {
                _hintParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    public StickyJoint[] GetAllJointsInRange()
    {
        return Physics.OverlapSphere(transform.position, _hintRadius)
                        .Where(collider => collider.transform != transform && collider.GetComponentInParent<StickyJoint>() != null)
                        .Select(collider => collider.GetComponentInParent<StickyJoint>())
                        .ToArray();
    }

    private void Awake()
    {
        _stickyJoint = GetComponentInParent<StickyJoint>();
        _hintParticles = GetComponent<ParticleSystem>();
        IsVisible = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        _stickyJoint.TryCreateJointWith(other.GetComponentInParent<StickyJoint>());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _hintRadius);
    }
}
