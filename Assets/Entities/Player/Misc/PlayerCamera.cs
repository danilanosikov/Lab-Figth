using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Cappa.Player
{
    public class PlayerCamera : MonoBehaviour
    {

        [Header("General:\n\n")]

        [Tooltip("The speed with which the camera will move towards its tarrget, if subject is further than comfortRadius")]
        [SerializeField] float swiftness = 1f;
        [Tooltip("The radius,\nin which camera wouldn't be triggered\nfor linear movement.\n.")]
        [SerializeField, Min(0)] float comfortRadius = 6f;

        float cameraHight = 3f;

        GameObject Target => transform.parent.gameObject.GetComponentInChildren<PlayerController>().gameObject;

        Vector3 Position
        {

            get => transform.position;
            set => transform.position = value;

        }

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




        /* Logic */




        void Move()
        {
            if (Mathf.Abs(Distance.magnitude) > comfortRadius) Position += swiftness * Time.deltaTime * Step * Distance.normalized;
        }

    }
}
