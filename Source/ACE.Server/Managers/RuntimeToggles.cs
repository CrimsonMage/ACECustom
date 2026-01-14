using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using ACE.Database;
using ACE.Database.Models.Shard;

namespace ACE.Server.Managers
{
    /// <summary>
    /// Runtime toggles for server features that can be changed during gameplay.
    /// Values may be persisted to the database to survive server restarts.
    /// 
    /// IMPORTANT:
    /// These toggles are read only at enqueue boundaries.
    /// Disabling a toggle affects new work only.
    /// In-flight cooperative operations are allowed to complete.
    /// 
    /// For example, if CooperativeLoginEnabled is disabled while logins are mid-flight:
    /// - New login attempts will use the legacy path
    /// - In-flight cooperative login phases will continue to completion
    /// - This prevents partial state corruption from mid-execution toggle changes
    /// </summary>
    public static class RuntimeToggles
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string CooperativeLoginEnabledKey = "runtime_toggle_cooperative_login_enabled";
        private const string CooperativeLandblockLoadEnabledKey = "runtime_toggle_cooperative_landblock_load_enabled";

        // Initialization state: 0 = not started, 1 = in progress, 2 = completed
        private static int _initState = 0;
        private static volatile bool _initialized = false;
        private static volatile bool _cooperativeLoginEnabled = false;
        private static volatile bool _cooperativeLandblockLoadEnabled = false;

        /// <summary>
        /// Gets whether cooperative login is enabled.
        /// IMPORTANT: This value is read only at enqueue boundaries (e.g., when PlayerEnterWorldInternal decides which path to use).
        /// Changing this toggle affects new work only; in-flight cooperative login phases continue to completion.
        /// </summary>
        public static bool CooperativeLoginEnabled
        {
            get
            {
                EnsureInitialized();
                return _cooperativeLoginEnabled;
            }
            private set => _cooperativeLoginEnabled = value;
        }

        /// <summary>
        /// Gets whether cooperative landblock loading is enabled.
        /// IMPORTANT: This value is read only at enqueue boundaries.
        /// Changing this toggle affects new work only; in-flight cooperative landblock loads continue to completion.
        /// </summary>
        public static bool CooperativeLandblockLoadEnabled
        {
            get
            {
                EnsureInitialized();
                return _cooperativeLandblockLoadEnabled;
            }
            private set => _cooperativeLandblockLoadEnabled = value;
        }

        /// <summary>
        /// Ensures RuntimeToggles has been initialized. Throws if accessed before initialization.
        /// </summary>
        private static void EnsureInitialized()
        {
            if (!_initialized)
                throw new InvalidOperationException("RuntimeToggles used before Initialize()");
        }

        /// <summary>
        /// Initializes runtime toggles by loading values from the database.
        /// Must be called during server startup before any code accesses the toggles.
        /// Thread-safe: concurrent calls are safe; only one thread will perform initialization.
        /// </summary>
        public static void Initialize()
        {
            // Atomically claim initialization: only one thread can change from 0 to 1
            if (Interlocked.CompareExchange(ref _initState, 1, 0) != 0)
            {
                // Another thread is initializing or already initialized
                log.Warn("[RUNTIME TOGGLE] RuntimeToggles.Initialize() called multiple times - ignoring duplicate call");
                return;
            }

            LoadFromDatabase();
            _initialized = true;
            Interlocked.Exchange(ref _initState, 2);
            log.Info($"RuntimeToggles initialized - CooperativeLogin: {_cooperativeLoginEnabled}, CooperativeLandblockLoad: {_cooperativeLandblockLoadEnabled}");
        }

