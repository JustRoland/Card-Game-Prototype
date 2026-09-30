using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Sound
{
    public class SoundManager : MonoBehaviour
    {

        public static SoundManager Instance;

        [SerializeField] private AudioMixer audioMixer;
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambientAudioSource;
        [SerializeField] private AudioSource playerAudioSource;
    

        [Header("Sound Effects")]
        [SerializeField] private AudioClip walkSound;
        [SerializeField] private AudioClip crouchWalkSound;
        [SerializeField] private AudioClip sprintSound;
        [SerializeField] private AudioClip jumpSound;
        [SerializeField] private AudioClip landSound;
        [SerializeField] private AudioClip dashSound;

        [Header("Background Sounds")] 
        [SerializeField] private AudioClip music;
        [SerializeField] private AudioClip ambience;
        
        public AudioMixer AudioMixer => audioMixer;


        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void PlayPlayerSound(SoundEffects soundEffect)
        {
            switch (soundEffect)
            {
                case SoundEffects.Walk:
                    if (walkSound == null) return;
                    playerAudioSource.clip = walkSound;
                    playerAudioSource.Play();
                    break;
                case SoundEffects.CrouchWalk:
                    if (crouchWalkSound == null) return;
                    playerAudioSource.clip = crouchWalkSound;
                    playerAudioSource.Play();
                    break;
                case SoundEffects.Sprint:
                    if (sprintSound == null) return;
                    playerAudioSource.clip = sprintSound;
                    playerAudioSource.Play();
                    break;
                case SoundEffects.Jump:
                    if (jumpSound == null) return;
                    playerAudioSource.clip = jumpSound;
                    playerAudioSource.Play();
                    break;
                case SoundEffects.Dash:
                    if (dashSound == null) return;
                    playerAudioSource.clip = dashSound;
                    playerAudioSource.Play();
                    break;
                case SoundEffects.Land:
                    if (landSound == null) return;
                    playerAudioSource.clip = landSound;
                    playerAudioSource.Play();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(soundEffect), soundEffect, null);
            }
        }
    
    
    }

    public enum SoundEffects
    {
        Walk,
        CrouchWalk,
        Sprint,
        Jump,
        Land,
        Dash,
    }
}
