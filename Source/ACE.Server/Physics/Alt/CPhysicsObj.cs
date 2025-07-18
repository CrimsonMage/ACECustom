using System;
using System.Collections.Generic;
using System.Threading;
using System.Numerics;
using ACE.Entity;
using ACE.Server.WorldObjects;
using ACE.Entity.Enum;
using ACE.Server.Physics.Alt;

namespace ACE.Server.Physics.Alt
{
    /// <summary>
    /// GDLE Physics Object - ported from C++ CPhysicsObj
    /// This is a basic implementation that will be gradually expanded
    /// Thread-safe implementation for multi-threaded ACE environment
    /// </summary>
    public class CPhysicsObj
    {
        // Thread synchronization
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        
        // Core properties (thread-safe access)
        private uint _id;
        private ObjectGuid _objectGuid;
        private Position _position;
        private PhysicsState _state;
        private Vector3 _velocity;
        private Vector3 _acceleration;
        private Vector3 _omega;
        private float _scale;
        private float _friction;
        private float _elasticity;
        private float _mass;
        
        // State flags (thread-safe access)
        private bool _isActive;
        private bool _isAnimating;
        private bool _isMoving;
        
        // Object references (thread-safe access)
        private WorldObject _weenieObject;
        private uint _motionTableId;
        private uint _setupId;
        
        // Collision and physics (thread-safe access)
        private bool _collidingWithEnvironment;
        private Dictionary<uint, CollisionRecord> _collisionTable;
        
        // Timing (thread-safe access)
        private double _updateTime;
        private double _lastUpdateTime;

