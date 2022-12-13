using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cappa.Core
{
    /* Camera in future should project a line passing through target and itself
     * to the ground and on rotation "collide?" with solid objects,
     * so the target would always stay o in sight. */

    
    /// <summary>
    ///  Default Player Behaviour
    /// </summary>
    public class Player : NetworkBehaviour
    {
        
        /// <summary>
        /// Its body - Controller
        /// </summary>
        [NonSerialized] private CharacterController Body;
        
        /// <summary>
        /// Player's Camera
        /// </summary>
        [NonSerialized] private Camera Camera;
        
        /// <summary>
        /// Overall speed of the character
        /// </summary>
        [SerializeField] private float swiftness = 5f;

        /// <summary>
        /// Responsiveness of velocity feedback
        /// </summary>
        [SerializeField, Range(0f, 1f)] private float responsiveness = 0.5f;
        
        /// <summary>
        /// Minimal distance to the camera allowed for character jerk prevention
        /// </summary>
        [SerializeField] private float jitterPreventionDistance = 1.3f;
        
        /// <summary>
        /// Sprint Swiftness
        /// </summary>
        [SerializeField] private float sprintSwiftnessMultiplier = 3f;
        
        /// <summary>
        /// Acceleration to the max speed on sprint
        /// </summary>
        [SerializeField] private float acceleration = 0.3f;
        
        /// <summary>
        /// Camera world's object
        /// </summary>
        [SerializeField] private new Transform camera;

        
        /// <summary>
        /// Checks whether the entity, which tries to access this doesn't have an authority to do so
        /// </summary>
        private bool CalledByAlien => !IsOwner || !IsLocalPlayer;





        /// <summary>
        ///     Input Conditions
        /// </summary>
        private Vector2 input;
        private Vector2 Input
        {
            get
            {
                var value = input;
                var gets_closer = value.y < 0;
                
                if (gets_closer && UnderTheCamera) value.y = 0;
                return value;
            }
            set => input = value;
        }

        
        /// <summary>
        /// Velocity Conditions
        /// </summary>
        private float velocity;
        private float Velocity
        {
            get
            {
                // Convenience Measures
                var too_fast = velocity > sprintSwiftnessMultiplier * swiftness;
                var far_away = Camera.OutOfReach;
                var sprinting = velocity > swiftness;

                // Calculation
                var vel = far_away switch {
                    
                    // Accelerate
                    true when !too_fast => velocity + acceleration * Time.deltaTime,
                    
                    // Decelerate
                    false when sprinting => velocity - acceleration * Time.deltaTime,
                    
                    // Keep Current
                    _ => velocity
                };

                return Mathf.Abs(vel);
            }
            set => velocity = value;
        }
        
        
        /// <summary>
        /// Relative direction
        /// </summary>
        [SuppressMessage("ReSharper", "JoinDeclarationAndInitializer")]
        private Vector3 Direction
        {
            get
            {
                var camera = Camera.transform;
                
                
                
                // Upward vector setup
                var up = camera.up;
                up.y = 0;
                up.Normalize();
                
                
                
                
                // Forward vector setup
                var forward = camera.forward;
                forward.y = 0;
                forward.Normalize();
                
                
                
                
                // Right vector setup
                var right = camera.right;
                right.y = 0;
                right.Normalize();

                
                
                
                // Input Components Converted in Global Space
                Vector3 vertical_component;
                Vector3 horizontal_component;
                
                
                
                
                
                // Boolean Statements For Better Readability 
                var looks_straight = Mathf.Abs(Input.y * forward.magnitude) > 0; // If player looks down - false
                var go_forward = Input.y > 0;
                
                
                
                
                
                
                
                // Stabilisation of movement, when camera is rotated drastically + Conversion to World's space
                vertical_component = looks_straight switch
                {
                    true => Input.y * forward,
                    
                    false when go_forward => Input.y * up,
                    
                    _ => Vector3.zero //otherwise
                };
                horizontal_component = Input.x * right;
                
                
                
                // Removal of Y component to ensure stability during fast and big rotations
                var result = vertical_component + horizontal_component;
                result.y = 0;

                
                return result;
            }
        }
        
        
        /// <summary>
        /// If a player is Under its camera
        /// </summary>
        private bool UnderTheCamera
        {
            get
            {
                // Signed distance from this to camera with no Y component for simplicity;
                var s_dist = -Camera.Distance;
                s_dist.y = 0;

                // Absolute distance
                var distance = Mathf.Abs(s_dist.magnitude);
                
                // Too Close to Camera in terms of XZ Plane
                var close = distance < jitterPreventionDistance;
                
                return close;
            }
        }
        
        
        /// <summary>
        /// Set-Up Method
        /// </summary>
        private void Start()
        {
            // Controller Caching
            Body = gameObject.GetComponent<CharacterController>();
            
            // Camera Caching
            Camera = camera.gameObject.GetComponent<Camera>();
            
            // Initialization Block
            Velocity = swiftness;
            
        }
        
        
        /// <summary>
        /// Called Once a global.deltaTime
        /// </summary>
        private void Update()
        {
            if (CalledByAlien) return;
            
            var vel = Mathf.Lerp(1, Velocity, responsiveness);
            
            Body.SimpleMove(vel * Direction); // Not delta in time required
        }


        /// <summary>
        /// Called On Input, Which collerates to character movement
        /// </summary>
        /// <param name="trigger"></param>
        [UsedImplicitly]
        public void OnMove(InputAction.CallbackContext trigger)
        {
            if (CalledByAlien) return;
            Input = trigger.ReadValue<Vector2>();
        }

    }
}