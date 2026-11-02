using System;
using System.Collections.Generic;
using TetherRush.Art;
using UnityEngine;

namespace TetherRush.Course
{
    public static class LevelCatalog
    {
        public static int Count => 25;

        public static CourseDefinition Get(int index)
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            var b = new CourseBuilder();
            switch (index)
            {
                case 0:
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
                case 4:
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
                default:
                    AuthorLaterLevel(b, index);
                    break;
            }
            b.Course.Id = "level-" + (index + 1);
            b.Course.Title = "Bölüm " + (index + 1);
            return b.Build();
        }

        // Independent timelines: N narrow, C center, W wide moving wall, S side moving wall,
        // R rotor, P Pulse gate, B marked block. Pulse beats reserve the preceding 50 metres.
        private static void AuthorLaterLevel(CourseBuilder b, int index)
        {
            Beat[] beats;
            switch (index)
            {
                case 5:  b.Course.Length = 124; beats = new[] { N(14), C(34), N(56), W(78), S(102) }; break;
                case 6:  b.Course.Length = 132; beats = new[] { C(18), N(42), C(64), R(86), N(112) }; break;
                case 7:  b.Course.Length = 144; beats = new[] { N(14), P(74), W(96), N(122) }; break;
                case 8:  b.Course.Length = 152; beats = new[] { W(20), S(44), R(68), N(92), C(116), N(138) }; break;
                case 9:  b.Course.Length = 160; beats = new[] { N(14), C(36), W(58), N(80), C(104), S(128), N(146) }; break;
                case 10: b.Course.Length = 168; beats = new[] { B(64), N(84), R(108), N(132), C(152) }; break;
                case 11: b.Course.Length = 176; beats = new[] { C(18), W(40), S(64), C(88), N(112), R(136), N(158) }; break;
                case 12: b.Course.Length = 184; beats = new[] { N(14), P(78), S(100), W(124), R(148), N(170) }; break;
                case 13: b.Course.Length = 192; beats = new[] { W(20), N(44), C(68), S(92), R(116), N(140), W(164), N(178) }; break;
                case 14: b.Course.Length = 200; beats = new[] { N(14), C(36), B(100), N(122), W(144), S(166), R(186) }; break;
                case 15: b.Course.Length = 208; beats = new[] { P(64), N(86), C(108), N(130), B(190) }; break;
                case 16: b.Course.Length = 216; beats = new[] { C(18), S(42), W(66), R(90), N(114), C(138), S(162), R(186), N(202) }; break;
                case 17: b.Course.Length = 224; beats = new[] { N(14), B(78), W(100), N(124), P(184), W(206) }; break;
                case 18: b.Course.Length = 232; beats = new[] { W(18), S(42), C(66), N(90), R(114), S(138), W(162), N(186), C(210) }; break;
                case 19: b.Course.Length = 240; beats = new[] { N(14), C(36), P(100), S(122), R(146), N(170), B(230) }; break;
                case 20: b.Course.Length = 248; beats = new[] { B(64), W(86), S(110), R(134), N(158), C(182), W(206), S(230) }; break;
                case 21: b.Course.Length = 256; beats = new[] { N(14), P(78), R(100), S(124), B(184), N(206), C(226), N(242) }; break;
                case 22: b.Course.Length = 264; beats = new[] { W(18), N(42), B(106), W(128), R(152), S(176), P(236), N(256) }; break;
                case 23: b.Course.Length = 272; beats = new[] { P(64), S(86), R(110), B(174), W(196), N(220), C(244), S(260) }; break;
                default: b.Course.Length = 280; beats = new[] { P(64), N(86), B(154), S(176), P(244), W(266) }; break;
            }

            b.Course.Description = "Dar ve geniş geçişlerin ritmini oku; iki küreyi birlikte koru.";
            b.Hint(0, "RİTMİ OKU • Basılı tutunca açıl, bırakınca daral.");
            bool held = false;
            foreach (var beat in beats)
            {
                if (beat.Kind == 'P' || beat.Kind == 'B')
                {
                    b.Course.UsesEnergy = true;
                    b.Course.Description = "Geçişleri zamanla; her işaretli hedef için beş yeni enerji topla ve menzilde bırak.";
                    if (!held) LaterCue(b, beat.Z - 50, true);
                    b.Center(beat.Z - 40);
                    b.Charge(beat.Z - 32);
                    b.Bonuses(beat.Z - 10, 2.2f);
                    b.PulseTarget(beat.Z, beat.Kind == 'P');
                    LaterCue(b, beat.Z - 4, false, true);
                    held = false;
                    continue;
                }

                bool wide = beat.Kind == 'C' || beat.Kind == 'W' || beat.Kind == 'R';
                if (wide != held) LaterCue(b, beat.Z - 10, wide);
                held = wide;
                switch (beat.Kind)
                {
                    case 'N': b.Narrow(beat.Z); break;
                    case 'C': b.Center(beat.Z); break;
                    case 'W': b.WideMovingWall(beat.Z); break;
                    case 'S': b.SideMovingWall(beat.Z); break;
                    case 'R': b.Arm(beat.Z); break;
                }
                b.Bonuses(beat.Z + 3, wide ? 2.2f : .65f);
            }
            // No held input is needed at the finish or carried into the next run.
            if (held) LaterCue(b, beats[beats.Length - 1].Z + 8, false);
        }

        private static void LaterCue(CourseBuilder b, float z, bool press, bool pulse = false)
        {
            b.Cue(z, press);
            b.Hint(z - 2, press ? "BASILI TUT • Geniş geç; enerjiyi bağla topla."
                : pulse ? "ŞİMDİ BIRAK • Dolu enerjiyle işaretli hedefi aç."
                : "BIRAK • Dar konuma dön.");
        }

        private readonly struct Beat
        {
            internal readonly char Kind;
            internal readonly float Z;
            internal Beat(char kind, float z) { Kind = kind; Z = z; }
        }
        private static Beat N(float z) => new Beat('N', z);
        private static Beat C(float z) => new Beat('C', z);
        private static Beat W(float z) => new Beat('W', z);
        private static Beat S(float z) => new Beat('S', z);
        private static Beat R(float z) => new Beat('R', z);
        private static Beat P(float z) => new Beat('P', z);
        private static Beat B(float z) => new Beat('B', z);
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
