using System;
using UnityEngine;

namespace Cappa.Core
{
    
    /// <summary>
    /// Default Camera Behavior. Follows the set target and rotates towards it.
    /// </summary>
    public class Camera : MonoBehaviour
    {
        
        /// <summary>
        /// Camera's Target
        /// </summary>
        [SerializeField] private Transform target;
        
        
        
        /// <summary>
        /// follower module
        /// </summary>
        [SerializeField] private Follower follower;
        
        
        
        
        /// <summary>
        /// Rotation module
        /// </summary>
        [SerializeField] private Rotor rotor;

        
        

        /// <summary>
        /// Unity Camera Component
        /// </summary>
        private UnityEngine.Camera U_Camera
        {
            get
            {
                var host = gameObject;
                var u_cam = host.GetComponent<UnityEngine.Camera>();
                    
                return u_cam;
            }
        }
        
        

        /// <summary>
        /// Distance to the target
        /// </summary>
        public Vector3 Distance
        {
            get
            {
                var target_position = target.position;

                var distance = target_position - transform.position;

                return distance;
            }
        }

        
        /// <summary>
        /// State, which tells if the target is out of reach.
        /// </summary>
        public bool OutOfReach => follower.OutOfReach;

        
        
        /// <summary>
        /// Set-Up Method
        /// </summary>
        private void Start()
        {
            // IDE Optimisation Suggestion
            var transform = this.transform;
            var target = this.target;
            var follower = this.follower;
            var rotor = this.rotor;
            
            // Follower Initialization
            follower.target = target;
            follower.camera = this;

            //Rotor initialization
            rotor.target = target;
            rotor.camera = transform;
            rotor.u_camera = U_Camera;
        }
        
        

        /// <summary>
        /// Called Each Frame
        /// </summary>
        private void Update()
        {
            follower.FollowTarget();
            rotor.RotateToTarget();
        }









        /// <summary>
        /// Following Logic
        /// </summary>
        [Serializable]
        internal class Follower
        {
            
            
            
            /// <summary>
            /// Target to follow
            /// </summary>
            [NonSerialized] public Transform target;
            
            /// <summary>
            /// Host
            /// </summary>
            [NonSerialized] public Camera camera;

            
            
            
            
            
            
            
            /// <summary>
            /// The speed of the camera following
            /// </summary>
            [SerializeField] [Range(0f, 6f)] public float swiftness = 1f;
            
            /// <summary>
            /// The rate of inconvenience growth, depending on the distance to the comfort radius border
            /// </summary>
            [Range(0f, 6f)] public float intolerance = 2.72f;
            
            
            /// <summary>
            /// How drastically camera will accelerate
            /// </summary>
            [Range(0f, 6f)] public float stiffness = 1.61f;
            
            /// <summary>
            /// Camera height offset
            /// </summary>
            [SerializeField] [Range(0f, 6f)] public float offset = 1f;
            
            /// <summary>
            /// While the target in this radius - camera stands still.
            /// </summary>
            [SerializeField] [Range(0f, 20f)] public float comfortRadius = 6f;

            
            
            
            
            
            
            
            
            
            
            
            /// <summary>
            /// If the target is out of the comfort radius
            /// </summary>
            public bool OutOfReach
            {
                get
                {
                    // Signed distance
                    var sgn_dist = camera.Distance;
                    sgn_dist.y = 0;

                    // Absolute distance
                    var dist = Mathf.Abs(sgn_dist.magnitude);
                    
                    // Calculation
                    var far_away = dist > comfortRadius;

                    return far_away;
                }
            }

            
            
            /// <summary>
            /// Velocity, which is composite of Stress and Swiftness.
            /// Zero if target is in reach.
            /// </summary>
            private Vector3 Velocity
            {
                get
                {
                    // Distance to target
                    var dist = Mathf.Abs(camera.Distance.magnitude);
                    
                    var dir = camera.Distance.normalized;
                    
                    var vel = dist * Stress * swiftness * dir;
                    
                    return OutOfReach ? vel : Vector3.zero;
                }
            }

            
            
            
            /// <summary>
            /// Stress, which grows larger, the further target gets away from the camera's convenience radius
            /// </summary>
            private float Stress
            {
                get
                {
                    // Distance to target
                    var dist = Mathf.Abs(camera.Distance.magnitude);
                    
                    // Distance to the comfort circle border
                    var b_dist =  Mathf.Abs(dist - comfortRadius);
                    
                    var pow = stiffness * Mathf.Sqrt(stiffness * b_dist);
                    
                    var inconvenience = Mathf.Pow(intolerance, pow);

                    return inconvenience;
                }
            }






            /// <summary>
            /// Follow the target
            /// </summary>
            public void FollowTarget()
            {
                var cam = camera.transform;
                
                // Convenience measures
                var cam_pos = cam.position;
                var t_height = target.position.y;
                
                // Next position
                var step = cam_pos + Time.deltaTime * Velocity;
                cam_pos = step;
                
                // Set camera to the offseted height of the target
                cam_pos.y = t_height + offset;
                
                // Move
                cam.position = cam_pos;
            }
            
            
            
        }
        
        
        
        /// <summary>
        /// Rotation Logic
        /// </summary>
        [Serializable]
        internal class Rotor
        {
            
