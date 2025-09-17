using Cainos.LucidEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Cainos.PixelArtPlatformer_VillageProps
{
    public class Chest : MonoBehaviour
    {
        private bool inCollider = false;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.W) && inCollider)
            {
                IsOpened = true;
            }
            if (inCollider == false)
            {
                IsOpened = false;
            }
        }

        [FoldoutGroup("Reference")]
        public Animator animator;

        [FoldoutGroup("Runtime"), ShowInInspector, DisableInEditMode]
        public bool IsOpened
        {
            get { return isOpened; }
            set
            {
                isOpened = value;
                animator.SetBool("IsOpened", isOpened);
            }
        }
        private bool isOpened;

        [FoldoutGroup("Runtime"),Button("Open"), HorizontalGroup("Runtime/Button")]
        public void Open()
        {
            IsOpened = true;
        }

        [FoldoutGroup("Runtime"), Button("Close"), HorizontalGroup("Runtime/Button")]
        public void Close()
        {
            IsOpened = false;
        }

        private void OnTriggerEnter2D(Collider2D other)   //Collider med player, när player går in i collidern så är inCollider true. Kan tweakas för att bli bättre
        {
            inCollider = true;
            if (other.CompareTag("Player"))
            {
                
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            inCollider = false;
            if (other.CompareTag("Player"))
            {
               
            }
        }

    }
}
