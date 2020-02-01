using System.Collections.Generic;
using UnityEngine;

public class TrainMovement : MonoBehaviour
{
    [SerializeField] private List<Waypoint> _waypoints = new List<Waypoint>();
    [SerializeField] private float _linearSpeed = 10.0f;
    [SerializeField] private float _turningSpeed = 45.0f;
    [SerializeField] private Rigidbody _engineRigidBody;

    private Waypoint _currentWaypoint;

    private void Awake()
    {
        foreach (var waypoint in _waypoints)
        {
            waypoint.OnWaypointEnter += MoveToNextWaypoint;
        }
    }

    private void MoveToNextWaypoint(Waypoint hitWaypoint)
    {
        int newIndex = _waypoints.IndexOf(hitWaypoint);

        newIndex++;

        if (newIndex >= _waypoints.Count)
        {
            newIndex = 0;
        }

        _currentWaypoint = _waypoints[newIndex];
    }

    private void FixedUpdate()
    {
        if (_currentWaypoint != null)
        {
            Vector3 direction = (_currentWaypoint.Position - _engineRigidBody.position).normalized;
            _engineRigidBody.MovePosition(_engineRigidBody.position + direction * _linearSpeed * Time.deltaTime);
            _engineRigidBody.transform.rotation = Quaternion.RotateTowards(_engineRigidBody.transform.rotation, Quaternion.FromToRotation(_engineRigidBody.transform.forward, direction), _turningSpeed * Time.deltaTime);
        }
    }
}


