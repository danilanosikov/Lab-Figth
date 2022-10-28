using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationStabilizer : MonoBehaviour
{
    // Stiffnes of an imaginary spring attached to the ends of the up vector and the tilt vector
    [SerializeField] float stiffness = 2f;
    [SerializeField] float damping = 4;

    Rigidbody body;
    Vector3 pVelocity => body.GetPointVelocity(applicationPoint);
    Vector3 pAcceleration => pVelocity.magnitude*pVelocity/2;
    Vector3 applicationPoint => new(transform.position.x, (transform.position.y + 1) * transform.localScale.y, transform.position.z);
    Vector3 rForce => 1000 * stiffness * tilt.normalized; // Spring resistance force (k * dx = F);
    Vector3 dForce => damping * pAcceleration; // Damping Force;

    float dt => Time.deltaTime;
    Vector3 tilt => Vector3.up - transform.up;

    // Start is called before the first frame update
    void Start() => body = gameObject.GetComponent<Rigidbody>();

    // Update is called once per frame
    void Update() {
        body.AddForceAtPosition(dt * (rForce - pAcceleration), applicationPoint); 
    }
}
