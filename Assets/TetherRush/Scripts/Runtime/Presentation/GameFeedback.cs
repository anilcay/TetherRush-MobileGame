using TetherRush.Core;
using UnityEngine;

namespace TetherRush.Presentation
{
    /// <summary>Visual/audio only. Tick is driven by the game, including zero delta while paused.</summary>
    public sealed class GameFeedback : MonoBehaviour
    {
        private const int SparkCount = 28, GhostCount = 24, RingCount = 4;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int Emission = Shader.PropertyToID("_EmissionColor");
        private static Color Teal => VisualTheme.Tether;
        private static Color Gold => VisualTheme.PulseAccent;
        private MaterialPropertyBlock block;
        private readonly Spark[] sparks = new Spark[SparkCount];
        private readonly Ghost[] ghosts = new Ghost[GhostCount];
        private readonly Ring[] rings = new Ring[RingCount];
        private Transform left, right, effects;
        private Renderer[] tetherRenderers;
        private Transform leftMesh, rightMesh;
        private Quaternion leftRotation, rightRotation;
        private Material effectMaterial;
        private Material glowMaterial;
        private LineRenderer beamGlow;
        private readonly LineRenderer[] coreHalos = new LineRenderer[2];
        private readonly LineRenderer[] coreTrails = new LineRenderer[2];
        private readonly Vector3[][] trailPoints = new Vector3[2][];
        private readonly Vector3[][] haloPoints = { new Vector3[48], new Vector3[48] };
        private readonly Vector3[] ringPoints = new Vector3[64];
        private static readonly Vector3[] HaloCircle = MakeCircle(48), RingCircle = MakeCircle(64);
        private readonly System.Collections.Generic.List<MeshRenderer> targetRenderers = new System.Collections.Generic.List<MeshRenderer>(10);
        private readonly System.Collections.Generic.Dictionary<int, Material[]> ghostSlots = new System.Collections.Generic.Dictionary<int, Material[]>();
        private bool[] tetherUsesEmission;
        private bool tetherTintValid, coreEmissionValid;
        private Color lastTetherTint;
        private float lastTetherGlow, lastCoreEmission;
        private Renderer[][] coreRenderers;
        private float pickupFlash, pulseFlash;
        private AudioSource audioSource;
        private AudioClip collectClip, readyClip, pulseClip, relayClip, failClip, successClip;
        private readonly AudioClip[] coinClips = new AudioClip[3];
        private readonly System.Collections.Generic.List<AudioClip> ownedClips = new System.Collections.Generic.List<AudioClip>();
        private int coinSequence, lastCoinFrame = -1;
        private float nextCoinTime, nextEnergyTime;
        public int CoinSequence => coinSequence;
        public AudioSource Source => audioSource;
        private float clock, radius, terminalTime;
        private bool initialized, wasReady, audioEnabled = true, ownsSource;
        private Color terminalColor;
        private int sparkCursor, ghostCursor, ringCursor;

        private sealed class Spark
        {
            public LineRenderer View;
            public Vector3 Origin, Velocity;
            public Color Color;
            public float Age, Life;
            public bool Flow;
        }
        private sealed class Ghost
        {
            public Transform Transform;
            public MeshFilter Filter;
            public MeshRenderer Renderer;
            public Vector3 Origin, Travel, Scale;
            public Quaternion Rotation;
            public float Age, Life;
            public Color Color;
            public int MaterialSlotCount = 1;
        }
        private sealed class Ring
        {
            public LineRenderer View;
            public float Age, Life, Radius;
            public Color Color;
        }

        private float volume = .55f;
        public float Volume
        {
            get => volume;
            set { volume = float.IsNaN(value) || float.IsInfinity(value) ? .55f : Mathf.Clamp01(value); if (audioSource != null) audioSource.volume = volume; }
        }

        public bool AudioEnabled
        {
            get => audioEnabled;
            set { audioEnabled = value; if (audioSource != null) audioSource.mute = !value; }
        }

