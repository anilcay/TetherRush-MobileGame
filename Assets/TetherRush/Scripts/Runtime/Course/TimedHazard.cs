using System;
using UnityEngine;

namespace TetherRush.Course
{
    public enum TimedHazardKind { MovingWall, RotatingArm }

    /// <summary>Authoritative X/Z geometry. Time is run distance / forward speed, never wall clock time.</summary>
    public sealed class TimedHazard
    {
        public TimedHazardKind Kind;
        public Vector2 Center;
        public Vector2 HalfExtents;
        public float Amplitude, AngularSpeed, Phase;
        public string Hint;
        // A reported near miss can be at most this far from true contact, in world units.
        public const double ContactPrecision = 0.0005;

        public Vector2 PositionAt(float time)
        {
            Validate(time);
            double x, z;
            Position(time, out x, out z);
            return new Vector2((float)x, (float)z);
        }

        /// <summary>Radians, standard counterclockwise rotation in the (X,Z) plane.</summary>
        public float AngleAt(float time)
        {
            Validate(time);
            return Kind == TimedHazardKind.RotatingArm ? (float)Angle(time) : 0f;
        }

        /// <summary>
        /// Sweeps a circle along a linear segment against the actual moving OBB, including touching.
        /// Intervals are rejected only when signed clearance exceeds a bound on all relative motion.
        /// Ambiguous intervals subdivide until that bound is at most ContactPrecision; no sample-rate
        /// or iteration cap can silently skip a contact. Zero duration still sweeps the spatial segment.
        /// </summary>
        public bool HitsSweptSphere(Vector2 from, Vector2 to, float sphereRadius, float startTime, float endTime)
        {
            Validate(startTime);
            Validate(endTime);
            if (endTime < startTime) throw new ArgumentOutOfRangeException(nameof(endTime));
            if (!Finite(from.x) || !Finite(from.y) || !Finite(to.x) || !Finite(to.y))
                throw new ArgumentException("Sweep endpoints must be finite.");
            if (!Finite(sphereRadius) || sphereRadius < 0) throw new ArgumentOutOfRangeException(nameof(sphereRadius));

            double dx = (double)to.x - from.x, dz = (double)to.y - from.y;
            double duration = (double)endTime - startTime;
            double radius = Math.Sqrt((double)HalfExtents.x * HalfExtents.x + (double)HalfExtents.y * HalfExtents.y);
            double rigidSpeed = Math.Abs((double)AngularSpeed) *
                (Kind == TimedHazardKind.MovingWall ? Amplitude : radius);
            double travelBound = Math.Sqrt(dx * dx + dz * dz) + rigidSpeed * duration;
            if (Clearance(0, from, dx, dz, sphereRadius, startTime, duration) <= 0 ||
                Clearance(1, from, dx, dz, sphereRadius, startTime, duration) <= 0) return true;
            return Search(0, 1, from, dx, dz, sphereRadius, startTime, duration, travelBound);
        }

        private bool Search(double a, double b, Vector2 from, double dx, double dz,
            double radius, double start, double duration, double travelBound)
        {
            double middle = (a + b) * .5;
            double clearance = Clearance(middle, from, dx, dz, radius, start, duration);
            if (clearance <= 0) return true;
            double bound = travelBound * (b - a) * .5;
            if (clearance > bound) return false;
            if (bound <= ContactPrecision) return true;
            return Search(a, middle, from, dx, dz, radius, start, duration, travelBound) ||
                   Search(middle, b, from, dx, dz, radius, start, duration, travelBound);
        }

        private double Clearance(double u, Vector2 from, double dx, double dz,
            double radius, double start, double duration)
        {
            double time = start + duration * u;
            double cx, cz;
            Position(time, out cx, out cz);
            double x = from.x + dx * u - cx, z = from.y + dz * u - cz;
            double angle = Kind == TimedHazardKind.RotatingArm ? Angle(time) : 0;
            double cos = Math.Cos(angle), sin = Math.Sin(angle);
            // Inverse OBB transform, then exact rounded rectangle/circle distance.
            double qx = Math.Abs(cos * x + sin * z) - HalfExtents.x;
            double qz = Math.Abs(-sin * x + cos * z) - HalfExtents.y;
            double ox = Math.Max(qx, 0), oz = Math.Max(qz, 0);
            return Math.Sqrt(ox * ox + oz * oz) + Math.Min(Math.Max(qx, qz), 0) - radius;
        }

        private double Angle(double time) { return (double)Phase + AngularSpeed * time; }
        private void Position(double time, out double x, out double z)
        {
            x = Center.x;
            z = Center.y;
            if (Kind == TimedHazardKind.MovingWall) x += Amplitude * Math.Sin(Angle(time));
        }

        private void Validate(float time)
        {
            if (!Finite(time) || time < 0) throw new ArgumentOutOfRangeException(nameof(time));
            if (Kind != TimedHazardKind.MovingWall && Kind != TimedHazardKind.RotatingArm)
                throw new ArgumentOutOfRangeException(nameof(Kind));
            if (!Finite(Center.x) || !Finite(Center.y) || !Finite(HalfExtents.x) || !Finite(HalfExtents.y) ||
                HalfExtents.x <= 0 || HalfExtents.y <= 0 || !Finite(Amplitude) || Amplitude < 0 ||
                !Finite(AngularSpeed) || !Finite(Phase))
                throw new ArgumentException("Hazard geometry and motion must be finite; extents positive, amplitude nonnegative.");
        }

        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }
}
