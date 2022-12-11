using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Cappa.Core
{
    [RequireComponent(typeof(Rigidbody))]
    public class Hoverer : MonoBehaviour
        {

            
            /// <summary>
            /// Height to float on
            /// </summary>
            [SerializeField] [Range(0, 20f)] public float height = 1f;
            
            /// <summary>
            /// Responsiveness of the hoverer
            /// </summary>
            [SerializeField] [Range(0, 100f)] public float stiffness = 3f;
            
            /// <summary>
            /// Damps the kick from below
            /// </summary>
            [SerializeField] [Range(0, 2000f)] public float damping;
            
            /// <summary>
            /// RigidBody of the hoverer
            /// </summary>
            private Rigidbody Body;

            /// <summary>
            /// Ground information
            /// </summary>
            [SuppressMessage("ReSharper", "IdentifierTypo")]
            private RaycastHit GroundInfo
            {
                get
                {
                    var locl = transform;
                    var down = -locl.up;
                    var pos = locl.position;
                    
                    Physics.Raycast(pos, down, out var hit, height);
                    return hit;
                }
            }

            /// <summary>
            /// Current velocity of the ground this is standing on
            /// </summary>
            private Vector3 GroundVelocity
            {
                get
                {
                    var body = GroundInfo.rigidbody;
                    return body != null ? body.velocity : Vector3.zero;
                }
            }

            /// <summary>
            /// Property, which tells whether this is not Air-born;
            /// </summary>
            [SuppressMessage("ReSharper", "IdentifierTypo")]
            private bool Grounded
            {
                get
                {
                    var pos = transform.position;

                    var down = -transform.up;

                    var hit_the_grnd = Physics.Raycast(pos, down, out var hit, height);
                    
                    return hit_the_grnd;
                }
            }

            /// <summary>
            /// Hover Force
            /// </summary>
            private Vector3 Force
            {
                get
                {
                    var down = -transform.up;
                    
                    if (!Grounded)
                    {
                        return -Physics.gravity.y * transform.localScale.y * Vector3.up;
                    }

                    // Spring tension length
                    var dx = GroundInfo.distance - height;


                    // Down Ray Target Coordinates in the World Space
                    var direction = transform.TransformDirection(down);


                    // Velocity component in terms of ray direction, usually local downward y axis is the ray direction;
                    // (direction component of velocity)
                    // Converted to the "ray axis" not to make the machine compute muiltiple axis afterwards,
                    // but rather one, which can be represented as a single dimention variable (float);
                    var dcv = Vector3.Dot(direction, Body.velocity);

                    // Same but for the ground, {this} is standing on;
                    var g_dcv = Vector3.Dot(direction, GroundVelocity);


                    // Velocity relative to the ground
                    var rv = dcv - g_dcv;


                    // The absolute value of force which would keep {this} floating;
                    var spring_force = dx * stiffness * Body.mass - rv * damping;

                    // Transition to the actual force;
                    return spring_force * direction - Body.mass * Physics.gravity;
                }
            }
            
            
            
            
            
            
            
            /// <summary>
            /// Set-Up Method
            /// </summary>
            private void Start()
            {
                Body = gameObject.GetComponent<Rigidbody>();
            }

            /// <summary>
            /// Starting Point
            /// </summary>
            private void FixedUpdate()
            {
                Body.AddForce(Force);
            }

        }
}