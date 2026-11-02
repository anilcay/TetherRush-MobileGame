using System;
using TetherRush.Core;
using UnityEngine;

namespace TetherRush
{
    public static class PlayerSaveStore
    {
        private const string SaveKey = "TetherRush.Progress.v1";

        public static PlayerProgress Load()
        {
            return PlayerProgress.FromJson(PlayerPrefs.GetString(SaveKey, string.Empty));
        }

        // Call after results/settings changes or lifecycle callbacks, never each frame.
        public static void Save(PlayerProgress data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            PlayerPrefs.SetString(SaveKey, data.ToJson());
            PlayerPrefs.Save();
        }

        public static int CalculateScore(float distance, int bonus)
        {
            double safeDistance = float.IsNaN(distance) || distance < 0f ? 0d : distance;
            double total = Math.Floor(safeDistance) + Math.Max(0, bonus) * 100d;
            return total >= int.MaxValue ? int.MaxValue : (int)total;
        }
    }
}
