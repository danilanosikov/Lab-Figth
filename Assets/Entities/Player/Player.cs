using System.Collections;
using System.Collections.Generic;
using Cappa.Player;
using UnityEngine;




namespace Cappa.Core {

    public class Player : MonoBehaviour
    {

        [Header("Base Stats:\n\n")]
        [SerializeField, Range(0, 100)] float baseHealth;
        [SerializeField, Range(0, 100)] float baseStrength; // Damage + Knockback
        [SerializeField, Range(0, 100)] float baseSpeed;



        [Header("Multipliers:\n\n")] // max - (10x)
        [SerializeField, Range(0f, 10f)] float agility;
        [SerializeField, Range(0f, 10f)] float power;


        void Attack() { }

        void GetDamaged() { }

        void Heal() { }

        void Suicide() { }

        void GetKilled(ref GameObject killer) { }
    }
}