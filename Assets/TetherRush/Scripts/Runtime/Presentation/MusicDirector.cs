using UnityEngine;

namespace TetherRush.Presentation
{
    /// <summary>App-owned continuous music. Run objects and cameras never own its lifetime.</summary>
    [DisallowMultipleComponent]
    public sealed class MusicDirector : MonoBehaviour
    {
        private AudioSource source;
        private bool initialized, contextPaused, focusLost, applicationPaused, componentPaused;
        private bool inRun;
        private float master = .55f;
        public AudioSource Source => source;
        public bool IsPaused => contextPaused || focusLost || applicationPaused || componentPaused;
        public float TargetVolume => (inRun ? .50f : .65f) * master;

        public void Initialize(bool enabled, float masterVolume)
        {
            if (initialized) { SetPreferences(enabled, masterVolume); return; }
            initialized = true;
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false; source.loop = true; source.spatialBlend = 0;
            source.priority = 192; source.volume = 0;
            source.clip = AudioPalette.Music;
            SetPreferences(enabled, masterVolume);
            if (source.clip != null) source.Play();
            ApplyPause();
        }
        public void SetPreferences(bool enabled, float masterVolume)
        {
            master = float.IsNaN(masterVolume) || float.IsInfinity(masterVolume) ? .55f : Mathf.Clamp01(masterVolume);
            if (source != null) source.mute = !enabled;
        }
        public void SetContext(bool inRun, bool paused)
        {
            this.inRun = inRun; contextPaused = paused;
            ApplyPause();
        }
        public void PlayUiClick()
        {
            if (source != null && !source.mute && !IsPaused && isActiveAndEnabled && AudioPalette.UiClick != null)
                source.PlayOneShot(AudioPalette.UiClick, 1.2f);
        }
        private void Update()
        {
            if (source == null) return;
            source.volume = Mathf.MoveTowards(source.volume, TargetVolume, Time.unscaledDeltaTime * .35f);
        }
        private void ApplyPause()
        {
            if (source == null) return;
            if (IsPaused) source.Pause(); else source.UnPause();
        }
        private void OnApplicationFocus(bool focused) { focusLost = !focused; ApplyPause(); }
        private void OnApplicationPause(bool paused) { applicationPaused = paused; ApplyPause(); }
        private void OnDisable() { componentPaused = true; ApplyPause(); }
        private void OnEnable() { componentPaused = false; ApplyPause(); }
        private void OnDestroy() { if (source != null) Destroy(source); }
    }
}
