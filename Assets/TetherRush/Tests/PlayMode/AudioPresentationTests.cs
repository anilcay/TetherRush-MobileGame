using System.Collections;
using NUnit.Framework;
using TetherRush.Core;
using TetherRush.Presentation;
using UnityEngine;
using UnityEngine.TestTools;

namespace TetherRush.Tests
{
    public sealed class AudioPresentationTests
    {
        private GameObject root;
        [TearDown] public void Cleanup() { if (root != null) Object.DestroyImmediate(root); }

        [Test]
        public void ImportedPaletteHasStereoLoopAndShortMonoCues()
        {
            Assert.That(AudioPalette.Music, Is.Not.Null);
            Assert.That(AudioPalette.Music.channels, Is.EqualTo(2));
            Assert.That(AudioPalette.Music.length, Is.InRange(60f, 90f));
            var clips = new[] { AudioPalette.Energy, AudioPalette.Coin(0), AudioPalette.Coin(1), AudioPalette.Coin(2),
                AudioPalette.Ready, AudioPalette.Pulse, AudioPalette.Relay, AudioPalette.Fail, AudioPalette.Success, AudioPalette.UiClick };
            foreach (var clip in clips)
            {
                Assert.That(clip, Is.Not.Null);
                Assert.That(clip.channels, Is.EqualTo(1), clip.name);
                Assert.That(clip.length, Is.InRange(.025f, 2f), clip.name);
                Assert.That(clip.frequency, Is.GreaterThanOrEqualTo(22050), clip.name);
                Assert.That(clip.loadType, Is.EqualTo(AudioClipLoadType.DecompressOnLoad), clip.name);
                var samples = new float[clip.samples];
                Assert.That(clip.GetData(samples, 0), Is.True, clip.name);
                float peak = 0;
                foreach (float sample in samples) peak = Mathf.Max(peak, Mathf.Abs(sample));
                Assert.That(peak, Is.InRange(.2f, .52f), clip.name + " must preserve mastered headroom");
            }
            Assert.That(AudioPalette.Coin(3), Is.SameAs(AudioPalette.Coin(0)));
            Assert.That(AudioPalette.Coin(0), Is.Not.SameAs(AudioPalette.Coin(1)));
        }

        [Test]
        public void MusicInitializesOnceAndUsesFiniteMasterPreferences()
        {
            root = new GameObject("Music preferences fixture");
            var music = root.AddComponent<MusicDirector>(); music.Initialize(false, .55f);
            var source = music.Source; var clip = source.clip;
            music.Initialize(false, .5f);
            Assert.That(root.GetComponents<AudioSource>().Length, Is.EqualTo(1));
            Assert.That(source, Is.SameAs(music.Source)); Assert.That(source.clip, Is.SameAs(clip));
            Assert.That(source.loop, Is.True); Assert.That(source.spatialBlend, Is.Zero);
            Assert.That(source.playOnAwake, Is.False); Assert.That(source.mute, Is.True);
            music.SetContext(false, false); Assert.That(music.TargetVolume, Is.EqualTo(.325f).Within(.0001f));
            music.SetContext(true, false); Assert.That(music.TargetVolume, Is.EqualTo(.25f).Within(.0001f));
            music.SetPreferences(false, float.NaN); Assert.That(music.TargetVolume, Is.EqualTo(.50f * .55f).Within(.0001f));
            music.SetPreferences(false, 8); Assert.That(music.TargetVolume, Is.EqualTo(.50f).Within(.0001f));
            music.SetPreferences(false, -8); Assert.That(music.TargetVolume, Is.Zero);
            music.SetPreferences(true, .55f); Assert.That(source.mute, Is.False);
            music.SetPreferences(false, .55f);
        }

        [Test]
        public void FocusAndApplicationPauseCannotOverrideAnExplicitGamePause()
        {
            root = new GameObject("Music pause fixture");
            var music = root.AddComponent<MusicDirector>(); music.Initialize(false, .55f);
            var clip = music.Source.clip;
            music.SetContext(true, true);
            root.SendMessage("OnApplicationFocus", false); root.SendMessage("OnApplicationFocus", true);
            Assert.That(music.IsPaused, Is.True);
            music.SetContext(true, false); Assert.That(music.IsPaused, Is.False);
            root.SendMessage("OnApplicationPause", true);
            root.SendMessage("OnApplicationFocus", true); Assert.That(music.IsPaused, Is.True);
            root.SendMessage("OnApplicationPause", false); Assert.That(music.IsPaused, Is.False);
            root.SetActive(false); Assert.That(music.IsPaused, Is.True);
            root.SetActive(true); Assert.That(music.IsPaused, Is.False);
            Assert.That(music.Source.clip, Is.SameAs(clip));
            Assert.That(AudioListener.pause, Is.False, "Local music pause must not alter the global listener");
        }

