using System;
using System.Globalization;
using NUnit.Framework;
using TetherRush.Core;

namespace TetherRush.Tests
{
    public sealed class PlayerProgressTests
    {
        [Test]
        public void NewProgressHasTwentyFiveEmptyLevelsAndAudibleDefaults()
        {
            AssertDefaults(new PlayerProgress());
            Assert.That(PlayerProgress.LevelCount, Is.EqualTo(25));
        }

        [Test]
        public void LevelCompletionAndBonusNeverDecreaseAcrossRetries()
        {
            var data = new PlayerProgress();
            Assert.That(data.CompleteLevel(2, 0), Is.True);
            Assert.That(data.CompleteLevel(2, 0), Is.False);
            Assert.That(data.CompleteLevel(2, 7), Is.True);
            Assert.That(data.CompleteLevel(2, 3), Is.False);
            Assert.That(data.IsCompleted(2), Is.True);
            Assert.That(data.BestBonus(2), Is.EqualTo(7));
            Assert.That(data.IsCompleted(1), Is.False);
        }

        [Test]
        public void EndlessRecordAcceptsOnlyHigherScores()
        {
            var data = new PlayerProgress();
            Assert.That(data.RecordEndless(0), Is.False);
            Assert.That(data.RecordEndless(123), Is.True);
            Assert.That(data.RecordEndless(123), Is.False);
            Assert.That(data.RecordEndless(12), Is.False);
            Assert.That(data.RecordEndless(int.MaxValue), Is.True);
            Assert.That(PlayerProgress.FromJson(data.ToJson()).BestEndlessScore, Is.EqualTo(int.MaxValue));
        }

