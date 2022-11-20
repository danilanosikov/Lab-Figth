using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cappa.Core
{
    public class MainMenu : MonoBehaviour
    {
        
        private UIDocument GUI => GetComponent<UIDocument>();
        
        /// <summary>
        /// Main UI Element (UI Toolkit).
        /// </summary>
        private VisualElement Root => GUI.rootVisualElement;

        /// <summary>
        /// Host Launch Button.
        /// </summary>
        private Button Host => Root.Q<Button>("Host");
        
        /// <summary>
        /// Client Launch Button.
        /// </summary>
        private Button Client => Root.Q<Button>("Client");

        /// <summary>
        /// Tells whether application has its network role.
        /// </summary>

        /// <summary>
        /// Tells whether menu is closed.
        /// </summary>
        private bool Closed { get; set; }


        /// <summary>
        /// Entry Points
        /// </summary>
        private void Start() => Closed = false;
        private void OnEnable() => ListenForSelection();

        /// <summary>
        /// Closes Main Menu (Hides it, for now)
        /// </summary>
        private void CloseMenu()
        {
            if (Closed) return;
            GUI.gameObject.SetActive(false);
            Closed = true;
        }
        
        /// <summary>
        /// Waits, until user decides if they are a client or a server.
        /// </summary>
        private void ListenForSelection()
        {
            if (Closed) return;
            ListenForHostSelection();
            ListenClientForSelection();
        }
        private void ListenForHostSelection()
        {

            Host.clicked += () => {
                
                NetworkManager.Singleton.StartHost();
                
                CloseMenu();
                
            };
        
        }
        private void ListenClientForSelection()
        {
            
            Client.clicked += () => {
                
                NetworkManager.Singleton.StartClient();
                
                CloseMenu();
            };
            
        }
    }
}
