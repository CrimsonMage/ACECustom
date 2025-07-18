using System;
using System.Numerics;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Server.WorldObjects;
using ACE.Server.Physics.Alt;

namespace ACE.Server.Physics
{
    /// <summary>
    /// Adapter for the GDLE physics system
    /// Implements IPhysicsSystem interface for the ported GDLE physics
    /// </summary>
    public class GDLEPhysicsAdapter : IPhysicsSystem
    {
        public IPhysicsObject CreatePhysicsObject(uint setupId, ObjectGuid objectId, bool isDynamic)
        {
            // Create GDLE physics object using the ported classes
            var physicsObj = new GDLEPhysicsObjectAdapter(setupId, objectId, isDynamic);
            return physicsObj;
        }

        public IPhysicsObject CreatePhysicsObject(int? variationId)
        {
            // Create GDLE physics object with variation
            var physicsObj = new GDLEPhysicsObjectAdapter(variationId);
            return physicsObj;
        }

        public IPhysicsObject CreateAnimObject(uint setupId, bool createParts)
        {
            // Create GDLE anim object
            var physicsObj = new GDLEPhysicsObjectAdapter(setupId, createParts);
            return physicsObj;
        }

        public IPhysicsObject CreateParticleObject(int numParts, Sphere sortingSphere, int? variationId)
        {
            // Create GDLE particle object
            var physicsObj = new GDLEPhysicsObjectAdapter(numParts, sortingSphere, variationId);
            return physicsObj;
        }

