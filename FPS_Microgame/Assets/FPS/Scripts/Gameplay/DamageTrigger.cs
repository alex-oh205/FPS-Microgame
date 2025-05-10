using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    [Tooltip("Where the damage originated")]
    public Transform root;

    [Tooltip("Damage of the trigger")]
    public float Damage;

    [Tooltip("Layers this trigger can collide with")]
    public LayerMask HittableLayers = -1;

    WeaponController weaponController;

    List<Collider> m_IgnoredColliders;

    const QueryTriggerInteraction k_TriggerInteraction = QueryTriggerInteraction.Collide;

    private void Awake()
    {
        weaponController = GetComponentInParent<WeaponController>();
    }

    private void OnTriggerStay(Collider collider)
    {
        Debug.Log("collide");
        if (weaponController)
        {
            Debug.Log("exists");
            if (weaponController.IsShootingFire)
            {
                Debug.Log("fire");
                m_IgnoredColliders = new List<Collider>();

                // Ignore colliders of owner
                if (weaponController)
                {
                    Collider[] ownerColliders = weaponController.Owner.GetComponentsInChildren<Collider>();
                    m_IgnoredColliders.AddRange(ownerColliders);
                }

                if (collider.GetComponent<Damageable>() && root)
                {
                    PlayerWeaponsManager[] playerWeaponsManagers = weaponController.Owner.GetComponents<PlayerWeaponsManager>();
                    PlayerWeaponsManager mainWeaponsManager = null;
                    foreach (var weaponsManager in playerWeaponsManagers)
                    {
                        if (weaponsManager.isMain)
                        {
                            mainWeaponsManager = weaponsManager;
                            break;
                        }
                    }

                    bool hitsValid = true;
                    if (mainWeaponsManager)
                    {
                        // Make sure trigger doesn't go through walls
                        Vector3 cameraToRoot = root.position - mainWeaponsManager.WeaponCamera.transform.position;
                        if (Physics.Raycast(mainWeaponsManager.WeaponCamera.transform.position, cameraToRoot.normalized,
                            out RaycastHit closeHit, cameraToRoot.magnitude, HittableLayers, k_TriggerInteraction))
                        {
                            if (IsHitValid(closeHit) && closeHit.collider.GetComponent<Damageable>() == null)
                            {
                                hitsValid = false;
                                OnHit(closeHit.collider);
                            }
                        }
                    }

                    if (hitsValid)
                    {
                        Debug.Log("hit");
                        // Hit detection
                        Vector3 colliderDir = collider.transform.position - root.position;
                        RaycastHit[] hits = Physics.RaycastAll(root.position, colliderDir.normalized, colliderDir.magnitude, HittableLayers, k_TriggerInteraction);
                        foreach (var hit in hits)
                        {
                            if (IsHitValid(hit) && hit.collider.GetComponent<Damageable>() == null)
                            {
                                hitsValid = false;
                                break;
                            }
                        }

                        if (hitsValid)
                        {
                            Debug.Log("hit");
                            OnHit(collider);
                        }
                    }
                }
            }
        }
    }

    bool IsHitValid(RaycastHit hit)
    {
        // true if collider is a shield and trigger is from player
        if (hit.collider.GetComponent<Shield>() != null)
        {
            if (weaponController.Owner.CompareTag("Player"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // ignore hits with an ignore component
        if (hit.collider.GetComponent<IgnoreHitDetection>())
        {
            return false;
        }

        // ignore hits with triggers that don't have a Damageable component
        if (hit.collider.isTrigger && hit.collider.GetComponent<Damageable>() == null)
        {
            return false;
        }

        // ignore hits with specific ignored colliders (self colliders, by default)
        if (m_IgnoredColliders != null && m_IgnoredColliders.Contains(hit.collider))
        {
            return false;
        }

        return true;
    }

    void OnHit(Collider collider)
    {
        Damageable damageable = collider.GetComponent<Damageable>();
        if (damageable)
        {
            if (collider.CompareTag("Generator"))
            {
                if (weaponController.Owner.CompareTag("Player"))
                {
                    damageable.InflictDamage(Damage, false, weaponController.Owner);
                }
            }
            else
            {
                damageable.InflictDamage(Damage, false, weaponController.Owner);
            }
        }
    }
}
