using UnityEngine;

public class RootMovement : StickyBody
{
    [SerializeField, Range(0.0f, 2000.0f)] private float _rootMass = 1.0f;
    [SerializeField, Range(0.05f, 2.0f)] private float _accelerationTime = 0.25f;
    [SerializeField, Range(0.0f, 5.0f)] private float _targetSpeed = 0.5f;

    private void FixedUpdate()
    {
        Rigidbody.mass = _rootMass;

        Vector3 pushDirection = GetPlayerInputDirection();
        Vector3 pushForce = (pushDirection * _targetSpeed / _accelerationTime) * TotalStickyMass;
        Rigidbody.AddForce(pushForce, ForceMode.Force);
        ClampPlanarSpeed();
    }

    private Vector3 GetPlayerInputDirection()
    {
        Vector3 inputDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            inputDirection += Vector3.forward;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            inputDirection += Vector3.back;
        }

        if (Input.GetKey(KeyCode.D))
        {
            inputDirection += Vector3.right;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            inputDirection += Vector3.left;
        }

        // player has full control of their velocity on the horizontal plane, not against gravity
        inputDirection = Vector3.ProjectOnPlane(inputDirection, Vector3.up); 

        return inputDirection.normalized;
    }

    private void ClampPlanarSpeed()
    {
        Vector3 planarVelocity = Vector3.ProjectOnPlane(Rigidbody.velocity, Vector3.up);

        Rigidbody.velocity -= planarVelocity;
        Rigidbody.velocity += planarVelocity.normalized * Mathf.Clamp(planarVelocity.magnitude, 0.0f, _targetSpeed);
    }
}
