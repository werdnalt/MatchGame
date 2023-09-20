using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXManager : MonoBehaviour
{
    public static FXManager Instance;

    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private ParticleSystem deathParticles;

    public enum ParticleType
    {
        Death,
        Hit
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void PlayParticles(ParticleType particleType, Vector3 position)
    {
        switch (particleType)
        {
            case (ParticleType.Hit):
                var particles = Instantiate(hitParticles.gameObject, transform);
                particles.transform.position = position;
                break;
        }
    }
}
