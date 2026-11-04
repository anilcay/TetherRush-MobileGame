using System;
using System.Collections.Generic;

namespace TetherRush.Core
{
    public enum RunPhase { Ready, Running, Paused, Failed, Completed }

    public sealed class RunModel
    {
        private readonly HashSet<int> energyCollectibles = new HashSet<int>();
        private readonly HashSet<int> bonusCollectibles = new HashSet<int>();
        private bool releaseArmed;

        public RunTuning Tuning { get; }
        public RunPhase Phase { get; private set; }
        public float Distance { get; private set; }
        public float ActualElapsedTime { get; private set; }
        public float HalfSeparation { get; private set; }
        public int Energy { get; private set; }
        public int Bonus { get; private set; }
        public int PulseCount { get; private set; }
        public bool IsHeld { get; private set; }
        public string FailureReason { get; private set; }

        public RunModel(RunTuning tuning)
        {
            Tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
            Tuning.Validate();
            Reset();
        }

        public void StartRun()
        {
            Reset();
            Phase = RunPhase.Running;
        }

        public void Reset()
        {
            Tuning.Validate();
            Phase = RunPhase.Ready;
            Distance = 0f;
            ActualElapsedTime = 0f;
            HalfSeparation = Tuning.MinHalfSeparation;
            Energy = 0;
            Bonus = 0;
            PulseCount = 0;
            FailureReason = string.Empty;
            energyCollectibles.Clear();
            bonusCollectibles.Clear();
            CancelInput();
        }

        public void Press()
        {
            if (Phase != RunPhase.Running) return;
            IsHeld = true;
            releaseArmed = true;
        }

        public bool Release()
        {
            if (Phase != RunPhase.Running || !IsHeld || !releaseArmed) return false;
            CancelInput();
            if (Energy != Tuning.EnergyCapacity) return false;
            Energy = 0;
            PulseCount++;
            return true;
        }

        public void CancelInput()
        {
            IsHeld = false;
            releaseArmed = false;
        }

        public void Tick(float dt)
        {
            Tick(dt, Tuning.ForwardSpeed);
        }

        /// <summary>Advance with an explicit forward speed, leaving separation and input unchanged.</summary>
        public void Tick(float dt, float forwardSpeed)
        {
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt < 0f)
                throw new ArgumentOutOfRangeException(nameof(dt), "Delta time must be finite and nonnegative.");
            if (float.IsNaN(forwardSpeed) || float.IsInfinity(forwardSpeed) || forwardSpeed <= 0f)
                throw new ArgumentOutOfRangeException(nameof(forwardSpeed), "Forward speed must be finite and positive.");
            if (Phase != RunPhase.Running) return;
            float target = IsHeld ? Tuning.MaxHalfSeparation : Tuning.MinHalfSeparation;
            float step = Tuning.SeparationSpeed * dt;
            HalfSeparation = HalfSeparation < target
                ? Math.Min(HalfSeparation + step, target)
                : Math.Max(HalfSeparation - step, target);
            Distance += forwardSpeed * dt;
            ActualElapsedTime += dt;
        }

        public bool CollectEnergy(int collectibleId, int amount = 1)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (Phase != RunPhase.Running || !energyCollectibles.Add(collectibleId)) return false;
            Energy = (int)Math.Min((long)Energy + amount, Tuning.EnergyCapacity);
            return true;
        }

        public bool CollectBonus(int collectibleId, int amount = 1)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (Phase != RunPhase.Running || !bonusCollectibles.Add(collectibleId)) return false;
            Bonus = (int)Math.Min((long)Bonus + amount, int.MaxValue);
            return true;
        }

        /// <summary>Retire a streamed object only after it has permanently left the course window.</summary>
        public void RetireCollectible(int collectibleId)
        {
            energyCollectibles.Remove(collectibleId); bonusCollectibles.Remove(collectibleId);
        }
        public int TrackedCollectibleCount => energyCollectibles.Count + bonusCollectibles.Count;

        public void Fail(string reason)
        {
            if (Phase != RunPhase.Running) return;
            FailureReason = reason ?? string.Empty;
            Phase = RunPhase.Failed;
            CancelInput();
        }

        public void Complete()
        {
            if (Phase != RunPhase.Running) return;
            Phase = RunPhase.Completed;
            CancelInput();
        }

        public void Pause()
        {
            if (Phase != RunPhase.Running) return;
            Phase = RunPhase.Paused;
            CancelInput();
        }

        public void Resume()
        {
            if (Phase != RunPhase.Paused) return;
            CancelInput();
            Phase = RunPhase.Running;
        }
    }
}
