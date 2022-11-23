using System;
using System.Collections;
using System.Collections.Generic;
using Cappa.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cappa.Player
{

    public class Player : MonoBehaviour
    {
        [SerializeField] Movement movement;

        public Vector2 Input { get; private set; }


        void Start()
        {
            InitializeMovement();
        }

        void FixedUpdate()
        {
            movement.Move();
        }


        void OnMove(InputValue value) => Input = value.Get<Vector2>();

        /* Initialization */

        void InitializeMovement() {
            var player = this;
            movement = new(ref player, ref movement.subject, movement.swiftness);
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
    class Camera
    {
        [SerializeField] Object camera;

        public void Behave() => throw new NotImplementedException();

        [Serializable] internal class Object {
            // Component owner.
            [SerializeField] Transform subject;

            // Unity Camera.
            UnityEngine.Camera camera => subject.gameObject.GetComponent<UnityEngine.Camera>();

            Transform Transform => camera.transform;
            Vector3 Position => Transform.position;
            Quaternion Rotation => Transform.rotation;

            // Forward Direction with no Y affection.
            Vector3 Forward => new(Transform.forward.x, 0, Transform.forward.z);

            // Right Direction with no Y affection.
            Vector3 Right => new(Transform.right.x, 0, Transform.right.z);
        }
    }

}
