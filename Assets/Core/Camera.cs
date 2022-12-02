using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cappa.Core
{
        public class Camera : MonoBehaviour{
            
        [SerializeField] Transform target;

        [SerializeField] Follower follower;
        [SerializeField] Rotor rotor;

        public Vector3 Distance
        {
            get
            {
                var target_position = target.position;

                var distance = target_position - transform.position;

                return distance;
            }
        }
        
        public Transform Transform => transform;
        void Start()
        {
            follower.target = target;
            follower.transform = transform;
            follower.camera = this;
            
            rotor.target = target;
            rotor.camera = transform;
        }

        public void Behave() {
            follower.Behave();
            rotor.Behave();
        }
        

        [Serializable] internal class Follower
        {
            [NonSerialized] public Transform transform, target;
            [NonSerialized] public Camera camera;
            
            [SerializeField, Range(0f, 6f)] float swiftness = 1f;
            [SerializeField, Range(0f, 20f)] float minimalRadius = 6f;
            [SerializeField, Range(0f, 20f)] float hightOffset = 3f;

            private bool OutOfReach => Mathf.Abs(camera.Distance.magnitude) > minimalRadius;
            float Discomfort
            {
                // Difference in distnce between target and nearest to it cameras circle point;
                get
                {

                    var distance_to_nearest_border = Mathf.Abs(Mathf.Abs(camera.Distance.magnitude) - minimalRadius);

                    var inconvinience_strength = Mathf.Pow(2.72f, Mathf.Sqrt(distance_to_nearest_border));

                    return inconvinience_strength;
                }
            }

            Vector3 Velocity
            {
                get
                {
                    var way = camera.Distance.normalized;

                    var strength = swiftness * 0.3f * Discomfort;

                    var step = strength * way;

                    return OutOfReach ? step : Vector3.zero;
                }
            }

            public void Behave()
            {
                var target_position = transform.position + Time.deltaTime * Velocity;
                target_position.y = target.position.y + hightOffset;

                transform.position = target_position;

            }
        }

        [Serializable] internal class Rotor
        {
            [NonSerialized] public Transform camera, target;
            
            
            [SerializeField, Range(0f, 15f)] float swiftness = 3f, deadzone = 10f, centrePercesion = 0f;
            [SerializeField] Vector3 focusOffset = Vector3.zero;


            private UnityEngine.Camera Camera => camera.gameObject.GetComponent<UnityEngine.Camera>();

            Vector3 Direction => (target.position + focusOffset - camera.position).normalized;

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

            private float AngularVelocity => Discomfort * swiftness;

            float Angle
            {
                get
                {
                    var f = camera.transform.forward; f.y = 0;
                    var d = Direction; d.y = 0;
                    return Vector3.Angle(f, d);
                }
            }

            float FOV=> Camera.fieldOfView;

            bool OnScreen => !(Angle > (FOV / 2));

            bool InFocus => !(Angle > (FOV / 2) - deadzone);

            bool NearScreenEdge => !InFocus && OnScreen;

            bool InCentre => Mathf.Abs(Angle) <= centrePercesion;

            void Rotate(Vector3 To)
            {
                var desired_rotation = Quaternion.LookRotation(To, Vector3.up);
                var rotation = Quaternion.RotateTowards(camera.rotation, desired_rotation, AngularVelocity);
                camera.rotation = rotation;
                
            }

            void RotateToTarget() => Rotate(Direction);

            public void Behave() => RotateToTarget();
            
        }
        
        }
}
