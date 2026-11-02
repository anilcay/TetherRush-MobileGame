using System;
using System.Globalization;
using System.Text;

namespace TetherRush.Core
{
    public sealed class PlayerProgress
    {
        public const int LevelCount = 5;
        private const float DefaultVolume = 0.55f;
        private readonly bool[] completed = new bool[LevelCount];
        private readonly int[] bestBonus = new int[LevelCount];
        private float volume = DefaultVolume;

        public bool AudioEnabled { get; set; } = true;
        public float Volume
        {
            get => volume;
            set => volume = float.IsNaN(value) || float.IsInfinity(value)
                ? DefaultVolume : Math.Max(0f, Math.Min(1f, value));
        }
        public int BestEndlessScore { get; private set; }

        public bool IsCompleted(int zeroBasedLevel)
        {
            ValidateIndex(zeroBasedLevel);
            return completed[zeroBasedLevel];
        }

        public int BestBonus(int index)
        {
            ValidateIndex(index);
            return bestBonus[index];
        }

        public bool CompleteLevel(int index, int bonus)
        {
            ValidateIndex(index);
            if (bonus < 0) throw new ArgumentOutOfRangeException(nameof(bonus));
            bool changed = !completed[index] || bonus > bestBonus[index];
            completed[index] = true;
            bestBonus[index] = Math.Max(bestBonus[index], bonus);
            return changed;
        }

        public bool RecordEndless(int score)
        {
            if (score < 0) throw new ArgumentOutOfRangeException(nameof(score));
            if (score <= BestEndlessScore) return false;
            BestEndlessScore = score;
            return true;
        }

        public string ToJson()
        {
            var json = new StringBuilder("{\"version\":1,\"audioEnabled\":");
            json.Append(AudioEnabled ? "true" : "false");
            json.Append(",\"volume\":").Append(Volume.ToString("R", CultureInfo.InvariantCulture));
            json.Append(",\"bestEndlessScore\":").Append(BestEndlessScore.ToString(CultureInfo.InvariantCulture));
            json.Append(",\"completed\":[");
            for (int i = 0; i < LevelCount; i++)
            {
                if (i > 0) json.Append(',');
                json.Append(completed[i] ? "true" : "false");
            }
            json.Append("],\"bestBonus\":[");
            for (int i = 0; i < LevelCount; i++)
            {
                if (i > 0) json.Append(',');
                json.Append(bestBonus[i].ToString(CultureInfo.InvariantCulture));
            }
            return json.Append("]}").ToString();
        }

        // A bounded, schema-specific JSON reader keeps Core free of Unity and JSON packages.
        // Any incomplete/unknown/corrupt schema resets the entire record to safe defaults.
        public static PlayerProgress FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json.Length > 4096) return new PlayerProgress();
            try
            {
                var reader = new SaveReader(json);
                var result = new PlayerProgress();
                int seen = 0;
                reader.Expect('{');
                while (true)
                {
                    string key = reader.ReadKey();
                    reader.Expect(':');
                    int flag;
                    switch (key)
                    {
                        case "version":
                            flag = 1;
                            if (reader.ReadInt() != 1) throw new FormatException();
                            break;
                        case "audioEnabled": flag = 2; result.AudioEnabled = reader.ReadBool(); break;
                        case "volume": flag = 4; result.Volume = reader.ReadFloat(); break;
                        case "bestEndlessScore": flag = 8; result.BestEndlessScore = reader.ReadInt(); break;
                        case "completed":
                            flag = 16;
                            reader.Expect('[');
                            for (int i = 0; i < LevelCount; i++)
                            {
                                if (i > 0) reader.Expect(',');
                                result.completed[i] = reader.ReadBool();
                            }
                            reader.Expect(']');
                            break;
                        case "bestBonus":
                            flag = 32;
                            reader.Expect('[');
                            for (int i = 0; i < LevelCount; i++)
                            {
                                if (i > 0) reader.Expect(',');
                                result.bestBonus[i] = reader.ReadInt();
                            }
                            reader.Expect(']');
                            break;
                        default: throw new FormatException();
                    }
                    if ((seen & flag) != 0) throw new FormatException();
                    seen |= flag;
                    if (reader.Take('}')) break;
                    reader.Expect(',');
                }
                if (seen != 63 || !reader.AtEnd || result.BestEndlessScore < 0) throw new FormatException();
                for (int i = 0; i < LevelCount; i++)
                    if (result.bestBonus[i] < 0 || (!result.completed[i] && result.bestBonus[i] != 0))
                        throw new FormatException();
                return result;
            }
            catch (FormatException) { return new PlayerProgress(); }
            catch (OverflowException) { return new PlayerProgress(); }
        }

        private static void ValidateIndex(int index)
        {
            if (index < 0 || index >= LevelCount) throw new ArgumentOutOfRangeException(nameof(index));
        }

        private sealed class SaveReader
        {
            private readonly string source;
            private int position;
            public SaveReader(string source) { this.source = source; }
            public bool AtEnd { get { SkipSpace(); return position == source.Length; } }
            private void SkipSpace()
            {
                while (position < source.Length && (source[position] == ' ' || source[position] == '\t'
                    || source[position] == '\r' || source[position] == '\n')) position++;
            }
            public bool Take(char token)
            {
                SkipSpace();
                if (position >= source.Length || source[position] != token) return false;
                position++;
                return true;
            }
            public void Expect(char token) { if (!Take(token)) throw new FormatException(); }
            public string ReadKey()
            {
                Expect('"');
                int start = position;
                while (position < source.Length && source[position] != '"')
                {
                    // All schema keys are ASCII; escaped/unknown keys are rejected.
                    char c = source[position++];
                    if (c < ' ' || c == '\\') throw new FormatException();
                }
                if (position == source.Length) throw new FormatException();
                string key = source.Substring(start, position - start);
                position++;
                return key;
            }
            public bool ReadBool()
            {
                SkipSpace();
                if (Literal("true")) return true;
                if (Literal("false")) return false;
                throw new FormatException();
            }
            private bool Literal(string text)
            {
                if (position + text.Length > source.Length
                    || string.CompareOrdinal(source, position, text, 0, text.Length) != 0) return false;
                position += text.Length;
                return true;
            }
            public int ReadInt()
            {
                return int.Parse(ReadNumber(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            }
            public float ReadFloat()
            {
                float value = float.Parse(ReadNumber(), NumberStyles.Float, CultureInfo.InvariantCulture);
                if (float.IsNaN(value) || float.IsInfinity(value)) throw new FormatException();
                return value;
            }
            private bool Digit => position < source.Length && source[position] >= '0' && source[position] <= '9';
            private string ReadNumber()
            {
                SkipSpace();
                int start = position;
                if (position < source.Length && source[position] == '-') position++;
                if (!Digit) throw new FormatException();
                if (source[position] == '0') position++;
                else while (Digit) position++;
                if (position < source.Length && source[position] == '.')
                {
                    position++;
                    if (!Digit) throw new FormatException();
                    while (Digit) position++;
                }
                if (position < source.Length && (source[position] == 'e' || source[position] == 'E'))
                {
                    position++;
                    if (position < source.Length && (source[position] == '+' || source[position] == '-')) position++;
                    if (!Digit) throw new FormatException();
                    while (Digit) position++;
                }
                return source.Substring(start, position - start);
            }
        }
    }
}
