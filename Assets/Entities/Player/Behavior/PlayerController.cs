using System;
using System.Collections;
using System.Collections.Generic;
using Cappa.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cappa.Player
{
    [RequireComponent(typeof(PhysicalObject))]
    public class PlayerController : MonoBehaviour
    {


        Rigidbody body;
        Vector2 input;



        [SerializeField] float limit = 50f, swiftness = 5f;



        public float Speed => Mathf.Abs(body.velocity.magnitude);
        public float Limit => limit;



        // Camera
        Camera camera => transform.parent.Find("Camera").gameObject.GetComponent<Camera>();
        Vector3 forward =>  new(camera.transform.forward.x, 0, camera.transform.forward.z);
        Vector3 right => new(camera.transform.right.x, 0, camera.transform.right.z);


        // Unity Default
        void Start() => body = gameObject.GetComponent<Rigidbody>();
        void FixedUpdate() => Move();




        void Move()
        {
            if (input == Vector2.zero) Decelerate();
            else Accelerate();
        }









        void Accelerate(float strength = 1) {

            var dir = input.y * forward + input.x * right;

            var vel = body.velocity; vel.y = 0;

            var turn =  dir + Mathf.Abs(Vector3.Dot(vel, dir)) * dir.normalized;

            var frce = dir + turn;
            frce.y = 0;

            Move(strength * frce);
        }










        void Decelerate(float strength = 1)
        {
            if (input != Vector2.zero) return;

            var vel = body.velocity; vel.y = 0;
            Move(strength * -vel);
        }










        void Move(Vector3 dir) {
            body.AddForce(swiftness * body.mass * dir);
            if (body.velocity.magnitude > limit) body.velocity = body.velocity / body.velocity.magnitude * limit;
        }








        void OnMove(InputValue inp) => input = inp.Get<Vector2>();
    }
}
