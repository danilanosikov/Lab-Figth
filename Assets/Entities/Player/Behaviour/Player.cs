using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cappa.Core
{
    
    /* Camera in future should project a line passing through target and itself
     * to the ground and on rotation "collide?" with solid objects,
     * so the target would always stay o in sight. */

    public class Player : MonoBehaviour
    {
        CharacterController Body;
        Camera Camera;

        [SerializeField] public float swiftness = 5f, jitterPreventionDistance = 1.3f;
        [SerializeField] Transform camera;

        private Vector2 Input;

        Vector3 RelativeDirection
        {
            get
            {
                var up = Camera.Transform.up;
                up.y = 0;
                up.Normalize();
                var forward = Camera.Transform.forward;
                forward.y = 0;
                forward.Normalize();
                var right = Camera.Transform.right;
                right.y = 0;
                right.Normalize();

                Vector3 vertical;

                if (Mathf.Abs(Input.y * forward.magnitude) > 0.8f) vertical = Input.y * forward;
                else
                {
                    if (Input.y > 0) vertical = Input.y * up;
                    else vertical = Vector3.zero;
                }

                var horizontal = Input.x * right;

                var direction = vertical + horizontal;
                direction.y = 0;

                return direction;
            }
        }



        void Start()
        {
            Body = gameObject.GetComponent<CharacterController>();
            Camera = camera.gameObject.GetComponent<Camera>();
        }

        void FixedUpdate() => Move();
        void Update() => Camera.Behave();

        void OnMove(InputValue value)
        {
            Input = value.Get<Vector2>();
        }


        void Move()
        {
            var distance_to_camera = camera.position - transform.position; distance_to_camera.y = 0;
            if(Input.y < 0 && distance_to_camera.magnitude < jitterPreventionDistance) Input.y = 0;
            
            Body.SimpleMove(swiftness * RelativeDirection);
        }

        
    }
}
