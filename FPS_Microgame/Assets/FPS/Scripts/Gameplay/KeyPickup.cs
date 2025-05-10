using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class KeyPickup : Pickup
    {
        [SerializeField] private KeyType keyType;
        public enum KeyType
        {
            Red,
            Green,
            Blue
        }

        protected override void OnPicked(PlayerCharacterController player)
        {
            KeyHolder keyHolder = player.GetComponent<KeyHolder>();
            keyHolder.AddKey(keyType);
            PlayPickupFeedback();
            Destroy(gameObject);
        }
    }
}