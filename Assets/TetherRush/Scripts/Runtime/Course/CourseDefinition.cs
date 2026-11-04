using System;
using TetherRush.Art;
using UnityEngine;

namespace TetherRush.Course
{
    /// <summary>Fresh mutable data per run. Distances are world Z; chunk Length is its span, not its end Z.</summary>
    public sealed class CourseDefinition
    {
        public string Id = "", Title = "", Description = "";
        public float Length;
        public bool UsesEnergy;
        public HazardSpec[] Hazards = Array.Empty<HazardSpec>();
        public PickupSpec[] Pickups = Array.Empty<PickupSpec>();
        public TimedHazard[] TimedHazards = Array.Empty<TimedHazard>();
        public RelaySpec[] Relays = Array.Empty<RelaySpec>();
        public HintSpec[] Hints = Array.Empty<HintSpec>();
        // Reference route, not autoplay. Each cue is a real press/release event.
        public InputCue[] SafeRoute = Array.Empty<InputCue>();
    }

    public sealed class HazardSpec
    {
        public string Label = "", Reason = "";
        public Vector3 Position, Size;
        public bool Marked, Gate;
        public int RelayId = -1;
        public VisualKind Visual;
    }

    public sealed class RelaySpec
    {
        public int Id;
        public Vector3 Position;
        public float RequiredHalfSpan = 1.5f;
    }

    public sealed class PickupSpec
    {
        public Vector3 Position;
        public bool Bonus;
    }

    public struct HintSpec { public float Distance; public string Text; }
    public struct InputCue { public float Distance; public bool Press; }
}
