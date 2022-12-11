using UnityEngine;

namespace Cappa.Core
{
    [RequireComponent(typeof(Rigidbody))]
    public class RotationStabilizer : MonoBehaviour
    {
        /// <summary>
        ///     Base strength of rotation
        /// </summary>
        [SerializeField] [Range(0f, 100f)] public float stiffness = 1f;

        /// <summary>
        ///     Damping of the rotation
        /// </summary>
        [SerializeField] [Range(1f, 100f)] public float smoothness = 1f;

        /// <summary>
        ///     Rigidbody Base
        /// </summary>
        private Rigidbody Body;


        /// <summary>
        /// Damping factor of torque
        /// </summary>
        private Vector3 Damping
        {
            get
            {
                // Smoothness
                var smth = 1 / (1000 * this.smoothness);
                
                
                // Angular Velocity without y component
                var ang_vel = -Body.angularVelocity;
                ang_vel.y = 0;
                
                // Raw Damping
                var dmp = ang_vel + Body.mass * Body.angularVelocity;
                
                // Smoothed Damping
                var smth_dmp = smth * dmp;

                return smth_dmp;
            }
        }
        

        /// <summary>
        ///     Calculation of the strength of rotation
        /// </summary>
        private Vector3 Torque
        {
            get
            {
                
                // Raw Torque.
                var raw_trq = 100 * stiffness * Angle * Axis;

                // Damped Torque
                var trq = raw_trq - Damping;

                return trq;
            }
        }

        /// <summary>
        ///     Stabilization rotation
        /// </summary>
        private Quaternion Rotation
        {
            get
            {
                // Current rotation
                var rot = transform.rotation;
                
                // Shortest rotation between two quaternions
                var rotation = Quaternion.Slerp(rot, Quaternion.identity, 0.000001f);

                // Disabling rotation by y axis, to calculate it separately.
                rotation.y = 0;

                return rotation;
            }
        }

        /// <summary>
        ///     Rotation axis
        /// </summary>
        private Vector3 Axis
        {
            get
            {
                Rotation.ToAngleAxis(out var angle, out var axis);
                return axis.normalized;
            }
        }

        /// <summary>
        ///     Rotation angle in radians
        /// </summary>
        private float Angle
        {
            get
            {
                // Rotation Conversion
                Rotation.ToAngleAxis(out var angle, out var axis);

                // Radiant Conversion
                var r_angle = angle * Mathf.Deg2Rad;

                return -r_angle;
            }
        }


        /// <summary>
        ///     Set-Up Method
        /// </summary>
        private void Start()
        {
            Body = gameObject.GetComponent<Rigidbody>();
        }

        /// <summary>
        ///     Starting Point
        /// </summary>
        private void FixedUpdate()
        {
            Body.AddTorque(Torque); // - floor angular speed multiplied by damping;
        }
    }
}