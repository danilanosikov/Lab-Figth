using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Drawing;

namespace Cappa.Player
{
    public class PlayerCamera : MonoBehaviour
    {

        /* Field Section */

        [Header("Movement:\n")]

        [Tooltip("The speed with which the camera will move towards its tarrget, if subject is further than comfortRadius")]
        [SerializeField, Range(0f, 20f)] float swiftness = 1f;
        [Tooltip("The radius,\nin which camera wouldn't be triggered\nfor linear movement.\n.")]
        [SerializeField, Range(0, 100)] float comfortRadius = 6f;


        [Header("\n\nRotation:\n")]
        [SerializeField, Range(-20f, 20f)] float horizontalOffset = 0f;
        [SerializeField, Range(0f, 20f)] float rotationSwiftness = 2f;
        [SerializeField, Range(0f, 20f)] float deadZone = 5f;





        /* Camera-Related Properties */



        float cameraHight = 3f;
        float FOV => gameObject.GetComponent<Camera>().fieldOfView;
        Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }






        /* Target-Related Properties. */


        GameObject Target => transform.parent.gameObject.GetComponentInChildren<PlayerController>().gameObject;
        Vector3 WayToTarget {
            get
            {
                var t_pos = Target.transform.position;
                t_pos.y -= cameraHight;
                return t_pos - Position;
            }
        }
        bool TargetOutOfRange => Mathf.Abs(WayToTarget.magnitude) > comfortRadius;
        Vector3 Step => 0.3f * swiftness * Mathf.Sqrt(Mathf.Abs(WayToTarget.magnitude)) * WayToTarget.normalized;
        float AngleToTarget {

            get {
                var f = transform.forward; f.y = 0;
                var d = WayToTarget.normalized; d.y = 0;

                return Vector3.Angle(f, d);
            }

        }
        bool TargetInFOV => !(AngleToTarget > (FOV / 2));
        bool TargetInFocus => !(AngleToTarget > (FOV / 2) - deadZone);
        bool TargetInDeadZone => !TargetInFocus && TargetInFOV;
        bool TargetInCentre =>Mathf.Abs(AngleToTarget) <= deadZone;
        Vector3 StepToTarget => TargetOutOfRange ? Step : Vector3.zero;




        /* Unity's Default Functions */

        void Start()
        {
            cameraHight = Target.transform.position.y - Position.y;
        }
        void Update() => Follow();
        void FixedUpdate() => Behold();


        /* Movement Implementation */

        void Follow() => Position += Time.deltaTime * StepToTarget;



        /* Rotation Implementation */

        Quaternion CalculateRotation(Vector3 distance) => Quaternion.RotateTowards(Quaternion.LookRotation(transform.forward, transform.up), Quaternion.LookRotation(distance.normalized, transform.up), 1f);
        bool Rotate(Vector3 point)
        {
            var dif = point - Position;
            transform.localRotation = CalculateRotation(dif);
            return Vector3.Dot(transform.forward, dif.normalized) > 0.98f; // true on small rotation
        } 
        void LookAtTarget()
        {
            var pos = Target.transform.position; pos.y -= cameraHight;
            Rotate(pos);
        }




        void Behold()
        {
            if (TargetInFocus) return;
            else if (TargetInDeadZone) LookAtTarget();
            else LookAtTarget();
        }


    }
}
