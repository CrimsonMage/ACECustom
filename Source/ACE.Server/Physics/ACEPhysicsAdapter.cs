using System;
using System.Numerics;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Server.WorldObjects;
using ACE.Server.Physics.Common;
using ACE.Server.Physics.Alt;

namespace ACE.Server.Physics
{
    /// <summary>
    /// Adapter for the ACE physics system
    /// Implements IPhysicsSystem interface for the existing ACE physics
    /// </summary>
    public class ACEPhysicsAdapter : IPhysicsSystem
    {
        public IPhysicsObject CreatePhysicsObject(uint setupId, ObjectGuid objectId, bool isDynamic)
        {
            // Create ACE physics object using the existing PhysicsObj class
            var physicsObj = new ACEPhysicsObjectAdapter(setupId, objectId, isDynamic);
            return physicsObj;
        }

        public IPhysicsObject CreatePhysicsObject(int? variationId)
        {
            // Create ACE physics object with variation
            var physicsObj = new ACEPhysicsObjectAdapter(variationId);
            return physicsObj;
        }

        public IPhysicsObject CreateAnimObject(uint setupId, bool createParts)
        {
            // Create ACE anim object
            var physicsObj = new ACEPhysicsObjectAdapter(setupId, createParts);
            return physicsObj;
        }

        public IPhysicsObject CreateParticleObject(int numParts, Sphere sortingSphere, int? variationId)
        {
            // Create ACE particle object
            var physicsObj = new ACEPhysicsObjectAdapter(numParts, sortingSphere, variationId);
            return physicsObj;
        }

