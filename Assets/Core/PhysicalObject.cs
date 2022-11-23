using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Cappa.Core
{

    [RequireComponent(typeof(Rigidbody))]
    public class PhysicalObject : MonoBehaviour
    {
        [SerializeField] Transform subject;
        [SerializeField] Hoverer hoverer;
        [SerializeField] RotationStabilizer rotor;

        void Start()
        {
            InitializeHoverer();
        }

        void FixedUpdate()
        {
            hoverer.Behave();
        }


        /* Initialization */

        void InitializeHoverer() {
            hoverer = new(subject, hoverer.height, hoverer.stiffness, hoverer.damping);
        }
        void InitializeRotor() {
            rotor = new(subject, rotor.stiffness, rotor.smoothness);
        }

    }




    [Serializable]
    class Hoverer
    {
        [NonSerialized] public Transform subject;

        [SerializeField, Range(0, 20f)] public float height = 1f;
        [SerializeField, Range(0, 100f)] public float stiffness = 3f;
        [SerializeField, Range(0, 2000f)] public float damping = 0f;

        Transform Transform => subject.transform;
        Rigidbody Body => subject.gameObject.GetComponent<Rigidbody>();

        Vector3 Down => -Transform.up;

        RaycastHit Ground
        {
            get
            {
                Physics.Raycast(Transform.position, Down, out var hit, height);
                return hit;
            }
        }
        Vector3 GroundVelocity
        {
            get
            {
                var body = Ground.rigidbody;
                return body != null ? body.velocity : Vector3.zero;
            }
        }
        bool Grounded => Physics.Raycast(Transform.position, Down, out var hit, height);

        Vector3 Force
        {
            get
            {
                if (!Grounded) return -Physics.gravity.y * Transform.localScale.y * Vector3.up;
                else
                {
                    // Spring tension length
                    var dx = Ground.distance - height;


                    // Down Ray Target Coordinates in the World Space
                    var direction = Transform.TransformDirection(Down);


                    // Velocity component in terms of ray direction, usually local downward y axis is the ray direction;
                    // (direction component of velocity)
                    // Converted to the "ray axis" not to make the machine compute muiltiple axis afterwards,
                    // but rather one, which can be represented as a single dimention variable (float);
                    float dcv = Vector3.Dot(direction, Body.velocity);

                    // Same but for the ground, {this} is standing on;
                    float g_dcv = Vector3.Dot(direction, GroundVelocity);


                    // Velocity relative to the ground
                    float rv = dcv - g_dcv;


                    // The absolute value of force which would keep {this} floating;
                    var spring_force = (dx * stiffness * Body.mass) - (rv * damping);

                    // Transition to the actual force;
                    return spring_force * direction - Body.mass * Physics.gravity;
                }
            }
        }

        public Hoverer(Transform subject, float height = 1f, float stiffness = 3f, float damping = 1f) {
            this.subject = subject;
            this.height = height;
            this.stiffness = stiffness;
            this.damping = damping;
        }

        public void Behave() => Body.AddForce(Force);
    }





    [Serializable]
    class RotationStabilizer
    {
        [NonSerialized] public Transform subject;

        [SerializeField, Range(0f, 100f)] public float stiffness = 1f;
        [SerializeField, Range(1f, 100f)] public float smoothness = 1f;

        Transform Transform => subject.transform;
        Rigidbody Body => subject.gameObject.GetComponent<Rigidbody>();

        Vector3 Torque
        {
            get
            {
                var stiffness = this.stiffness;
                var smoothness = 1 / (1000 * this.smoothness);
                var angular_velocity = -Body.angularVelocity; angular_velocity.y = 0;

                // Non-dmped torque.
                var trq = 100 * stiffness * Angle * Axis;
                var damping = smoothness * (angular_velocity + Body.mass * Body.angularVelocity); // If wouldn't work change + to -;

                // Damped torque
                return trq -= damping;
            }
        }

        // Stabilization rotation.
        Quaternion Rotation
        {
            get
            {
                // Shortest rotation between two quaternions;
                Quaternion rotation = Quaternion.Slerp(Transform.rotation, Quaternion.identity, 0.000001f);

                //Disabling rotation by y axis, to calculate it saparately.
                rotation.y = 0;

                return rotation;
            }
        }

        // Rotation axis.
        Vector3 Axis
        {
            get
            {
                Rotation.ToAngleAxis(out var angle, out var axis);
                return axis.normalized;
            }
        }

        // Rotation angle in radians.
        float Angle
        {
            get
            {
                Rotation.ToAngleAxis(out var angle, out var axis);
                return -angle * Mathf.Deg2Rad;
            }
        }

        public RotationStabilizer(Transform subject, float stiffness = 3f, float smoothness = 1f)
        {
            this.subject = subject;
            this.stiffness = stiffness;
            this.smoothness = smoothness;
        }

        // RotationStabilizer behaviour.
        public void Behave() => Body.AddTorque(Torque); // - floor angular speed multiplied by damping;

    }

}