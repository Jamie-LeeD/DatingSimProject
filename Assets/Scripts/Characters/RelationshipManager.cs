using System;
using System.Collections.Generic;
using DatingSim.SaveSystem;
using UnityEngine;

namespace DatingSim.Characters
{
    public class RelationshipManager : MonoBehaviour
    {
        [Serializable]
        private class CharacterAffectionDebugEntry
        {
            public string characterId;
            public int affection;
        }

        [Header("Defaults")]
        [SerializeField] private int defaultAffection;
        [SerializeField] private CharacterDatabase characterDatabase;

        [Header("Route Unlock Thresholds (Optional Overrides)")]
        [SerializeField] private List<RelationshipThreshold> additionalRouteThresholds = new List<RelationshipThreshold>();

        [Header("Inspector Debug (Runtime)")]
        [SerializeField] private List<CharacterAffectionDebugEntry> affectionDebugEntries = new List<CharacterAffectionDebugEntry>();
        [SerializeField] private List<string> unlockedRouteDebugEntries = new List<string>();
        [SerializeField] private bool logAffectionChanges = true;

        public event Action<string, int, int, int> AffectionChanged;
        public event Action<string, string, int> RouteUnlocked;

        private readonly Dictionary<string, int> affectionByCharacter = new Dictionary<string, int>();
        private readonly HashSet<string> unlockedRoutes = new HashSet<string>();
        private readonly Dictionary<string, List<RelationshipThreshold>> thresholdsByCharacter = new Dictionary<string, List<RelationshipThreshold>>();

        private void Awake()
        {
            RebuildThresholdLookup();
            RefreshDebugState();
        }

        public int GetAffection(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return 0;
            }

            if (affectionByCharacter.TryGetValue(characterId, out int value))
            {
                return value;
            }

            if (characterDatabase != null && characterDatabase.TryGetCharacter(characterId, out CharacterProfile profile))
            {
                return profile.StartingAffection;
            }

            return defaultAffection;
        }