        public void Initialize(Transform leftCore, Transform rightCore, Transform tether, Camera camera,
            Material tetherMaterial, float pulseRadius)
        {
            if (initialized) return;
            initialized = true;
            block = new MaterialPropertyBlock();
            left = leftCore; right = rightCore; radius = Mathf.Max(0, pulseRadius);
            // Never rotate the core roots: their transforms belong to the simulation manager.
            leftMesh = FindChildMesh(left); rightMesh = FindChildMesh(right);
            if (leftMesh != null) leftRotation = leftMesh.localRotation;
            if (rightMesh != null) rightRotation = rightMesh.localRotation;
            tetherRenderers = tether != null ? tether.GetComponentsInChildren<Renderer>() : new Renderer[0];
            tetherUsesEmission = new bool[tetherRenderers.Length];
            for (int i = 0; i < tetherRenderers.Length; i++)
                foreach (var material in tetherRenderers[i].sharedMaterials)
                    if (material != null && material.HasProperty(Emission)) tetherUsesEmission[i] = true;
            effects = new GameObject("Pooled feedback (visual only)").transform;
            effects.SetParent(transform, false);
            Shader shader = Shader.Find("Tether Rush/NeonEffect") ?? Shader.Find("Sprites/Default");
            effectMaterial = new Material(shader) { name = "Feedback transparent (owned)" };
            effectMaterial.SetColor(BaseColor, Color.white);
            glowMaterial = new Material(Shader.Find("Tether Rush/NeonGlow") ?? shader) { name = "Feedback glow (owned)" };
            glowMaterial.SetColor(BaseColor, Color.white);
            coreRenderers = new[] { left != null ? left.GetComponentsInChildren<MeshRenderer>() : new Renderer[0],
                right != null ? right.GetComponentsInChildren<MeshRenderer>() : new Renderer[0] };
            beamGlow = MakeLine("Tether soft aura", 2, false, .16f);
            for (int i = 0; i < 2; i++)
            {
                coreHalos[i] = MakeLine("Core orbital halo", 48, true, .028f);
                coreTrails[i] = MakeLine("Core wake", 12, false, .11f);
                coreTrails[i].startWidth = .11f; coreTrails[i].endWidth = .005f;
                trailPoints[i] = new Vector3[12];
            }
            for (int i = 0; i < SparkCount; i++) sparks[i] = new Spark { View = MakeLine("Pickup spark", 2, false, .035f) };
            for (int i = 0; i < RingCount; i++) rings[i] = new Ring { View = MakeLine("Pulse / feedback ring", 64, true, .045f) };
            for (int i = 0; i < GhostCount; i++)
            {
                var go = new GameObject("Target mesh echo", typeof(MeshFilter), typeof(MeshRenderer));
                go.transform.SetParent(effects, false);
                var renderer = go.GetComponent<MeshRenderer>(); renderer.sharedMaterial = effectMaterial;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows = false;
                renderer.enabled = false;
                ghosts[i] = new Ghost { Transform = go.transform, Filter = go.GetComponent<MeshFilter>(), Renderer = renderer };
            }
            if (camera != null)
            {
                audioSource = camera.GetComponent<AudioSource>();
                if (audioSource == null) { audioSource = camera.gameObject.AddComponent<AudioSource>(); ownsSource = true; }
                audioSource.playOnAwake = false; audioSource.loop = false; audioSource.spatialBlend = 0; audioSource.volume = volume;
                audioSource.mute = !audioEnabled;
            }
            // Imported clips are shared; only missing-asset fallback clips belong to this instance.
            collectClip = Cue(AudioPalette.Energy, "Energy", .085f, 540, 800, .18f);
            for (int i = 0; i < coinClips.Length; i++)
                coinClips[i] = Cue(AudioPalette.Coin(i), "Coin" + (i + 1), .12f, 880 + i * 110, 1320 + i * 110, .16f);
            readyClip = Cue(AudioPalette.Ready, "Ready", .23f, 580, 1160, .15f);
            pulseClip = Cue(AudioPalette.Pulse, "Pulse", .22f, 320, 90, .23f);
            relayClip = Cue(AudioPalette.Relay, "Relay", .18f, 440, 880, .17f);
            failClip = Cue(AudioPalette.Fail, "Failure", .23f, 175, 65, .19f);
            successClip = Cue(AudioPalette.Success, "Completion", .35f, 660, 1320, .17f);
            ResetPresentation();
        }

