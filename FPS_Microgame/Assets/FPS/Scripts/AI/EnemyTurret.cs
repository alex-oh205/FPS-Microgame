using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(EnemyController))]
    public class EnemyTurret : MonoBehaviour
    {
        public enum AIState
        {
            Idle,
            Attack,
            Stunned
        }

        public Transform TurretPivot;
        public Transform TurretAimPoint;
        public Animator Animator;
        public float AimRotationSharpness = 5f;
        public float LookAtRotationSharpness = 2.5f;
        public float DetectionFireDelay = 1f;
        public float AimingTransitionBlendTime = 1f;

        [Tooltip("The random hit damage effects")]
        public ParticleSystem[] RandomHitSparks;

        [Tooltip("Effect when stunned")]
        public ParticleSystem StunnedVFX;

        [Tooltip("Effects when detecting a target")]
        public ParticleSystem[] OnDetectVfx;
        public AudioClip OnDetectSfx;

        public AIState AiState { get; private set; }

        EnemyController m_EnemyController;
        Health m_Health;
        Quaternion m_RotationWeaponForwardToPivot;
        float m_TimeStartedDetection;
        float m_TimeLostDetection;
        Quaternion m_PreviousPivotAimingRotation;
        Quaternion m_PivotAimingRotation;
        Quaternion m_StunnedPivotRotation;

        const string k_AnimOnDamagedParameter = "OnDamaged";
        const string k_AnimIsActiveParameter = "IsActive";

        void Start()
        {
            m_Health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, EnemyTurret>(m_Health, this, gameObject);
            m_Health.OnDamaged += OnDamaged;

            m_EnemyController = GetComponent<EnemyController>();
            DebugUtility.HandleErrorIfNullGetComponent<EnemyController, EnemyTurret>(m_EnemyController, this,
                gameObject);

            m_EnemyController.onDetectedTarget += OnDetectedTarget;
            m_EnemyController.onLostTarget += OnLostTarget;

            // Remember the rotation offset between the pivot's forward and the weapon's forward
            m_RotationWeaponForwardToPivot =
                Quaternion.Inverse(m_EnemyController.GetCurrentWeapon().WeaponMuzzle.rotation) * TurretPivot.rotation;

            // Start with idle
            AiState = AIState.Idle;

            m_TimeStartedDetection = Mathf.NegativeInfinity;
            m_PreviousPivotAimingRotation = TurretPivot.rotation;
            m_StunnedPivotRotation = TurretPivot.rotation;
        }

        void Update()
        {
            UpdateCurrentAiState();
        }

        void LateUpdate()
        {
            UpdateTurretAiming();
        }

        void UpdateCurrentAiState()
        {
            // Handle logic 
            switch (AiState)
            {
                case AIState.Attack:
                    bool mustShoot = Time.time > m_TimeStartedDetection + DetectionFireDelay;
                    // Calculate the desired rotation of our turret (aim at target)
                    Vector3 directionToTarget =
                        (m_EnemyController.KnownDetectedTarget.transform.position - TurretAimPoint.position).normalized;
                    Quaternion offsettedTargetRotation =
                        Quaternion.LookRotation(directionToTarget) * m_RotationWeaponForwardToPivot;
                    m_PivotAimingRotation = Quaternion.Slerp(m_PreviousPivotAimingRotation, offsettedTargetRotation,
                        (mustShoot ? AimRotationSharpness : LookAtRotationSharpness) * Time.deltaTime);

                    // shoot
                    if (mustShoot)
                    {
                        Vector3 correctedDirectionToTarget =
                            (m_PivotAimingRotation * Quaternion.Inverse(m_RotationWeaponForwardToPivot)) *
                            Vector3.forward;

                        m_EnemyController.TryAttack(TurretAimPoint.position + correctedDirectionToTarget);
                    }

                    break;
                case AIState.Stunned:
                    if (!m_EnemyController.IsStunned)
                    {
                        m_EnemyController.DetectionModule.ResetTarget();
                        StunnedVFX?.Stop();
                    }
                    break;
            }

            if (m_EnemyController.IsStunned)
            {
                if (AiState != AIState.Stunned)
                {
                    m_EnemyController.DetectionModule.OnStun();
                    m_StunnedPivotRotation = TurretPivot.rotation;
                    Animator.speed = 0f;
                    StunnedVFX?.Play();
                }

                AiState = AIState.Stunned;
            }
        }

        void UpdateTurretAiming()
        {
            switch (AiState)
            {
                case AIState.Attack:
                    TurretPivot.rotation = m_PivotAimingRotation;
                    break;
                case AIState.Stunned:
                    TurretPivot.rotation = m_StunnedPivotRotation;
                    break;
                default:
                    // Use the turret rotation of the animation
                    TurretPivot.rotation = Quaternion.Slerp(m_PivotAimingRotation, TurretPivot.rotation,
                        (Time.time - m_TimeLostDetection) / AimingTransitionBlendTime);
                    break;
            }

            m_PreviousPivotAimingRotation = TurretPivot.rotation;
        }

        void OnDamaged(float dmg, GameObject source, bool stun, float stunDuration)
        {
            if (RandomHitSparks.Length > 0)
            {
                int n = Random.Range(0, RandomHitSparks.Length - 1);
                RandomHitSparks[n].Play();
            }

            Animator.SetTrigger(k_AnimOnDamagedParameter);
        }

        void OnDetectedTarget()
        {
            if (AiState == AIState.Stunned)
            {
                return;
            }

            if (AiState == AIState.Idle)
            {
                AiState = AIState.Attack;
            }

            for (int i = 0; i < OnDetectVfx.Length; i++)
            {
                OnDetectVfx[i].Play();
            }

            if (OnDetectSfx)
            {
                AudioUtility.CreateSFX(OnDetectSfx, transform.position, AudioUtility.AudioGroups.EnemyDetection, 1f);
            }

            Animator.SetBool(k_AnimIsActiveParameter, true);
            m_TimeStartedDetection = Time.time;
        }

        void OnLostTarget()
        {
            if (AiState == AIState.Stunned)
            {
                if (m_EnemyController.IsStunned)
                {
                    return;
                }

                Animator.speed = 1f;
            }

            if (AiState == AIState.Attack || AiState == AIState.Stunned)
            {
                AiState = AIState.Idle;
            }

            for (int i = 0; i < OnDetectVfx.Length; i++)
            {
                OnDetectVfx[i].Stop();
            }

            m_EnemyController.GetCurrentWeapon().HandleShootInputs(false, false, false);

            Animator.SetBool(k_AnimIsActiveParameter, false);
            m_TimeLostDetection = Time.time;
        }
    }
}