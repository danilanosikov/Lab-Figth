using System.Collections;
using System.Collections.Generic;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEngine;



//targetHight = floatHight + Vector3.Dot(transform.TransformDirection(Down), Centre - transform.position)


namespace Cappa.Core
{

    [RequireComponent(typeof(Rigidbody))]
    public class PhysicalObject : MonoBehaviour
    {

        [Header("Stabilizer Settings\n")]
        [SerializeField] Vector3 stabilizationAxis = Vector3.up;

        [SerializeField, Range(0f, 100f)] float rotationStiffness = 1f;
        [SerializeField, Range(1f, 100000f)] float rotationSmoothness = 1f;

        //[SerializeField] bool useLocalAxis = false;


        [Header("\n\n\nFloat Settings\n")]

        [SerializeField, Range(0, 20f)] float floatHight = 1f;

        [SerializeField, Range(0, 100f)] float stiffness = 3f;

        [SerializeField, Range(0, 2000f)] float damping = 0f;




        [Header("\n\n\nMiscellenious\n")]

        [SerializeField] bool drawGizmos = false;

        [SerializeField, Range(0, 2), Tooltip("If modified:\nBehavoiur may break\nor become unstable.")]
        float rotationBaseMultiplier = 1f;


        Rigidbody body;
        Vector3 Velocity => body.velocity;
        Vector3 Centre => body.worldCenterOfMass;
        Vector3 Down => -transform.up;

        RaycastHit Ground { get; set; }
        Vector3 GroundVelocity { get; set; }
        bool Grounded { get; set; }

        Vector3 Force { get; set; }

        Vector3 Torque
        {
            get
            {
                Vector3 ang_vel = body.angularVelocity, mod_ang_vel = new(ang_vel.x, 0, ang_vel.z);
                // Angle in radiants with appropriate direction;
                float r_angle = -angle * Mathf.Deg2Rad;

                // Torque not affected by other torques and forces
                Vector3 unaf_trq = r_angle * RotationAxis;
                Vector3 affection = 1/rotationSmoothness * -mod_ang_vel;

                Vector3 trq = 100 * rotationStiffness * unaf_trq - affection;

                Vector3 dmpt_trq = trq - 1/rotationSmoothness * body.mass * body.angularVelocity;
                return dmpt_trq;
            }
        }
        Vector3 RotationAxis;
        float angle;


        void Awake()
        {
            body = GetComponent<Rigidbody>();
            Raycast();
        }

        void Update()
        {
            DrawGizmos();
        }

        void FixedUpdate()
        {
            Hover();
            Stabilize();
            Raycast();
        }



        void Raycast()
        {
            Grounded = Physics.Raycast(Centre, Down, out var hit, floatHight);
            Ground = hit; var body = Ground.rigidbody;
            GroundVelocity = body != null ? body.velocity : Vector3.zero;
        }

        void CalculateForce()
        {

            if (!Grounded) { Force = Vector3.up * transform.localScale.y * -Physics.gravity.y; return; }


            // Spring tension length
            var dx = Ground.distance - floatHight;


            // Down Ray Target Coordinates in the World Space
            var direction = transform.TransformDirection(Down);


            // Velocity component in terms of ray direction, usually local downward y axis is the ray direction;
            // (direction component of velocity)
            // Converted to the "ray axis" not to make the machine compute muiltiple axis afterwards,
            // but rather one, which can be represented as a single dimention variable (float);
            float dcv = Vector3.Dot(direction, body.velocity);

            // Same but for the ground, {this} is standing on;
            float g_dcv = Vector3.Dot(direction, GroundVelocity);


            // Velocity relative to the ground
            float rv = dcv - g_dcv;


            // The absolute value of force which would keep {this} floating;
            var spring_force = (dx * stiffness * body.mass) - (rv * damping);

            // Transition to the actual force;
            Force = spring_force * direction - body.mass * Physics.gravity;
        }

        void CalculateRotation()
        {
            var trgt = Quaternion.Euler(stabilizationAxis);

            // Shortest rotation between two quaternions;
            Quaternion rot = Quaternion.Slerp(transform.rotation, trgt, rotationBaseMultiplier * 0.000001f);
            rot.y = 0; //Disabling rotation by y axis, to calculate it saparately.

            // Spits an angle and the axis, around which rotation happens on what angle, in degrees.
            rot.ToAngleAxis(out angle, out RotationAxis);
            RotationAxis.Normalize();
        }



        void Hover() {
            CalculateForce();
            body.AddForce(Force);
        }

        void Stabilize() {
            CalculateRotation();
            body.AddTorque(Torque); // - Floor Rotation speed multiplied by damping;
        }



        void DrawGizmos()
        {
            if (!drawGizmos) return;
            Debug.DrawRay(Centre, Down * floatHight, Color.blue);
            Debug.DrawRay(Centre, Force, Color.yellow);
            Debug.DrawRay(transform.position, RotationAxis.normalized, Color.magenta);
        }
    }
}