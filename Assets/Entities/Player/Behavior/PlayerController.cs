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

        [SerializeField] float limit = 50f, swiftness = 1;

        Rigidbody body;
        Vector2 input;


        void Start() => body = gameObject.GetComponent<Rigidbody>();
        void FixedUpdate() => Move();



        void Move()
        {
            if (input == Vector2.zero) Decelerate();
            else Accelerate();
        }



        void Accelerate(float strength = 1) {

            var inp = (Vector3) input; inp.z = inp.y; inp.y = 0;
            var vel = body.velocity; vel.y = 0;

            var turn =  inp + Mathf.Abs(Vector3.Dot(vel, inp)) * inp.normalized;

            var frce = inp + turn;
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