        public bool EnterWorld(IPhysicsObject obj, ACE.Entity.Position position)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                // TODO: Implement ACE enter world
                return false;
            }
            return false;
        }

        public void LeaveWorld(IPhysicsObject obj)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                // TODO: Implement ACE leave world
            }
        }

        public void DestroyObject(IPhysicsObject obj)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                // TODO: Implement ACE destroy object
            }
        }

        public bool IsActive(IPhysicsObject obj)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.is_active();
            }
            return false;
        }

        public void SetActive(IPhysicsObject obj, bool active)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                adapter.PhysicsObj.set_active(active);
            }
        }

        public ACE.Entity.Enum.PhysicsState GetState(IPhysicsObject obj)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.State;
            }
            return ACE.Entity.Enum.PhysicsState.Static;
        }

        public void SetState(IPhysicsObject obj, ACE.Entity.Enum.PhysicsState state)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                adapter.PhysicsObj.State = state;
            }
        }

        public ACE.Entity.Position GetPosition(IPhysicsObject obj)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                // Convert from ACE.Server.Physics.Common.Position to ACE.Entity.Position
                var acePos = adapter.PhysicsObj.Position;
                return new ACE.Entity.Position(acePos.ObjCellID, acePos.Frame.Origin.X, acePos.Frame.Origin.Y, acePos.Frame.Origin.Z, 
                    acePos.Frame.Orientation.X, acePos.Frame.Orientation.Y, acePos.Frame.Orientation.Z, acePos.Frame.Orientation.W);
            }
            return new ACE.Entity.Position();
        }

        public void SetPosition(IPhysicsObject obj, ACE.Entity.Position position)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                // Convert from ACE.Entity.Position to ACE.Server.Physics.Common.Position
                var frame = new ACE.Server.Physics.Animation.AFrame();
                frame.Origin = new Vector3(position.PositionX, position.PositionY, position.PositionZ);
                frame.Orientation = new System.Numerics.Quaternion(position.RotationX, position.RotationY, position.RotationZ, position.RotationW);
                
                var acePos = new ACE.Server.Physics.Common.Position();
                acePos.ObjCellID = position.Cell;
                acePos.Frame = frame;
                acePos.Variation = position.Variation;
                
                adapter.PhysicsObj.Position = acePos;
            }
        }

        public Vector3 GetVelocity(IPhysicsObject obj)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.Velocity;
            }
            return Vector3.Zero;
        }

        public void SetVelocity(IPhysicsObject obj, Vector3 velocity)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                adapter.PhysicsObj.Velocity = velocity;
            }
        }

        public Vector3 GetAcceleration(IPhysicsObject obj)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.Acceleration;
            }
            return Vector3.Zero;
        }

        public void SetAcceleration(IPhysicsObject obj, Vector3 acceleration)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                adapter.PhysicsObj.Acceleration = acceleration;
            }
        }

        public bool IsAnimating(IPhysicsObject obj)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.IsAnimating;
            }
            return false;
        }

        public bool IsMovingOrAnimating(IPhysicsObject obj)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.IsMovingOrAnimating;
            }
            return false;
        }

        public void SetMotionTableId(IPhysicsObject obj, uint motionTableId)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                adapter.PhysicsObj.SetMotionTableID(motionTableId);
            }
        }

        public void SetScale(IPhysicsObject obj, float scale)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                adapter.PhysicsObj.SetScaleStatic(scale);
            }
        }

        public bool UpdateObject(IPhysicsObject obj)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.update_object();
            }
            return false;
        }

        public void UpdateObjectInternal(IPhysicsObject obj, float quantum)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                adapter.PhysicsObj.UpdateObjectInternal(quantum);
            }
        }

        public bool HasCollision(IPhysicsObject obj)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.CollidingWithEnvironment;
            }
            return false;
        }

        public void ReportCollision(IPhysicsObject obj, IPhysicsObject target)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                // TODO: Implement ACE collision reporting
            }
        }

        public void ReportCollisionEnd(IPhysicsObject obj, IPhysicsObject target)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                // TODO: Implement ACE collision end reporting
            }
        }

        public void SetObjectGuid(IPhysicsObject obj, ObjectGuid guid)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                adapter.PhysicsObj.set_object_guid(guid);
            }
        }

        public void SetWeenieObject(IPhysicsObject obj, WorldObject worldObject)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                adapter.PhysicsObj.set_weenie_obj(new ACE.Server.Physics.Common.WeenieObject(worldObject));
            }
        }

        public bool IsPlayer(IPhysicsObject obj)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.IsPlayer;
            }
            return false;
        }

        public bool IsStatic(IPhysicsObject obj)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.State.HasFlag(ACE.Entity.Enum.PhysicsState.Static);
            }
            return false;
        }

        public bool IsMissile(IPhysicsObject obj)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.State.HasFlag(ACE.Entity.Enum.PhysicsState.Missile);
            }
            return false;
        }

        public bool IsEthereal(IPhysicsObject obj)
        {
            if (obj is ACEPhysicsObjectAdapter adapter)
            {
                return adapter.PhysicsObj.State.HasFlag(ACE.Entity.Enum.PhysicsState.Ethereal);
            }
            return false;
        }
    }

    /// <summary>
    /// Adapter for ACE physics objects to implement IPhysicsObject interface
    /// </summary>
    public class ACEPhysicsObjectAdapter : IPhysicsObject
    {
        // ACE physics object reference
        public PhysicsObj PhysicsObj { get; private set; }

        // Constructors
        public ACEPhysicsObjectAdapter(uint setupId, ObjectGuid objectId, bool isDynamic)
        {
            PhysicsObj = PhysicsObj.makeObject(setupId, objectId.Full, isDynamic);
        }

        public ACEPhysicsObjectAdapter(int? variationId)
        {
            PhysicsObj = new PhysicsObj(variationId);
        }

        public ACEPhysicsObjectAdapter(uint setupId, bool createParts)
        {
            PhysicsObj = new PhysicsObj(null);
            PhysicsObj.makeAnimObject(setupId, createParts);
        }

        public ACEPhysicsObjectAdapter(int numParts, Sphere sortingSphere, int? variationId)
        {
            PhysicsObj = PhysicsObj.makeParticleObject(numParts, sortingSphere, variationId);
        }

        // IPhysicsObject implementation
        public uint Id => PhysicsObj.ID;
        public ObjectGuid ObjectGuid => PhysicsObj.ObjID;
        public ACE.Entity.Position Position 
        { 
            get 
            {
                var acePos = PhysicsObj.Position;
                return new ACE.Entity.Position(acePos.ObjCellID, acePos.Frame.Origin.X, acePos.Frame.Origin.Y, acePos.Frame.Origin.Z,
                    acePos.Frame.Orientation.X, acePos.Frame.Orientation.Y, acePos.Frame.Orientation.Z, acePos.Frame.Orientation.W);
            }
            set 
            {
                var frame = new ACE.Server.Physics.Animation.AFrame();
                frame.Origin = new Vector3(value.PositionX, value.PositionY, value.PositionZ);
                frame.Orientation = new System.Numerics.Quaternion(value.RotationX, value.RotationY, value.RotationZ, value.RotationW);
                
                var acePos = new ACE.Server.Physics.Common.Position();
                acePos.ObjCellID = value.Cell;
                acePos.Frame = frame;
                acePos.Variation = value.Variation;
                
                PhysicsObj.Position = acePos;
            }
        }
        public ACE.Entity.Enum.PhysicsState State { get => PhysicsObj.State; set => PhysicsObj.State = value; }
        public Vector3 Velocity { get => PhysicsObj.Velocity; set => PhysicsObj.Velocity = value; }
        public Vector3 Acceleration { get => PhysicsObj.Acceleration; set => PhysicsObj.Acceleration = value; }
        public float Scale { get => PhysicsObj.Scale; set => PhysicsObj.Scale = value; }
        public bool IsActive => PhysicsObj.is_active();
        public bool IsAnimating => PhysicsObj.IsAnimating;
        public bool IsMovingOrAnimating => PhysicsObj.IsMovingOrAnimating;
        public bool IsPlayer => PhysicsObj.IsPlayer;
        public bool IsStatic => PhysicsObj.State.HasFlag(ACE.Entity.Enum.PhysicsState.Static);
        public bool IsMissile => PhysicsObj.State.HasFlag(ACE.Entity.Enum.PhysicsState.Missile);
        public bool IsEthereal => PhysicsObj.State.HasFlag(ACE.Entity.Enum.PhysicsState.Ethereal);

        public bool EnterWorld(ACE.Entity.Position position)
        {
            // Convert position and call ACE method
            var frame = new ACE.Server.Physics.Animation.AFrame();
            frame.Origin = new Vector3(position.PositionX, position.PositionY, position.PositionZ);
            frame.Orientation = new System.Numerics.Quaternion(position.RotationX, position.RotationY, position.RotationZ, position.RotationW);
            
            var acePos = new ACE.Server.Physics.Common.Position();
            acePos.ObjCellID = position.Cell;
            acePos.Frame = frame;
            acePos.Variation = position.Variation;
            
            return PhysicsObj.enter_world(acePos);
        }

        public void LeaveWorld()
        {
            PhysicsObj.leave_world();
        }

        public void Destroy()
        {
            PhysicsObj.DestroyObject();
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
            PhysicsObj.State = state;
        }

        public void SetPosition(ACE.Entity.Position position)
        {
            var frame = new ACE.Server.Physics.Animation.AFrame();
            frame.Origin = new Vector3(position.PositionX, position.PositionY, position.PositionZ);
            frame.Orientation = new System.Numerics.Quaternion(position.RotationX, position.RotationY, position.RotationZ, position.RotationW);
            
            var acePos = new ACE.Server.Physics.Common.Position();
            acePos.ObjCellID = position.Cell;
            acePos.Frame = frame;
            acePos.Variation = position.Variation;
            
            PhysicsObj.Position = acePos;
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
            PhysicsObj.set_weenie_obj(new ACE.Server.Physics.Common.WeenieObject(worldObject));
        }
    }
} 