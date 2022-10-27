using UnityEngine;

namespace CappaTeam.Util {

    [SelectionBase]
    public class RotationStabilizer : MonoBehaviour
    {
        void Start() {
            var body = gameObject.GetComponent<Rigidbody>();
            body.solverIterations = 60;
            body.centerOfMass = new(body.centerOfMass.x, body.centerOfMass.y - transform.localScale.y, body.centerOfMass.z);
        }
    }

}