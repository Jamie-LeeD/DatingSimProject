using System.Collections.Generic;
using System.IO;
using DatingSim.Core;
using UnityEditor;
using UnityEngine;

namespace DatingSim.EditorTools
{
    public static class PlaceholderAssetGenerator
    {
        private const string RootFolder = "Assets/Art/Placeholders";
        private const string BackgroundFolder = "Assets/Art/Placeholders/Backgrounds";
        private const string PortraitFolder = "Assets/Art/Placeholders/Portraits";
        private const string UiFolder = "Assets/Art/Placeholders/UI";
        private const string AudioFolder = "Assets/Audio/Placeholders";
        private const string DataFolder = "Assets/Data/Placeholders";

        [MenuItem("Tools/Dating Sim/Generate Placeholder Assets")]
        public static void Generate()
        {
            EnsureFolders();

            var backgroundEntries = new List<PlaceholderSpriteEntry>
            {
                new PlaceholderSpriteEntry { id = "bg_school_gate_day", sprite = CreateSpriteAsset("bg_school_gate_day", BackgroundFolder, 1920, 1080, new Color(0.21f, 0.47f, 0.86f)) },
                new PlaceholderSpriteEntry { id = "bg_hallway_day", sprite = CreateSpriteAsset("bg_hallway_day", BackgroundFolder, 1920, 1080, new Color(0.25f, 0.66f, 0.51f)) },
                new PlaceholderSpriteEntry { id = "bg_classroom_day", sprite = CreateSpriteAsset("bg_classroom_day", BackgroundFolder, 1920, 1080, new Color(0.95f, 0.73f, 0.27f)) }
            };

            var portraitEntries = new List<PlaceholderSpriteEntry>
            {
                new PlaceholderSpriteEntry { id = "aiko_neutral", sprite = CreateSpriteAsset("portrait_aiko_neutral", PortraitFolder, 512, 768, new Color(0.92f, 0.48f, 0.69f)) },
                new PlaceholderSpriteEntry { id = "aiko_happy", sprite = CreateSpriteAsset("portrait_aiko_happy", PortraitFolder, 512, 768, new Color(0.97f, 0.62f, 0.78f)) },
                new PlaceholderSpriteEntry { id = "aiko_sad", sprite = CreateSpriteAsset("portrait_aiko_sad", PortraitFolder, 512, 768, new Color(0.72f, 0.40f, 0.58f)) },
                new PlaceholderSpriteEntry { id = "mc_neutral", sprite = CreateSpriteAsset("portrait_mc_neutral", PortraitFolder, 512, 768, new Color(0.36f, 0.56f, 0.95f)) },
                new PlaceholderSpriteEntry { id = "teacher_neutral", sprite = CreateSpriteAsset("portrait_teacher_neutral", PortraitFolder, 512, 768, new Color(0.95f, 0.50f, 0.34f)) }
            };

            var uiEntries = new List<PlaceholderSpriteEntry>
            {
                new PlaceholderSpriteEntry { id = "ui_dialogue_panel", sprite = CreateSpriteAsset("ui_dialogue_panel", UiFolder, 1024, 300, new Color(0.12f, 0.12f, 0.17f, 0.92f)) },
                new PlaceholderSpriteEntry { id = "ui_choice_button", sprite = CreateSpriteAsset("ui_choice_button", UiFolder, 700, 120, new Color(0.20f, 0.20f, 0.29f, 0.98f)) },
                new PlaceholderSpriteEntry { id = "ui_fade_panel", sprite = CreateSpriteAsset("ui_fade_panel", UiFolder, 512, 512, new Color(0f, 0f, 0f, 1f)) }
            };

            var audioEntries = new List<PlaceholderAudioEntry>
            {
                new PlaceholderAudioEntry { id = "bgm_menu_loop", clip = CreateToneWav("bgm_menu_loop", AudioFolder, 220f, 2.0f, 0.08f) },
                new PlaceholderAudioEntry { id = "sfx_button_click", clip = CreateToneWav("sfx_button_click", AudioFolder, 880f, 0.10f, 0.20f) },
                new PlaceholderAudioEntry { id = "sfx_dialogue_advance", clip = CreateToneWav("sfx_dialogue_advance", AudioFolder, 660f, 0.08f, 0.18f) }
            };

            CreateOrUpdateManifest(backgroundEntries, portraitEntries, uiEntries, audioEntries);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Placeholder Assets", "Placeholder assets generated successfully.", "OK");
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Art");
            EnsureFolder("Assets/Art", "Placeholders");
            EnsureFolder(RootFolder, "Backgrounds");
            EnsureFolder(RootFolder, "Portraits");
            EnsureFolder(RootFolder, "UI");

            EnsureFolder("Assets", "Audio");
            EnsureFolder("Assets/Audio", "Placeholders");

            EnsureFolder("Assets", "Data");
            EnsureFolder("Assets/Data", "Placeholders");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string fullPath = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static Sprite CreateSpriteAsset(string fileName, string folder, int width, int height, Color color)
        {
            string path = $"{folder}/{fileName}.png";
            WriteSolidPng(path, width, height, color);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                textureImporter.alphaIsTransparency = true;
                textureImporter.mipmapEnabled = false;
                textureImporter.filterMode = FilterMode.Bilinear;
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                textureImporter.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void WriteSolidPng(string path, int width, int height, Color color)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();

            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            Object.DestroyImmediate(texture);
        }

        private static AudioClip CreateToneWav(string fileName, string folder, float frequencyHz, float durationSeconds, float volume)
        {
            string path = $"{folder}/{fileName}.wav";
            WriteToneWav(path, frequencyHz, durationSeconds, volume);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
            if (audioImporter != null)
            {
                var settings = audioImporter.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.quality = 1f;
                audioImporter.defaultSampleSettings = settings;
                audioImporter.forceToMono = true;
                audioImporter.preloadAudioData = true;
                audioImporter.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        private static void WriteToneWav(string path, float frequencyHz, float durationSeconds, float volume)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * durationSeconds));
            short channels = 1;
            short bitsPerSample = 16;
            int byteRate = sampleRate * channels * bitsPerSample / 8;
            short blockAlign = (short)(channels * bitsPerSample / 8);
            int dataSize = sampleCount * blockAlign;

            using (var writer = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                writer.Write(new[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + dataSize);
                writer.Write(new[] { 'W', 'A', 'V', 'E' });
                writer.Write(new[] { 'f', 'm', 't', ' ' });
                writer.Write(16);
                writer.Write((short)1);
                writer.Write(channels);
                writer.Write(sampleRate);
                writer.Write(byteRate);
                writer.Write(blockAlign);
                writer.Write(bitsPerSample);
                writer.Write(new[] { 'd', 'a', 't', 'a' });
                writer.Write(dataSize);

                for (int i = 0; i < sampleCount; i++)
                {
                    float t = i / (float)sampleRate;
                    float sample = Mathf.Sin(2f * Mathf.PI * frequencyHz * t) * Mathf.Clamp01(volume);
                    short pcm = (short)Mathf.Clamp(sample * short.MaxValue, short.MinValue, short.MaxValue);
                    writer.Write(pcm);
                }
            }
        }

        private static void CreateOrUpdateManifest(
            List<PlaceholderSpriteEntry> backgroundEntries,
            List<PlaceholderSpriteEntry> portraitEntries,
            List<PlaceholderSpriteEntry> uiEntries,
            List<PlaceholderAudioEntry> audioEntries)
        {
            const string manifestPath = DataFolder + "/PlaceholderAssetManifest.asset";
            PlaceholderAssetManifest manifest = AssetDatabase.LoadAssetAtPath<PlaceholderAssetManifest>(manifestPath);
            if (manifest == null)
            {
                manifest = ScriptableObject.CreateInstance<PlaceholderAssetManifest>();
                AssetDatabase.CreateAsset(manifest, manifestPath);
            }

            manifest.SetData(backgroundEntries, portraitEntries, uiEntries, audioEntries);
            EditorUtility.SetDirty(manifest);
        }
    }
}
