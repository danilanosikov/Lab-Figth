using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.Windows;

namespace Cappa.Player
{

    public class Player : MonoBehaviour
    {
        [SerializeField] Movement movement;
        [SerializeField] Camera camera;

        public Vector2 Input { get; private set; }
        public Vector3 RelativeDirection {
            get {
                var up = camera.transfrom.up; up.y = 0; up.Normalize();
                var forward = camera.transfrom.forward; forward.y = 0;
                var right = camera.transfrom.right; right.y = 0; right.Normalize();

                //if (Input.y > 0 && ) {

                //}

                Vector3 vertical;

                if (Mathf.Abs(Input.y * forward.magnitude) > 0.3f) {
                    forward.Normalize();
                    vertical = Input.y * forward;
                }
                else
                {
                    forward.Normalize();
                    if (Input.y > 0) vertical = Input.y * up;
                    else vertical = Vector3.zero;
                }



                var horizontal = Input.x * right;

                var direction = vertical + horizontal;

                return direction;
            }
        }


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







        /* Input */

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

        [SerializeField] public Transform subject;
        [SerializeField] public float swiftness = 5f;

        CharacterController Body => subject.gameObject.GetComponent<CharacterController>();

        public Vector3 Direction {

            get {
                return player.RelativeDirection;
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

        [SerializeField] public Transform transfrom;
        [SerializeField] public Follower follower;
        [SerializeField] public Rotor rotor;

        public void Behave() {
            follower.Follow();
            rotor.Rotate();
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

        [Serializable]
        internal class Rotor
        {
            [SerializeField] Transform camera, target;
            [SerializeField, Range(0f, 15f)] float swiftness = 3f, deadzone = 10f, centrePercesion = 0f;


            [SerializeField] Vector3 focusOffset = Vector3.zero;







            UnityEngine.Camera Camera
            {
                get
                {
                    return camera.gameObject.GetComponent<UnityEngine.Camera>();
                }
            }

            Vector3 Direction
            {
                get
                {
                    var target_position = target.position;

                    target_position += focusOffset;

                    var distance = target_position - camera.position;

                    return distance.normalized;
                }
            }

            float Discomfort
            {
                // Difference in distnce between target and nearest to it cameras circle point;
                get
                {

                    var distance_to_nearest_border = Mathf.Abs(Mathf.Abs(Angle) - centrePercesion);

                    var inconvinience_strength = Mathf.Pow(2.72f, Mathf.Sqrt(distance_to_nearest_border));

                    return inconvinience_strength;
                }
            }

            float AngularVelocity
            {
                get
                {
                    return Discomfort * swiftness;
                }
            }

            float Angle
            {
                get
                {
                    var f = camera.transform.forward; f.y = 0;
                    var d = Direction; d.y = 0;
                    return Vector3.Angle(f, d);
                }
            }

            float FOV
            {
                get
                {
                    return Camera.fieldOfView;
                }
            }

            bool OnScreen => !(Angle > (FOV / 2));

            bool InFocus => !(Angle > (FOV / 2) - deadzone);

            bool NearScreenEdge => !InFocus && OnScreen;

            bool InCentre => Mathf.Abs(Angle) <= centrePercesion;


            void RotateToTarget()
            {
                var desired_rotation = Quaternion.LookRotation(Direction, Vector3.up);
                var rot = Quaternion.RotateTowards(camera.rotation, desired_rotation, AngularVelocity);
                camera.rotation = rot;
            }


            public void Rotate()
            {
                RotateToTarget();
            }

        }

    }
}