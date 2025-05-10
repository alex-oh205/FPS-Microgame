using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class WeaponHUDManager : MonoBehaviour
    {
        [Tooltip("UI panel containing the layoutGroup for displaying weapon ammo")]
        public RectTransform AmmoPanel;

        [Tooltip("Prefab for displaying weapon ammo")]
        public GameObject AmmoCounterPrefab;

        PlayerWeaponsManager[] m_PlayerWeaponsManagers;
        PlayerWeaponsManager m_MainWeaponsManager;
        List<AmmoCounter> m_AmmoCounters = new List<AmmoCounter>();

        void Start()
        {
            m_PlayerWeaponsManagers = FindObjectsOfType<PlayerWeaponsManager>();
            foreach (var weaponsManager in m_PlayerWeaponsManagers)
            {
                if (weaponsManager.isMain)
                {
                    m_MainWeaponsManager = weaponsManager;
                    break;
                }
            }
            DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, WeaponHUDManager>(m_MainWeaponsManager,
                this);

            WeaponController activeWeapon = m_MainWeaponsManager.GetActiveWeapon();
            if (activeWeapon)
            {
                AddWeapon(activeWeapon, m_MainWeaponsManager.ActiveWeaponIndex);
                ChangeWeapon(activeWeapon);
            }

            m_MainWeaponsManager.OnAddedWeapon += AddWeapon;
            m_MainWeaponsManager.OnRemovedWeapon += RemoveWeapon;
            m_MainWeaponsManager.OnSwitchedToWeapon += ChangeWeapon;
        }

        void AddWeapon(WeaponController newWeapon, int weaponIndex)
        {
            GameObject ammoCounterInstance = Instantiate(AmmoCounterPrefab, AmmoPanel);
            AmmoCounter newAmmoCounter = ammoCounterInstance.GetComponent<AmmoCounter>();
            DebugUtility.HandleErrorIfNullGetComponent<AmmoCounter, WeaponHUDManager>(newAmmoCounter, this,
                ammoCounterInstance.gameObject);

            newAmmoCounter.Initialize(newWeapon, weaponIndex);

            m_AmmoCounters.Add(newAmmoCounter);
        }

        void RemoveWeapon(WeaponController newWeapon, int weaponIndex)
        {
            int foundCounterIndex = -1;
            for (int i = 0; i < m_AmmoCounters.Count; i++)
            {
                if (m_AmmoCounters[i].WeaponCounterIndex == weaponIndex)
                {
                    foundCounterIndex = i;
                    Destroy(m_AmmoCounters[i].gameObject);
                }
            }

            if (foundCounterIndex >= 0)
            {
                m_AmmoCounters.RemoveAt(foundCounterIndex);
            }
        }

        void ChangeWeapon(WeaponController weapon)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(AmmoPanel);
        }
    }
}