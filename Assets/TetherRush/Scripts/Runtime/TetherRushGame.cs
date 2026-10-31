using System.Collections.Generic;
using TetherRush.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TetherRush.Art;
using TetherRush.Course;
using TetherRush.Presentation;

namespace TetherRush
{
    /// <summary>Authored M1 prototype. Simulation is shared by live input and PlayMode tests.</summary>
    public sealed class TetherRushGame : MonoBehaviour
    {
        public const float FinishDistance = PrototypeLayout.FinishDistance;
        private static bool preferredAudio = true;
        [SerializeField] private RunTuning tuning = new RunTuning();
        [SerializeField] private bool advancedCourse;
        public bool IsAdvancedCourse => advancedCourse;
        public float CourseFinishDistance => advancedCourse ? TimedCourseLayout.FinishDistance : FinishDistance;
        public int TimedHazardCount => timedHazards.Length;
        public GameFeedback Feedback { get; private set; }
        public void ConfigureCourse(bool advanced)
        {
            if (Model != null) throw new System.InvalidOperationException("Configure the course before Awake.");
            advancedCourse = advanced;
        }
        public RunModel Model { get; private set; }
        public bool AutoSimulate { get; set; } = true;
        public Transform LeftCore { get; private set; }
        public Transform RightCore { get; private set; }
        public int ActivatedTargetCount { get; private set; }
        public int RemainingEnergyCount { get { int n = 0; foreach (var c in pickups) if (!c.Bonus && c.View.activeSelf) n++; return n; } }

        private sealed class Hazard { public Bounds Bounds; public GameObject View, OpenFrame; public bool Marked; public bool Open; public bool Gate; public string Reason; }
        private sealed class Pickup { public Vector3 Position; public GameObject View; public bool Bonus; public int Id; }
        private readonly List<Hazard> hazards = new List<Hazard>();
        private readonly List<Pickup> pickups = new List<Pickup>();
        private readonly List<Material> materials = new List<Material>();
        private readonly List<RaycastResult> uiHits = new List<RaycastResult>();
        private readonly float[] sweepTimes = new float[6];
        private Material trackMat, redMat, goldMat, tealMat, peachMat, purpleMat, lineMat, darkMat, accentMat, dangerAccentMat, pulseAccentMat, tetherMat;
        private VisualLibrary visuals;
        private TimedHazard[] timedHazards = new TimedHazard[0];
        private Transform[] timedViews = new Transform[0];
        private Transform world, tether;
        private Camera gameCamera;
        private LineRenderer tetherLine;
        private GameHud hud;
        private FacilityStage stage;
        private int activeTouchId = -1;
        private bool mouseHeld, keyboardHeld;
        private RunPhase lastPhase = (RunPhase)(-1);

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Model = new RunModel(tuning);
            BuildWorld();
            Feedback = gameObject.AddComponent<GameFeedback>();
            Feedback.Initialize(LeftCore, RightCore, tether, gameCamera, tetherMat, tuning.PulseRadius);
            Feedback.AudioEnabled = !Application.isBatchMode && preferredAudio;
            BuildHud();
            RefreshVisuals(0);
        }

        public void StartRun()
        {
            ClearPointer();
            Model.StartRun();
            ActivatedTargetCount = 0;
            foreach (var h in hazards) { h.Open = false; h.View.SetActive(true); if (h.OpenFrame != null) h.OpenFrame.SetActive(false); }
            foreach (var c in pickups) c.View.SetActive(true);
            lastPhase = (RunPhase)(-1);
            RefreshVisuals(0);
            // Reset wakes after the simulation roots have returned to their new start poses.
            Feedback.ResetPresentation();
        }
        public void RestartRun() { StartRun(); }
        public void PauseRun() { Model.Pause(); ClearPointer(); RefreshVisuals(0); }
        public void ResumeRun() { Model.Resume(); ClearPointer(); RefreshVisuals(0); }
        private void ClearPointer() { activeTouchId = -1; mouseHeld = keyboardHeld = false; Model.CancelInput(); }
        private void OnApplicationFocus(bool focused) { if (!focused && Model != null) PauseRun(); }
        private void OnApplicationPause(bool paused) { if (paused && Model != null) PauseRun(); }

        private void Update()
        {
            if (!AutoSimulate) return;
            PollInput(out bool press, out bool release);
            SimulateStep(Time.deltaTime, press, release);
        }

