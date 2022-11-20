using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Cappa.Player
{
    public class PlayerCamera : MonoBehaviour
    {

        /* Field Section */

        [Header("Movement:\n")]

        [Tooltip("The speed with which the camera will move towards its target, if subject is further than comfortRadius")]
        [SerializeField, Range(0f, 20f)] float swiftness = 1f;
        [Tooltip("The radius,\nin which camera wouldn't be triggered\nfor linear movement.\n.")]
        [SerializeField, Range(0, 100)] float comfortRadius = 6f;


        [Header("\n\nRotation:\n")]
        [SerializeField, Range(0f, 20f)] float deadZone = 5f;
        [SerializeField, Range(0f, 20f)] float rotationSwiftness = 5f;





        /* Camera-Related Properties */



        float cameraHeight = 3f;
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
                t_pos.y -= cameraHeight;
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
        bool TargetInCentre => Mathf.Abs(AngleToTarget) <= deadZone;
        Vector3 StepToTarget => TargetOutOfRange ? Step : Vector3.zero;




        /* Unity's Default Functions */

        void Start() => cameraHeight = Target.transform.position.y - Position.y;

        void Update()
        {
            Follow();
            RotateToTarget();
        }


        /* Movement Implementation */

        void Follow() => Position += Time.deltaTime * StepToTarget;
        
        

        void RotateToTarget()
        {
            var tgt_dir = WayToTarget.normalized;
            
            var cr_rot = transform.rotation;

            var tgt_rot = Quaternion.LookRotation(tgt_dir, Vector3.up);

            var rot = Quaternion.RotateTowards(cr_rot, tgt_rot, rotationSwiftness);

            transform.rotation = rot;
        }

    }
}