            /// <summary>
            /// The host, which owns this rotor
            /// </summary>
            [NonSerialized] public Transform camera;
            
            /// <summary>
            /// The target, to which this rotor should rotate to
            /// </summary>
            [NonSerialized] public Transform target;
            
            /// <summary>
            /// Unity Camera
            /// </summary>
            [NonSerialized] public UnityEngine.Camera u_camera;





            /// <summary>
            /// Regular speed of rotation
            /// </summary>
            [SerializeField] [Range(0f, 15f)] private float swiftness = 3f;
            
            /// <summary>
            /// Responsiveness
            /// </summary>
            [SerializeField] [Range(0f, 1f)] private float smoothing = 0.8f;
            
            /// <summary>
            /// The rate at which Inconvenience will grow, when player is out of centre or focus
            /// </summary>
            [SerializeField] [Range(0f, 3f)] private float Intolerance = 2.72f;
            
            /// <summary>
            /// How close player have to be to be considered near screen border
            /// </summary>
            [SerializeField] [Range(0f, 15f)] private float deadzone = 10f;

            /// <summary>
            /// How big centre would be considered - zero would give a "pin-point precision"
            /// </summary>
            [SerializeField] [Range(0f, 15f)] private float centrePrecision;
            
            
            /// <summary>
            /// Direction to Target
            /// </summary>
            private Vector3 Direction
            {
                get
                {
                    // Local Variables for clarity
                    var t_pos = target.position;
                    var pos = camera.position;
                    
                    // Calculation
                    var dist = t_pos - pos;
                    var dir = dist.normalized;
                    
                    return dir;
                }
            }
            
            /// <summary>
            /// Affection on the rotation speed, depending on how far to rotate to the target
            /// </summary>
            private float Inconvenience
            {
                get
                {
                    // Distance to the nearest border of a circle of comfort
                    var ncb_dist = Mathf.Abs(Mathf.Abs(Angle) - centrePrecision);

                    // Inconvenience strength
                    var inc_str = Mathf.Pow(Intolerance, Mathf.Sqrt(ncb_dist));

                    return inc_str;
                }
            }
            
            /// <summary>
            /// Rotation Speed
            /// </summary>
            private float RotationSpeed => Inconvenience * swiftness;
            
            /// <summary>
            /// Field of view
            /// </summary>
            private float FOV => u_camera.fieldOfView;
            
            /// <summary>
            /// Angle from camera look direction, projected on XY plane and
            /// the direction to the target, projected there as well
            /// </summary>
            private float Angle
            {
                get
                {
                    // Current look direction
                    var frw = camera.transform.forward;
                    frw.y = 0;
                    
                    // The direction of the target
                    var dir = Direction;
                    dir.y = 0;
                    
                    /*
                     * No Y component for readability and lack of overkill.
                     */

                    // Calculation
                    var ang = Vector3.Angle(frw, dir);
                    
                    return ang;
                }
            }
            
            /// <summary>
            /// If target is on screen
            /// </summary>
            private bool OnScreen
            {
                get
                {
                    // Current angle
                    var ang = Angle;
                    
                    // The half of the total fov
                    var h_fv = FOV / 2;

                    var on_screen = ang < h_fv;
                    
                    return on_screen;
                }
            }
            
            /// <summary>
            /// If target is somewhere in between the centre and the edge of the screen
            /// </summary>
            private bool InFocus
            {
                get
                {
                    var ang = Angle;
                    
                    // the half of the total fov
                    var h_fv = FOV / 2;
                    
                    // A piece of half the screen without edges with "deadzone" precision
                    var split_fv = h_fv - deadzone;


                    var out_of_focus = ang > split_fv;
                    
                    return !out_of_focus;
                }
            }
            
            /// <summary>
            /// If the target is near to the screen edge
            /// </summary>
            private bool NearScreenEdge
            {
                get
                {
                    // Out of focus, but still on the screen
                    var res = !InFocus && OnScreen;
                    
                    return res;
                }
            }
            
            /// <summary>
            /// If target is in centre of the screen
            /// </summary>
            private bool InCentre
            {
                get
                {
                    // Absolut angle value
                    var angle = Mathf.Abs(Angle);
                    
                    // If angle between the camera and the target is close to zero with set precision
                    var result = angle <= centrePrecision;
                        
                    return result;
                }
            }
            

            /// <summary>
            /// Rotation to set direction
            /// </summary>
            /// <param name="To"></param>
            public void Rotate(Vector3 To)
            {
                // Smoothing conversion
                var smooth = 1 - smoothing;
                
                // Desired Rotation
                var d_rot = Quaternion.LookRotation(To, Vector3.up);
                
                // Smoothed Rotation
                var slerped = Quaternion.Slerp(camera.rotation, d_rot, smooth);
                
                // Result
                var rotation = Quaternion.RotateTowards(slerped, d_rot, RotationSpeed);
                
                camera.rotation = rotation;
            }
            
            
            
            /// <summary>
            /// Rotation to target
            /// </summary>
            public void RotateToTarget()
            {
                
                // Smoothing conversion
                var smooth = 1 - smoothing;
                
                // Look direction
                var frw = camera.transform.forward;
                
                // Smoothed Direction
                var dir = Vector3.Slerp(frw, Direction, smooth);

                // Rotation
                Rotate(dir);
            }
        }
        
        
    }
}