        public void SetAffection(string characterId, int newValue)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return;
            }

            if (characterDatabase != null && characterDatabase.TryGetCharacter(characterId, out CharacterProfile profile))
            {
                newValue = Mathf.Clamp(newValue, profile.MinAffection, profile.MaxAffection);
            }

            int previousValue = GetAffection(characterId);
            affectionByCharacter[characterId] = newValue;
            int delta = newValue - previousValue;

            AffectionChanged?.Invoke(characterId, previousValue, newValue, delta);
            TryUnlockRoutes(characterId, newValue);
            RefreshDebugState();

            if (logAffectionChanges)
            {
                Debug.Log($"[RelationshipManager] {characterId} affection changed {previousValue} -> {newValue} (delta {delta}).", this);
            }
        }

        public void AddAffection(string characterId, int delta)
        {
            if (string.IsNullOrWhiteSpace(characterId) || delta == 0)
            {
                return;
            }

            SetAffection(characterId, GetAffection(characterId) + delta);
        }

        public bool IsRouteUnlocked(string routeId)
        {
            return !string.IsNullOrWhiteSpace(routeId) && unlockedRoutes.Contains(routeId);
        }

        public RelationshipSaveData CreateSaveData()
        {
            var saveData = new RelationshipSaveData();

            foreach (KeyValuePair<string, int> kvp in affectionByCharacter)
            {
                saveData.affectionValues.Add(new CharacterAffectionSaveEntry
                {
                    characterId = kvp.Key,
                    affection = kvp.Value
                });
            }

            foreach (string routeId in unlockedRoutes)
            {
                saveData.unlockedRouteIds.Add(routeId);
            }

            return saveData;
        }

        public void LoadFromSaveData(RelationshipSaveData saveData)
        {
            affectionByCharacter.Clear();
            unlockedRoutes.Clear();

            if (saveData != null)
            {
                if (saveData.affectionValues != null)
                {
                    for (int i = 0; i < saveData.affectionValues.Count; i++)
                    {
                        CharacterAffectionSaveEntry entry = saveData.affectionValues[i];
                        if (entry == null || string.IsNullOrWhiteSpace(entry.characterId))
                        {
                            continue;
                        }

                        affectionByCharacter[entry.characterId] = entry.affection;
                    }
                }

                if (saveData.unlockedRouteIds != null)
                {
                    for (int i = 0; i < saveData.unlockedRouteIds.Count; i++)
                    {
                        string routeId = saveData.unlockedRouteIds[i];
                        if (!string.IsNullOrWhiteSpace(routeId))
                        {
                            unlockedRoutes.Add(routeId);
                        }
                    }
                }
            }

            ValidateThresholdUnlocksAgainstCurrentAffection();
            RefreshDebugState();
        }

        [ContextMenu("Rebuild Threshold Lookup")]
        private void RebuildThresholdLookup()
        {
            thresholdsByCharacter.Clear();

            if (characterDatabase != null)
            {
                IReadOnlyList<CharacterProfile> profiles = characterDatabase.Characters;
                for (int i = 0; i < profiles.Count; i++)
                {
                    CharacterProfile profile = profiles[i];
                    if (profile == null || string.IsNullOrWhiteSpace(profile.CharacterId))
                    {
                        continue;
                    }

                    IReadOnlyList<CharacterRouteInfo> routes = profile.Routes;
                    for (int routeIndex = 0; routeIndex < routes.Count; routeIndex++)
                    {
                        CharacterRouteInfo route = routes[routeIndex];
                        if (route == null || string.IsNullOrWhiteSpace(route.routeId))
                        {
                            continue;
                        }

                        AddThreshold(new RelationshipThreshold
                        {
                            characterId = profile.CharacterId,
                            routeId = route.routeId,
                            requiredAffection = route.requiredAffection
                        });
                    }
                }
            }

            for (int i = 0; i < additionalRouteThresholds.Count; i++)
            {
                RelationshipThreshold threshold = additionalRouteThresholds[i];
                if (threshold == null || string.IsNullOrWhiteSpace(threshold.characterId) || string.IsNullOrWhiteSpace(threshold.routeId))
                {
                    continue;
                }

                AddThreshold(threshold);
            }
        }

        private void AddThreshold(RelationshipThreshold threshold)
        {
            if (!thresholdsByCharacter.TryGetValue(threshold.characterId, out List<RelationshipThreshold> list))
            {
                list = new List<RelationshipThreshold>();
                thresholdsByCharacter[threshold.characterId] = list;
            }

            list.Add(threshold);
        }

        private void TryUnlockRoutes(string characterId, int affectionValue)
        {
            if (!thresholdsByCharacter.TryGetValue(characterId, out List<RelationshipThreshold> thresholds))
            {
                return;
            }

            for (int i = 0; i < thresholds.Count; i++)
            {
                RelationshipThreshold threshold = thresholds[i];
                if (threshold == null)
                {
                    continue;
                }

                if (affectionValue < threshold.requiredAffection)
                {
                    continue;
                }

                if (unlockedRoutes.Add(threshold.routeId))
                {
                    RouteUnlocked?.Invoke(threshold.routeId, characterId, affectionValue);
                    if (logAffectionChanges)
                    {
                        Debug.Log($"[RelationshipManager] Route unlocked: {threshold.routeId} (character: {characterId}, affection: {affectionValue}).", this);
                    }
                }
            }
        }

        private void ValidateThresholdUnlocksAgainstCurrentAffection()
        {
            foreach (KeyValuePair<string, int> kvp in affectionByCharacter)
            {
                TryUnlockRoutes(kvp.Key, kvp.Value);
            }
        }

        private void RefreshDebugState()
        {
            affectionDebugEntries.Clear();
            unlockedRouteDebugEntries.Clear();

            foreach (KeyValuePair<string, int> kvp in affectionByCharacter)
            {
                affectionDebugEntries.Add(new CharacterAffectionDebugEntry
                {
                    characterId = kvp.Key,
                    affection = kvp.Value
                });
            }

            foreach (string routeId in unlockedRoutes)
            {
                unlockedRouteDebugEntries.Add(routeId);
            }
        }
    }
}
