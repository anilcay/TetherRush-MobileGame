using System;
using System.Collections.Generic;
using TetherRush.Art;
using UnityEngine;

namespace TetherRush.Course
{
    public static class LevelCatalog
    {
        public static int Count => 5;

        public static CourseDefinition Get(int index)
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            var b = new CourseBuilder();
            switch (index)
            {
                case 0:
                    b.Course.Title = "Daral ve Açıl";
                    b.Course.Description = "Bırakınca daral, basılı tutunca açıl. İki küreyi de koru.";
                    b.Course.Length = 56;
                    b.Narrow(16); b.Center(36); b.Narrow(48);
                    b.Bonuses(22, 2.2f); b.Bonuses(39, 2.2f);
                    b.Cue(26, true); b.Cue(42, false);
                    b.Hint(0, "BIRAK • Dar geçitten iki küreyle geç.");
                    b.Hint(24, "BASILI TUT • Orta bariyerin iki yanından geç.");
                    b.Hint(40, "BIRAK • Yeniden daral.");
                    break;
                case 1:
                    b.Course.Title = "Bağı Keşfet";
                    b.Course.Description = "Enerjiyi bağ toplar. Altın bonusları kürelerle topla.";
                    b.Course.Length = 64; b.Course.UsesEnergy = true;
                    b.Center(18); b.Charge(26); b.Bonuses(46, 2.2f); b.Narrow(58); b.Bonuses(62, 2.2f);
                    b.Cue(8, true); b.Cue(52, false);
                    b.Hint(0, "BASILI TUT • Bağı iki yana aç.");
                    b.Hint(22, "BAĞLA TOPLA • Beş camgöbeği parça enerjiyi doldurur.");
                    b.Hint(43, "BONUS • Altın kristallere kürelerle dokun.");
                    b.Hint(50, "BIRAK • Dolu enerji bir Pulse üretir; dar geçide hazırlan.");
                    break;
                case 2:
                    b.Course.Title = "İlk Pulse";
                    b.Course.Description = "Beş enerji topla. İşaretli kapıya yaklaşınca bırak.";
                    b.Course.Length = 76; b.Course.UsesEnergy = true;
                    b.Center(18); b.Charge(26); b.PulseTarget(58, true); b.Narrow(68);
                    b.Bonuses(46, 2.2f); b.Bonuses(72, 2.2f);
                    b.Cue(8, true); b.Cue(54, false);
                    b.Hint(0, "BASILI TUT • Açıl ve beş enerjiyi bağla topla.");
                    b.Hint(44, "PULSE HAZIR • Kapıya kadar basılı tut.");
                    b.Hint(53, "ŞİMDİ BIRAK • Yakın menzilli Pulse kapıyı açar.");
                    b.Hint(61, "Normal kırmızı engeller Pulse ile açılmaz.");
                    break;
                case 3:
                    b.Course.Title = "Zamanlama";
                    b.Course.Description = "Kayan duvarı oku, dönen kolun iki yanından geç.";
                    b.Course.Length = 96;
                    b.WideMovingWall(24); b.SideMovingWall(46); b.Arm(70); b.Narrow(86);
                    b.Bonuses(30, 2.2f); b.Bonuses(51, .65f); b.Bonuses(75, 2.2f); b.Bonuses(91, 2.2f);
                    b.Cue(14, true); b.Cue(34, false); b.Cue(58, true); b.Cue(78, false);
                    b.Hint(0, "HAREKETLİ DUVAR • Basılı tut, geniş geç.");
                    b.Hint(32, "BIRAK • Duvar yana kayarken dar geç.");
                    b.Hint(56, "BASILI TUT • Dönen kolun iki yanından geç.");
                    b.Hint(77, "BIRAK • Son dar geçit.");
                    break;
                default:
                    b.Course.Title = "Karma Parkur";
                    b.Course.Description = "Daral, enerji topla, hedefi kır ve hareketli engelleri aş.";
                    b.Course.Length = 124; b.Course.UsesEnergy = true;
                    b.Narrow(14); b.Center(32); b.Charge(40); b.PulseTarget(70, false);
                    b.WideMovingWall(88); b.Arm(108);
                    b.Bonuses(60, 2.2f); b.Bonuses(96, 2.2f); b.Bonuses(119, .65f);
                    b.Cue(22, true); b.Cue(66, false); b.Cue(78, true); b.Cue(114, false);
                    b.Hint(0, "BIRAK • Önce dar geçit.");
                    b.Hint(20, "BASILI TUT • Orta bariyer, ardından beş enerji.");
                    b.Hint(58, "İŞARETLİ BLOK • Enerjiyi hedefe kadar sakla.");
                    b.Hint(65, "ŞİMDİ BIRAK • Pulse yalnızca işaretli hedefi kırar.");
                    b.Hint(76, "BASILI TUT • Kayan duvar ve dönen kol.");
                    b.Hint(113, "BIRAK • Bitişe dar ilerle.");
                    break;
            }
            b.Course.Id = "level-" + (index + 1);
            return b.Build();
        }
    }

    /// <summary>Shared authoring primitives. Offset also calibrates motion against global distance / 7.</summary>
    internal sealed class CourseBuilder
    {
        internal readonly CourseDefinition Course = new CourseDefinition();
        private readonly float offset;
        private readonly List<HazardSpec> hazards = new List<HazardSpec>();
        private readonly List<PickupSpec> pickups = new List<PickupSpec>();
        private readonly List<TimedHazard> timed = new List<TimedHazard>();
        private readonly List<HintSpec> hints = new List<HintSpec>();
        private readonly List<InputCue> route = new List<InputCue>();

        internal CourseBuilder(float offset = 0) { this.offset = offset; }
        internal void Cue(float z, bool press) { route.Add(new InputCue { Distance = offset + z, Press = press }); }
        internal void Hint(float z, string text) { hints.Add(new HintSpec { Distance = offset + z, Text = text }); }
        internal void Narrow(float z)
        {
            for (int side = -1; side <= 1; side += 2)
                Hazard("Dar geçit", side * 2.425f, z, 2.35f, 1, false, false,
                    "Dar geçit: bırak ve daral.", VisualKind.SplitPylon);
        }
        internal void Center(float z)
        {
            Hazard("Orta bariyer", 0, z, 2.5f, 1.2f, false, false,
                "Orta bariyer: basılı tut ve açıl.", VisualKind.ChevronBarrier);
        }
        internal void PulseTarget(float z, bool gate)
        {
            Hazard(gate ? "Enerji kapısı" : "İşaretli blok", 0, z, 7.2f, .7f, true, gate,
                "Kapalı Pulse hedefi: beş enerjiyle menzilde bırak.", gate ? VisualKind.EnergyGate : VisualKind.BreakableBlock);
        }
        private void Hazard(string label, float x, float z, float width, float depth, bool marked, bool gate,
            string reason, VisualKind visual)
        {
            hazards.Add(new HazardSpec { Label = label, Position = new Vector3(x, .65f, offset + z),
                Size = new Vector3(width, 1.3f, depth), Marked = marked, Gate = gate, Reason = reason, Visual = visual });
        }
        internal void Charge(float firstZ)
        {
            // Every particle needs a wide tether; all five arrive well before the release window.
            for (int i = 0; i < 5; i++) pickups.Add(new PickupSpec {
                Position = new Vector3(i % 2 == 0 ? -1.55f : 1.55f, .55f, offset + firstZ + i * 4), Bonus = false });
        }
        internal void Bonuses(float z, float half)
        {
            pickups.Add(new PickupSpec { Position = new Vector3(-half, .55f, offset + z), Bonus = true });
            pickups.Add(new PickupSpec { Position = new Vector3(half, .55f, offset + z), Bonus = true });
        }
        internal void WideMovingWall(float z)
        {
            float absoluteZ = offset + z;
            timed.Add(new TimedHazard { Kind = TimedHazardKind.MovingWall, Center = new Vector2(0, absoluteZ),
                HalfExtents = new Vector2(.65f, .45f), Amplitude = .65f, AngularSpeed = .8f,
                Phase = -.8f * (absoluteZ / 7f), Hint = "Hareketli duvar: basılı tut, geniş geç." });
        }
        internal void SideMovingWall(float z)
        {
            float absoluteZ = offset + z;
            timed.Add(new TimedHazard { Kind = TimedHazardKind.MovingWall, Center = new Vector2(0, absoluteZ),
                HalfExtents = new Vector2(.45f, .45f), Amplitude = 2.2f, AngularSpeed = .6f,
                Phase = Mathf.PI * .5f - .6f * (absoluteZ / 7f), Hint = "Duvar yana kayıyor: bırak, dar geç." });
        }
        internal void Arm(float z)
        {
            float absoluteZ = offset + z;
            timed.Add(new TimedHazard { Kind = TimedHazardKind.RotatingArm, Center = new Vector2(0, absoluteZ),
                HalfExtents = new Vector2(2.7f, .14f), AngularSpeed = .9f,
                Phase = Mathf.PI * .5f - .9f * (absoluteZ / 7f), Hint = "Dönen kol: basılı tut, iki yandan geç." });
        }
        internal CourseDefinition Build()
        {
            Course.Hazards = hazards.ToArray(); Course.Pickups = pickups.ToArray();
            Course.TimedHazards = timed.ToArray(); Course.Hints = hints.ToArray(); Course.SafeRoute = route.ToArray();
            return Course;
        }
    }
}
