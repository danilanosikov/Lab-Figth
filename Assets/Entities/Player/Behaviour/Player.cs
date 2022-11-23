using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cappa.Player
{

    public class Player : MonoBehaviour
    {
        [SerializeField] Movement movement;
        [SerializeField] Camera camera;

        public Vector2 Input { get; private set; }


        void Start()
        {
            InitializeMovement();
            InitializeCamera();
        }

        void FixedUpdate()
        {
            movement.Move();
        }
        void Update()
        {
            camera.Behave();
        }


        void OnMove(InputValue value) => Input = value.Get<Vector2>();

        /* Initialization */

        void InitializeMovement() {
            var player = this;
            movement = new(ref player, ref movement.subject, movement.swiftness);
        }
        void InitializeCamera() {

        }
    }



    [Serializable]
    class Movement {

        Player player;

        [SerializeField]  public Transform subject;
        [SerializeField] public float swiftness = 5f;

        CharacterController Body => subject.gameObject.GetComponent<CharacterController>();

        public Vector3 Direction {

            get {
                var input = player.Input;
                return new(input.x, 0, input.y);
            }

        }

        public void Move() => Body.SimpleMove(swiftness * Direction);

        public Movement(ref Player player, ref Transform subject, float swiftness = 5f) {
            this.subject = subject;
            this.player = player;
            this.swiftness = swiftness;
        }

    }



    [Serializable]
    class Camera {
        [SerializeField] Follower follower;

        public void Behave() {
            follower.Follow();
        }




        [Serializable]
        internal class Follower
        {
            [SerializeField] Transform camera, target;
            [SerializeField, Range(0f, 6f)] float swiftness = 1f;
            [SerializeField, Range(0f, 20f)] float minimalRadius = 6f;

            float HightOffset
            {
                get
                {
                    var difference = target.position.y - camera.position.y;
                    var offset = -difference;
                    return offset;
                }
            }

            Vector3 Distance
            {
                get
                {
                    var target_position = target.position;

                    target_position.y += HightOffset;

                    var distance = target_position - camera.position;

                    return distance;
                }
            }
            bool OutOfReach
            {
                get => Mathf.Abs(Distance.magnitude) > minimalRadius;
            }
            float Discomfort
            {
                // Difference in distnce between target and nearest to it cameras circle point;
                get
                {

                    var distance_to_nearest_border = Mathf.Abs(Mathf.Abs(Distance.magnitude) - minimalRadius);

                    var inconvinience_strength = Mathf.Pow(2.72f, Mathf.Sqrt(distance_to_nearest_border));

                    return inconvinience_strength;
                }
            }

            Vector3 Velocity
            {
                get
                {
                    var way = Distance.normalized;

                    var strength = swiftness * 0.3f * Discomfort;

                    var step = strength * way;

                    return OutOfReach ? step : Vector3.zero;
                }
            }

            public void Follow()
            {
                camera.position += Time.deltaTime * Velocity;
            }
        }
    }

}