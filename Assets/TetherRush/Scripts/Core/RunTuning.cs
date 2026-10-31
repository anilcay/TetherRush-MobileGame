using System;

namespace TetherRush.Core
{
    [Serializable]
    public sealed class RunTuning
    {
        public float ForwardSpeed = 7f;
        public float MinHalfSeparation = .65f;
        public float MaxHalfSeparation = 2.2f;
        public float SeparationSpeed = 6f;
        public float SphereRadius = .38f;
        public float PulseRadius = 5f;
        public int EnergyCapacity = 5;

        internal void Validate()
        {
            RequirePositive(ForwardSpeed, nameof(ForwardSpeed));
            RequirePositive(MinHalfSeparation, nameof(MinHalfSeparation));
            RequirePositive(MaxHalfSeparation, nameof(MaxHalfSeparation));
            RequirePositive(SeparationSpeed, nameof(SeparationSpeed));
            RequirePositive(SphereRadius, nameof(SphereRadius));
            RequirePositive(PulseRadius, nameof(PulseRadius));
            if (MaxHalfSeparation < MinHalfSeparation)
                throw new ArgumentException("Maximum separation must be at least the minimum separation.");
            if (EnergyCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(EnergyCapacity));
        }

        private static void RequirePositive(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                throw new ArgumentOutOfRangeException(name, "Value must be finite and positive.");
        }
    }
}
