using TetherRush.Core;
using TetherRush.Course;
using TetherRush.Presentation;
using UnityEngine;

namespace TetherRush
{
    public enum MenuPage { Home, Levels, Endless, Settings, Playing }

    /// <summary>Owns menu navigation, run lifetimes and local records. No scene reload is needed per attempt.</summary>
    public sealed class TetherRushApp : MonoBehaviour
    {
        public PlayerProgress Progress { get; private set; }
        public MainMenuView Menu { get; private set; }
        public TetherRushGame Game { get; private set; }
        public MenuPage Page { get; private set; }
        public MusicDirector Music { get; private set; }
        private Camera menuCamera;
        private PlayerProgress suppliedProgress;
        private bool persist = true;
        private int mapFocusLevel = -1;

        public void ConfigureSession(PlayerProgress progress, bool persistToDisk = false)
        {
            if (Menu != null) throw new System.InvalidOperationException("Configure before Awake.");
            suppliedProgress = progress ?? throw new System.ArgumentNullException(nameof(progress));
            persist = persistToDisk;
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Progress = suppliedProgress ?? PlayerSaveStore.Load();
            var cameraObject = new GameObject("Menu camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(transform, false);
            menuCamera = cameraObject.GetComponent<Camera>();
            menuCamera.clearFlags = CameraClearFlags.SolidColor;
            menuCamera.backgroundColor = VisualTheme.Void; menuCamera.depth = -10;
            Music = gameObject.AddComponent<MusicDirector>();
            Music.Initialize(Progress.AudioEnabled && !Application.isBatchMode, Progress.Volume);
            Menu = gameObject.AddComponent<MainMenuView>();
            Menu.Initialize(index => { Music.PlayUiClick(); PlayLevel(index); },
                () => { Music.PlayUiClick(); StartEndless(); }, SetAudio, SetVolume);
            Menu.SequentialLevels = true;
            Menu.RequestedHome = () => { Music.PlayUiClick(); ShowHome(); };
            Menu.RequestedLevels = () => { Music.PlayUiClick(); ShowLevels(); };
            Menu.RequestedEndless = () => { Music.PlayUiClick(); ShowEndless(); };
            Menu.RequestedSettings = () => { Music.PlayUiClick(); ShowSettings(); };
            ShowHome();
        }

        public void ShowHome()
        {
            CloseRun(); Page = MenuPage.Home;
            int completed = 0;
            for (int i = 0; i < PlayerProgress.LevelCount; i++) if (Progress.IsCompleted(i)) completed++;
            Menu.SetProgress(completed, Progress.HighestUnlockedLevel);
            Menu.ShowHome(Progress.BestEndlessScore, Progress.AudioEnabled, Progress.Volume);
        }
        public void ShowLevels()
        {
            CloseRun(); Page = MenuPage.Levels;
            var titles = new string[LevelCatalog.Count]; var descriptions = new string[LevelCatalog.Count];
            var lengths = new float[LevelCatalog.Count]; var completed = new bool[LevelCatalog.Count]; var bonuses = new int[LevelCatalog.Count];
            for (int i = 0; i < LevelCatalog.Count; i++)
            {
                var level = LevelCatalog.Get(i); titles[i] = level.Title; descriptions[i] = level.Description;
                lengths[i] = level.Length / 7f; completed[i] = Progress.IsCompleted(i); bonuses[i] = Progress.BestBonus(i);
            }
            Menu.ShowLevels(titles, descriptions, lengths, completed, bonuses);
            Menu.FocusLevel(mapFocusLevel >= 0 ? mapFocusLevel : Progress.HighestUnlockedLevel);
        }
        public void ShowEndless()
        {
            CloseRun(); Page = MenuPage.Endless; Menu.ShowEndless(Progress.BestEndlessScore);
        }
        public void ShowSettings()
        {
            CloseRun(); Page = MenuPage.Settings; Menu.ShowSettings(Progress.AudioEnabled, Progress.Volume);
        }
        public void SetAudio(bool enabled)
        {
            Progress.AudioEnabled = enabled;
            if (Game != null) Game.Feedback.AudioEnabled = enabled && !Application.isBatchMode;
            Music.SetPreferences(enabled && !Application.isBatchMode, Progress.Volume);
            Save();
        }
        public void SetVolume(float volume)
        {
            Progress.Volume = volume;
            if (Game != null) Game.Feedback.Volume = Progress.Volume;
            Music.SetPreferences(Progress.AudioEnabled && !Application.isBatchMode, Progress.Volume);
            Save();
        }
        public void PlayLevel(int index)
        {
            LevelCatalog.Get(index); // Validate before ending an existing session.
            if (!Progress.IsUnlocked(index)) return;
            BeginRun(index, false);
        }
        public void StartEndless() { BeginRun(-1, true); }
        private void BeginRun(int index, bool endless)
        {
            CloseRun(); Menu.Hide(); menuCamera.gameObject.SetActive(false); Page = MenuPage.Playing;
            Music.SetContext(true, false);
            var run = new GameObject(endless ? "Sonsuz Akış" : "Bölüm " + (index + 1));
            run.transform.SetParent(transform, false); run.SetActive(false);
            Game = run.AddComponent<TetherRushGame>(); Game.NavigationEnabled = true;
            if (endless) Game.ConfigureEndless();
            else { Game.ConfigureLevel(index); mapFocusLevel = index; }
            Game.BestScore = Progress.BestEndlessScore;
            run.SetActive(true);
            Game.Feedback.AudioEnabled = Progress.AudioEnabled && !Application.isBatchMode;
            Game.Feedback.Volume = Progress.Volume;
            Game.PhaseChanged += RecordResult;
            Game.AudioPreferenceChanged += SetAudio;
            Game.HomeRequested += endless ? (System.Action)ShowHome : ShowLevels;
            Game.NextRequested += NextLevel;
            Game.StartRun();
        }
        private void NextLevel()
        {
            if (Game != null && !Game.IsEndless && Game.Model.Phase == RunPhase.Completed && Game.SelectedLevelIndex + 1 < LevelCatalog.Count)
                PlayLevel(Game.SelectedLevelIndex + 1);
        }
        private void RecordResult(RunPhase phase)
        {
            if (Game == null) return;
            Music.SetContext(true, phase == RunPhase.Paused);
            bool changed = false;
            if (Game.IsEndless && phase == RunPhase.Failed) changed = Progress.RecordEndless(Game.Score);
            else if (!Game.IsEndless && phase == RunPhase.Completed)
            {
                changed = Progress.CompleteLevel(Game.SelectedLevelIndex, Game.Model.Bonus);
                mapFocusLevel = Mathf.Min(Game.SelectedLevelIndex + 1, LevelCatalog.Count - 1);
            }
            Game.BestScore = Progress.BestEndlessScore;
            if (changed) Save();
        }
        private void CloseRun()
        {
            if (Game != null)
            {
                Game.PhaseChanged -= RecordResult; Game.AudioPreferenceChanged -= SetAudio;
                Game.HomeRequested -= ShowHome; Game.HomeRequested -= ShowLevels; Game.NextRequested -= NextLevel;
                Game.gameObject.SetActive(false); Destroy(Game.gameObject); Game = null;
            }
            if (menuCamera != null) menuCamera.gameObject.SetActive(true);
            if (Music != null) Music.SetContext(false, false);
        }
        private void Save() { if (persist) PlayerSaveStore.Save(Progress); }
    }
}
