using System;
using System.Collections.Generic;
using UnityEngine;

namespace DatingSim.Dialogue
{
    public static class DialogueJsonLoader
    {
        public static bool TryLoad(TextAsset jsonAsset, out DialogueSequence sequence, out string error)
        {
            sequence = null;
            error = string.Empty;

            if (jsonAsset == null)
            {
                error = "Dialogue JSON asset is null.";
                return false;
            }

            try
            {
                sequence = JsonUtility.FromJson<DialogueSequence>(jsonAsset.text);
            }
            catch (Exception ex)
            {
                error = $"Failed to parse dialogue JSON: {ex.Message}";
                return false;
            }

            if (sequence == null)
            {
                error = "Dialogue JSON parsed to null sequence.";
                return false;
            }

            if (sequence.lines == null || sequence.lines.Count == 0)
            {
                error = "Dialogue sequence has no lines.";
                return false;
            }

            var lineIds = new HashSet<string>();
            for (int i = 0; i < sequence.lines.Count; i++)
            {
                DialogueLine line = sequence.lines[i];
                if (line == null)
                {
                    error = $"Dialogue line at index {i} is null.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(line.lineId))
                {
                    error = $"Dialogue line at index {i} is missing lineId.";
                    return false;
                }

                if (!lineIds.Add(line.lineId))
                {
                    error = $"Duplicate lineId found: {line.lineId}";
                    return false;
                }

                line.choices ??= new List<DialogueChoice>();
            }

            if (string.IsNullOrWhiteSpace(sequence.startLineId))
            {
                sequence.startLineId = sequence.lines[0].lineId;
            }

            if (!lineIds.Contains(sequence.startLineId))
            {
                error = $"startLineId '{sequence.startLineId}' was not found in lines.";
                return false;
            }

            return true;
        }
    }
}
