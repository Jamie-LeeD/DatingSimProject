using System;
using System.Collections.Generic;
using UnityEngine;

namespace DatingSim.Core
{
    [Serializable]
    public class PlaceholderSpriteEntry
    {
        public string id;
        public Sprite sprite;
    }

    [Serializable]
    public class PlaceholderAudioEntry
    {
        public string id;
        public AudioClip clip;
    }

    [CreateAssetMenu(menuName = "DatingSim/Debug/Placeholder Asset Manifest", fileName = "PlaceholderAssetManifest")]
    public class PlaceholderAssetManifest : ScriptableObject
    {
        [Header("Background Sprites")]
        [SerializeField] private List<PlaceholderSpriteEntry> backgrounds = new List<PlaceholderSpriteEntry>();

        [Header("Portrait Sprites")]
        [SerializeField] private List<PlaceholderSpriteEntry> portraits = new List<PlaceholderSpriteEntry>();

        [Header("UI Sprites")]
        [SerializeField] private List<PlaceholderSpriteEntry> uiSprites = new List<PlaceholderSpriteEntry>();

        [Header("Temporary Audio Clips")]
        [SerializeField] private List<PlaceholderAudioEntry> audioClips = new List<PlaceholderAudioEntry>();

        public IReadOnlyList<PlaceholderSpriteEntry> Backgrounds => backgrounds;
        public IReadOnlyList<PlaceholderSpriteEntry> Portraits => portraits;
        public IReadOnlyList<PlaceholderSpriteEntry> UiSprites => uiSprites;
        public IReadOnlyList<PlaceholderAudioEntry> AudioClips => audioClips;

        public void SetData(
            List<PlaceholderSpriteEntry> backgroundEntries,
            List<PlaceholderSpriteEntry> portraitEntries,
            List<PlaceholderSpriteEntry> uiEntries,
            List<PlaceholderAudioEntry> audioEntries)
        {
            backgrounds = backgroundEntries;
            portraits = portraitEntries;
            uiSprites = uiEntries;
            audioClips = audioEntries;
        }
    }
}
