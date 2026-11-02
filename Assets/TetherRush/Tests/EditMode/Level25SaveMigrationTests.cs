using System;
using NUnit.Framework;
using TetherRush.Core;

namespace TetherRush.Tests
{
    public sealed class Level25SaveMigrationTests
    {
        // Literal installed five-level schema, deliberately not produced by the current writer.
        private const string LegacyJson = "{\"version\":1,\"audioEnabled\":false,\"volume\":0.23,"
            + "\"bestEndlessScore\":98765,\"completed\":[true,false,true,false,true],\"bestBonus\":[4,0,9,0,2]}";

        [Test]
        public void LegacyRecordExpandsAndRetainsEveryExistingValue()
        {
            PlayerProgress data = PlayerProgress.FromJson(LegacyJson);
            Assert.That(data.AudioEnabled, Is.False);
            Assert.That(data.Volume, Is.EqualTo(0.23f));
            Assert.That(data.BestEndlessScore, Is.EqualTo(98765));
            bool[] completed = { true, false, true, false, true };
            int[] bonus = { 4, 0, 9, 0, 2 };
            for (int i = 0; i < 5; i++)
            {
                Assert.That(data.IsCompleted(i), Is.EqualTo(completed[i]));
                Assert.That(data.BestBonus(i), Is.EqualTo(bonus[i]));
            }
            for (int i = 5; i < PlayerProgress.LevelCount; i++)
            {
                Assert.That(data.IsCompleted(i), Is.False);
                Assert.That(data.BestBonus(i), Is.Zero);
            }
            Assert.That(data.ToJson(), Does.StartWith("{\"version\":2,"));
            Assert.That(PlayerProgress.FromJson(data.ToJson()).ToJson(), Is.EqualTo(data.ToJson()));
        }

        [Test]
        public void LegacyNonSequentialCompletionsUnlockThroughHighestCompletedPlusNext()
        {
            PlayerProgress data = PlayerProgress.FromJson(LegacyJson);
            Assert.That(data.HighestUnlockedLevel, Is.EqualTo(5));
            for (int i = 0; i < PlayerProgress.LevelCount; i++)
                Assert.That(data.IsUnlocked(i), Is.EqualTo(i <= 5));
            Assert.That(data.IsCompleted(1), Is.False);
            Assert.That(data.IsCompleted(3), Is.False);
        }

        [Test]
        public void UnlockStateIsComputedWithoutRestrictingCompletionWrites()
        {
            var data = new PlayerProgress();
            Assert.That(data.HighestUnlockedLevel, Is.Zero);
            Assert.That(data.IsUnlocked(0), Is.True);
            Assert.That(data.IsUnlocked(1), Is.False);
            data.CompleteLevel(12, 3);
            Assert.That(data.HighestUnlockedLevel, Is.EqualTo(13));
            data.CompleteLevel(0, 0);
            Assert.That(data.HighestUnlockedLevel, Is.EqualTo(13));
            data.CompleteLevel(24, 7);
            Assert.That(data.HighestUnlockedLevel, Is.EqualTo(24));
            Assert.That(PlayerProgress.FromJson(data.ToJson()).HighestUnlockedLevel, Is.EqualTo(24));
        }

