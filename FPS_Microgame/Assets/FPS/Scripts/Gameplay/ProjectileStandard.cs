using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ProjectileStandard : ProjectileBase
    {
        public enum ProjectileType
        {
            Normal,
            Beam,
            Fire
        }

        [Header("General")] [Tooltip("Radius of this projectile's collision detection")]
        public float Radius = 0.01f;

        [Tooltip("Transform representing the root of the projectile (used for accurate collision detection)")]
        public Transform Root;

        [Tooltip("Transform representing the tip of the projectile (used for accurate collision detection)")]
        public Transform Tip;

        [Tooltip("LifeTime of the projectile")]
        public float MaxLifeTime = 5f;

        [Tooltip("VFX prefab to spawn upon impact")]
        public GameObject ImpactVfx;

        [Tooltip("LifeTime of the VFX before being destroyed")]
        public float ImpactVfxLifetime = 5f;

        [Tooltip("Offset along the hit normal where the VFX will be spawned")]
        public float ImpactVfxSpawnOffset = 0.1f;

        [Tooltip("Clip to play on impact")] 
        public AudioClip ImpactSfxClip;

        [Tooltip("Layers this projectile can collide with")]
        public LayerMask HittableLayers = -1;

        [Tooltip("Type of projectile")]
        public ProjectileType projectileType = ProjectileType.Normal;

        [Header("Movement")] [Tooltip("Speed of the projectile")]
        public float Speed = 20f;

        [Tooltip("Downward acceleration from gravity")]
        public float GravityDownAcceleration = 0f;

        [Tooltip(
            "Distance over which the projectile will correct its course to fit the intended trajectory (used to drift projectiles towards center of screen in First Person view). At values under 0, there is no correction")]
        public float TrajectoryCorrectionDistance = -1;

        [Tooltip("Determines if the projectile inherits the velocity that the weapon's muzzle had when firing")]
        public bool InheritWeaponVelocity = false;

        [Header("Damage")] [Tooltip("Damage of the projectile")]
        public float Damage = 40f;

        [Tooltip("Area of damage. Keep empty if you don't want area damage")]
        public DamageArea AreaOfDamage;

        [Tooltip("Stun target on hit")]
        public bool Stun = false;

        public float StunDuration = 3f;

        [Header("Debug")] [Tooltip("Color of the projectile radius debug view")]
        public Color RadiusColor = Color.cyan * 0.2f;

        [HideInInspector]
        public bool collideWithShield;

        ProjectileBase m_ProjectileBase;
        Vector3 m_LastRootPosition;
        Vector3 m_Velocity;
        bool m_HasTrajectoryOverride;
        float m_ShootTime;
        Vector3 m_TrajectoryCorrectionVector;
        Vector3 m_ConsumedTrajectoryCorrectionVector;
        List<Collider> m_IgnoredColliders;
        
        const int k_EnemyLayer = 6;
        const QueryTriggerInteraction k_TriggerInteraction = QueryTriggerInteraction.Collide;

        void OnEnable()
        {
            m_ProjectileBase = GetComponent<ProjectileBase>();
            DebugUtility.HandleErrorIfNullGetComponent<ProjectileBase, ProjectileStandard>(m_ProjectileBase, this, gameObject);

            m_ProjectileBase.OnShoot += OnShoot;

            if (projectileType != ProjectileType.Beam)
            {
                if (projectileType == ProjectileType.Fire)
                {
                    StartCoroutine(Lifetime());
                }
                else
                {
                    Destroy(gameObject, MaxLifeTime);
                }
            }
        }

        IEnumerator Lifetime()
        {
            yield return new WaitForSeconds(MaxLifeTime);

            // prevent deletion of fire particles
            transform.DetachChildren();

            Destroy(gameObject);
        }

        new void OnShoot()
        {
            m_ShootTime = Time.time;
            m_LastRootPosition = Root.position;
            m_Velocity = transform.forward * Speed;
            m_IgnoredColliders = new List<Collider>();
            transform.position += m_ProjectileBase.InheritedMuzzleVelocity * Time.deltaTime;

            // Ignore colliders of owner
            Collider[] ownerColliders = m_ProjectileBase.Owner.GetComponentsInChildren<Collider>();
            m_IgnoredColliders.AddRange(ownerColliders);

            // Handle case of player shooting (make projectiles not go through walls, and remember center-of-screen trajectory)
            PlayerWeaponsManager[] playerWeaponsManagers = m_ProjectileBase.Owner.GetComponents<PlayerWeaponsManager>();
            PlayerWeaponsManager mainWeaponsManager = null;
            foreach (var weaponsManager in playerWeaponsManagers)
            {
                if (weaponsManager.isMain)
                {
                    mainWeaponsManager = weaponsManager;
                    break;
                }
            }
            
            if (mainWeaponsManager)
            {
                Vector3 cameraToMuzzle = (m_ProjectileBase.InitialPosition -
                                              mainWeaponsManager.WeaponCamera.transform.position);

                if (projectileType == ProjectileType.Normal || projectileType == ProjectileType.Fire)
                {
                    m_HasTrajectoryOverride = true;

                    m_TrajectoryCorrectionVector = Vector3.ProjectOnPlane(-cameraToMuzzle,
                        mainWeaponsManager.WeaponCamera.transform.forward);
                    if (TrajectoryCorrectionDistance == 0)
                    {
                        transform.position += m_TrajectoryCorrectionVector;
                        m_ConsumedTrajectoryCorrectionVector = m_TrajectoryCorrectionVector;
                    }
                    else if (TrajectoryCorrectionDistance < 0)
                    {
                        m_HasTrajectoryOverride = false;
                    }
                }

                if (Physics.Raycast(mainWeaponsManager.WeaponCamera.transform.position, cameraToMuzzle.normalized,
                    out RaycastHit hit, cameraToMuzzle.magnitude, HittableLayers, k_TriggerInteraction))
                {
                    if (IsHitValid(hit))
                    {
                        OnHit(hit.point, hit.normal, hit.collider);
                    }
                }
            }
        }

        void Update()
        {
            if (projectileType == ProjectileType.Beam)
            {
                Destroy(gameObject);

                // Hit detection
                if (m_ProjectileBase.Owner != null)
                {
                    Vector3 origin = transform.position;
                    Vector3 direction = transform.forward;
                    if (m_ProjectileBase.Owner.CompareTag("Player"))
                    {
                        // Get player camera
                        Transform playerCamera = null;
                        Camera[] cameras = m_ProjectileBase.Owner.GetComponentsInChildren<Camera>();
                        foreach (Camera cam in cameras)
                        {
                            if (cam.CompareTag("MainCamera"))
                            {
                                playerCamera = cam.transform;
                                break;
                            }
                        }

                        if (playerCamera != null)
                        {
                            origin = playerCamera.position;
                            direction = playerCamera.forward;
                        }
                    }

                    // Get closest undamageable object
                    RaycastHit thisHit = new RaycastHit();
                    thisHit.distance = Mathf.Infinity;
                    RaycastHit closestObjHit = new RaycastHit();
                    closestObjHit.distance = Mathf.Infinity;

                    RaycastHit[] hits = Physics.SphereCastAll(origin, Radius, direction, Mathf.Infinity, HittableLayers, k_TriggerInteraction);
                    foreach (var hit in hits)
                    {
                        if (IsHitValid(hit))
                        {
                            if (hit.distance < closestObjHit.distance && hit.collider.GetComponent<Damageable>() == null)
                            {
                                closestObjHit = hit;
                            }
                        }
                    }

                    // Hit all damageables not obstructed by objects
                    foreach (var hit in hits)
                    {
                        if (IsHitValid(hit))
                        {
                            thisHit = hit;

                            // Handle case of casting while already inside a collider
                            if (thisHit.distance <= 0f)
                            {
                                thisHit.point = Root.position;
                                thisHit.normal = -transform.forward;
                            }

                            if (thisHit.distance <= closestObjHit.distance)
                            {
                                OnHit(thisHit.point, thisHit.normal, thisHit.collider);
                            }
                        }
                    }
                }
            }
            else
            {
                // Move
                transform.position += m_Velocity * Time.deltaTime;
                if (InheritWeaponVelocity)
                {
                    transform.position += m_ProjectileBase.InheritedMuzzleVelocity * Time.deltaTime;
                }

                // Drift towards trajectory override (this is so that projectiles can be centered 
                // with the camera center even though the actual weapon is offset)
                if (m_HasTrajectoryOverride && m_ConsumedTrajectoryCorrectionVector.sqrMagnitude <
                    m_TrajectoryCorrectionVector.sqrMagnitude)
                {
                    Vector3 correctionLeft = m_TrajectoryCorrectionVector - m_ConsumedTrajectoryCorrectionVector;
                    float distanceThisFrame = (Root.position - m_LastRootPosition).magnitude;
                    Vector3 correctionThisFrame =
                        (distanceThisFrame / TrajectoryCorrectionDistance) * m_TrajectoryCorrectionVector;
                    correctionThisFrame = Vector3.ClampMagnitude(correctionThisFrame, correctionLeft.magnitude);
                    m_ConsumedTrajectoryCorrectionVector += correctionThisFrame;

                    // Detect end of correction
                    if (m_ConsumedTrajectoryCorrectionVector.sqrMagnitude == m_TrajectoryCorrectionVector.sqrMagnitude)
                    {
                        m_HasTrajectoryOverride = false;
                    }

                    transform.position += correctionThisFrame;
                }

                // Orient towards velocity
                transform.forward = m_Velocity.normalized;

                // Gravity
                if (GravityDownAcceleration > 0)
                {
                    // add gravity to the projectile velocity for ballistic effect
                    m_Velocity += Vector3.down * GravityDownAcceleration * Time.deltaTime;
                }

                // Hit detection
                {
                    RaycastHit closestHit = new RaycastHit();
                    closestHit.distance = Mathf.Infinity;
                    bool foundHit = false;

                    // Sphere cast
                    Vector3 displacementSinceLastFrame = Tip.position - m_LastRootPosition;
                    RaycastHit[] hits = Physics.SphereCastAll(m_LastRootPosition, Radius,
                        displacementSinceLastFrame.normalized, displacementSinceLastFrame.magnitude, HittableLayers,
                        k_TriggerInteraction);
                    foreach (var hit in hits)
                    {
                        if (IsHitValid(hit) && hit.distance < closestHit.distance)
                        {
                            foundHit = true;
                            closestHit = hit;
                        }
                    }

                    if (foundHit)
                    {
                        // Handle case of casting while already inside a collider
                        if (closestHit.distance <= 0f)
                        {
                            closestHit.point = Root.position;
                            closestHit.normal = -transform.forward;
                        }

                        OnHit(closestHit.point, closestHit.normal, closestHit.collider);
                    }
                }

                m_LastRootPosition = Root.position;
            }
        }

        bool IsHitValid(RaycastHit hit)
        {
            // true if collider is a shield and the bullet is from player
            if (hit.collider.GetComponent<Shield>() != null)
            {
                if (m_ProjectileBase.Owner.CompareTag("Player"))
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

        void OnHit(Vector3 point, Vector3 normal, Collider collider)
        {
            // damage
            if (AreaOfDamage)
            {
                // area damage
                AreaOfDamage.InflictDamageInArea(Damage, point, HittableLayers, k_TriggerInteraction,
                    m_ProjectileBase.Owner, m_ProjectileBase.OwnerActor, Stun, StunDuration);
            }
            else
            {
                // point damage
                Damageable damageable = collider.GetComponent<Damageable>();
                if (damageable)
                {
                    // If of the same affiliation, deal no damage
                    Actor hitActor = collider.GetComponentInParent<Actor>();
                    if (!hitActor || hitActor.Affiliation != m_ProjectileBase.OwnerActor.Affiliation)
                    {
                        damageable.InflictDamage(Damage, false, m_ProjectileBase.Owner, Stun, StunDuration);
                    }
                }
            }

            // impact vfx
            if (ImpactVfx)
            {
                GameObject impactVfxInstance = Instantiate(ImpactVfx, point + (normal * ImpactVfxSpawnOffset),
                    Quaternion.LookRotation(normal));
                if (ImpactVfxLifetime > 0)
                {
                    Destroy(impactVfxInstance.gameObject, ImpactVfxLifetime);
                }
            }

            // impact sfx
            if (ImpactSfxClip)
            {
                AudioUtility.CreateSFX(ImpactSfxClip, point, AudioUtility.AudioGroups.Impact, 1f, 3f);
            }

            // prevent deletion of particles
            if (projectileType == ProjectileType.Fire || projectileType == ProjectileType.Beam)
            {
                transform.DetachChildren();
            }

            // Self Destruct
            if (projectileType != ProjectileType.Beam)
            {
                Destroy(gameObject);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = RadiusColor;
            Gizmos.DrawSphere(transform.position, Radius);
        }
    }
}