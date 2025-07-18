using System;

namespace ACE.Server.Physics.Alt
{
    /// <summary>
    /// Physics global constants and settings ported from GDLE PhysicsGlobals
    /// </summary>
    public static class PhysicsGlobals
    {
        /// <summary>
        /// Floor Z coordinate (equivalent to GDLE PhysicsGlobals::floor_z)
        /// </summary>
        public static double FloorZ { get; set; } = Math.Cos(3437.746770784939);

        /// <summary>
        /// Ceiling Z coordinate (equivalent to GDLE PhysicsGlobals::ceiling_z)
        /// </summary>
        public static double CeilingZ { get; set; } = 1000.0;

        /// <summary>
        /// Gravity constant (equivalent to GDLE PhysicsGlobals::gravity)
        /// </summary>
        public static double Gravity { get; set; } = -9.8000002;

        /// <summary>
        /// Minimum quantum for physics updates
        /// </summary>
        public const double MinQuantum = 1.0 / 30.0; // 30 FPS

        /// <summary>
        /// Huge quantum threshold
        /// </summary>
        public const double HugeQuantum = 1.0; // 1 second

        /// <summary>
        /// Default friction coefficient
        /// </summary>
        public const float DefaultFriction = 0.95f;

        /// <summary>
        /// Default elasticity coefficient
        /// </summary>
        public const float DefaultElasticity = 0.05f;

        /// <summary>
        /// Default mass
        /// </summary>
        public const float DefaultMass = 1.0f;

        /// <summary>
        /// Default scale
        /// </summary>
        public const float DefaultScale = 1.0f;

        /// <summary>
        /// Maximum velocity
        /// </summary>
        public const float MaxVelocity = 50.0f;

        /// <summary>
        /// Walkable allowance (cosine of walkable angle)
        /// </summary>
        public const float WalkableAllowance = 0.7f;

        /// <summary>
        /// Step up height
        /// </summary>
        public const float StepUpHeight = 0.5f;

        /// <summary>
        /// Step down height
        /// </summary>
        public const float StepDownHeight = 0.5f;

        /// <summary>
        /// Epsilon for floating point comparisons
        /// </summary>
        public const float Epsilon = 0.001f;

        /// <summary>
        /// Large epsilon for physics calculations
        /// </summary>
        public const float LargeEpsilon = 0.0002f;

        /// <summary>
        /// Initialize physics globals
        /// </summary>
        public static void Initialize()
        {
            FloorZ = Math.Cos(3437.746770784939);
            CeilingZ = 1000.0;
            Gravity = -9.8000002;
        }

        /// <summary>
        /// Check if a normal is walkable
        /// </summary>
        public static bool IsWalkableNormal(System.Numerics.Vector3 normal)
        {
            return normal.Y > WalkableAllowance;
        }

        /// <summary>
        /// Apply gravity to velocity
        /// </summary>
        public static void ApplyGravity(ref System.Numerics.Vector3 velocity, float deltaTime)
        {
            velocity.Y += (float)Gravity * deltaTime;
        }

        /// <summary>
        /// Clamp velocity to maximum
        /// </summary>
        public static void ClampVelocity(ref System.Numerics.Vector3 velocity)
        {
            if (velocity.Length() > MaxVelocity)
            {
                velocity = System.Numerics.Vector3.Normalize(velocity) * MaxVelocity;
            }
        }

        /// <summary>
        /// Apply friction to velocity
        /// </summary>
        public static void ApplyFriction(ref System.Numerics.Vector3 velocity, float friction)
        {
            velocity *= friction;
        }

        /// <summary>
        /// Check if two floats are approximately equal
        /// </summary>
        public static bool ApproximatelyEqual(float a, float b)
        {
            return Math.Abs(a - b) < Epsilon;
        }

        /// <summary>
        /// Check if two floats are approximately equal with custom epsilon
        /// </summary>
        public static bool ApproximatelyEqual(float a, float b, float epsilon)
        {
            return Math.Abs(a - b) < epsilon;
        }

        /// <summary>
        /// Check if a float is approximately zero
        /// </summary>
        public static bool ApproximatelyZero(float value)
        {
            return Math.Abs(value) < Epsilon;
        }

        /// <summary>
        /// Check if a float is approximately zero with custom epsilon
        /// </summary>
        public static bool ApproximatelyZero(float value, float epsilon)
        {
            return Math.Abs(value) < epsilon;
        }
    }
} 