        [TestCase(-1)]
        [TestCase(25)]
        [TestCase(int.MaxValue)]
        public void EveryLevelApiRejectsInvalidIndex(int index)
        {
            var data = new PlayerProgress();
            Assert.Throws<ArgumentOutOfRangeException>(() => data.IsCompleted(index));
            Assert.Throws<ArgumentOutOfRangeException>(() => data.BestBonus(index));
            Assert.Throws<ArgumentOutOfRangeException>(() => data.CompleteLevel(index, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => data.IsUnlocked(index));
        }

        [Test]
        public void NegativeRecordsAreRejectedWithoutChangingProgress()
        {
            var data = new PlayerProgress();
            Assert.Throws<ArgumentOutOfRangeException>(() => data.CompleteLevel(0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => data.RecordEndless(-1));
            AssertDefaults(data);
        }

        [TestCase(-10f, 0f)]
        [TestCase(10f, 1f)]
        [TestCase(0.25f, 0.25f)]
        [TestCase(float.NaN, 0.55f)]
        [TestCase(float.PositiveInfinity, 0.55f)]
        [TestCase(float.NegativeInfinity, 0.55f)]
        public void VolumeAlwaysRemainsFiniteAndClamped(float value, float expected)
        {
            var data = new PlayerProgress { Volume = value };
            Assert.That(data.Volume, Is.EqualTo(expected));
        }

        [Test]
        public void RoundTripKeepsEveryFieldAndUsesInvariantNumbers()
        {
            CultureInfo previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
                var data = new PlayerProgress { AudioEnabled = false, Volume = 0.37f };
                for (int i = 0; i < PlayerProgress.LevelCount; i++) data.CompleteLevel(i, i + 2);
                data.RecordEndless(456);
                string json = data.ToJson();
                Assert.That(json, Does.Contain("\"volume\":0.37"));
                PlayerProgress restored = PlayerProgress.FromJson(json);
                Assert.That(restored.ToJson(), Is.EqualTo(json));
                Assert.That(restored.AudioEnabled, Is.False);
                Assert.That(restored.Volume, Is.EqualTo(0.37f));
                Assert.That(restored.BestEndlessScore, Is.EqualTo(456));
                for (int i = 0; i < PlayerProgress.LevelCount; i++)
                {
                    Assert.That(restored.IsCompleted(i), Is.True);
                    Assert.That(restored.BestBonus(i), Is.EqualTo(i + 2));
                }
            }
            finally { CultureInfo.CurrentCulture = previous; }
        }

        [Test]
        public void ParserAllowsFieldOrderAndJsonWhitespace()
        {
            string json = " { \n\"bestBonus\":[0,0,0,0,2], \"completed\":[false,false,false,false,true],"
                + "\"bestEndlessScore\":42,\"volume\":5.5e-1,\"audioEnabled\":false,\"version\":1 } \t";
            var data = PlayerProgress.FromJson(json);
            Assert.That(data.AudioEnabled, Is.False);
            Assert.That(data.Volume, Is.EqualTo(0.55f));
            Assert.That(data.BestEndlessScore, Is.EqualTo(42));
            Assert.That(data.BestBonus(4), Is.EqualTo(2));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("broken")]
        [TestCase("{}")]
        [TestCase("{\"version\":1}")]
        public void EmptyOrCorruptSaveReturnsDefaults(string json)
        {
            AssertDefaults(PlayerProgress.FromJson(json));
        }

        [TestCase("\"version\":2", "\"version\":3")]
        [TestCase("\"version\":2", "\"version\":0")]
        [TestCase("\"version\":2", "\"version\":2,\"version\":2")]
        [TestCase("\"version\":2", "\"unknown\":1")]
        [TestCase("\"bestEndlessScore\":42", "\"bestEndlessScore\":-1")]
        [TestCase("\"bestEndlessScore\":42", "\"bestEndlessScore\":2147483648")]
        [TestCase("\"volume\":0.55", "\"volume\":NaN")]
        [TestCase("\"volume\":0.55", "\"volume\":1e999")]
        [TestCase("\"volume\":0.55", "\"volume\":00.5")]
        [TestCase("\"completed\":[false,", "\"completed\":[")]
        [TestCase("\"bestBonus\":[0,", "\"bestBonus\":[-1,")]
        [TestCase("\"bestBonus\":[0,", "\"bestBonus\":[1,")]
        [TestCase("\"bestBonus\":[0,", "\"bestBonus\":[0,0,")]
        public void UnsupportedOrInvalidSaveIsRejectedAtomically(string from, string to)
        {
            var data = new PlayerProgress { AudioEnabled = false };
            data.RecordEndless(42);
            AssertDefaults(PlayerProgress.FromJson(data.ToJson().Replace(from, to)));
        }

        [Test]
        public void TrailingGarbageTruncationAndOversizedSaveReturnDefaults()
        {
            var data = new PlayerProgress { AudioEnabled = false };
            string json = data.ToJson();
            AssertDefaults(PlayerProgress.FromJson(json + "false"));
            for (int i = 0; i < json.Length; i++)
                AssertDefaults(PlayerProgress.FromJson(json.Substring(0, i)));
            AssertDefaults(PlayerProgress.FromJson(new string(' ', 4097)));
        }

        [TestCase(-2f, 0f)]
        [TestCase(2f, 1f)]
        public void ValidFiniteSavedVolumeIsClamped(float saved, float expected)
        {
            string json = new PlayerProgress().ToJson().Replace("\"volume\":0.55",
                "\"volume\":" + saved.ToString(CultureInfo.InvariantCulture));
            Assert.That(PlayerProgress.FromJson(json).Volume, Is.EqualTo(expected));
        }

        private static void AssertDefaults(PlayerProgress data)
        {
            Assert.That(data.AudioEnabled, Is.True);
            Assert.That(data.Volume, Is.EqualTo(0.55f));
            Assert.That(data.BestEndlessScore, Is.Zero);
            for (int i = 0; i < PlayerProgress.LevelCount; i++)
            {
                Assert.That(data.IsCompleted(i), Is.False);
                Assert.That(data.BestBonus(i), Is.Zero);
            }
        }
    }
}
