using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class StanceHUD : MonoBehaviour
    {
        [Tooltip("Image component for the stance sprites")]
        public Image StanceImage;

        [Tooltip("Sprite to display when standing")]
        public Sprite StandingSprite;

        [Tooltip("Sprite to display when crouching")]
        public Sprite CrouchingSprite;

        [Tooltip("Sprite to display when sprinting")]
        public Sprite SprintingSprite;

        void Start()
        {
            PlayerCharacterController character = FindObjectOfType<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, StanceHUD>(character, this);
            character.OnStanceChanged += OnStanceChanged;

            OnStanceChanged(character.playerState);
        }

        void OnStanceChanged(PlayerCharacterController.PlayerState state)
        {
            switch (state)
            {
                case PlayerCharacterController.PlayerState.Normal:
                    StanceImage.sprite = StandingSprite;
                    break;
                case PlayerCharacterController.PlayerState.Crouching:
                    StanceImage.sprite = CrouchingSprite;
                    break;
                case PlayerCharacterController.PlayerState.Sprinting:
                    StanceImage.sprite = SprintingSprite;
                    break;
                default:
                    StanceImage.sprite = StandingSprite;
                    break;
            }
        }
    }
}