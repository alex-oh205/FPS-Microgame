using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class KeyDoor : MonoBehaviour
    {
        [Tooltip("Reference to the door's audio source")] public AudioSource audioSource;
        [Tooltip("Sound that will be played on opening the door")] public AudioClip openSfx;

        [SerializeField] private KeyPickup.KeyType keyType;
        
        private Animator animator;
        private bool isOpen = false;

        public KeyPickup.KeyType GetKeyType()
        {
            return keyType;
        }

        public void OpenDoor()
        {
            isOpen = true;
            animator = GetComponent<Animator>();
            animator.enabled = true;
            if (openSfx && audioSource)
            {
                audioSource.PlayOneShot(openSfx);
            }
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.name == "Player")
            {
                KeyHolder keyHolder = collider.GetComponent<KeyHolder>();
                if (keyHolder.ContainsKey(keyType) && isOpen == false)
                {
                    // Currently holding key to open this door, and the door is not open
                    OpenDoor();
                    keyHolder.RemoveKey(keyType);
                }
            }
        }
    }
}