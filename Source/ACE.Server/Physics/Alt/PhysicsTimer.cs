using System;

namespace ACE.Server.Physics.Alt
{
    /// <summary>
    /// Physics timing system ported from GDLE PhysicsTimer
    /// </summary>
    public static class PhysicsTimer
    {
        /// <summary>
        /// Current physics time (equivalent to GDLE PhysicsTimer::curr_time)
        /// </summary>
        public static double CurrentTime { get; private set; } = -1.0;

        /// <summary>
        /// Invalid time constant (equivalent to GDLE INVALID_TIME)
        /// </summary>
        public const double InvalidTime = -1.0;

        /// <summary>
        /// Minimum quantum for physics updates
        /// </summary>
        public const double MinQuantum = 1.0 / 30.0; // 30 FPS

        /// <summary>
        /// Huge quantum threshold
        /// </summary>
        public const double HugeQuantum = 1.0; // 1 second

        /// <summary>
        /// Update the current physics time
        /// </summary>
        public static void UpdateTime()
        {
            CurrentTime = GetCurrentTime();
        }

        /// <summary>
        /// Get current system time in seconds
        /// </summary>
        public static double GetCurrentTime()
        {
            return DateTime.UtcNow.Ticks / (double)TimeSpan.TicksPerSecond;
        }

        /// <summary>
        /// Check if time is valid
        /// </summary>
        public static bool IsValidTime(double time)
        {
            return time != InvalidTime && time >= 0.0;
        }

        /// <summary>
        /// Get time delta between two times
        /// </summary>
        public static double GetTimeDelta(double startTime, double endTime)
        {
            if (!IsValidTime(startTime) || !IsValidTime(endTime))
                return 0.0;

            return endTime - startTime;
        }

        /// <summary>
        /// Clamp time delta to reasonable bounds
        /// </summary>
        public static double ClampTimeDelta(double deltaTime)
        {
            if (deltaTime < 0.0)
                return 0.0;
            if (deltaTime > HugeQuantum)
                return HugeQuantum;
            return deltaTime;
        }

        /// <summary>
        /// Check if enough time has passed for an update
        /// </summary>
        public static bool ShouldUpdate(double lastUpdateTime, double currentTime)
        {
            var deltaTime = GetTimeDelta(lastUpdateTime, currentTime);
            return deltaTime >= MinQuantum;
        }

        /// <summary>
        /// Initialize the physics timer
        /// </summary>
        public static void Initialize()
        {
            CurrentTime = GetCurrentTime();
        }

        /// <summary>
        /// Reset the physics timer
        /// </summary>
        public static void Reset()
        {
            CurrentTime = InvalidTime;
        }
    }
} 