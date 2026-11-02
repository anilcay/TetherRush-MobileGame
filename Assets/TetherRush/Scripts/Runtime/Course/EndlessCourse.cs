using System;

namespace TetherRush.Course
{
    /// <summary>Five authored, composable patterns; no speed escalation or previous-charge requirement.</summary>
    public static class EndlessCourse
    {
        public const float ChunkLength = 112f;

        public static CourseDefinition CreateChunk(int index, int seed = 1)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            // Hash uses defined unsigned overflow, independent of UnityEngine.Random's global state.
            uint hash = Mix(unchecked((uint)seed));
            int pattern = (int)(((uint)index + hash % 5u) % 5u);
            float jitter = (Mix(unchecked((uint)index) ^ hash) % 5u) - 2f;
            var b = new CourseBuilder(index * ChunkLength + jitter);
            b.Course.Id = "endless-" + seed + "-" + index;
            b.Course.Title = "Sonsuz Akış";
            b.Course.Description = "Sabit tempoda ilerle. İki küreyi koru, bonuslarla skorunu artır.";
            b.Course.Length = ChunkLength;
            switch (pattern)
            {
                case 0:
                    b.Narrow(16); b.Center(36); b.Narrow(58); b.Center(78); b.Narrow(98);
                    b.Bonuses(42, 2.2f); b.Bonuses(84, 2.2f);
                    b.Cue(26, true); b.Cue(46, false); b.Cue(68, true); b.Cue(88, false);
                    b.Hint(6, "RİTİM • Dar geçitte bırak, orta bariyerde basılı tut.");
                    break;
                case 1:
                    b.Course.UsesEnergy = true;
                    b.Center(20); b.Charge(28); b.PulseTarget(60, true); b.Narrow(76); b.Center(94);
                    b.Bonuses(48, 2.2f);
                    b.Cue(10, true); b.Cue(56, false); b.Cue(84, true); b.Cue(102, false);
                    b.Hint(6, "BASILI TUT • Beş enerji topla; kapıya kadar sakla.");
                    b.Hint(55, "ŞİMDİ BIRAK • Pulse kapısı menzilde.");
                    b.Hint(82, "BASILI TUT • Son orta bariyer.");
                    break;
                case 2:
                    b.WideMovingWall(24); b.SideMovingWall(48); b.Arm(78); b.Narrow(100);
                    b.Bonuses(30, 2.2f); b.Bonuses(54, .65f); b.Bonuses(84, 2.2f);
                    b.Cue(14, true); b.Cue(36, false); b.Cue(66, true); b.Cue(88, false);
                    b.Hint(6, "BASILI TUT • Hareketli duvarı geniş geç.");
                    b.Hint(34, "BIRAK • Yana kayan duvarı dar geç.");
                    b.Hint(64, "BASILI TUT • Dönen kolun iki yanından geç.");
                    break;
                case 3:
                    b.Narrow(16); b.Center(36); b.WideMovingWall(56); b.Narrow(76); b.Center(94);
                    b.Bonuses(44, 2.2f); b.Bonuses(62, 2.2f);
                    b.Cue(26, true); b.Cue(66, false); b.Cue(84, true); b.Cue(102, false);
                    b.Hint(6, "AKIŞ • Orta bariyerden sonra hareketli duvarı da geniş geç.");
                    b.Hint(64, "BIRAK • Dar geçitten sonra yeniden basılı tut.");
                    break;
                default:
                    b.Course.UsesEnergy = true;
                    b.Center(18); b.Charge(26); b.PulseTarget(58, false); b.Narrow(74); b.Center(94);
                    b.Bonuses(46, 2.2f);
                    b.Cue(8, true); b.Cue(54, false); b.Cue(84, true); b.Cue(102, false);
                    b.Hint(6, "BASILI TUT • Beş enerji, ardından işaretli blok.");
                    b.Hint(53, "ŞİMDİ BIRAK • İşaretli bloğu Pulse ile kır.");
                    b.Hint(82, "BASILI TUT • Son orta bariyer.");
                    break;
            }
            return b.Build();
        }

        private static uint Mix(uint value)
        {
            unchecked { value ^= value >> 16; value *= 0x7feb352du; value ^= value >> 15;
                value *= 0x846ca68bu; return value ^ (value >> 16); }
        }
    }
}
