using System;
using UnityEngine;

namespace DatingSim.Core
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Settings")]
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

        public event Action<AudioClip> BgmChanged;
        public event Action<AudioClip> SfxPlayed;

        public float MasterVolume => masterVolume;
        public float BgmVolume => bgmVolume;
        public float SfxVolume => sfxVolume;

        private void Awake()
        {
            EnsureAudioSources();
            ApplyVolumes();
        }

        public void PlayBgm(AudioClip clip, bool loop = true)
        {
            if (bgmSource == null || clip == null)
            {
                return;
            }

            if (bgmSource.clip == clip && bgmSource.isPlaying)
            {
                return;
            }

            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();
            BgmChanged?.Invoke(clip);
        }

        public void StopBgm()
        {
            if (bgmSource == null)
            {
                return;
            }

            bgmSource.Stop();
            bgmSource.clip = null;
        }

        public void PlaySfx(AudioClip clip)
        {
            if (sfxSource == null || clip == null)
            {
                return;
            }

            sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
            SfxPlayed?.Invoke(clip);
        }

        public void SetMasterVolume(float value)
        {
            masterVolume = Mathf.Clamp01(value);
            ApplyVolumes();
        }

        public void SetBgmVolume(float value)
        {
            bgmVolume = Mathf.Clamp01(value);
            ApplyVolumes();
        }

        public void SetSfxVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            ApplyVolumes();
        }

        private void EnsureAudioSources()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.playOnAwake = false;
                bgmSource.loop = true;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.loop = false;
            }
        }

        private void ApplyVolumes()
        {
            if (bgmSource != null)
            {
                bgmSource.volume = bgmVolume * masterVolume;
            }

            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume * masterVolume;
            }
        }
    }
}
