using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyperCasual.Runner;

public class ContainerCollision : MonoBehaviour
{    
    private ParticleSystem explosionParticles;
    private AudioSource audio;

    bool exploded = false;

    private void Start()
    {
        explosionParticles = GetComponentInChildren<ParticleSystem>();
        audio = GetComponentInChildren<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boss"))
        {
            if (!exploded)
                Explode();
        }
    }

    private void Explode()
    {
        exploded = true;

        // Play the explosion particle effect
        explosionParticles.Play();
        audio.Play();
        GetComponentInChildren<BreakableWindow>().breakWindow();
        StartCoroutine(PlayerController.Instance.WaitAndEndGame());
    }
}