        [UnityTest]
        public IEnumerator MenuRunRetryNavigationKeepsOneAppMusicSourceAndSharedClip()
        {
            root = new GameObject("Music app fixture"); root.SetActive(false);
            var app = root.AddComponent<TetherRushApp>(); app.ConfigureSession(new PlayerProgress()); root.SetActive(true);
            var music = root.GetComponent<MusicDirector>(); Assert.That(music, Is.Not.Null);
            music.SetPreferences(false, .55f); var source = music.Source; var clip = source.clip;
            app.PlayLevel(0); app.Game.AutoSimulate = false; app.Game.Feedback.AudioEnabled = false;
            app.Game.PauseRun(); Assert.That(music.IsPaused, Is.True);
            app.Game.RestartRun(); Assert.That(music.IsPaused, Is.False);
            app.ShowLevels(); app.ShowHome(); app.StartEndless();
            app.Game.AutoSimulate = false; app.Game.Feedback.AudioEnabled = false;
            yield return null;
            Assert.That(root.GetComponents<MusicDirector>().Length, Is.EqualTo(1));
            Assert.That(root.GetComponents<AudioSource>().Length, Is.EqualTo(1));
            Assert.That(music.Source, Is.SameAs(source)); Assert.That(source.clip, Is.SameAs(clip));
            app.ShowHome(); yield return null;
            Assert.That(root.GetComponentsInChildren<AudioSource>(true).Length, Is.EqualTo(1));
            Assert.That(clip, Is.Not.Null, "Run destruction must preserve shared music assets");
        }

        [UnityTest]
        public IEnumerator MusicFadesWithUnscaledTimeAndUiDoesNotReplaceTheLoop()
        {
            root = new GameObject("Music fade fixture");
            var music = root.AddComponent<MusicDirector>(); music.Initialize(false, .55f);
            float originalTimeScale = Time.timeScale;
            try
            {
                Time.timeScale = 0;
                yield return null; yield return null;
                Assert.That(music.Source.volume, Is.GreaterThan(0));
                Assert.That(music.Source.volume, Is.LessThanOrEqualTo(music.TargetVolume));
                var clip = music.Source.clip;
                music.PlayUiClick(); Assert.That(music.Source.clip, Is.SameAs(clip));
                music.SetPreferences(false, 0); yield return null;
                Assert.That(music.TargetVolume, Is.Zero);
            }
            finally { Time.timeScale = originalTimeScale; }
        }

        [UnityTest]
        public IEnumerator CoinPairsCoalesceWithoutChangingRandomAndRetryResetsSequence()
        {
            root = new GameObject("Coin sound fixture"); root.SetActive(false);
            var game = root.AddComponent<TetherRushGame>(); root.SetActive(true);
            game.AutoSimulate = false; var feedback = game.Feedback; feedback.AudioEnabled = false;
            var randomState = Random.state;
            feedback.BonusCollected(Vector3.zero); feedback.BonusCollected(Vector3.zero);
            Assert.That(feedback.CoinSequence, Is.EqualTo(1));
            feedback.Tick(.01f, false, false, RunPhase.Running); yield return null;
            feedback.BonusCollected(Vector3.zero); Assert.That(feedback.CoinSequence, Is.EqualTo(1), "Minimum coin interval");
            feedback.Tick(.04f, false, false, RunPhase.Running); yield return null;
            feedback.BonusCollected(Vector3.zero); Assert.That(feedback.CoinSequence, Is.EqualTo(2));
            Assert.That(Random.state, Is.EqualTo(randomState), "Audio must not affect seeded course selection");
            feedback.Volume = float.PositiveInfinity; Assert.That(feedback.Source.volume, Is.EqualTo(.55f));
            feedback.ResetPresentation(); Assert.That(feedback.CoinSequence, Is.Zero);
            Assert.That(feedback.Source.isPlaying, Is.False);
            var energy = AudioPalette.Energy;
            Object.Destroy(root); root = null; yield return null;
            Assert.That(energy, Is.Not.Null, "Feedback does not own imported clip assets");
        }
    }
}
