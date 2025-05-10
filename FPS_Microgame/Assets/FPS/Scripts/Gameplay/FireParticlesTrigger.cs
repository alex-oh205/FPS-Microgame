using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class FireParticlesTrigger : MonoBehaviour
{
    ParticleSystem ps;
    List<Particle> inside;
    Collider[] allColliders;
    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
        inside = new List<Particle>();

        allColliders = FindObjectsOfType<Collider>();
        foreach (Collider collider in allColliders)
        {
            if (collider.name != "DamageTrigger" && collider.gameObject.GetComponent<Damageable>() == null)
            {
                ps.trigger.AddCollider(collider);
            }
        }
    }

    private void OnParticleTrigger()
    {
        int numInside = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Inside, inside);

        // iterate
        for (int i = 0; i < numInside; i++)
        {
            Particle p = inside[i];
            p.remainingLifetime = 0f;
            inside[i] = p;
        }

        // set
        ps.SetTriggerParticles(ParticleSystemTriggerEventType.Inside, inside);
    }
}
