using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Sound;

namespace Utility
{
    public class Settings : MonoBehaviour
    {
        public static Settings Instance;
        
        public float MasterVolume { get; private set; }
        public float MusicVolume { get; private set; }
        public float SFXVolume { get; private set; }
        public float AmbienceVolume { get; private set; }

        public bool AllowHeadBobbing { get; private set; }
        public float MouseSensitivity { get; private set; }


        [Header("References")] [SerializeField]
        private GameObject overlay;
        [SerializeField] private Slider mouseSensitivitySlider;
        [SerializeField] private Toggle allowHeadBobbingToggle;
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider ambienceVolumeSlider;
        
        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
            
            LoadSettings();
            overlay.SetActive(false);
        }


        public void LoadSettings()
        {
            MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.66f);
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.66f);
            SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0.66f);
            AmbienceVolume = PlayerPrefs.GetFloat("AmbienceVolume", 0.66f);

            AllowHeadBobbing = PlayerPrefs.GetInt("AllowHeadBobbing", 1) == 1;
            MouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 0.12f);
            
            mouseSensitivitySlider.value = MouseSensitivity;
            allowHeadBobbingToggle.isOn = AllowHeadBobbing;
            masterVolumeSlider.value = MasterVolume;
            musicVolumeSlider.value = MusicVolume;
            sfxVolumeSlider.value = SFXVolume;
            ambienceVolumeSlider.value = AmbienceVolume;
            
            
            if (SoundManager.Instance == null) return;
            SoundManager.Instance.AudioMixer.SetFloat("MasterVolume", Mathf.Log10(MasterVolume) * 20);
            SoundManager.Instance.AudioMixer.SetFloat("MusicVolume", Mathf.Log10(MusicVolume) * 20);
            SoundManager.Instance.AudioMixer.SetFloat("SFXVolume", Mathf.Log10(SFXVolume) * 20);
            SoundManager.Instance.AudioMixer.SetFloat("AmbienceVolume", Mathf.Log10(AmbienceVolume) * 20);
        }

        public void UpdateMasterVolume(float volume)
        {
            MasterVolume = volume;
            PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
            if (SoundManager.Instance == null) return;
            SoundManager.Instance.AudioMixer.SetFloat("MasterVolume", Mathf.Log10(MasterVolume) * 20);
        }

        public void UpdateMusicVolume(float volume)
        {
            MusicVolume = volume;
            PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
            if (SoundManager.Instance == null) return;
            SoundManager.Instance.AudioMixer.SetFloat("MusicVolume", Mathf.Log10(MusicVolume) * 20);
        }

        public void UpdateSFXVolume(float volume)
        {
            SFXVolume = volume;
            PlayerPrefs.SetFloat("SFXVolume", SFXVolume);
            if (SoundManager.Instance == null) return;
            SoundManager.Instance.AudioMixer.SetFloat("SFXVolume", Mathf.Log10(SFXVolume) * 20);
        }

        public void UpdateAmbienceVolume(float volume)
        {
            AmbienceVolume = volume;
            PlayerPrefs.SetFloat("AmbienceVolume", AmbienceVolume);
            if (SoundManager.Instance == null) return;
            SoundManager.Instance.AudioMixer.SetFloat("AmbienceVolume", AmbienceVolume);
        }

        public void UpdateAllowHeadBobbing(bool value)
        {
            AllowHeadBobbing = value;
            PlayerPrefs.SetInt("AllowHeadBobbing", value ? 1 : 0);
        }

        public void UpdateMouseSensitivity(float value)
        {
            MouseSensitivity = value;
            PlayerPrefs.SetFloat("MouseSensitivity", MouseSensitivity);
        }

        public void ToggleOverlay()
        {
            overlay.SetActive(!overlay.activeSelf);
            if (overlay.activeSelf)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Time.timeScale = 0f;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Time.timeScale = 1f;
            }
        }
    }
}