        [Test]
        public void AllTwentyFiveRecordsRoundTripAtMaximumValuesWithinSizeLimit()
        {
            var data = new PlayerProgress { AudioEnabled = false, Volume = 1f };
            for (int i = 0; i < PlayerProgress.LevelCount; i++) data.CompleteLevel(i, int.MaxValue);
            data.RecordEndless(int.MaxValue);
            string json = data.ToJson();
            Assert.That(json.Length, Is.LessThanOrEqualTo(4096));
            PlayerProgress restored = PlayerProgress.FromJson(json);
            Assert.That(restored.ToJson(), Is.EqualTo(json));
            for (int i = 0; i < PlayerProgress.LevelCount; i++)
            {
                Assert.That(restored.IsCompleted(i), Is.True);
                Assert.That(restored.BestBonus(i), Is.EqualTo(int.MaxValue));
                Assert.That(restored.IsUnlocked(i), Is.True);
                Assert.That(restored.CompleteLevel(i, 0), Is.False);
                Assert.That(restored.BestBonus(i), Is.EqualTo(int.MaxValue));
            }
            Assert.That(restored.RecordEndless(1), Is.False);
            Assert.That(restored.BestEndlessScore, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void MigratedProgressRemainsMonotonicWhenNewAndOldLevelsAreRetried()
        {
            PlayerProgress data = PlayerProgress.FromJson(LegacyJson);
            Assert.That(data.CompleteLevel(2, 1), Is.False);
            Assert.That(data.BestBonus(2), Is.EqualTo(9));
            Assert.That(data.RecordEndless(98764), Is.False);
            Assert.That(data.CompleteLevel(24, 8), Is.True);
            Assert.That(data.CompleteLevel(24, 2), Is.False);
            PlayerProgress restored = PlayerProgress.FromJson(data.ToJson());
            Assert.That(restored.BestBonus(2), Is.EqualTo(9));
            Assert.That(restored.BestBonus(24), Is.EqualTo(8));
            Assert.That(restored.BestEndlessScore, Is.EqualTo(98765));
            Assert.That(restored.AudioEnabled, Is.False);
            Assert.That(restored.Volume, Is.EqualTo(0.23f));
        }

        [TestCase("\"version\":1,", "")]
        [TestCase("\"audioEnabled\":false,", "")]
        [TestCase("\"volume\":0.23,", "")]
        [TestCase("\"bestEndlessScore\":98765,", "")]
        [TestCase("\"completed\":[true,false,true,false,true],", "")]
        [TestCase(",\"bestBonus\":[4,0,9,0,2]", "")]
        [TestCase("[true,false,true,false,true]", "[]")]
        [TestCase("[true,false,true,false,true]", "[true,false,true,false]")]
        [TestCase("[true,false,true,false,true]", "[true,false,true,false,true,false]")]
        [TestCase("[4,0,9,0,2]", "null")]
        [TestCase("[4,0,9,0,2]", "[]")]
        [TestCase("[4,0,9,0,2]", "[4,0,9,0]")]
        [TestCase("[4,0,9,0,2]", "[4,0,9,0,2,0]")]
        [TestCase("[4,0,9,0,2]", "[4,1,9,0,2]")]
        [TestCase("\"version\":1", "\"version\":2")]
        [TestCase("\"version\":1", "\"version\":3")]
        public void InvalidLegacyRecordResetsAtomically(string from, string to)
        {
            Assert.That(PlayerProgress.FromJson(LegacyJson.Replace(from, to)).ToJson(),
                Is.EqualTo(new PlayerProgress().ToJson()));
        }

        [TestCase(1, 25, 25)]
        [TestCase(1, 5, 25)]
        [TestCase(1, 25, 5)]
        [TestCase(2, 5, 5)]
        [TestCase(2, 24, 25)]
        [TestCase(2, 25, 24)]
        [TestCase(2, 26, 25)]
        [TestCase(2, 25, 26)]
        public void VersionRequiresExactArrayLengthsEvenWhenVersionAppearsLast(int version, int completed, int bonus)
        {
            string json = "{\"audioEnabled\":false,\"volume\":0.23,\"bestEndlessScore\":7,\"completed\":["
                + string.Join(",", Array.ConvertAll(new bool[completed], value => "false"))
                + "],\"bestBonus\":[" + string.Join(",", new int[bonus]) + "],\"version\":" + version + "}";
            Assert.That(PlayerProgress.FromJson(json).ToJson(), Is.EqualTo(new PlayerProgress().ToJson()));
        }

        [Test]
        public void CurrentRecordAcceptsReorderedFieldsAndVersionLast()
        {
            string json = "{\"bestBonus\":[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,6],"
                + "\"completed\":[false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,true],"
                + "\"bestEndlessScore\":42,\"volume\":0.25,\"audioEnabled\":false,\"version\":2}";
            PlayerProgress data = PlayerProgress.FromJson(json);
            Assert.That(data.AudioEnabled, Is.False);
            Assert.That(data.Volume, Is.EqualTo(0.25f));
            Assert.That(data.BestEndlessScore, Is.EqualTo(42));
            Assert.That(data.IsCompleted(24), Is.True);
            Assert.That(data.BestBonus(24), Is.EqualTo(6));
        }
    }
}
