using UnityEngine;

namespace TetherRush.Presentation
{
    /// <summary>Imported, shared assets. Callers must never destroy these clips.</summary>
    public static class AudioPalette
    {
        private static bool loaded;
        public static AudioClip Music { get { Load(); return music; } }
        public static AudioClip Energy { get { Load(); return energy; } }
        public static AudioClip Ready { get { Load(); return ready; } }
        public static AudioClip Pulse { get { Load(); return pulse; } }
        public static AudioClip Relay { get { Load(); return relay; } }
        public static AudioClip Fail { get { Load(); return fail; } }
        public static AudioClip Success { get { Load(); return success; } }
        public static AudioClip UiClick { get { Load(); return uiClick; } }
        private static AudioClip music, energy, ready, pulse, relay, fail, success, uiClick;
        private static readonly AudioClip[] coins = new AudioClip[3];
        public static AudioClip Coin(int index) { Load(); return coins[((index % 3) + 3) % 3]; }
        private static AudioClip Clip(string name) => Resources.Load<AudioClip>("Audio/" + name);
        private static void Load()
        {
            if (loaded) return;
            loaded = true;
            music = Clip("TetherFlow"); energy = Clip("Energy");
            ready = Clip("Ready"); pulse = Clip("Pulse"); relay = Clip("Relay");
            fail = Clip("Fail"); success = Clip("Success"); uiClick = Clip("UiClick");
            for (int i = 0; i < coins.Length; i++) coins[i] = Clip("Coin" + (i + 1));
        }
    }
}
