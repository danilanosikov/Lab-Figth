using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Cappa.Player
{
    public class PlayerCamera : MonoBehaviour
    {

        [Header("Movement:\n")]

        [Tooltip("The speed with which the camera will move towards its tarrget, if subject is further than comfortRadius")]
        [SerializeField, Range(0f, 20f)] float swiftness = 1f;
        [Tooltip("The radius,\nin which camera wouldn't be triggered\nfor linear movement.\n.")]
        [SerializeField, Range(0, 100)] float comfortRadius = 6f;


        [Header("\n\nRotation:\n")]
        [SerializeField, Range(-20f, 20f)] float horizontalOffset = 0f;
        [SerializeField, Range(0f, 20f)] float RotationSwiftness = 2f;

        float cameraHight = 3f;

        GameObject Target => transform.parent.gameObject.GetComponentInChildren<PlayerController>().gameObject;

        Vector3 Position
        {

            get => transform.position;
            set => transform.position = value;

        }
        Quaternion Rotation => transform.rotation;

        // A step to take towards target each update, when it is out of comfort radius;
        float Step => 0.3f * Mathf.Sqrt(Mathf.Abs(Distance.magnitude));

        Vector3 Distance {
            get
            {

                var t_pos = Target.transform.position;
                t_pos.y -= cameraHight;
                return t_pos - Position;
            }
        }


        void Start()
        {
            cameraHight = Target.transform.position.y - Position.y;
        }

        /* Unity Default Functions */

        void Update()
        {
            Move();
        }


        void FixedUpdate()
        {
            Rotate();
        }




        /* Logic */


        void Move()
        {
            if (Mathf.Abs(Distance.magnitude) > comfortRadius) Position += swiftness * Time.deltaTime * Step * Distance.normalized;
        }

        void Rotate()
        {
            var frwrd = Quaternion.LookRotation(transform.forward, transform.up);
            var trgt = Quaternion.LookRotation(Distance.normalized, transform.up);

            var dr = Quaternion.RotateTowards(frwrd, trgt, 1f);

            transform.localRotation = dr;
        }

    }
}