        public void SimulateStep(float dt, bool press = false, bool release = false)
        {
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt < 0) throw new System.ArgumentOutOfRangeException(nameof(dt));
            if (press) Model.Press();
            // Small swept intervals preserve the separation clamp corner even during a large manual step.
            float remaining = dt;
            bool releasePending = release;
            while (remaining > 0 && Model.Phase == RunPhase.Running)
            {
                float step = Mathf.Min(remaining, 1f / 120f);
                float oldZ = Model.Distance, oldHalf = Model.HalfSeparation;
                Model.Tick(step);
                ResolveContacts(oldZ, oldHalf);
                // Resolve the input in the first simulation step, not after an entire slow frame.
                // Contacts in that step still precede release, and fatal contact has priority.
                if (releasePending)
                {
                    if (Model.Release()) ActivatePulse();
                    releasePending = false;
                }
                remaining -= step;
                if (Model.Phase == RunPhase.Running && Model.Distance >= CourseFinishDistance) Model.Complete();
            }
            if (releasePending && Model.Release()) ActivatePulse();
            RefreshVisuals(dt);
        }

        private void ResolveContacts(float oldZ, float oldHalf)
        {
            Vector2 a = new Vector2(oldHalf, oldZ), b = new Vector2(Model.HalfSeparation, Model.Distance);
            foreach (var h in hazards)
            {
                if (h.Open) continue;
                if (SweptCircleBox(a, b, h.Bounds, tuning.SphereRadius) ||
                    SweptCircleBox(new Vector2(-a.x, a.y), new Vector2(-b.x, b.y), h.Bounds, tuning.SphereRadius))
                { Model.Fail(h.Reason); return; }
            }
            float startTime = oldZ / tuning.ForwardSpeed, endTime = Model.Distance / tuning.ForwardSpeed;
            foreach (var h in timedHazards)
                if (h.HitsSweptSphere(a, b, tuning.SphereRadius, startTime, endTime) ||
                    h.HitsSweptSphere(new Vector2(-a.x, a.y), new Vector2(-b.x, b.y), tuning.SphereRadius, startTime, endTime))
                { Model.Fail(h.Hint); return; }
            foreach (var c in pickups)
            {
                if (!c.View.activeSelf) continue;
                bool hit;
                if (c.Bonus)
                {
                    Vector2 p = new Vector2(c.Position.x, c.Position.z);
                    float radius = tuning.SphereRadius + .19f;
                    hit = SegmentDistanceSquared(a, b, p) <= radius * radius ||
                        SegmentDistanceSquared(new Vector2(-a.x, a.y), new Vector2(-b.x, b.y), p) <= radius * radius;
                }
                else
                {
                    float dz = b.y - a.y;
                    float lo = dz > 0 ? Mathf.Clamp01((c.Position.z - .18f - a.y) / dz) : 0;
                    float hi = dz > 0 ? Mathf.Clamp01((c.Position.z + .18f - a.y) / dz) : 1;
                    float half = Mathf.Max(Mathf.Lerp(a.x, b.x, lo), Mathf.Lerp(a.x, b.x, hi));
                    hit = c.Position.z + .18f >= a.y && c.Position.z - .18f <= b.y && Mathf.Abs(c.Position.x) <= half + .08f;
                }
                if (hit && (c.Bonus ? Model.CollectBonus(c.Id) : Model.CollectEnergy(c.Id)))
                {
                    if (c.Bonus) Feedback.BonusCollected(c.Position); else Feedback.EnergyCollected(c.Position);
                    c.View.SetActive(false);
                }
            }
        }

        private static float SegmentDistanceSquared(Vector2 a, Vector2 b, Vector2 p)
        { Vector2 d = b - a; float t = d.sqrMagnitude > 0 ? Mathf.Clamp01(Vector2.Dot(p - a, d) / d.sqrMagnitude) : 0; return (p - a - d * t).sqrMagnitude; }

        // Exact swept circle / rectangle distance: the distance function is quadratic between slab crossings.
        private bool SweptCircleBox(Vector2 a, Vector2 b, Bounds box, float radius)
        {
            Vector2 min = new Vector2(box.min.x, box.min.z), max = new Vector2(box.max.x, box.max.z), d = b - a;
            int n = 2; sweepTimes[0] = 0; sweepTimes[1] = 1;
            for (int axis = 0; axis < 2; axis++) if (Mathf.Abs(d[axis]) > .000001f)
            {
                float t0 = (min[axis] - a[axis]) / d[axis], t1 = (max[axis] - a[axis]) / d[axis];
                if (t0 > 0 && t0 < 1) sweepTimes[n++] = t0;
                if (t1 > 0 && t1 < 1) sweepTimes[n++] = t1;
            }
            System.Array.Sort(sweepTimes, 0, n);
            for (int i = 0; i < n - 1; i++)
            {
                float lo = sweepTimes[i], hi = sweepTimes[i + 1], mid = (lo + hi) * .5f;
                Vector2 q = Vector2.zero, v = Vector2.zero;
                for (int axis = 0; axis < 2; axis++)
                {
                    float p = a[axis] + d[axis] * mid;
                    if (p < min[axis]) { q[axis] = a[axis] - min[axis]; v[axis] = d[axis]; }
                    else if (p > max[axis]) { q[axis] = a[axis] - max[axis]; v[axis] = d[axis]; }
                }
                float t = v.sqrMagnitude > 0 ? Mathf.Clamp(-Vector2.Dot(q, v) / v.sqrMagnitude, lo, hi) : lo;
                if ((q + v * t).sqrMagnitude <= radius * radius) return true;
            }
            return false;
        }

        private void ActivatePulse()
        {
            Vector3 center = new Vector3(0, .55f, Model.Distance);
            foreach (var h in hazards)
            {
                if (!h.Marked || h.Open) continue;
                Vector3 near = h.Bounds.ClosestPoint(center); near.y = center.y;
                if ((near - center).sqrMagnitude <= tuning.PulseRadius * tuning.PulseRadius + .00001f)
                {
                    Feedback.TargetOpened(h.View, h.Gate);
                    h.Open = true; h.View.SetActive(false);
                    if (h.OpenFrame != null) h.OpenFrame.SetActive(true);
                    ActivatedTargetCount++;
                }
            }
            Feedback.Pulse(center);
        }

        private void PollInput(out bool press, out bool release)
        {
            press = release = false;
            if (Model.Phase != RunPhase.Running) return;
            var screen = Touchscreen.current;
            if (screen != null)
            {
                if (activeTouchId >= 0)
                {
                    bool found = false;
                    foreach (var touch in screen.touches)
                    {
                        if (touch.touchId.ReadValue() != activeTouchId) continue;
                        found = true;
                        if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled) { Model.CancelInput(); activeTouchId = -1; }
                        else if (touch.press.wasReleasedThisFrame) { release = true; activeTouchId = -1; }
                        else if (!touch.press.isPressed) { Model.CancelInput(); activeTouchId = -1; }
                        break;
                    }
                    if (!found) { activeTouchId = -1; Model.CancelInput(); }
                    return;
                }
                foreach (var touch in screen.touches)
                    if (touch.press.wasPressedThisFrame && !PointerOverUi(touch.position.ReadValue()))
                    { activeTouchId = touch.touchId.ReadValue(); press = true; return; }
            }
            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame && !PointerOverUi(mouse.position.ReadValue())) { mouseHeld = true; press = true; }
                if (mouseHeld && mouse.leftButton.wasReleasedThisFrame) { mouseHeld = false; release = true; }
            }
