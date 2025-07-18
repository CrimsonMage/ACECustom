using System.Numerics;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Server.WorldObjects;

namespace ACE.Server.Physics
{
    /// <summary>
    /// Interface for physics system implementations
    /// Defines the contract that both ACE and GDLE physics systems must implement
    /// </summary>
    public interface IPhysicsSystem
    {
        /// <summary>
        /// Create a physics object with setup ID and object ID
        /// </summary>
        IPhysicsObject CreatePhysicsObject(uint setupId, ObjectGuid objectId, bool isDynamic);

        /// <summary>
        /// Create a physics object with variation ID
        /// </summary>
        IPhysicsObject CreatePhysicsObject(int? variationId);

        /// <summary>
        /// Create an animation object
        /// </summary>
        IPhysicsObject CreateAnimObject(uint setupId, bool createParts);

        /// <summary>
        /// Create a particle object
        /// </summary>
        IPhysicsObject CreateParticleObject(int numParts, Sphere sortingSphere, int? variationId);

        /// <summary>
        /// Enter world with physics object at position
        /// </summary>
        bool EnterWorld(IPhysicsObject obj, Position position);

        /// <summary>
        /// Leave world with physics object
        /// </summary>
        void LeaveWorld(IPhysicsObject obj);

        /// <summary>
        /// Destroy physics object
        /// </summary>
        void DestroyObject(IPhysicsObject obj);

        /// <summary>
        /// Check if physics object is active
        /// </summary>
        bool IsActive(IPhysicsObject obj);

        /// <summary>
        /// Set physics object active state
        /// </summary>
        void SetActive(IPhysicsObject obj, bool active);

        /// <summary>
        /// Get physics object state
        /// </summary>
        PhysicsState GetState(IPhysicsObject obj);

        /// <summary>
        /// Set physics object state
        /// </summary>
        void SetState(IPhysicsObject obj, PhysicsState state);

        /// <summary>
        /// Get physics object position
        /// </summary>
        Position GetPosition(IPhysicsObject obj);

        /// <summary>
        /// Set physics object position
        /// </summary>
        void SetPosition(IPhysicsObject obj, Position position);

        /// <summary>
        /// Get physics object velocity
        /// </summary>
        Vector3 GetVelocity(IPhysicsObject obj);

        /// <summary>
        /// Set physics object velocity
        /// </summary>
        void SetVelocity(IPhysicsObject obj, Vector3 velocity);

        /// <summary>
        /// Get physics object acceleration
        /// </summary>
        Vector3 GetAcceleration(IPhysicsObject obj);

        /// <summary>
        /// Set physics object acceleration
        /// </summary>
        void SetAcceleration(IPhysicsObject obj, Vector3 acceleration);

        /// <summary>
        /// Check if physics object is animating
        /// </summary>
        bool IsAnimating(IPhysicsObject obj);

        /// <summary>
        /// Check if physics object is moving or animating
        /// </summary>
        bool IsMovingOrAnimating(IPhysicsObject obj);

        /// <summary>
        /// Set motion table ID for physics object
        /// </summary>
        void SetMotionTableId(IPhysicsObject obj, uint motionTableId);

        /// <summary>
        /// Set scale for physics object
        /// </summary>
        void SetScale(IPhysicsObject obj, float scale);

        /// <summary>
        /// Update physics object
        /// </summary>
        bool UpdateObject(IPhysicsObject obj);

        /// <summary>
        /// Update physics object internally with quantum
        /// </summary>
        void UpdateObjectInternal(IPhysicsObject obj, float quantum);

        /// <summary>
        /// Check if physics object has collision
        /// </summary>
        bool HasCollision(IPhysicsObject obj);

        /// <summary>
        /// Report collision between physics objects
        /// </summary>
        void ReportCollision(IPhysicsObject obj, IPhysicsObject target);

        /// <summary>
        /// Report collision end between physics objects
        /// </summary>
        void ReportCollisionEnd(IPhysicsObject obj, IPhysicsObject target);

        /// <summary>
        /// Set object GUID for physics object
        /// </summary>
        void SetObjectGuid(IPhysicsObject obj, ObjectGuid guid);

        /// <summary>
        /// Set weenie object for physics object
        /// </summary>
        void SetWeenieObject(IPhysicsObject obj, WorldObject worldObject);

        /// <summary>
        /// Check if physics object is a player
        /// </summary>
        bool IsPlayer(IPhysicsObject obj);

        /// <summary>
        /// Check if physics object is static
        /// </summary>
        bool IsStatic(IPhysicsObject obj);

        /// <summary>
        /// Check if physics object is a missile
        /// </summary>
        bool IsMissile(IPhysicsObject obj);

        /// <summary>
        /// Check if physics object is ethereal
        /// </summary>
        bool IsEthereal(IPhysicsObject obj);
    }

    /// <summary>
    /// Interface for physics objects
    /// Defines the contract for physics objects that can be used by either system
    /// </summary>
    public interface IPhysicsObject
    {
        /// <summary>
        /// Object ID
        /// </summary>
        uint Id { get; }

        /// <summary>
        /// Object GUID
        /// </summary>
        ObjectGuid ObjectGuid { get; }

        /// <summary>
        /// Object position
        /// </summary>
        Position Position { get; set; }

        /// <summary>
        /// Physics state
        /// </summary>
        PhysicsState State { get; set; }

        /// <summary>
        /// Object velocity
        /// </summary>
        Vector3 Velocity { get; set; }

        /// <summary>
        /// Object acceleration
        /// </summary>
        Vector3 Acceleration { get; set; }

        /// <summary>
        /// Object scale
        /// </summary>
        float Scale { get; set; }

        /// <summary>
        /// Is object active
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Is object animating
        /// </summary>
        bool IsAnimating { get; }

        /// <summary>
        /// Is object moving or animating
        /// </summary>
        bool IsMovingOrAnimating { get; }

        /// <summary>
        /// Is object a player
        /// </summary>
        bool IsPlayer { get; }

        /// <summary>
        /// Is object static
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        /// Is object a missile
        /// </summary>
        bool IsMissile { get; }

        /// <summary>
        /// Is object ethereal
        /// </summary>
        bool IsEthereal { get; }

        /// <summary>
        /// Enter world at position
        /// </summary>
        bool EnterWorld(Position position);

        /// <summary>
        /// Leave world
        /// </summary>
        void LeaveWorld();

        /// <summary>
        /// Destroy object
        /// </summary>
        void Destroy();

        /// <summary>
        /// Update object
        /// </summary>
        bool UpdateObject();

        /// <summary>
        /// Set active state
        /// </summary>
        void SetActive(bool active);

        /// <summary>
        /// Set physics state
        /// </summary>
        void SetState(PhysicsState state);

        /// <summary>
        /// Set position
        /// </summary>
        void SetPosition(Position position);

        /// <summary>
        /// Set velocity
        /// </summary>
        void SetVelocity(Vector3 velocity);

        /// <summary>
        /// Set acceleration
        /// </summary>
        void SetAcceleration(Vector3 acceleration);

        /// <summary>
        /// Set motion table ID
        /// </summary>
        void SetMotionTableId(uint motionTableId);

        /// <summary>
        /// Set scale
        /// </summary>
        void SetScale(float scale);

        /// <summary>
        /// Set object GUID
        /// </summary>
        void SetObjectGuid(ObjectGuid guid);

        /// <summary>
        /// Set weenie object
        /// </summary>
        void SetWeenieObject(WorldObject worldObject);
    }

    /// <summary>
    /// Enum for physics system types
    /// </summary>
    public enum PhysicsSystemType
    {
        ACE,
        GDLE
    }
} 