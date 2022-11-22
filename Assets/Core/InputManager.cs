using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cappa.Core
{
    public class InputManager : MonoBehaviour
    {
        InputValue input;
        public Vector2 Input => input.Get<Vector2>();

        void OnMove(InputValue value) => input = value;
    }
}