#if UNITY_EDITOR
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.spaceKey.wasPressedThisFrame) { keyboardHeld = true; press = true; }
                if (keyboardHeld && keyboard.spaceKey.wasReleasedThisFrame) { keyboardHeld = false; release = true; }
            }
#endif
        }
        private bool PointerOverUi(Vector2 position)
        {
            if (EventSystem.current == null) return false;
            uiHits.Clear();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = position }, uiHits);
            return uiHits.Count > 0;
        }

        private Material MakeMaterial(string label, Color color)
        {
            Shader shader = Shader.Find("Tether Rush/NeonSurface") ?? Shader.Find("Universal Render Pipeline/Unlit");
            var material = new Material(shader) { name = label };
            material.SetColor("_BaseColor", color);
            if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", color * .12f);
            materials.Add(material); return material;
        }
        private GameObject ModelVisual(VisualKind kind, string label, Vector3 pos, Vector3 size, Material material)
        {
            Material edge = kind == VisualKind.TrackTile || kind == VisualKind.RelayPylon ? lineMat :
                kind == VisualKind.Barrier || kind == VisualKind.SplitPylon || kind == VisualKind.ChevronBarrier ||
                kind == VisualKind.MovingWall || kind == VisualKind.RotatingArm ? dangerAccentMat :
                kind == VisualKind.EnergyGate || kind == VisualKind.BreakableBlock || kind == VisualKind.PulseBadge ? pulseAccentMat : null;
            return visuals.Create(kind, label, world, pos, size, material, edge);
        }
        private void AddHazard(string label, Vector3 pos, Vector3 size, bool marked, string reason, bool gate = false, VisualKind variant = VisualKind.Barrier)
        {
            var obj = ModelVisual(marked ? (gate ? VisualKind.EnergyGate : VisualKind.BreakableBlock) : variant,
                label, pos, size, marked ? goldMat : redMat);
            var hazard = new Hazard { Bounds = new Bounds(pos, size), View = obj, Marked = marked, Gate = gate, Reason = reason };
            hazards.Add(hazard);
            if (gate)
            {
                foreach (var renderer in obj.GetComponentsInChildren<MeshRenderer>())
                {
                    if (renderer.name.IndexOf("Frame", System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                    var filter = renderer.GetComponent<MeshFilter>();
                    if (filter == null) continue;
                    var frame = new GameObject("Opened gate frame", typeof(MeshFilter), typeof(MeshRenderer));
                    frame.transform.SetParent(world, false);
                    frame.transform.SetPositionAndRotation(renderer.transform.position, renderer.transform.rotation);
                    frame.transform.localScale = renderer.transform.lossyScale;
                    frame.GetComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
                    frame.GetComponent<MeshRenderer>().sharedMaterials = renderer.sharedMaterials;
                    frame.SetActive(false); hazard.OpenFrame = frame; break;
                }
            }
            if (marked)
            {
                var sign = ModelVisual(VisualKind.PulseBadge, label + " pulse mark", pos + new Vector3(0, 1.1f, 0), new Vector3(.65f, .65f, .12f), goldMat);
                sign.transform.SetParent(obj.transform, true);
            }
        }
        private void BuildWorld()
        {
            world = new GameObject(advancedCourse ? "Timing course" : "Foundation course").transform; world.SetParent(transform, false);
            trackMat = MakeMaterial("Track", VisualTheme.Track);
            redMat = MakeMaterial("Fatal obstacle shell", VisualTheme.DangerBody);
            goldMat = MakeMaterial("Pulse target / bonus", VisualTheme.PulseTarget);
            tealMat = MakeMaterial("Energy", VisualTheme.Energy);
            peachMat = MakeMaterial("Core A", VisualTheme.CoreA);
            purpleMat = MakeMaterial("Core B", VisualTheme.CoreB);
            lineMat = MakeMaterial("Track markings", VisualTheme.TrackAccent);
            darkMat = MakeMaterial("Recessed panels", VisualTheme.Inlay);
            accentMat = MakeMaterial("Edge accents", VisualTheme.Silver);
            dangerAccentMat = MakeMaterial("Fatal warning lights", VisualTheme.Danger);
            pulseAccentMat = MakeMaterial("Pulse target lights", VisualTheme.PulseAccent);
            visuals = new VisualLibrary(darkMat, accentMat);
            for (int z = -10; z <= CourseFinishDistance + 10; z += 10)
                ModelVisual(VisualKind.TrackTile, "Track tile", new Vector3(0, -.22f, z + 5), new Vector3(7.2f, .4f, 10), trackMat);
            AddHazard("Narrow gate left", new Vector3(-2.425f, .65f, 20), new Vector3(2.35f, 1.3f, 1), false, "Dar geçit: bırak ve daral.", variant: VisualKind.SplitPylon);
            AddHazard("Narrow gate right", new Vector3(2.425f, .65f, 20), new Vector3(2.35f, 1.3f, 1), false, "Dar geçit: bırak ve daral.", variant: VisualKind.SplitPylon);
            if (advancedCourse)
            {
                timedHazards = TimedCourseLayout.Create(); timedViews = new Transform[timedHazards.Length];
                for (int i = 0; i < timedHazards.Length; i++)
                {
                    var h = timedHazards[i];
                    timedViews[i] = ModelVisual(h.Kind == TimedHazardKind.RotatingArm ? VisualKind.RotatingArm : VisualKind.MovingWall,
                        "Timed hazard " + i, new Vector3(h.Center.x, .65f, h.Center.y),
                        new Vector3(h.HalfExtents.x * 2, 1.3f, h.HalfExtents.y * 2), redMat).transform;
                }
                foreach (var point in new[] { new Vector2(2.2f, 52), new Vector2(.65f, 82), new Vector2(2.2f, 110) })
                { AddPickup(new Vector3(-point.x, .55f, point.y), true, pickups.Count); AddPickup(new Vector3(point.x, .55f, point.y), true, pickups.Count); }
            }
            else
            {
                AddHazard("Center barrier", new Vector3(0, .65f, 42), new Vector3(2.5f, 1.3f, 1.2f), false, "Orta bariyer: basılı tut ve açıl.", variant: VisualKind.ChevronBarrier);
                AddHazard("Energy gate", new Vector3(0, .65f, 100), new Vector3(7.2f, 1.3f, .7f), true, "Kapalı enerji kapısı! Tam enerjiyle menzilde bırak.", true);
                AddHazard("Marked breakable", new Vector3(0, .65f, 153), new Vector3(7.2f, 1.3f, .7f), true, "İşaretli hedef kapalı! Tam enerjiyle menzilde bırak.");
                for (int i = 0; i < PrototypeLayout.EnergyDistances.Length; i++)
                    AddPickup(new Vector3(PrototypeLayout.EnergyOffsets[i], .55f, PrototypeLayout.EnergyDistances[i]), false, i);
                foreach (float z in new[] { 49f, 85f, 141f, 167f })
                { AddPickup(new Vector3(-2.2f, .55f, z), true, pickups.Count); AddPickup(new Vector3(2.2f, .55f, z), true, pickups.Count); }
            }
            ModelVisual(VisualKind.FinishMarker, "Finish arch", new Vector3(0, 1.6f, CourseFinishDistance), new Vector3(7.2f, 3.2f, .45f), tealMat);
            LeftCore = ModelVisual(VisualKind.Core, "Left core", Vector3.zero, Vector3.one * tuning.SphereRadius * 2, peachMat).transform;
            RightCore = ModelVisual(VisualKind.Core, "Right core", Vector3.zero, Vector3.one * tuning.SphereRadius * 2, purpleMat).transform;
            tether = new GameObject("Energy tether").transform; tether.SetParent(world, false);
            tetherMat = new Material(Shader.Find("Tether Rush/NeonEffect") ?? Shader.Find("Sprites/Default")) { name = "Tether beam" };
            tetherMat.SetColor("_BaseColor", VisualTheme.Tether); materials.Add(tetherMat);
            tetherLine = tether.gameObject.AddComponent<LineRenderer>(); tetherLine.sharedMaterial = tetherMat;
            tetherLine.positionCount = 2; tetherLine.useWorldSpace = true; tetherLine.widthMultiplier = .065f; tetherLine.numCapVertices = 4;
            var cameraObj = new GameObject("Main Camera"); cameraObj.transform.SetParent(world); cameraObj.tag = "MainCamera";
            gameCamera = cameraObj.AddComponent<Camera>(); gameCamera.clearFlags = CameraClearFlags.SolidColor; gameCamera.backgroundColor = VisualTheme.Void;
            gameCamera.fieldOfView = 62; gameCamera.nearClipPlane = .1f; gameCamera.farClipPlane = 90;
            cameraObj.AddComponent<AudioListener>();
            for (int z = 0; z <= CourseFinishDistance + 12; z += 14)
                for (int side = -1; side <= 1; side += 2)
                    ModelVisual(VisualKind.RelayPylon, "Facility relay", new Vector3(side * 4.8f, .65f, z), new Vector3(.45f, 1.7f, .65f), trackMat);
            stage = gameObject.AddComponent<FacilityStage>();
            stage.Initialize(world, LeftCore, RightCore, gameCamera);

        }
        private void AddPickup(Vector3 pos, bool bonus, int id)
        {
            var obj = ModelVisual(bonus ? VisualKind.BonusCrystal : VisualKind.EnergyPickup, bonus ? "Bonus crystal" : "Tether energy",
                pos, Vector3.one * (bonus ? .38f : .32f), bonus ? goldMat : tealMat);
            pickups.Add(new Pickup { Position = pos, View = obj, Bonus = bonus, Id = id });
        }
        private void BuildHud()
        {
            hud = gameObject.AddComponent<GameHud>();
            hud.Initialize(advancedCourse,
                () => { if (Model.Phase == RunPhase.Paused) ResumeRun(); else StartRun(); },
                PauseRun,
                () => { Feedback.AudioEnabled = !Feedback.AudioEnabled; preferredAudio = Feedback.AudioEnabled; RefreshVisuals(0); },
                () => SceneManager.LoadScene(advancedCourse ? "Prototype" : "Challenge"));
        }

        private int Section()
        {
            float z = Model.Distance;
            if (advancedCourse) return z < 24 ? 0 : z < 54 ? 10 : z < 79 ? 11 : z < 110 ? 12 : 13;
            if (z < 24) return 0;
            if (z < 47) return 1;
            if (z < 83) return 2;
            if (z < 101) return 3;
            if (z < 139) return 4;
            if (z < 154) return 5;
            return 6;
        }
        private string SectionHint(int section)
        {
            switch (section)
            {
                case 0: return "DAR GEÇİT • Bırak, dar kal";
                case 1: return "ORTA BARİYER • Şimdi basılı tut";
                case 2: return "ENERJİ • Açık bağla 5 parçayı topla";
                case 3:
                    if (ActivatedTargetCount >= 1) return "KAPI AÇILDI • Geçiş hazır";
                    if (Model.Energy < tuning.EnergyCapacity) return "ENERJİ EKSİK • Kapı Pulse ister";
                    return Model.Distance >= 94.65f && Model.Distance <= 99 ? "MENZİLDE • Şimdi BIRAK!" : "PULSE KAPISI • Tam enerjiyle yaklaş";
                case 4: return "YENİ ENERJİ • Tekrar basılı tut";
                case 5:
                    if (ActivatedTargetCount >= 2) return "HEDEF AÇILDI • Bitişe ilerle";
                    if (Model.Energy < tuning.EnergyCapacity) return "ENERJİ EKSİK • Hedef Pulse ister";
                    return Model.Distance >= 147.65f && Model.Distance <= 152 ? "MENZİLDE • Şimdi BIRAK!" : "İŞARETLİ HEDEF • Tam enerjiyle yaklaş";
                case 10: return "HAREKETLİ DUVAR • Basılı tut, geniş geç";
                case 11: return "DUVAR YANA KAYIYOR • Bırak, dar geç";
                case 12: return "DÖNEN KOL • Basılı tut, uçlardan geç";
                case 13: return "BİTİŞ • İki küreyi de koru";
                default: return "BİTİŞ • Altın bonuslar kürelerle toplanır";
            }
        }
        private void RefreshVisuals(float dt)
        {
            float z = Model.Distance, half = Model.HalfSeparation;
            LeftCore.position = new Vector3(-half, .55f, z); RightCore.position = new Vector3(half, .55f, z);
            tetherLine.SetPosition(0, LeftCore.position); tetherLine.SetPosition(1, RightCore.position);
            float runTime = z / tuning.ForwardSpeed;
            for (int i = 0; i < timedHazards.Length; i++)
            {
                var position = timedHazards[i].PositionAt(runTime);
                timedViews[i].SetPositionAndRotation(new Vector3(position.x, .65f, position.y),
                    Quaternion.Euler(0, -timedHazards[i].AngleAt(runTime) * Mathf.Rad2Deg, 0));
            }
            gameCamera.transform.position = new Vector3(0, 10, z - 11);
            gameCamera.transform.rotation = Quaternion.LookRotation(new Vector3(0, .1f, z + 8) - gameCamera.transform.position);
            stage.Tick();
            hud.Present(new HudState
            {
                Phase = Model.Phase, AdvancedCourse = advancedCourse, AudioEnabled = Feedback.AudioEnabled,
                CanSwitchCourse = Application.CanStreamedLevelBeLoaded(advancedCourse ? "Prototype" : "Challenge"),
                Distance = z, FinishDistance = CourseFinishDistance, Energy = Model.Energy,
                EnergyCapacity = tuning.EnergyCapacity, Bonus = Model.Bonus, ActivatedTargets = ActivatedTargetCount,
                Hint = SectionHint(Section()), FailureReason = Model.FailureReason
            }, Model.Phase == RunPhase.Paused ? 0 : dt);
            Feedback.Tick(Model.Phase == RunPhase.Paused ? 0 : dt, Model.IsHeld, Model.Energy == tuning.EnergyCapacity, Model.Phase);
            if (lastPhase == Model.Phase) return;
            lastPhase = Model.Phase;
            if (Model.Phase == RunPhase.Failed) Feedback.Failed();
            else if (Model.Phase == RunPhase.Completed) Feedback.Completed();
        }
        private void OnDestroy()
        {
            foreach (var material in materials) if (material != null) Destroy(material);
        }
    }
}
