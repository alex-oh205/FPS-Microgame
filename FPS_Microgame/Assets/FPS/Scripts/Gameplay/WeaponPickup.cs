using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class WeaponPickup : Pickup
    {
        [Tooltip("The prefab for the weapon that will be added to the player on pickup")]
        public WeaponController WeaponPrefab;

        protected override void Start()
        {
            base.Start();

            // Set all children layers to default (to prefent seeing weapons through meshes)
            foreach (Transform t in GetComponentsInChildren<Transform>())
            {
                if (t != transform)
                    t.gameObject.layer = 0;
            }
        }

        protected override void OnPicked(PlayerCharacterController byPlayer)
        {
            PlayerWeaponsManager[] playerWeaponsManagers = byPlayer.GetComponents<PlayerWeaponsManager>();
            PlayerWeaponsManager rightHand = null;
            PlayerWeaponsManager leftHand = null;
            foreach (var weaponsManager in playerWeaponsManagers)
            {
                if (weaponsManager.isMain)
                {
                    rightHand = weaponsManager;
                } else
                {
                    leftHand = weaponsManager;
                }
            }
            if (rightHand)
            {
                if (WeaponPrefab.WeaponName == "Grapple Gun")
                {
                    if (leftHand.AddWeapon(WeaponPrefab))
                    {
                        // Handle auto-switching to weapon if no weapons currently
                        if (leftHand.GetActiveWeapon() == null)
                        {
                            leftHand.SwitchWeapon(true);
                        }

                        PlayPickupFeedback();
                        Destroy(gameObject);
                    }
                } else
                {
                    if (rightHand.AddWeapon(WeaponPrefab))
                    {
                        // Handle auto-switching to weapon if no weapons currently
                        if (rightHand.GetActiveWeapon() == null)
                        {
                            rightHand.SwitchWeapon(true);
                        }

                        PlayPickupFeedback();
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}