        /// <summary>
        /// Loads runtime toggle values from the database.
        /// </summary>
        private static void LoadFromDatabase()
        {
            var loginToggle = DatabaseManager.ShardConfig.GetBool(CooperativeLoginEnabledKey);
            if (loginToggle != null)
            {
                CooperativeLoginEnabled = loginToggle.Value;
            }
            else
            {
                // Initialize with default value if not in database
                DatabaseManager.ShardConfig.AddBool(CooperativeLoginEnabledKey, false, "Enables cooperative login path that breaks PlayerEnterWorld into yieldable phases");
                CooperativeLoginEnabled = false;
            }

            var landblockToggle = DatabaseManager.ShardConfig.GetBool(CooperativeLandblockLoadEnabledKey);
            if (landblockToggle != null)
            {
                CooperativeLandblockLoadEnabled = landblockToggle.Value;
            }
            else
            {
                // Initialize with default value if not in database
                DatabaseManager.ShardConfig.AddBool(CooperativeLandblockLoadEnabledKey, false, "Enables cooperative landblock loading");
                CooperativeLandblockLoadEnabled = false;
            }
        }

        /// <summary>
        /// Sets the CooperativeLoginEnabled toggle and persists to database.
        /// Database persistence runs asynchronously off the world thread to avoid blocking.
        /// </summary>
        public static bool SetCooperativeLoginEnabled(bool value, bool persistToDatabase = true)
        {
            var oldValue = CooperativeLoginEnabled;
            if (oldValue == value)
                return true;

            CooperativeLoginEnabled = value;
            log.Info($"[RUNTIME TOGGLE] CooperativeLoginEnabled changed from {oldValue} to {value}");

            if (persistToDatabase)
            {
                // Persist to database asynchronously off the world thread
                Task.Run(() =>
                {
                    try
                    {
                        if (DatabaseManager.ShardConfig.BoolExists(CooperativeLoginEnabledKey))
                        {
                            DatabaseManager.ShardConfig.SaveBool(new ConfigPropertiesBoolean
                            {
                                Key = CooperativeLoginEnabledKey,
                                Value = value,
                                Description = "Enables cooperative login path that breaks PlayerEnterWorld into yieldable phases"
                            });
                        }
                        else
                        {
                            DatabaseManager.ShardConfig.AddBool(CooperativeLoginEnabledKey, value, "Enables cooperative login path that breaks PlayerEnterWorld into yieldable phases");
                        }
                        log.Debug($"[RUNTIME TOGGLE] CooperativeLoginEnabled persisted to database: {value}");
                    }
                    catch (Exception ex)
                    {
                        log.Error($"[RUNTIME TOGGLE] Failed to persist CooperativeLoginEnabled to database: {ex.Message}", ex);
                    }
                });
            }

            return true;
        }

        /// <summary>
        /// Sets the CooperativeLandblockLoadEnabled toggle and persists to database.
        /// Database persistence runs asynchronously off the world thread to avoid blocking.
        /// </summary>
        public static bool SetCooperativeLandblockLoadEnabled(bool value, bool persistToDatabase = true)
        {
            var oldValue = CooperativeLandblockLoadEnabled;
            if (oldValue == value)
                return true;

            CooperativeLandblockLoadEnabled = value;
            log.Info($"[RUNTIME TOGGLE] CooperativeLandblockLoadEnabled changed from {oldValue} to {value}");

            if (persistToDatabase)
            {
                // Persist to database asynchronously off the world thread
                Task.Run(() =>
                {
                    try
                    {
                        if (DatabaseManager.ShardConfig.BoolExists(CooperativeLandblockLoadEnabledKey))
                        {
                            DatabaseManager.ShardConfig.SaveBool(new ConfigPropertiesBoolean
                            {
                                Key = CooperativeLandblockLoadEnabledKey,
                                Value = value,
                                Description = "Enables cooperative landblock loading"
                            });
                        }
                        else
                        {
                            DatabaseManager.ShardConfig.AddBool(CooperativeLandblockLoadEnabledKey, value, "Enables cooperative landblock loading");
                        }
                        log.Debug($"[RUNTIME TOGGLE] CooperativeLandblockLoadEnabled persisted to database: {value}");
                    }
                    catch (Exception ex)
                    {
                        log.Error($"[RUNTIME TOGGLE] Failed to persist CooperativeLandblockLoadEnabled to database: {ex.Message}", ex);
                    }
                });
            }

            return true;
        }
    }
}
