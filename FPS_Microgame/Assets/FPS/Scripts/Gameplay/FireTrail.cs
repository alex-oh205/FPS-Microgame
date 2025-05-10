using Unity.FPS.Gameplay;
using UnityEngine;

public class FireTrail : MonoBehaviour
{
    ParticleSystem m_FireTrail;

    void Start()
    {
        m_FireTrail = GetComponent<ParticleSystem>();
    }
    void Update()
    {
        if (GetComponentInParent<ProjectileStandard>() == null && m_FireTrail.isPlaying)
        {
            m_FireTrail.Stop();
        }

        if (!m_FireTrail.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}
