using DatingSim.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DatingSim.UI
{
    public class MenuSettingsPanelController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private AudioManager audioManager;

        [Header("Sliders")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI masterValueLabel;
        [SerializeField] private TextMeshProUGUI musicValueLabel;
        [SerializeField] private TextMeshProUGUI sfxValueLabel;

        private void Awake()
        {
            SyncFromAudioManager();
            BindSliderEvents();
            RefreshLabels();
        }

        private void OnDestroy()
        {
            UnbindSliderEvents();
        }

        private void BindSliderEvents()
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }
        }

        private void UnbindSliderEvents()
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            }
        }

        private void SyncFromAudioManager()
        {
            if (audioManager == null)
            {
                return;
            }

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.SetValueWithoutNotify(audioManager.MasterVolume);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.SetValueWithoutNotify(audioManager.BgmVolume);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.SetValueWithoutNotify(audioManager.SfxVolume);
            }
        }

        private void OnMasterVolumeChanged(float value)
        {
            if (audioManager != null)
            {
                audioManager.SetMasterVolume(value);
            }

            RefreshLabels();
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (audioManager != null)
            {
                audioManager.SetBgmVolume(value);
            }

            RefreshLabels();
        }

        private void OnSfxVolumeChanged(float value)
        {
            if (audioManager != null)
            {
                audioManager.SetSfxVolume(value);
            }

            RefreshLabels();
        }

        private void RefreshLabels()
        {
            if (masterValueLabel != null && masterVolumeSlider != null)
            {
                masterValueLabel.text = ToPercent(masterVolumeSlider.value);
            }

            if (musicValueLabel != null && musicVolumeSlider != null)
            {
                musicValueLabel.text = ToPercent(musicVolumeSlider.value);
            }

            if (sfxValueLabel != null && sfxVolumeSlider != null)
            {
                sfxValueLabel.text = ToPercent(sfxVolumeSlider.value);
            }
        }

        private static string ToPercent(float value)
        {
            return Mathf.RoundToInt(value * 100f) + "%";
        }
    }
}