        public void Tick(float dt, bool held, bool ready, RunPhase phase)
        {
            if (!initialized) return;
            bool paused = phase == RunPhase.Paused;
            if (audioSource != null)
            {
                if (paused) audioSource.Pause(); else audioSource.UnPause();
            }
            if (paused || dt <= 0 || float.IsNaN(dt) || float.IsInfinity(dt)) return;
            clock += dt;
            if (ready && !wasReady && phase == RunPhase.Running) Play(readyClip);
            wasReady = ready;
            bool active = phase == RunPhase.Ready || phase == RunPhase.Running;
            if (active)
            {
                float speed = held ? 44 : 22;
                if (leftMesh != null) leftMesh.localRotation = leftRotation * Quaternion.Euler(clock * speed, clock * 28, 0);
                if (rightMesh != null) rightMesh.localRotation = rightRotation * Quaternion.Euler(-clock * speed, -clock * 28, 0);
            }
            terminalTime = Mathf.Max(0, terminalTime - dt);
            Color tint = ready ? VisualTheme.TetherReady : Teal;
            // NeonEffect uses only the tint; retain shimmer for materials that actually support emission.
            float glow = ready ? 1.2f + .2f * Mathf.Sin(clock * 21) : .35f;
            bool tintChanged = !tetherTintValid || lastTetherTint != tint;
            for (int i = 0; i < tetherRenderers.Length; i++)
                if (tetherRenderers[i] != null && (tintChanged || (tetherUsesEmission[i] && lastTetherGlow != glow)))
                    Tint(tetherRenderers[i], tint, glow);
            lastTetherTint = tint; lastTetherGlow = glow; tetherTintValid = true;
            pickupFlash = Mathf.Max(0, pickupFlash - dt * 3);
            pulseFlash = Mathf.Max(0, pulseFlash - dt * 3.5f);
            UpdateCoreVisuals(dt, active, ready, tint);
            TickSparks(dt); TickRings(dt); TickGhosts(dt);
        }

