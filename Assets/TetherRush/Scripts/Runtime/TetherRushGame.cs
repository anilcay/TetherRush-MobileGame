using System.Collections.Generic;
using TetherRush.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
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
        private Material trackMat, redMat, goldMat, tealMat, purpleMat, lineMat, darkMat, accentMat, tetherMat;
        private VisualLibrary visuals;
        private TimedHazard[] timedHazards = new TimedHazard[0];
        private Transform[] timedViews = new Transform[0];
        private Transform world, tether;
        private Camera gameCamera;
        private LineRenderer tetherLine;
        private Canvas canvas;
        private RectTransform safeArea, energyFill, progressFill;
        private Text energyText, hintText, statsText, overlayText, actionText, pauseText, audioText;
        private GameObject overlay, pauseButton, courseButton;
        private int activeTouchId = -1;
        private bool mouseHeld, keyboardHeld;
        private RunPhase lastPhase = (RunPhase)(-1);
        private int lastEnergy = -1, lastBonus = -1, lastMeter = -1, lastSection = -1;
        private Rect lastSafeArea;

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
            Feedback.ResetPresentation();
            lastPhase = (RunPhase)(-1);
            RefreshVisuals(0);
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
            return visuals.Create(kind, label, world, pos, size, material, kind == VisualKind.TrackTile ? lineMat : null);
        }
        private void AddHazard(string label, Vector3 pos, Vector3 size, bool marked, string reason, bool gate = false)
        {
            var obj = ModelVisual(marked ? (gate ? VisualKind.EnergyGate : VisualKind.BreakableBlock) : VisualKind.Barrier,
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
                var sign = ModelVisual(VisualKind.PulseBadge, label + " pulse mark", pos + new Vector3(0, 1.1f, 0), new Vector3(.65f, .65f, .12f), tealMat);
                sign.transform.SetParent(obj.transform, true);
            }
        }
        private void BuildWorld()
        {
            world = new GameObject(advancedCourse ? "Timing course" : "Foundation course").transform; world.SetParent(transform, false);
            trackMat = MakeMaterial("Track", new Color(.035f, .055f, .09f));
            redMat = MakeMaterial("Fatal obstacle", new Color(.95f, .16f, .24f));
            goldMat = MakeMaterial("Pulse target / bonus", new Color(1f, .69f, .16f));
            tealMat = MakeMaterial("Left core / energy", new Color(.08f, .95f, .9f));
            purpleMat = MakeMaterial("Right core", new Color(.72f, .35f, 1f));
            lineMat = MakeMaterial("Track markings", new Color(.12f, .26f, .36f));
            darkMat = MakeMaterial("Recessed panels", new Color(.014f, .023f, .04f));
            accentMat = MakeMaterial("Edge accents", new Color(.62f, .78f, .86f));
            visuals = new VisualLibrary(darkMat, accentMat);
            for (int z = -10; z <= CourseFinishDistance + 10; z += 10)
                ModelVisual(VisualKind.TrackTile, "Track tile", new Vector3(0, -.22f, z + 5), new Vector3(7.2f, .4f, 10), trackMat);
            AddHazard("Narrow gate left", new Vector3(-2.425f, .65f, 20), new Vector3(2.35f, 1.3f, 1), false, "Dar geçit: bırak ve daral.");
            AddHazard("Narrow gate right", new Vector3(2.425f, .65f, 20), new Vector3(2.35f, 1.3f, 1), false, "Dar geçit: bırak ve daral.");
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
                AddHazard("Center barrier", new Vector3(0, .65f, 42), new Vector3(2.5f, 1.3f, 1.2f), false, "Orta bariyer: basılı tut ve açıl.");
                AddHazard("Energy gate", new Vector3(0, .65f, 100), new Vector3(7.2f, 1.3f, .7f), true, "Kapalı enerji kapısı! Tam enerjiyle menzilde bırak.", true);
                AddHazard("Marked breakable", new Vector3(0, .65f, 153), new Vector3(7.2f, 1.3f, .7f), true, "İşaretli hedef kapalı! Tam enerjiyle menzilde bırak.");
                for (int i = 0; i < PrototypeLayout.EnergyDistances.Length; i++)
                    AddPickup(new Vector3(PrototypeLayout.EnergyOffsets[i], .55f, PrototypeLayout.EnergyDistances[i]), false, i);
                foreach (float z in new[] { 49f, 85f, 141f, 167f })
                { AddPickup(new Vector3(-2.2f, .55f, z), true, pickups.Count); AddPickup(new Vector3(2.2f, .55f, z), true, pickups.Count); }
            }
            ModelVisual(VisualKind.FinishMarker, "Finish arch", new Vector3(0, 1.6f, CourseFinishDistance), new Vector3(7.2f, 3.2f, .45f), tealMat);
            LeftCore = ModelVisual(VisualKind.Core, "Left core", Vector3.zero, Vector3.one * tuning.SphereRadius * 2, tealMat).transform;
            RightCore = ModelVisual(VisualKind.Core, "Right core", Vector3.zero, Vector3.one * tuning.SphereRadius * 2, purpleMat).transform;
            tether = new GameObject("Energy tether").transform; tether.SetParent(world, false);
            tetherMat = new Material(Shader.Find("Tether Rush/NeonEffect") ?? Shader.Find("Sprites/Default")) { name = "Tether beam" };
            tetherMat.SetColor("_BaseColor", new Color(.08f, .95f, .9f)); materials.Add(tetherMat);
            tetherLine = tether.gameObject.AddComponent<LineRenderer>(); tetherLine.sharedMaterial = tetherMat;
            tetherLine.positionCount = 2; tetherLine.useWorldSpace = true; tetherLine.widthMultiplier = .065f; tetherLine.numCapVertices = 4;
            var cameraObj = new GameObject("Main Camera"); cameraObj.transform.SetParent(world); cameraObj.tag = "MainCamera";
            gameCamera = cameraObj.AddComponent<Camera>(); gameCamera.clearFlags = CameraClearFlags.SolidColor; gameCamera.backgroundColor = new Color(.018f, .025f, .05f);
            gameCamera.fieldOfView = 62; gameCamera.nearClipPlane = .1f; gameCamera.farClipPlane = 90;
            cameraObj.AddComponent<AudioListener>();
        }
        private void AddPickup(Vector3 pos, bool bonus, int id)
        {
            var obj = ModelVisual(bonus ? VisualKind.BonusCrystal : VisualKind.EnergyPickup, bonus ? "Bonus crystal" : "Tether energy",
                pos, Vector3.one * (bonus ? .38f : .32f), bonus ? goldMat : tealMat);
            pickups.Add(new Pickup { Position = pos, View = obj, Bonus = bonus, Id = id });
        }
        private RectTransform UiRect(string label, Transform parent, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(label, typeof(RectTransform)); var rect = go.GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = offsetMin; rect.offsetMax = offsetMax; return rect;
        }
        private Text UiText(string label, Transform parent, string value, int size, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = UiRect(label, parent, min, max, offsetMin, offsetMax); var text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.text = value; text.fontSize = size;
            text.color = Color.white; text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }
        private Image UiImage(string label, Transform parent, Color color, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = UiRect(label, parent, min, max, offsetMin, offsetMax); var img = rect.gameObject.AddComponent<Image>();
            img.color = color; img.raycastTarget = false; return img;
        }
        private GameObject UiButton(string label, Transform parent, string caption, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax, UnityEngine.Events.UnityAction action, out Text text)
        {
            var bg = UiImage(label, parent, new Color(.12f, .36f, .43f), min, max, offsetMin, offsetMax); bg.raycastTarget = true;
            var button = bg.gameObject.AddComponent<Button>(); button.targetGraphic = bg; button.onClick.AddListener(action);
            text = UiText("Caption", bg.transform, caption, 28, Vector2.zero, Vector2.one, new Vector2(8, 5), new Vector2(-8, -5));
            return bg.gameObject;
        }
        private void BuildHud()
        {
            var go = new GameObject("M1 HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false); canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(540, 960); scaler.matchWidthOrHeight = .5f;
            safeArea = UiRect("Safe area", go.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            UiImage("HUD background", safeArea, new Color(.02f, .035f, .065f, .95f), new Vector2(0, 1), Vector2.one, new Vector2(0, -185), Vector2.zero);
            UiText("Title", safeArea, "TETHER RUSH  /  " + (advancedCourse ? "02" : "01"), 24, new Vector2(0, 1), Vector2.one, new Vector2(16, -49), new Vector2(-225, -8));
            pauseButton = UiButton("Pause", safeArea, "Duraklat", new Vector2(1, 1), Vector2.one, new Vector2(-125, -53), new Vector2(-10, -9), () => PauseRun(), out pauseText);
            pauseText.fontSize = 20;
            UiButton("Audio", safeArea, "Ses Açık", new Vector2(1, 1), Vector2.one, new Vector2(-218, -53), new Vector2(-132, -9),
                () => { Feedback.AudioEnabled = !Feedback.AudioEnabled; preferredAudio = Feedback.AudioEnabled; audioText.text = Feedback.AudioEnabled ? "Ses Açık" : "Sessiz"; }, out audioText);
            audioText.fontSize = 16; audioText.text = Feedback.AudioEnabled ? "Ses Açık" : "Sessiz";
            statsText = UiText("Stats", safeArea, "", 22, new Vector2(0, 1), Vector2.one, new Vector2(16, -88), new Vector2(-16, -52));
            var bar = UiImage("Energy background", safeArea, new Color(.12f, .17f, .23f), new Vector2(0, 1), Vector2.one, new Vector2(24, -133), new Vector2(-24, -97));
            energyFill = UiImage("Energy fill", bar.transform, new Color(.04f, .6f, .57f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).rectTransform;
            energyText = UiText("Energy", bar.transform, "", 21, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            hintText = UiText("Section instruction", safeArea, "", 25, new Vector2(0, 1), Vector2.one, new Vector2(16, -181), new Vector2(-16, -138));
            var progress = UiImage("Progress background", safeArea, new Color(.1f, .16f, .22f), Vector2.zero, new Vector2(1, 0), new Vector2(20, 22), new Vector2(-20, 30));
            progressFill = UiImage("Progress", progress.transform, new Color(.1f, .85f, .8f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).rectTransform;
            UiText("Control reminder", safeArea, advancedCourse ? "BASILI TUT: AÇIL   •   BIRAK: DARAL" : "BASILI TUT: AÇIL   •   BIRAK: DARAL / PULSE", 19, Vector2.zero, new Vector2(1, 0), new Vector2(16, 38), new Vector2(-16, 88));
            var panel = UiImage("Run overlay", safeArea, new Color(.015f, .025f, .055f, .95f), new Vector2(.05f, .23f), new Vector2(.95f, .77f), Vector2.zero, Vector2.zero);
            overlay = panel.gameObject; panel.raycastTarget = true;
            overlayText = UiText("Run information", panel.transform, "", 23, new Vector2(0, .42f), Vector2.one, new Vector2(24, 10), new Vector2(-24, -20));
            UiButton("Start retry resume", panel.transform, "Başla", new Vector2(.12f, .06f), new Vector2(.88f, .22f), Vector2.zero, Vector2.zero,
                () => { if (Model.Phase == RunPhase.Paused) ResumeRun(); else StartRun(); }, out actionText);
            courseButton = UiButton("Other course", panel.transform, advancedCourse ? "Temel parkur" : "Zamanlama parkuru",
                new Vector2(.12f, .24f), new Vector2(.88f, .37f), Vector2.zero, Vector2.zero,
                () => SceneManager.LoadScene(advancedCourse ? "Prototype" : "Challenge"), out var courseText);
            courseText.fontSize = 22;
            if (EventSystem.current == null)
            {
                var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)); events.transform.SetParent(transform);
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
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
            if (Screen.safeArea != lastSafeArea)
            {
                lastSafeArea = Screen.safeArea;
                if (Screen.width > 0 && Screen.height > 0)
                { safeArea.anchorMin = new Vector2(lastSafeArea.xMin / Screen.width, lastSafeArea.yMin / Screen.height); safeArea.anchorMax = new Vector2(lastSafeArea.xMax / Screen.width, lastSafeArea.yMax / Screen.height); }
            }
            progressFill.anchorMax = new Vector2(Mathf.Clamp01(z / CourseFinishDistance), 1);
            if (lastEnergy != Model.Energy)
            {
                lastEnergy = Model.Energy; energyFill.anchorMax = new Vector2((float)Model.Energy / tuning.EnergyCapacity, 1);
                energyText.text = advancedCourse ? "ZAMANLAMA • Daral / genişle" : Model.Energy == tuning.EnergyCapacity ? "ENERJİ TAM • Menzilde bırak = Pulse" : "ENERJİ  " + Model.Energy + " / " + tuning.EnergyCapacity;
                energyFill.GetComponent<Image>().color = Model.Energy == tuning.EnergyCapacity ? new Color(.45f, .48f, .08f) : new Color(.04f, .6f, .57f);
            }
            int meter = Mathf.FloorToInt(z), section = Section();
            if (lastBonus != Model.Bonus || lastMeter != meter)
            { lastBonus = Model.Bonus; lastMeter = meter; statsText.text = meter + " / " + CourseFinishDistance + " m     •     BONUS " + Model.Bonus; }
            // Meter changes include the short in-range cue before each mandatory Pulse target.
            if (lastSection != section || Model.Phase == RunPhase.Running) { lastSection = section; hintText.text = SectionHint(section); }
            Feedback.Tick(Model.Phase == RunPhase.Paused ? 0 : dt, Model.IsHeld, Model.Energy == tuning.EnergyCapacity, Model.Phase);
            if (lastPhase == Model.Phase) return;
            lastPhase = Model.Phase; overlay.SetActive(Model.Phase != RunPhase.Running); pauseButton.SetActive(Model.Phase == RunPhase.Running);
            courseButton.SetActive((Model.Phase == RunPhase.Ready || Model.Phase == RunPhase.Completed || Model.Phase == RunPhase.Failed) &&
                Application.CanStreamedLevelBeLoaded(advancedCourse ? "Prototype" : "Challenge"));
            switch (Model.Phase)
            {
                case RunPhase.Ready:
                    overlayText.text = advancedCourse ? "ZAMANLAMA PARKURU\n\nBasılı tut: açıl. Bırak: daral.\n\nHareketli duvarları ve\ndönen kolu dikkatle oku.\nİki küreyi de bitişe ulaştır." :
                        "İKİ KÜRE, TEK BAĞ\n\nBasılı tut: açıl. Bırak: daral.\n\nBağ enerji, küreler bonus toplar.\nTam enerjiyle hedefe yakın\nbırak: Pulse!\nKırmızı engeller ölümcüldür.";
                    actionText.text = "Başla"; break;
                case RunPhase.Paused:
                    overlayText.text = "DURAKLATILDI\n\nDevam ettikten sonra\nyeniden basılı tut.";
                    actionText.text = "Devam et"; break;
                case RunPhase.Failed:
                    Feedback.Failed();
                    overlayText.text = "DENEME BİTTİ\n\n" + Model.FailureReason + "\n\n" + meter + " m   •   Bonus " + Model.Bonus;
                    actionText.text = "Hemen tekrar dene"; break;
                case RunPhase.Completed:
                    Feedback.Completed();
                    overlayText.text = "BÖLÜM TAMAMLANDI\n\n" + CourseFinishDistance + " m   •   Bonus " + Model.Bonus +
                        (advancedCourse ? "\n\nHareketli engelleri geçtin!" : "\n\nAçılan Pulse hedefi: " + ActivatedTargetCount + " / 2");
                    actionText.text = "Yeniden oyna"; break;
            }
        }
        private void OnDestroy()
        {
            foreach (var material in materials) if (material != null) Destroy(material);
        }
    }
}
