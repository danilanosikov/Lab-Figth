using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//targetHight = floatHight + Vector3.Dot(transform.TransformDirection(Down), Centre - transform.position)




namespace CappaTeam.Util
{


    public class Hoverer : MonoBehaviour
    {
        
        [SerializeField, Range(0, 100)]
        float floatHight = 1f, stiffness = 3f, damping = 0f, forceMultiplier = 1f;


        Rigidbody body;
        Vector3 Velocity => body.velocity;
        Vector3 Centre => body.worldCenterOfMass;
        Vector3 Down => -transform.up;


        RaycastHit Ground { get; set; }
        Vector3 GroundVelocity { get; set; }
        bool Grounded { get; set; }
        Vector3 Force { get; set; }



        void Start() {
            body = GetComponent<Rigidbody>();
        }
        void FixedUpdate() => Hover();


        void Raycast() {
            Debug.DrawRay(Centre, Down * floatHight, Color.blue);

            Grounded = Physics.Raycast(Centre, Down, out var hit, floatHight);
            Ground = hit; var body = Ground.rigidbody;

            GroundVelocity = body != null ? body.velocity : Vector3.zero;
        }





        void CalculateForce()
        {



            if (!Grounded) { Force = Vector2.zero; return; }





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
            var spring_force = (dx * stiffness) - (rv * damping);

            // Transition to the actual force;
            Force =  spring_force * direction - body.mass * Physics.gravity;
        }


        void Hover() {
            Raycast();
            CalculateForce();

            var end_force =  forceMultiplier * Force;
            Debug.DrawRay(Centre, end_force, Color.yellow);
            body.AddForce(end_force);
        }
    }
}