        public void EnergyCollected(Vector3 position) { if (!initialized) return; pickupFlash = .65f; Emit(position, VisualTheme.Energy, true, 5); if (clock >= nextEnergyTime) { nextEnergyTime = clock + .025f; Play(collectClip, .8f); } }
        public void RelayConnected() { if (initialized) Play(relayClip, .8f); }
        public void BonusCollected(Vector3 position) { if (!initialized) return; Emit(position, VisualTheme.Bonus, false, 7); SpawnRing(position, .3f, .22f, VisualTheme.Bonus); PlayCoin(); }
        public void Pulse(Vector3 position)
        {
            if (!initialized) return;
            pulseFlash = 1;
            SpawnRing(position, radius, .38f, VisualTheme.TetherReady);
            SpawnRing(position, radius * .87f, .28f, VisualTheme.PulseAccent);
            Emit(position, VisualTheme.TetherReady, false, 8); Play(pulseClip);
        }
        public void TargetOpened(GameObject visual, bool isGate)
        {
            if (!initialized || visual == null) return;
            // Copy mesh references and transforms only. No colliders, scripts, or hierarchy are cloned.
            visual.GetComponentsInChildren(false, targetRenderers);
            int emitted = 0;
            foreach (var renderer in targetRenderers)
            {
                string label = renderer.name;
                if (isGate && label.IndexOf("Frame", System.StringComparison.OrdinalIgnoreCase) >= 0) continue;
                var filter = renderer.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null || emitted >= 10) continue;
                var ghost = ghosts[ghostCursor++ % GhostCount];
                ghost.Filter.sharedMesh = filter.sharedMesh;
                int slotCount = filter.sharedMesh.subMeshCount;
                if (ghost.MaterialSlotCount != slotCount)
                {
                    if (!ghostSlots.TryGetValue(slotCount, out var slots))
                    {
                        slots = new Material[slotCount];
                        for (int i = 0; i < slots.Length; i++) slots[i] = effectMaterial;
                        ghostSlots.Add(slotCount, slots);
                    }
                    ghost.Renderer.sharedMaterials = slots;
                    ghost.MaterialSlotCount = slotCount;
                }
                ghost.Origin = renderer.transform.position; ghost.Rotation = renderer.transform.rotation;
                ghost.Scale = renderer.transform.lossyScale; ghost.Age = 0; ghost.Life = isGate ? .4f : .32f; ghost.Color = Gold;
                float sign = label.IndexOf("DoorLeft", System.StringComparison.OrdinalIgnoreCase) >= 0 ? -1 :
                    label.IndexOf("DoorRight", System.StringComparison.OrdinalIgnoreCase) >= 0 ? 1 : (emitted % 2 == 0 ? -1 : 1);
                ghost.Travel = isGate ? new Vector3(sign * .9f, 0, 0) : new Vector3(sign * .35f, .25f + emitted * .025f, .12f);
                ghost.Transform.SetPositionAndRotation(ghost.Origin, ghost.Rotation); ghost.Transform.localScale = ghost.Scale;
                Tint(ghost.Renderer, Gold, 0); ghost.Renderer.enabled = true; emitted++;
            }
            targetRenderers.Clear();
            Emit(visual.transform.position, VisualTheme.PulseAccent, false, isGate ? 6 : 8);
            if (emitted == 0) SpawnRing(visual.transform.position, .65f, .22f, Gold);
        }
        public void Failed()
        {
            if (!initialized) return;
            terminalTime = .3f; terminalColor = VisualTheme.Danger;
            Emit(left != null ? left.position : Center, terminalColor, false, 5);
            Emit(right != null ? right.position : Center, terminalColor, false, 5);
            SpawnRing(Center, .7f, .3f, terminalColor); Play(failClip);
        }
        public void Completed()
        {
            if (!initialized) return;
            terminalTime = .45f; terminalColor = Gold;
            SpawnRing(Center, 1.25f, .5f, Gold); SpawnRing(Center, .9f, .38f, Teal);
            Emit(Center, Gold, false, 10); Play(successClip);
        }
        public void ResetPresentation()
        {
            if (!initialized) return;
            clock = terminalTime = pickupFlash = pulseFlash = 0; wasReady = false;
            tetherTintValid = coreEmissionValid = false;
            coinSequence = 0; lastCoinFrame = -1; nextCoinTime = nextEnergyTime = 0;
            if (leftMesh != null) leftMesh.localRotation = leftRotation;
            if (rightMesh != null) rightMesh.localRotation = rightRotation;
            foreach (var spark in sparks) { spark.Life = 0; spark.View.enabled = false; }
            foreach (var ring in rings) { ring.Life = 0; ring.View.enabled = false; }
            foreach (var ghost in ghosts) { ghost.Life = 0; ghost.Renderer.enabled = false; }
            foreach (var renderer in tetherRenderers) if (renderer != null) renderer.SetPropertyBlock(null);
            for (int i = 0; i < 2; i++)
            {
                Vector3 position = i == 0 ? (left != null ? left.position : Center) : (right != null ? right.position : Center);
                for (int p = 0; p < 12; p++) trailPoints[i][p] = position;
            }
            UpdateCoreVisuals(0, true, false, Teal);
            if (audioSource != null) audioSource.Stop();
        }

