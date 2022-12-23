using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject level;
    
    
    public Camera MainCamera => mainCamera;
    public GameObject Level => level;
}
