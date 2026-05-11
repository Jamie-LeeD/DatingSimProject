using DatingSim.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DatingSim.UI
{
    public class UiButtonSfx : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private AudioClip clickSfx;

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.AddListener(PlayClickSfx);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(PlayClickSfx);
            }
        }

        private void PlayClickSfx()
        {
            if (audioManager != null && clickSfx != null)
            {
                audioManager.PlaySfx(clickSfx);
            }
        }
    }
}