        // Public properties with thread-safe access
        public uint Id 
        { 
            get { _lock.EnterReadLock(); try { return _id; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _id = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        public ObjectGuid ObjectGuid 
        { 
            get { _lock.EnterReadLock(); try { return _objectGuid; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _objectGuid = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        public Position Position 
        { 
            get { _lock.EnterReadLock(); try { return _position; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _position = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        public PhysicsState State 
        { 
            get { _lock.EnterReadLock(); try { return _state; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _state = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        public Vector3 Velocity 
        { 
            get { _lock.EnterReadLock(); try { return _velocity; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _velocity = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        public Vector3 Acceleration 
        { 
            get { _lock.EnterReadLock(); try { return _acceleration; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _acceleration = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        public Vector3 Omega 
        { 
            get { _lock.EnterReadLock(); try { return _omega; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _omega = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        public float Scale 
        { 
            get { _lock.EnterReadLock(); try { return _scale; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _scale = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        public float Friction 
        { 
            get { _lock.EnterReadLock(); try { return _friction; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _friction = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        public float Elasticity 
        { 
            get { _lock.EnterReadLock(); try { return _elasticity; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _elasticity = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        public float Mass 
        { 
            get { _lock.EnterReadLock(); try { return _mass; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _mass = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        public bool IsActive 
        { 
            get { _lock.EnterReadLock(); try { return _isActive; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _isActive = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        public bool IsAnimating 
        { 
            get { _lock.EnterReadLock(); try { return _isAnimating; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _isAnimating = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        public bool IsMoving 
        { 
            get { _lock.EnterReadLock(); try { return _isMoving; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _isMoving = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        public WorldObject WeenieObject 
        { 
            get { _lock.EnterReadLock(); try { return _weenieObject; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _weenieObject = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        public uint MotionTableId 
        { 
            get { _lock.EnterReadLock(); try { return _motionTableId; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _motionTableId = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        public uint SetupId 
        { 
            get { _lock.EnterReadLock(); try { return _setupId; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _setupId = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        public bool CollidingWithEnvironment 
        { 
            get { _lock.EnterReadLock(); try { return _collidingWithEnvironment; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _collidingWithEnvironment = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        public Dictionary<uint, CollisionRecord> CollisionTable 
        { 
            get { _lock.EnterReadLock(); try { return _collisionTable; } finally { _lock.ExitReadLock(); } }
        }
        
        public double UpdateTime 
        { 
            get { _lock.EnterReadLock(); try { return _updateTime; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _updateTime = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        public double LastUpdateTime 
        { 
            get { _lock.EnterReadLock(); try { return _lastUpdateTime; } finally { _lock.ExitReadLock(); } }
            set { _lock.EnterWriteLock(); try { _lastUpdateTime = value; } finally { _lock.ExitWriteLock(); } }
        }
        
        // Constants (from original C++ code)
        public const float DEFAULT_FRICTION = 0.95f;
        public const float DEFAULT_ELASTICITY = 0.05f;
        public const float DEFAULT_MASS = 1.0f;
        public const float DEFAULT_SCALE = 1.0f;
        
        // Default physics state (from original C++ code)
        public static readonly PhysicsState DEFAULT_STATE = 
            PhysicsState.EdgeSlide | PhysicsState.LightingOn | PhysicsState.Gravity | PhysicsState.ReportCollisions;

        /// <summary>
        /// Collision record for tracking object collisions
        /// </summary>
        public class CollisionRecord
        {
            public uint ObjectID { get; set; }
            public Vector3 Point { get; set; }
            public Vector3 Normal { get; set; }
            public double TouchedTime { get; set; }
            public bool Ethereal { get; set; }
            public bool IsWalkable { get; set; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public CPhysicsObj()
        {
            InitializeDefaults();
        }

        /// <summary>
        /// Constructor with variation ID
        /// </summary>
        public CPhysicsObj(int? variationId)
        {
            InitializeDefaults();
            if (variationId.HasValue)
            {
                _lock.EnterWriteLock();
                try
                {
                    _position.Variation = variationId.Value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Initialize default values
        /// </summary>
        private void InitializeDefaults()
        {
            _lock.EnterWriteLock();
            try
            {
                _id = 0;
                _objectGuid = ObjectGuid.Invalid;
                _position = new Position();
                _state = DEFAULT_STATE;
                _velocity = Vector3.Zero;
                _acceleration = Vector3.Zero;
                _omega = Vector3.Zero;
                _scale = DEFAULT_SCALE;
                _friction = DEFAULT_FRICTION;
                _elasticity = DEFAULT_ELASTICITY;
                _mass = DEFAULT_MASS;
                
                _isActive = false;
                _isAnimating = false;
                _isMoving = false;
                
                _weenieObject = null;
                _motionTableId = 0;
                _setupId = 0;
                
                _collidingWithEnvironment = false;
                _collisionTable = new Dictionary<uint, CollisionRecord>();
                
                _updateTime = PhysicsTimer.CurrentTime;
                _lastUpdateTime = PhysicsTimer.CurrentTime;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Create a physics object with setup ID and object ID
        /// </summary>
        public static CPhysicsObj makeObject(uint setupId, uint objectId, bool isDynamic)
        {
            var obj = new CPhysicsObj();
            obj._lock.EnterWriteLock();
            try
            {
                obj._setupId = setupId;
                obj._id = objectId;
                obj._objectGuid = new ObjectGuid(objectId);
            }
            finally
            {
                obj._lock.ExitWriteLock();
            }
            
            // TODO: Initialize part array and other setup-specific properties
            // obj.InitPartArrayObject(setupId, true);
            
            return obj;
        }

        /// <summary>
        /// Create a particle physics object
        /// </summary>
        public static CPhysicsObj makeParticleObject(int numParts, Sphere sortingSphere, int? variationId)
        {
            var obj = new CPhysicsObj(variationId);
            obj._lock.EnterWriteLock();
            try
            {
                obj._state = PhysicsState.Static | PhysicsState.ReportCollisions;
            }
            finally
            {
                obj._lock.ExitWriteLock();
            }
            
            // TODO: Initialize particle-specific properties
            // obj.PartArray = PartArray.CreateParticle(obj, numParts, sortingSphere);
            
            return obj;
        }

        /// <summary>
        /// Set object GUID
        /// </summary>
        public void set_object_guid(ObjectGuid guid)
        {
            _lock.EnterWriteLock();
            try
            {
                _objectGuid = guid;
                _id = guid.Full;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Set weenie object
        /// </summary>
        public void set_weenie_obj(WorldObject worldObject)
        {
            _lock.EnterWriteLock();
            try
            {
                _weenieObject = worldObject;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Set motion table ID
        /// </summary>
        public void SetMotionTableID(uint motionTableId)
        {
            _lock.EnterWriteLock();
            try
            {
                _motionTableId = motionTableId;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Set scale statically
        /// </summary>
        public void SetScaleStatic(float scale)
        {
            _lock.EnterWriteLock();
            try
            {
                _scale = scale;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Set physics state
        /// </summary>
        public void set_state(PhysicsState state, bool sendEvent = false)
        {
            _lock.EnterWriteLock();
            try
            {
                _state = state;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            // TODO: Handle state change events
        }

        /// <summary>
        /// Set active state
        /// </summary>
        public bool set_active(bool active)
        {
            _lock.EnterWriteLock();
            try
            {
                if (active)
                {
                    if (_state.HasFlag(PhysicsState.Static))
                        return false;

                    if (!_isActive)
                        _updateTime = PhysicsTimer.CurrentTime;

                    _isActive = true;
                }
                else
                {
                    _isActive = false;
                }

                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Check if object is active
        /// </summary>
        public bool is_active()
        {
            _lock.EnterReadLock();
            try
            {
                return _isActive;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Enter world at position
        /// </summary>
        public bool enter_world(Position position)
        {
            _lock.EnterWriteLock();
            try
            {
                _position = position;
                _isActive = true;
                _updateTime = PhysicsTimer.CurrentTime;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            
            // TODO: Implement full enter world logic
            // - Cell management
            // - Collision detection setup
            // - Object maintenance registration
            
            return true;
        }

        /// <summary>
        /// Leave world
        /// </summary>
        public void leave_world()
        {
            _lock.EnterWriteLock();
            try
            {
                _isActive = false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            
            // TODO: Implement full leave world logic
            // - Remove from cells
            // - Clear collision records
            // - Unregister from object maintenance
        }

        /// <summary>
        /// Destroy object
        /// </summary>
        public void Destroy()
        {
            leave_world();
            
            // TODO: Implement full destroy logic
            // - Clean up resources
            // - Remove from object maintenance
        }

        /// <summary>
        /// Update object physics
        /// </summary>
        public bool update_object()
        {
            _lock.EnterReadLock();
            try
            {
                if (!_isActive)
                    return false;
            }
            finally
            {
                _lock.ExitReadLock();
            }

            var currentTime = PhysicsTimer.CurrentTime;
            double deltaTime;
            
            _lock.EnterReadLock();
            try
            {
                deltaTime = currentTime - _updateTime;
            }
            finally
            {
                _lock.ExitReadLock();
            }

            if (deltaTime < PhysicsGlobals.MinQuantum)
                return false;

            if (deltaTime > PhysicsGlobals.HugeQuantum)
            {
                _lock.EnterWriteLock();
                try
                {
                    _updateTime = currentTime;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
                return false;
            }

            // TODO: Implement full update logic
            // - Apply forces (gravity, etc.)
            // - Update position based on velocity
            // - Handle collisions
            // - Update animation state

            _lock.EnterWriteLock();
            try
            {
                _updateTime = currentTime;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            return true;
        }

        /// <summary>
        /// Update object internally with quantum
        /// </summary>
        public void UpdateObjectInternal(float quantum)
        {
            // TODO: Implement internal update logic
            // - Apply physics calculations
            // - Update position and velocity
            // - Handle collision detection
        }

        /// <summary>
        /// Make anim object
        /// </summary>
        public bool makeAnimObject(uint setupId, bool createParts)
        {
            _lock.EnterWriteLock();
            try
            {
                _setupId = setupId;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            
            // TODO: Implement anim object creation
            // - Initialize part array
            // - Set up animation components
            
            return true;
        }

        /// <summary>
        /// Check if object is a player
        /// </summary>
        public bool IsPlayer 
        { 
            get 
            { 
                _lock.EnterReadLock(); 
                try { return _id >= 0x50000001 && _id <= 0x5FFFFFFF; } 
                finally { _lock.ExitReadLock(); } 
            } 
        }

        /// <summary>
        /// Check if object is moving or animating
        /// </summary>
        public bool IsMovingOrAnimating 
        { 
            get 
            { 
                _lock.EnterReadLock(); 
                try { return _isAnimating || _isMoving || _velocity != Vector3.Zero || _acceleration != Vector3.Zero; } 
                finally { _lock.ExitReadLock(); } 
            } 
        }

        /// <summary>
        /// Get initial updates count (for compatibility)
        /// </summary>
        public int InitialUpdates => 1;

        /// <summary>
        /// Thread-safe collision table operations
        /// </summary>
        
        /// <summary>
        /// Add collision record thread-safely
        /// </summary>
        public void AddCollisionRecord(uint objectId, CollisionRecord record)
        {
            _lock.EnterWriteLock();
            try
            {
                _collisionTable[objectId] = record;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Remove collision record thread-safely
        /// </summary>
        public bool RemoveCollisionRecord(uint objectId)
        {
            _lock.EnterWriteLock();
            try
            {
                return _collisionTable.Remove(objectId);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Get collision record thread-safely
        /// </summary>
        public bool TryGetCollisionRecord(uint objectId, out CollisionRecord record)
        {
            _lock.EnterReadLock();
            try
            {
                return _collisionTable.TryGetValue(objectId, out record);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Clear all collision records thread-safely
        /// </summary>
        public void ClearCollisionTable()
        {
            _lock.EnterWriteLock();
            try
            {
                _collisionTable.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Get collision table count thread-safely
        /// </summary>
        public int GetCollisionTableCount()
        {
            _lock.EnterReadLock();
            try
            {
                return _collisionTable.Count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Get a copy of collision table thread-safely
        /// </summary>
        public Dictionary<uint, CollisionRecord> GetCollisionTableCopy()
        {
            _lock.EnterReadLock();
            try
            {
                return new Dictionary<uint, CollisionRecord>(_collisionTable);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Dispose method to clean up locks
        /// </summary>
        public void Dispose()
        {
            _lock?.Dispose();
        }

        // Additional GDLE physics methods

        /// <summary>
        /// Check if object is static (non-moving)
        /// </summary>
        public bool IsStatic()
        {
            _lock.EnterReadLock();
            try
            {
                return !_isMoving && !_isAnimating && _velocity.LengthSquared() < 0.001f;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Check if object is ethereal (passes through other objects)
        /// </summary>
        public bool IsEthereal()
        {
            _lock.EnterReadLock();
            try
            {
                return (_state & PhysicsState.Ethereal) != 0;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Check if object is a missile/projectile
        /// </summary>
        public bool IsMissile()
        {
            _lock.EnterReadLock();
            try
            {
                return (_state & PhysicsState.Missile) != 0;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Get object's bounding sphere
        /// </summary>
        public CSphere GetBoundingSphere()
        {
            _lock.EnterReadLock();
            try
            {
                // TODO: Implement proper bounding sphere calculation
                return new CSphere
                {
                    Center = _position.Pos,
                    Radius = 1.0f * _scale
                };
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Check collision with another physics object
        /// </summary>
        public bool CheckCollision(CPhysicsObj other)
        {
            if (other == null) return false;

            _lock.EnterReadLock();
            other._lock.EnterReadLock();
            try
            {
                // Simple sphere collision check
                var sphere1 = GetBoundingSphere();
                var sphere2 = other.GetBoundingSphere();
                
                var distance = Vector3.Distance(sphere1.Center, sphere2.Center);
                return distance < (sphere1.Radius + sphere2.Radius);
            }
            finally
            {
                other._lock.ExitReadLock();
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Apply force to the object
        /// </summary>
        public void ApplyForce(Vector3 force)
        {
            _lock.EnterWriteLock();
            try
            {
                _acceleration += force / _mass;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Apply impulse to the object
        /// </summary>
        public void ApplyImpulse(Vector3 impulse)
        {
            _lock.EnterWriteLock();
            try
            {
                _velocity += impulse / _mass;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Get object's kinetic energy
        /// </summary>
        public float GetKineticEnergy()
        {
            _lock.EnterReadLock();
            try
            {
                return 0.5f * _mass * _velocity.LengthSquared();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Get object's momentum
        /// </summary>
        public Vector3 GetMomentum()
        {
            _lock.EnterReadLock();
            try
            {
                return _mass * _velocity;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Set object's momentum
        /// </summary>
        public void SetMomentum(Vector3 momentum)
        {
            _lock.EnterWriteLock();
            try
            {
                _velocity = momentum / _mass;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Get object's angular momentum
        /// </summary>
        public Vector3 GetAngularMomentum()
        {
            _lock.EnterReadLock();
            try
            {
                // Simplified - assuming uniform mass distribution
                float momentOfInertia = _mass * _scale * _scale;
                return momentOfInertia * _omega;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Set object's angular momentum
        /// </summary>
        public void SetAngularMomentum(Vector3 angularMomentum)
        {
            _lock.EnterWriteLock();
            try
            {
                float momentOfInertia = _mass * _scale * _scale;
                _omega = angularMomentum / momentOfInertia;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Get object's total energy (kinetic + potential)
        /// </summary>
        public float GetTotalEnergy()
        {
            _lock.EnterReadLock();
            try
            {
                float kinetic = GetKineticEnergy();
                float potential = _mass * 9.81f * _position.Pos.Z; // Simple gravitational potential
                return kinetic + potential;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Check if object is in contact with ground
        /// </summary>
        public bool IsOnGround()
        {
            _lock.EnterReadLock();
            try
            {
                // TODO: Implement proper ground contact detection
                return _position.Pos.Z <= 0.1f;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Get object's contact normal with ground
        /// </summary>
        public Vector3 GetGroundNormal()
        {
            _lock.EnterReadLock();
            try
            {
                // TODO: Implement proper ground normal calculation
                return Vector3.UnitZ;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Apply friction to the object
        /// </summary>
        public void ApplyFriction(float deltaTime)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_velocity.LengthSquared() > 0.001f)
                {
                    var frictionForce = -_friction * _velocity;
                    _velocity += frictionForce * deltaTime;
                    
                    // Stop very small velocities
                    if (_velocity.LengthSquared() < 0.001f)
                    {
                        _velocity = Vector3.Zero;
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Apply gravity to the object
        /// </summary>
        public void ApplyGravity(float deltaTime)
        {
            _lock.EnterWriteLock();
            try
            {
                if ((_state & PhysicsState.Gravity) != 0)
                {
                    var gravity = new Vector3(0, 0, -9.81f);
                    _velocity += gravity * deltaTime;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Update object's position based on velocity
        /// </summary>
        public void UpdatePosition(float deltaTime)
        {
            _lock.EnterWriteLock();
            try
            {
                var newPos = _position.Pos + _velocity * deltaTime;
                _position.Pos = newPos;
                
                // Update moving state
                _isMoving = _velocity.LengthSquared() > 0.001f;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Update object's rotation based on angular velocity
        /// </summary>
        public void UpdateRotation(float deltaTime)
        {
            _lock.EnterWriteLock();
            try
            {
                // TODO: Implement proper rotation update using quaternions
                // For now, just update the rotation based on omega
                var rotationDelta = _omega * deltaTime;
                // This is a simplified approach - should use quaternion integration
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Get object's drag coefficient
        /// </summary>
        public float GetDragCoefficient()
        {
            _lock.EnterReadLock();
            try
            {
                // TODO: Implement proper drag coefficient calculation
                return 0.47f; // Default sphere drag coefficient
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Apply air resistance to the object
        /// </summary>
        public void ApplyAirResistance(float deltaTime, float airDensity = 1.225f)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_velocity.LengthSquared() > 0.001f)
                {
                    float dragCoeff = GetDragCoefficient();
                    float area = (float)(Math.PI * _scale * _scale); // Cross-sectional area
                    float speed = _velocity.Length();
                    
                    var dragForce = -0.5f * airDensity * speed * speed * dragCoeff * area * _velocity.Normalized();
                    _velocity += dragForce * deltaTime / _mass;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Check if object is in water
        /// </summary>
        public bool IsInWater()
        {
            _lock.EnterReadLock();
            try
            {
                // TODO: Implement proper water detection
                return _position.Pos.Z < 0.0f; // Simplified water level at Z=0
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Apply buoyancy force to the object
        /// </summary>
        public void ApplyBuoyancy(float deltaTime, float waterDensity = 1000.0f)
        {
            _lock.EnterWriteLock();
            try
            {
                if (IsInWater())
                {
                    float volume = (4.0f / 3.0f) * (float)Math.PI * _scale * _scale * _scale;
                    var buoyancyForce = new Vector3(0, 0, waterDensity * 9.81f * volume);
                    _velocity += buoyancyForce * deltaTime / _mass;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        // Admin move support for GDLE physics

        /// <summary>
        /// Create a transition for movement with admin move support and movewithoutportal logic
        /// </summary>
        public CTransition CreateTransition(Position oldPos, Position newPos, bool adminMove, bool moveWithoutPortal = false)
        {
            var transition = new CTransition();
            
            // Initialize transition with object info
            var objectInfo = GetObjectInfo(transition, adminMove, moveWithoutPortal);
            transition.ObjectInfo = objectInfo;
            
            // Set up sphere path
            transition.SpherePath = new SPHEREPATH();
            transition.SpherePath.StartPosition = oldPos.Pos;
            transition.SpherePath.EndPosition = newPos.Pos;
            
            // Add spheres based on object size
            var boundingSphere = GetBoundingSphere();
            transition.SpherePath.Spheres.Add(boundingSphere);
            
            // Set transition properties
            transition.StartPosition = oldPos.Pos;
            transition.EndPosition = newPos.Pos;
            transition.Scale = _scale;
            
            return transition;
        }

        /// <summary>
        /// Get object info for transition with admin move and movewithoutportal support
        /// </summary>
        public OBJECTINFO GetObjectInfo(CTransition transition, bool adminMove, bool moveWithoutPortal = false)
        {
            var objectInfo = new OBJECTINFO();
            
            // Set basic object properties
            objectInfo.ObjectID = _id;
            objectInfo.Position = _position;
            objectInfo.Velocity = _velocity;
            objectInfo.Scale = _scale;
            objectInfo.IsPlayer = IsPlayer;
            objectInfo.IsStatic = IsStatic();
            objectInfo.IsEthereal = IsEthereal();
            objectInfo.BoundingSphere = GetBoundingSphere();
            
            // Admin move bypasses collision detection UNLESS movewithoutportal is true
            if (!adminMove || moveWithoutPortal)
            {
                // Check for contact with environment
                if (_collidingWithEnvironment)
                {
                    objectInfo.State |= (int)OBJECTINFO.ObjectInfoEnum.ACTIVE_OI;
                    
                    // Check if on walkable surface
                    if (IsOnGround())
                    {
                        objectInfo.State |= (int)OBJECTINFO.ObjectInfoEnum.ACTIVE_OI;
                    }
                }
                
                // Check for sliding
                if (_velocity.LengthSquared() > 0.001f)
                {
                    // Could implement sliding normal calculation here
                }
            }
            
            // Set other object states
            if ((_state & PhysicsState.EdgeSlide) != 0)
                objectInfo.State |= (int)OBJECTINFO.ObjectInfoEnum.ACTIVE_OI;
                
            if ((_state & PhysicsState.Missile) != 0)
                objectInfo.State |= (int)OBJECTINFO.ObjectInfoEnum.ACTIVE_OI;
                
            if ((_state & PhysicsState.FreeRotate) != 0)
                objectInfo.State |= (int)OBJECTINFO.ObjectInfoEnum.ACTIVE_OI;
            
            return objectInfo;
        }

        /// <summary>
        /// Validate movement transition with admin move and movewithoutportal support
        /// </summary>
        public bool ValidateMovementTransition(CTransition transition, bool adminMove, bool moveWithoutPortal = false)
        {
            if (transition == null)
                return false;

            // Admin move bypasses most validation UNLESS movewithoutportal is true
            if (adminMove && !moveWithoutPortal)
            {
                return true;
            }

            // Regular movement validation
            return transition.ValidateMovement();
        }

        /// <summary>
        /// Process movement transition with admin move support and movewithoutportal logic
        /// </summary>
        public TransitionState ProcessMovementTransition(CTransition transition, bool adminMove, bool moveWithoutPortal = false)
        {
            if (transition == null)
                return TransitionState.OK_TS;

            // Admin move bypasses collision detection UNLESS moveWithoutPortal is true
            if (adminMove && !moveWithoutPortal)
            {
                // For admin move, just update position without collision checks
                _position.Pos = transition.EndPosition;
                _velocity = Vector3.Zero;
                return TransitionState.OK_TS;
            }

            // Regular collision detection and response (including movewithoutportal)
            var bspTree = GetBSPTreeForCell(_position.Cell);
            if (bspTree != null)
            {
                var collisionState = bspTree.FindCollisions(transition, _scale);
                
                if (collisionState == TransitionState.Collided_TS)
                {
                    // Handle collision response
                    HandleCollisionResponse(transition, moveWithoutPortal);
                    return TransitionState.Collided_TS;
                }
            }

            // No collision, update position
            _position.Pos = transition.EndPosition;
            return TransitionState.OK_TS;
        }

        /// <summary>
        /// Handle collision response for movement rejection with movewithoutportal support
        /// </summary>
        private void HandleCollisionResponse(CTransition transition, bool moveWithoutPortal)
        {
            if (transition == null || !transition.HasCollision)
                return;

            // If movewithoutportal is enabled, always reject movement and send back to start
            if (moveWithoutPortal)
            {
                // Force position back to starting position to prevent passing through solid objects
                _position.Pos = transition.StartPosition;
                _velocity = Vector3.Zero;
                return;
            }

            // Try different collision responses in order of preference
            
            // 1. Try to slide along surface
            if (transition.TrySlideAlongSurface())
            {
                _position.Pos = transition.EndPosition;
                return;
            }
            
            // 2. Try to step up over obstacle
            if (transition.TryStepUp())
            {
                _position.Pos = transition.EndPosition;
                return;
            }
            
            // 3. Try to step down to lower surface
            if (transition.TryStepDown())
            {
                _position.Pos = transition.EndPosition;
                return;
            }
            
            // 4. If all else fails, reject movement and keep original position
            // This is where the "send back to starting position" happens
            _position.Pos = transition.StartPosition;
            _velocity = Vector3.Zero;
        }

        /// <summary>
        /// Check if movement should be rejected with movewithoutportal support
        /// </summary>
        public bool ShouldRejectMovement(CTransition transition, bool adminMove, bool moveWithoutPortal = false)
        {
            if (adminMove && !moveWithoutPortal)
                return false; // Admin move never gets rejected unless movewithoutportal is enabled

            if (transition == null)
                return false;

            // Check if movement is valid
            if (!transition.ValidateMovement())
                return true;

            // Check for collisions
            var bspTree = GetBSPTreeForCell(_position.Cell);
            if (bspTree != null)
            {
                var collisionState = bspTree.FindCollisions(transition, _scale);
                if (collisionState == TransitionState.Collided_TS)
                {
                    // If movewithoutportal is enabled, always reject movement
                    if (moveWithoutPortal)
                        return true;

                    // Try collision responses
                    if (!transition.TrySlideAlongSurface() && 
                        !transition.TryStepUp() && 
                        !transition.TryStepDown())
                    {
                        // All collision responses failed, reject movement
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Force position update with movewithoutportal support
        /// </summary>
        public void ForcePositionUpdate(Position newPosition, bool adminMove = false, bool moveWithoutPortal = false)
        {
            _lock.EnterWriteLock();
            try
            {
                // If movewithoutportal is enabled, check for collisions even with admin move
                if (moveWithoutPortal)
                {
                    var oldPosition = _position;
                    var transition = CreateTransition(oldPosition, newPosition, adminMove, true);
                    
                    // Check if the new position would cause a collision
                    if (ShouldRejectMovement(transition, adminMove, true))
                    {
                        // Reject the movement and keep original position
                        _velocity = Vector3.Zero;
                        return;
                    }
                }
                
                _position = newPosition;
                _velocity = Vector3.Zero;
                
                if (adminMove && !moveWithoutPortal)
                {
                    // Admin move bypasses all physics checks
                    _isMoving = false;
                    _collidingWithEnvironment = false;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Check if object is stuck and needs to be unstuck
        /// </summary>
        public bool IsStuck()
        {
            _lock.EnterReadLock();
            try
            {
                // Check if object has been stationary for too long
                var timeSinceLastMove = PhysicsTimer.CurrentTime - _lastUpdateTime;
                return timeSinceLastMove > 5.0 && _velocity.LengthSquared() < 0.001f;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Unstick object by moving it to a valid position
        /// </summary>
        public bool UnstickObject()
        {
            _lock.EnterWriteLock();
            try
            {
                // Try to find a valid position nearby
                var originalPosition = _position.Pos;
                var attempts = 0;
                var maxAttempts = 10;
                
                while (attempts < maxAttempts)
                {
                    // Try different offsets
                    var offset = new Vector3(
                        (float)(new Random().NextDouble() - 0.5) * 2.0f,
                        (float)(new Random().NextDouble() - 0.5) * 2.0f,
                        0
                    );
                    
                    var testPosition = originalPosition + offset;
                    var testTransition = new CTransition();
                    testTransition.SpherePath = new SPHEREPATH();
                    testTransition.SpherePath.StartPosition = testPosition;
                    testTransition.SpherePath.EndPosition = testPosition;
                    testTransition.Scale = _scale;
                    
                    var bspTree = GetBSPTreeForCell(_position.Cell);
                    if (bspTree != null)
                    {
                        var placementState = bspTree.PlacementInsert(testTransition);
                        if (placementState == TransitionState.OK_TS)
                        {
                            // Valid position found, move object there
                            _position.Pos = testPosition;
                            _velocity = Vector3.Zero;
                            return true;
                        }
                    }
                    
                    attempts++;
                }
                
                // If no valid position found, try to move up
                var upPosition = originalPosition + new Vector3(0, 0, 5.0f);
                _position.Pos = upPosition;
                _velocity = Vector3.Zero;
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Get BSP tree for cell
        /// </summary>
        private BSPTree GetBSPTreeForCell(uint cellID)
        {
            // TODO: Implement proper BSP tree loading for cell
            // For now, return null - this would need to be implemented with actual cell data loading
            return null;
        }

        /// <summary>
        /// Get cell ID from position
        /// </summary>
        private uint GetCellIDFromPosition(Position position)
        {
            // TODO: Implement proper cell ID extraction from position
            // For now, return 0 - this would need to be implemented with actual position parsing
            return 0;
        }

        /// <summary>
        /// Get step up height for this physics object
        /// </summary>
        public float GetStepUpHeight()
        {
            // TODO: Implement proper step up height calculation
            // For now, return a default value
            return 0.5f;
        }

        /// <summary>
        /// Get step down height for this physics object
        /// </summary>
        public float GetStepDownHeight()
        {
            // TODO: Implement proper step down height calculation
            // For now, return a default value
            return 0.5f;
        }

        /// <summary>
        /// Find object collisions for transition
        /// </summary>
        public int FindObjCollisions(CTransition transition)
        {
            // TODO: Implement proper object collision detection
            // For now, return 0 (no collision)
            return 0;
        }
    }
} 