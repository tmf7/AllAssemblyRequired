using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Waypoint : MonoBehaviour
{
    public delegate void WaypointEnterHandler(Waypoint hitWaypoint);

    public event WaypointEnterHandler OnWaypointEnter;

    public Vector3 Position => transform.position;
    public Quaternion Rotation => transform.rotation;

    public static implicit operator Vector3(Waypoint waypoint)
    {
        return waypoint.transform.position;
    }

    public static implicit operator Quaternion(Waypoint waypoint)
    {
        return waypoint.transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        OnWaypointEnter?.Invoke(this);
    }
}