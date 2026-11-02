using NUnit.Framework;

namespace TetherRush.Tests
{
    public sealed class PlayerSaveStoreTests
    {
        // These tests deliberately never read or write the real progress PlayerPrefs key.
        [TestCase(0f, 0, 0)]
        [TestCase(12.9f, 3, 312)]
        [TestCase(-12f, -2, 0)]
        [TestCase(-12f, 2, 200)]
        [TestCase(12.9f, -2, 12)]
        [TestCase(float.NaN, 2, 200)]
        [TestCase(float.NegativeInfinity, 2, 200)]
        [TestCase(float.PositiveInfinity, 0, int.MaxValue)]
        [TestCase(float.MaxValue, 0, int.MaxValue)]
        [TestCase(0f, int.MaxValue, int.MaxValue)]
        [TestCase(float.MaxValue, int.MaxValue, int.MaxValue)]
        [TestCase(2147483520f, 1, 2147483620)]
        [TestCase(2147483520f, 2, int.MaxValue)]
        public void ScoreFloorsDistanceAndSaturatesWithoutOverflow(float distance, int bonus, int expected)
        {
            Assert.That(PlayerSaveStore.CalculateScore(distance, bonus), Is.EqualTo(expected));
        }
    }
}
