using System;
using System.Collections;
using System.Collections.Generic;
using Cappa.Core;
using UnityEngine;

namespace Cappa.Player
{

    public class Player : MonoBehaviour
    {
        [SerializeField] Movement movement;

    }


    [Serializable]
    class Behaviour
    {
        public readonly List<IBehaviour> Behaviours = new();
        void Tick() { foreach (var beh in Behaviours) beh.Behave(); }
    }





    [Serializable]
    class Camera : IBehaviour {

        // Component owner.
        [SerializeField] Transform subject;

        // Unity Camera.
        UnityEngine.Camera Component => subject.gameObject.GetComponent<UnityEngine.Camera>();

        // Forward Direction with no Y affection.
        Vector3 Forward => new(Component.transform.forward.x, 0, Component.transform.forward.z);

        // Right Direction with no Y affection.
        Vector3 Right => new(Component.transform.right.x, 0, Component.transform.right.z);

        void IBehaviour.Behave() => throw new NotImplementedException();
    }



    [Serializable]
    class Movement {


        [SerializeField] Transform subject;
        [SerializeField] Input input;
        [SerializeField] float swiftness = 5f;

        CharacterController Body => subject.gameObject.GetComponent<CharacterController>();

        public Vector3 Direction => input.Get;
        public Vector3 Acceleration
        {
            get
            {
                var t_dir = Direction;
                var c_dir = CurrentDirection;

                c_dir.y = 0;

                var collinearity = Mathf.Abs(Vector3.Dot(c_dir, t_dir));

                var force = (2 + collinearity) * t_dir;

                force.y = 0;

                return force;
            }
        }
        public Vector3 Deceleration
        {
            get
            {
                if (input.Get != Vector2.zero) return Vector3.zero;
                var direction = -CurrentDirection; direction.y = 0;
                return direction;
            }
        }
        public Vector3 CurrentDirection { get; }
        public Vector3 Force => (input.Get == Vector2.zero) ? Deceleration : Acceleration;

        public void Move() => Body.SimpleMove(swiftness * Direction);


        [Serializable]
        internal class Input
        {

            [SerializeField] Transform module;

            public Vector2 Get => module.gameObject.GetComponent<InputManager>().Input;

        }
    }





    [Serializable]
    class RotationStabilizer : IBehaviour
    {
        [SerializeField] Transform subject;

        [SerializeField, Range(0f, 100f)] float stiffness = 1f;
        [SerializeField, Range(1f, 100f)] float smoothness = 1f;

        Transform Transform => subject.transform;
        Rigidbody Body => subject.gameObject.GetComponent<Rigidbody>();

        Vector3 Torque
        {
            get
            {
                var stiffness = this.stiffness;
                var smoothness = 1/(1000 * this.smoothness);
                var angular_velocity = -Body.angularVelocity; angular_velocity.y = 0;

                // Non-dmped torque.
                var trq = 100 * stiffness * Angle * Axis;
                var damping = smoothness * (angular_velocity + Body.mass * Body.angularVelocity); // If wouldn't work change + to -;

                // Damped torque
                return trq -= damping;
            }
        }

        // Stabilization rotation.
        Quaternion Rotation {
            get {
                // Shortest rotation between two quaternions;
                Quaternion rotation = Quaternion.Slerp(Transform.rotation, Quaternion.identity, 0.000001f);

                //Disabling rotation by y axis, to calculate it saparately.
                rotation.y = 0;

                return rotation;
            }
        }

        // Rotation axis.
        Vector3 Axis {
            get
            {
                Rotation.ToAngleAxis(out var angle, out var axis);
                return axis.normalized;
            }
        }

        // Rotation angle in radians.
        float Angle {
            get {
                Rotation.ToAngleAxis(out var angle, out var axis);
                return -angle * Mathf.Deg2Rad;
            }
        }

        // RotationStabilizer behaviour.
        void IBehaviour.Behave() => Body.AddTorque(Torque); // - floor angular speed multiplied by damping;

    }



    [Serializable]
    class Hoverer : IBehaviour
    {
        [SerializeField] Transform subject;

        [SerializeField, Range(0, 20f)] float height = 1f;
        [SerializeField, Range(0, 100f)] float stiffness = 3f;
        [SerializeField, Range(0, 2000f)] float damping = 0f;

        Transform Transform => subject.transform;
        Rigidbody Body => subject.gameObject.GetComponent<Rigidbody>();

        Vector3 Down => -Transform.up;

        RaycastHit Ground {
            get
            {
                Physics.Raycast(Transform.position, Down, out var hit, height);
                return hit;
            }
        }
        Vector3 GroundVelocity {
            get {
                var body = Ground.rigidbody;
                return body != null ? body.velocity : Vector3.zero;
            }
        }
        bool Grounded => Physics.Raycast(Transform.position, Down, out var hit, height);

        Vector3 Force {
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

        void IBehaviour.Behave() => Body.AddForce(Force);
    }







    interface IBehaviour {
        public void Behave();
    }

}