        private Vector3 Center => left != null && right != null ? (left.position + right.position) * .5f : transform.position;
        private static Transform FindChildMesh(Transform root)
        {
            if (root == null) return null;
            var visualGroup = root.Find("Visual");
            if (visualGroup != null) return visualGroup;
            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>())
                if (renderer.transform != root) return renderer.transform;
            return null;
        }
        private LineRenderer MakeLine(string label, int count, bool loop, float width)
        {
            var go = new GameObject(label); go.transform.SetParent(effects, false);
            var line = go.AddComponent<LineRenderer>(); line.sharedMaterial = glowMaterial; line.useWorldSpace = true;
            line.positionCount = count; line.loop = loop; line.widthMultiplier = width; line.numCapVertices = 2;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; line.receiveShadows = false; line.enabled = false;
            return line;
        }
        private void UpdateCoreVisuals(float dt, bool active, bool ready, Color tetherColor)
        {
            if (left == null || right == null) return;
            beamGlow.enabled = active;
            if (active)
            {
                beamGlow.SetPosition(0, left.position); beamGlow.SetPosition(1, right.position);
                Color beamColor = tetherColor; beamColor.a = ready ? .28f : .16f;
                beamGlow.startColor = beamGlow.endColor = beamColor;
                beamGlow.widthMultiplier = .14f + (ready ? .035f : 0) + pulseFlash * .1f;
            }
            float emission = .13f + pickupFlash * .2f + pulseFlash * .28f + (ready ? .06f : 0);
            bool emissionChanged = !coreEmissionValid || emission != lastCoreEmission;
            for (int i = 0; i < 2; i++)
            {
                Color coreColor = i == 0 ? VisualTheme.CoreA : VisualTheme.CoreB;
                // Identity is constant through ready, collect, failure and completion states.
                if (emissionChanged) foreach (var renderer in coreRenderers[i]) if (renderer != null)
                {
                    // Keep authored metal/inlay submesh colors; only the reflected glow responds.
                    renderer.GetPropertyBlock(block);
                    block.SetColor(Emission, coreColor * emission);
                    renderer.SetPropertyBlock(block); block.Clear();
                }
                var halo = coreHalos[i]; halo.enabled = active;
                coreTrails[i].enabled = active;
                if (!active) continue; // Terminal sparks/rings/ghosts still advance in Tick.
                Vector3 position = i == 0 ? left.position : right.position;
                float haloRadius = .35f + .012f * Mathf.Sin(clock * 3 + i * Mathf.PI) + pickupFlash * .025f;
                for (int p = 0; p < 48; p++)
                {
                    float angle = p * Mathf.PI * 2 / 48;
                    haloPoints[i][p] = position + new Vector3(HaloCircle[p].x, .08f * Mathf.Sin(angle + clock * .7f), HaloCircle[p].z) * haloRadius;
                }
                halo.SetPositions(haloPoints[i]);
                Color haloColor = coreColor; haloColor.a = .7f;
                halo.startColor = halo.endColor = haloColor;
                var points = trailPoints[i];
                float follow = 1 - Mathf.Exp(-dt * 24);
                for (int p = 11; p > 0; p--) points[p] = Vector3.Lerp(points[p], points[p - 1], follow);
                points[0] = position;
                coreTrails[i].SetPositions(points);
                Color trailColor = coreColor; trailColor.a = .34f;
                coreTrails[i].startColor = trailColor; trailColor.a = 0; coreTrails[i].endColor = trailColor;
            }
            lastCoreEmission = emission; coreEmissionValid = true;
        }
        private static Vector3[] MakeCircle(int count)
        {
            var points = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                float angle = i * Mathf.PI * 2 / count;
                points[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            }
            return points;
        }
        private void Tint(Renderer renderer, Color color, float glow)
        {
            renderer.GetPropertyBlock(block); block.SetColor(BaseColor, color); block.SetColor(ColorId, color);
            block.SetColor(Emission, new Color(color.r * glow, color.g * glow, color.b * glow, color.a));
            renderer.SetPropertyBlock(block); block.Clear();
        }
        private void Emit(Vector3 position, Color color, bool flow, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var spark = sparks[sparkCursor++ % SparkCount]; float angle = i * 2.39996f;
                spark.Origin = position + new Vector3(Mathf.Cos(angle), .3f, Mathf.Sin(angle)) * .07f;
                spark.Velocity = new Vector3(Mathf.Cos(angle), .6f, Mathf.Sin(angle)) * .85f;
                spark.Age = 0; spark.Life = flow ? .25f : .3f; spark.Flow = flow; spark.Color = color;
                spark.View.SetPosition(0, spark.Origin); spark.View.SetPosition(1, spark.Origin + Vector3.up * .05f);
                spark.View.enabled = true;
            }
        }
        private void TickSparks(float dt)
        {
            foreach (var spark in sparks)
            {
                if (spark.Life <= 0) continue;
                spark.Age += dt; float t = Mathf.Clamp01(spark.Age / spark.Life);
                Vector3 position = spark.Flow ? Vector3.Lerp(spark.Origin, Center, t * t) : spark.Origin + spark.Velocity * spark.Age;
                Vector3 direction = spark.Flow ? (Center - position).normalized : spark.Velocity.normalized;
                spark.View.SetPosition(0, position); spark.View.SetPosition(1, position - direction * .16f * (1 - t));
                Color color = spark.Color; color.a = 1 - t; spark.View.startColor = spark.View.endColor = color;
                if (t >= 1) { spark.View.enabled = false; spark.Life = 0; }
            }
        }
        private void SpawnRing(Vector3 position, float maxRadius, float life, Color color)
        {
            var ring = rings[ringCursor++ % RingCount]; ring.Age = 0; ring.Life = life; ring.Radius = maxRadius; ring.Color = color;
            ring.View.transform.position = position; ring.View.enabled = true; UpdateRing(ring, 0);
        }
        private void UpdateRing(Ring ring, float t)
        {
            // Reach the exact gameplay radius at 65% lifetime, then retain that radius while fading.
            float width = .025f + .035f * (1 - t);
            float size = Mathf.Max(0, ring.Radius - width * .5f) * Mathf.Clamp01(t / .65f);
            Vector3 center = ring.View.transform.position;
            for (int i = 0; i < 64; i++) ringPoints[i] = center + RingCircle[i] * size;
            ring.View.SetPositions(ringPoints);
            Color color = ring.Color; color.a = 1 - t; ring.View.startColor = ring.View.endColor = color;
            ring.View.widthMultiplier = width;
        }
        private void TickRings(float dt)
        {
            foreach (var ring in rings)
            {
                if (ring.Life <= 0) continue;
                ring.Age += dt; float t = Mathf.Clamp01(ring.Age / ring.Life); UpdateRing(ring, t);
                if (t >= 1) { ring.View.enabled = false; ring.Life = 0; }
            }
        }
        private void TickGhosts(float dt)
        {
            foreach (var ghost in ghosts)
            {
                if (ghost.Life <= 0) continue;
                ghost.Age += dt; float t = Mathf.Clamp01(ghost.Age / ghost.Life);
                ghost.Transform.position = ghost.Origin + ghost.Travel * (1 - (1 - t) * (1 - t));
                ghost.Transform.localScale = ghost.Scale * (1 - .12f * t);
                Color color = ghost.Color; color.a = .8f * (1 - t); Tint(ghost.Renderer, color, 0);
                if (t >= 1) { ghost.Renderer.enabled = false; ghost.Filter.sharedMesh = null; ghost.Life = 0; }
            }
        }
        private void PlayCoin()
        {
            // Swept pairs share one cue; progression never touches gameplay Random state.
            if (lastCoinFrame == Time.frameCount || clock < nextCoinTime) return;
            lastCoinFrame = Time.frameCount; nextCoinTime = clock + .035f;
            Play(coinClips[coinSequence++ % coinClips.Length], .9f);
        }
        private void Play(AudioClip clip, float gain = .8f)
        {
            // Layer short cues so coins cannot truncate a Pulse or result signal.
            if (audioEnabled && audioSource != null && clip != null && isActiveAndEnabled)
                audioSource.PlayOneShot(clip, gain);
        }
        private AudioClip Cue(AudioClip imported, string name, float length, float from, float to, float amplitude)
        {
            if (imported != null) return imported;
            var clip = MakeCue(name, length, from, to, amplitude);
            ownedClips.Add(clip);
            return clip;
        }
        private static AudioClip MakeCue(string name, float length, float from, float to, float amplitude)
        {
            const int rate = 22050;
            var data = new float[Mathf.CeilToInt(length * rate)]; float phase = 0;
            for (int i = 0; i < data.Length; i++)
            {
                float t = (float)i / data.Length;
                phase += Mathf.Lerp(from, to, t) * Mathf.PI * 2 / rate;
                float envelope = Mathf.Min(1, t * 35) * (1 - t) * (1 - t);
                data[i] = (Mathf.Sin(phase) + .2f * Mathf.Sin(phase * 2)) * envelope * Mathf.Min(amplitude, .25f);
            }
            var clip = AudioClip.Create("Procedural " + name, data.Length, 1, rate, false); clip.SetData(data, 0); return clip;
        }
        private void OnDestroy()
        {
            if (effects != null) Destroy(effects.gameObject);
            if (effectMaterial != null) Destroy(effectMaterial);
            if (glowMaterial != null) Destroy(glowMaterial);
            if (ownsSource && audioSource != null) Destroy(audioSource);
            foreach (var clip in ownedClips) if (clip != null) Destroy(clip);
        }
    }
}