        public bool EnterWorld(IPhysicsObject obj, Position position)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                // TODO: Implement GDLE enter world
                return false;
            }
            return false;
        }

        public void LeaveWorld(IPhysicsObject obj)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                // TODO: Implement GDLE leave world
            }
        }

        public void DestroyObject(IPhysicsObject obj)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                // TODO: Implement GDLE destroy object
            }
        }

        public bool IsActive(IPhysicsObject obj)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.is_active();
            }
            return false;
        }

        public void SetActive(IPhysicsObject obj, bool active)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                adapter.PhysicsObj.set_active(active);
            }
        }

        public ACE.Entity.Enum.PhysicsState GetState(IPhysicsObject obj)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                return (ACE.Entity.Enum.PhysicsState)adapter.PhysicsObj.State;
            }
            return ACE.Entity.Enum.PhysicsState.Static;
        }

        public void SetState(IPhysicsObject obj, ACE.Entity.Enum.PhysicsState state)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                adapter.PhysicsObj.State = (Alt.PhysicsState)state;
            }
        }

        public Position GetPosition(IPhysicsObject obj)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.Position;
            }
            return new Position();
        }

        public void SetPosition(IPhysicsObject obj, Position position)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                adapter.PhysicsObj.Position = position;
            }
        }

        public Vector3 GetVelocity(IPhysicsObject obj)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.Velocity;
            }
            return Vector3.Zero;
        }

        public void SetVelocity(IPhysicsObject obj, Vector3 velocity)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                adapter.PhysicsObj.Velocity = velocity;
            }
        }

        public Vector3 GetAcceleration(IPhysicsObject obj)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.Acceleration;
            }
            return Vector3.Zero;
        }

        public void SetAcceleration(IPhysicsObject obj, Vector3 acceleration)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                adapter.PhysicsObj.Acceleration = acceleration;
            }
        }

        public bool IsAnimating(IPhysicsObject obj)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.IsAnimating;
            }
            return false;
        }

        public bool IsMovingOrAnimating(IPhysicsObject obj)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.IsMovingOrAnimating;
            }
            return false;
        }

        public void SetMotionTableId(IPhysicsObject obj, uint motionTableId)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                adapter.PhysicsObj.SetMotionTableID(motionTableId);
            }
        }

        public void SetScale(IPhysicsObject obj, float scale)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                adapter.PhysicsObj.SetScaleStatic(scale);
            }
        }

        public bool UpdateObject(IPhysicsObject obj)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.update_object();
            }
            return false;
        }

        public void UpdateObjectInternal(IPhysicsObject obj, float quantum)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                adapter.PhysicsObj.UpdateObjectInternal(quantum);
            }
        }

        public bool HasCollision(IPhysicsObject obj)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.CollidingWithEnvironment;
            }
            return false;
        }

        public void ReportCollision(IPhysicsObject obj, IPhysicsObject target)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                // TODO: Implement GDLE collision reporting
            }
        }

        public void ReportCollisionEnd(IPhysicsObject obj, IPhysicsObject target)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                // TODO: Implement GDLE collision end reporting
            }
        }

        public void SetObjectGuid(IPhysicsObject obj, ObjectGuid guid)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                adapter.PhysicsObj.set_object_guid(guid);
            }
        }

        public void SetWeenieObject(IPhysicsObject obj, WorldObject worldObject)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                adapter.PhysicsObj.set_weenie_obj(worldObject);
            }
        }

        public bool IsPlayer(IPhysicsObject obj)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.IsPlayer;
            }
            return false;
        }

        public bool IsStatic(IPhysicsObject obj)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                return ((ACE.Entity.Enum.PhysicsState)adapter.PhysicsObj.State).HasFlag(ACE.Entity.Enum.PhysicsState.Static);
            }
            return false;
        }

        public bool IsMissile(IPhysicsObject obj)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                return ((ACE.Entity.Enum.PhysicsState)adapter.PhysicsObj.State).HasFlag(ACE.Entity.Enum.PhysicsState.Missile);
            }
            return false;
        }

        public bool IsEthereal(IPhysicsObject obj)
        {
            if (obj is GDLEPhysicsObjectAdapter adapter)
            {
                return ((ACE.Entity.Enum.PhysicsState)adapter.PhysicsObj.State).HasFlag(ACE.Entity.Enum.PhysicsState.Ethereal);
            }
            return false;
        }
    }

    /// <summary>
    /// Adapter for GDLE physics objects to implement IPhysicsObject interface
    /// </summary>
    public class GDLEPhysicsObjectAdapter : IPhysicsObject, IDisposable
    {
        // GDLE physics object reference
        public Alt.CPhysicsObj PhysicsObj { get; private set; }

        // Constructors
        public GDLEPhysicsObjectAdapter(uint setupId, ObjectGuid objectId, bool isDynamic)
        {
            PhysicsObj = Alt.CPhysicsObj.makeObject(setupId, objectId.Full, isDynamic);
        }

        public GDLEPhysicsObjectAdapter(int? variationId)
        {
            PhysicsObj = new Alt.CPhysicsObj(variationId);
        }

        public GDLEPhysicsObjectAdapter(uint setupId, bool createParts)
        {
            PhysicsObj = new Alt.CPhysicsObj(null);
            PhysicsObj.makeAnimObject(setupId, createParts);
        }

        public GDLEPhysicsObjectAdapter(int numParts, Sphere sortingSphere, int? variationId)
        {
            PhysicsObj = Alt.CPhysicsObj.makeParticleObject(numParts, sortingSphere, variationId);
        }

        // IPhysicsObject implementation
        public uint Id => PhysicsObj.Id;
        public ObjectGuid ObjectGuid => PhysicsObj.ObjectGuid;
        public Position Position { get => PhysicsObj.Position; set => PhysicsObj.Position = value; }
        public ACE.Entity.Enum.PhysicsState State 
        { 
            get => (ACE.Entity.Enum.PhysicsState)PhysicsObj.State; 
            set => PhysicsObj.State = (Alt.PhysicsState)value; 
        }
        public Vector3 Velocity { get => PhysicsObj.Velocity; set => PhysicsObj.Velocity = value; }
        public Vector3 Acceleration { get => PhysicsObj.Acceleration; set => PhysicsObj.Acceleration = value; }
        public float Scale { get => PhysicsObj.Scale; set => PhysicsObj.Scale = value; }
        public bool IsActive => PhysicsObj.IsActive;
        public bool IsAnimating => PhysicsObj.IsAnimating;
        public bool IsMovingOrAnimating => PhysicsObj.IsMovingOrAnimating;
        public bool IsPlayer => PhysicsObj.IsPlayer;
        public bool IsStatic => ((ACE.Entity.Enum.PhysicsState)PhysicsObj.State).HasFlag(ACE.Entity.Enum.PhysicsState.Static);
        public bool IsMissile => ((ACE.Entity.Enum.PhysicsState)PhysicsObj.State).HasFlag(ACE.Entity.Enum.PhysicsState.Missile);
        public bool IsEthereal => ((ACE.Entity.Enum.PhysicsState)PhysicsObj.State).HasFlag(ACE.Entity.Enum.PhysicsState.Ethereal);

        public bool EnterWorld(Position position)
        {
            return PhysicsObj.enter_world(position);
        }

        public void LeaveWorld()
        {
            PhysicsObj.leave_world();
        }

        public void Destroy()
        {
            PhysicsObj.Destroy();
        }

        public bool UpdateObject()
        {
            return PhysicsObj.update_object();
        }

        public void SetActive(bool active)
        {
            PhysicsObj.set_active(active);
        }

        public void SetState(ACE.Entity.Enum.PhysicsState state)
        {
            PhysicsObj.State = (Alt.PhysicsState)state;
        }

        public void SetPosition(Position position)
        {
            PhysicsObj.Position = position;
        }

        public void SetVelocity(Vector3 velocity)
        {
            PhysicsObj.Velocity = velocity;
        }

        public void SetAcceleration(Vector3 acceleration)
        {
            PhysicsObj.Acceleration = acceleration;
        }

        public void SetMotionTableId(uint motionTableId)
        {
            PhysicsObj.SetMotionTableID(motionTableId);
        }

        public void SetScale(float scale)
        {
            PhysicsObj.SetScaleStatic(scale);
        }

        public void SetObjectGuid(ObjectGuid guid)
        {
            PhysicsObj.set_object_guid(guid);
        }

        public void SetWeenieObject(WorldObject worldObject)
        {
            PhysicsObj.set_weenie_obj(worldObject);
        }

        /// <summary>
        /// Dispose method to clean up the underlying physics object
        /// </summary>
        public void Dispose()
        {
            PhysicsObj?.Dispose();
            PhysicsObj = null;
        }
    }
} 