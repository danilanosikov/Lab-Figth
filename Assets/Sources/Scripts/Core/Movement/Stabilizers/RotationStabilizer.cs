using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

[SelectionBase]
public class RotationStabilizer : MonoBehaviour
{
    Rigidbody body;

    void Start() {
        body = gameObject.GetComponent<Rigidbody>();
        body.centerOfMass = new(body.centerOfMass.x, body.centerOfMass.y - transform.localScale.y, body.centerOfMass.z);
    }
}
