using System;
using System.Numerics;
using log4net;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Server.WorldObjects;
using ACE.Common;

namespace ACE.Server.Physics
{
    /// <summary>
    /// Manages the physics system and provides a unified interface
    /// </summary>
    public static class PhysicsSystemManager
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private static IPhysicsSystem _currentSystem;
        private static PhysicsSystemType _currentSystemType;

        /// <summary>
        /// Current physics system
        /// </summary>
        public static IPhysicsSystem CurrentSystem => _currentSystem;

        /// <summary>
        /// Current physics system type
        /// </summary>
        public static PhysicsSystemType CurrentSystemType => _currentSystemType;

        public enum PhysicsSystemType
        {
            ACE,
            GDLE
        }

        /// <summary>
        /// Initialize the physics system based on configuration
        /// </summary>
        public static void Initialize()
        {
            var configSystem = ConfigManager.Config.Server.PhysicsSystem ?? "ACE";
            
            switch (configSystem.ToUpperInvariant())
            {
                case "GDLE":
                    SetPhysicsSystem(PhysicsSystemType.GDLE);
                    break;
                case "ACE":
                default:
                    SetPhysicsSystem(PhysicsSystemType.ACE);
                    break;
            }
            
            log.Info($"Physics system initialized: {_currentSystemType}");
        }

        /// <summary>
        /// Set the physics system type
        /// </summary>
        public static void SetPhysicsSystem(PhysicsSystemType systemType)
        {
            _currentSystemType = systemType;
            
            switch (systemType)
            {
                case PhysicsSystemType.GDLE:
                    _currentSystem = new GDLEPhysicsAdapter();
                    break;
                case PhysicsSystemType.ACE:
                default:
                    _currentSystem = new ACEPhysicsAdapter();
                    break;
            }
            
            log.Info($"Physics system switched to: {systemType}");
        }

        // Factory methods for creating physics objects
        public static IPhysicsObject CreatePhysicsObject(uint setupId, ObjectGuid objectId, bool isDynamic)
        {
            return _currentSystem.CreatePhysicsObject(setupId, objectId, isDynamic);
        }

        public static IPhysicsObject CreatePhysicsObject(int? variationId)
        {
            return _currentSystem.CreatePhysicsObject(variationId);
        }

        public static IPhysicsObject CreateAnimObject(uint setupId, bool createParts)
        {
            return _currentSystem.CreateAnimObject(setupId, createParts);
        }

        public static IPhysicsObject CreateParticleObject(int numParts, Sphere sortingSphere, int? variationId)
        {
            return _currentSystem.CreateParticleObject(numParts, sortingSphere, variationId);
        }

        // World management methods
        public static bool EnterWorld(IPhysicsObject obj, Position position)
        {
            return _currentSystem.EnterWorld(obj, position);
        }

        public static void LeaveWorld(IPhysicsObject obj)
        {
            _currentSystem.LeaveWorld(obj);
        }

        public static void DestroyObject(IPhysicsObject obj)
        {
            _currentSystem.DestroyObject(obj);
        }

        // State management methods
        public static bool IsActive(IPhysicsObject obj)
        {
            return _currentSystem.IsActive(obj);
        }

        public static void SetActive(IPhysicsObject obj, bool active)
        {
            _currentSystem.SetActive(obj, active);
        }

        public static PhysicsState GetState(IPhysicsObject obj)
        {
            return _currentSystem.GetState(obj);
        }

        public static void SetState(IPhysicsObject obj, PhysicsState state)
        {
            _currentSystem.SetState(obj, state);
        }

        // Position and movement methods
        public static Position GetPosition(IPhysicsObject obj)
        {
            return _currentSystem.GetPosition(obj);
        }

        public static void SetPosition(IPhysicsObject obj, Position position)
        {
            _currentSystem.SetPosition(obj, position);
        }

        public static Vector3 GetVelocity(IPhysicsObject obj)
        {
            return _currentSystem.GetVelocity(obj);
        }

        public static void SetVelocity(IPhysicsObject obj, Vector3 velocity)
        {
            _currentSystem.SetVelocity(obj, velocity);
        }

        public static Vector3 GetAcceleration(IPhysicsObject obj)
        {
            return _currentSystem.GetAcceleration(obj);
        }

        public static void SetAcceleration(IPhysicsObject obj, Vector3 acceleration)
        {
            _currentSystem.SetAcceleration(obj, acceleration);
        }

        // Animation and movement methods
        public static bool IsAnimating(IPhysicsObject obj)
        {
            return _currentSystem.IsAnimating(obj);
        }

        public static bool IsMovingOrAnimating(IPhysicsObject obj)
        {
            return _currentSystem.IsMovingOrAnimating(obj);
        }

        public static void SetMotionTableId(IPhysicsObject obj, uint motionTableId)
        {
            _currentSystem.SetMotionTableId(obj, motionTableId);
        }

        public static void SetScale(IPhysicsObject obj, float scale)
        {
            _currentSystem.SetScale(obj, scale);
        }

        // Update methods
        public static bool UpdateObject(IPhysicsObject obj)
        {
            return _currentSystem.UpdateObject(obj);
        }

        public static void UpdateObjectInternal(IPhysicsObject obj, float quantum)
        {
            _currentSystem.UpdateObjectInternal(obj, quantum);
        }

        // Collision methods
        public static bool HasCollision(IPhysicsObject obj)
        {
            return _currentSystem.HasCollision(obj);
        }

        public static void ReportCollision(IPhysicsObject obj, IPhysicsObject target)
        {
            _currentSystem.ReportCollision(obj, target);
        }

        public static void ReportCollisionEnd(IPhysicsObject obj, IPhysicsObject target)
        {
            _currentSystem.ReportCollisionEnd(obj, target);
        }

        // Object management methods
        public static void SetObjectGuid(IPhysicsObject obj, ObjectGuid guid)
        {
            _currentSystem.SetObjectGuid(obj, guid);
        }

        public static void SetWeenieObject(IPhysicsObject obj, WorldObject worldObject)
        {
            _currentSystem.SetWeenieObject(obj, worldObject);
        }

        // Type checking methods
        public static bool IsPlayer(IPhysicsObject obj)
        {
            return _currentSystem.IsPlayer(obj);
        }

        public static bool IsStatic(IPhysicsObject obj)
        {
            return _currentSystem.IsStatic(obj);
        }

        public static bool IsMissile(IPhysicsObject obj)
        {
            return _currentSystem.IsMissile(obj);
        }

        public static bool IsEthereal(IPhysicsObject obj)
        {
            return _currentSystem.IsEthereal(obj);
        }
    }
} 