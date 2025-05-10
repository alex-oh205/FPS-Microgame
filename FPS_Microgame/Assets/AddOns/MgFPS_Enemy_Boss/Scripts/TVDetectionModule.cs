using Unity.FPS.AI;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Addons
{
    public class TVDetectionModule : DetectionModule
    {
        enum State
        {
            Idle,
            Detect,
            Attack,
            Hurt,
            Stunned
        }

        [Header("Parameters")] [Tooltip("Duration of the Detect state")] [SerializeField]
        private float m_DetectStateDuration = 1f;

        [Tooltip("Duration of the Hurt state")] [SerializeField]
        private float m_HurtStateDuration = 1f;

        [Header("SFX")] [Tooltip("Audio clip for the Detect state")] [SerializeField]
        private AudioClip m_DetectSFX = null;

        [Tooltip("Audio clip for the Hurt state")] [SerializeField]
        private AudioClip m_HurtSFX = null;

        [Header("Screen textures")] [SerializeField]
        private Texture m_IdleTexture = null;

        [SerializeField] private Texture m_IdleEmissionTexture = null;
        [SerializeField] private Texture m_AttackTexture = null;
        [SerializeField] private Texture m_AttackEmissionTexture = null;
        [SerializeField] private Texture m_OnDetectTexture = null;
        [SerializeField] private Texture m_OnDetectEmissionTexture = null;
        [SerializeField] private Texture m_HurtTexture = null;
        [SerializeField] private Texture m_HurtEmissionTexture = null;
        [SerializeField] private Texture m_StunnedTexture = null;
        [SerializeField] private Texture m_StunnedEmissionTexture = null;

        [Tooltip("Reference to the screen's Renderer component")] [SerializeField]
        private Renderer m_TVRenderer = null;

        private AudioSource m_AudioSource;
        private Material m_CurrentScreenMaterial;
        private State m_CurrentState;
        private float m_TimeLastBeenHurt = Mathf.NegativeInfinity;
        private float m_TimeStartSeeingTarget = Mathf.NegativeInfinity;

        const string kMainTexture = "_MainTex";
        const string kEmissionTexture = "_EmissionMap";
        const string k_AnimOnDetectParameter = "OnDetect";
        const string k_AnimOnLostParameter = "OnLost";

        protected override void Start()
        {
            base.Start();

            m_AudioSource = GetComponent<AudioSource>();
            DebugUtility.HandleErrorIfNullGetComponent<AudioSource, EnemyMobile>(m_AudioSource, this, gameObject);

            m_CurrentScreenMaterial = m_TVRenderer.material;
            SetScreenTextures(m_IdleTexture, m_IdleEmissionTexture);
        }

        private void OnDestroy()
        {
            Destroy(m_CurrentScreenMaterial);
        }

        public override void OnDetect()
        {
            base.OnDetect();
            if (m_CurrentState != State.Stunned)
            {
                m_CurrentState = State.Detect;
                m_TimeStartSeeingTarget = Time.time;
                SetScreenTextures(m_OnDetectTexture, m_OnDetectEmissionTexture);
                m_AudioSource.PlayOneShot(m_DetectSFX);
                Animator.SetTrigger(k_AnimOnDetectParameter);
            }
        }

        public override void OnLostTarget()
        {
            base.OnLostTarget();
            m_CurrentState = State.Idle;
            SetScreenTextures(m_IdleTexture, m_IdleEmissionTexture);
            Animator.SetTrigger(k_AnimOnLostParameter);
        }

        public override void OnDamaged(GameObject agent, GameObject damageSource)
        {
            base.OnDamaged(agent, damageSource);
            m_TimeLastBeenHurt = Time.time;
            if (m_CurrentState == State.Attack)
            {
                m_CurrentState = State.Hurt;
                SetScreenTextures(m_HurtTexture, m_HurtEmissionTexture);

                if (m_HurtSFX != null)
                    m_AudioSource.PlayOneShot(m_HurtSFX);
            }
        }

        public override void OnStun()
        {
            base.OnStun();
            m_CurrentState = State.Stunned;
            SetScreenTextures(m_StunnedTexture, m_StunnedEmissionTexture);
        }

        private void Update()
        {
            switch (m_CurrentState)
            {
                case State.Idle:
                    break;
                case State.Detect:
                    if ((Time.time - m_TimeStartSeeingTarget) > m_DetectStateDuration)
                    {
                        m_CurrentState = State.Attack;
                        SetScreenTextures(m_AttackTexture, m_AttackEmissionTexture);
                    }

                    break;
                case State.Attack:
                    break;
                case State.Hurt:
                    if ((Time.time - m_TimeLastBeenHurt) > m_HurtStateDuration)
                    {
                        m_CurrentState = State.Attack;
                        SetScreenTextures(m_AttackTexture, m_AttackEmissionTexture);
                    }

                    break;
                case State.Stunned:
                    break;
                default:
                    break;
            }
        }

        private void SetScreenTextures(Texture mainTexture, Texture emissionTexture)
        {
            m_CurrentScreenMaterial.SetTexture(kMainTexture, mainTexture);
            m_CurrentScreenMaterial.SetTexture(kEmissionTexture, emissionTexture);
        }
    }
}