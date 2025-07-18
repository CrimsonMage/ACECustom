using System;
using System.Numerics;
using ACE.Entity;

namespace ACE.Server.Physics.Alt
{
    // Add PhysicsState enum
    public enum PhysicsState
    {
        None = 0,
        Ethereal = 1 << 0,
        Static = 1 << 1,
        Dynamic = 1 << 2,
        Missile = 1 << 6,
        EdgeSlide = 1 << 7,
        LightingOn = 1 << 8,
        Gravity = 1 << 9,
        ReportCollisions = 1 << 10,
        FreeRotate = 1 << 11
    }

    public struct OBJECTINFO
    {
        public enum ObjectInfoEnum
        {
            DEFAULT_OI = 0x0,
            CONTACT_OI = 0x1,
            ON_WALKABLE_OI = 0x2,
            IS_VIEWER_OI = 0x4,
            PATH_CLIPPED_OI = 0x8,
            FREE_ROTATE_OI = 0x10,
            PERFECT_CLIP_OI = 0x40,
            IS_IMPENETRABLE = 0x80,
            IS_PLAYER = 0x100,
            EDGE_SLIDE = 0x200,
            IGNORE_CREATURES = 0x400,
            IS_PK = 0x800,
            IS_PKLITE = 0x1000,
            ACTIVE_OI = 0x2000,
            FORCE_ObjectInfoEnum_32_BIT = unchecked((int)0x7FFFFFFF)
        }

        public CPhysicsObj Object; // 0
        public int State; // 4
        public float Scale; // 8
        public float StepUpHeight; // 0xC
        public float StepDownHeight; // 0x10
        public int Ethereal; // 0x14
        public int StepDown; // 0x18
        public uint TargetID; // 0x1C

        // Additional properties for compatibility
        public uint ObjectID { get; set; }
        public Position Position { get; set; }
        public Vector3 Velocity { get; set; }
        public bool IsPlayer { get; set; }
        public bool IsStatic { get; set; }
        public bool IsEthereal { get; set; }
        public CSphere BoundingSphere { get; set; }

        // Methods to be implemented
        public void Init(CPhysicsObj obj, int objectState) 
        { 
            Object = obj;
            State = objectState;
            Scale = obj.Scale;
            StepUpHeight = obj.GetStepUpHeight();
            StepDownHeight = obj.GetStepDownHeight();
            // Fix: Use the correct enum type for physics state
            Ethereal = (obj.State & PhysicsState.Ethereal) != 0 ? 1 : 0;
            StepDown = (~(int)((uint)obj.State >> 6) & 1); // if not a missile MISSILE_PS
            
            var weenieObj = obj.WeenieObject;
            if (weenieObj != null)
            {
                // TODO: Implement these properties in WorldObject
                // if (weenieObj.IsImpenetrable)
                //     State |= (int)ObjectInfoEnum.IS_IMPENETRABLE;
                // if (weenieObj.IsPlayer)
                //     State |= (int)ObjectInfoEnum.IS_PLAYER;
                // if (weenieObj.IsPK)
                //     State |= (int)ObjectInfoEnum.IS_PK;
                // if (weenieObj.IsPKLite)
                //     State |= (int)ObjectInfoEnum.IS_PKLITE;

                // TargetID = weenieObj.GetPhysicsTargetID();
            }
        }
        
        public int MissileIgnore(CPhysicsObj collideObject) 
        { 
            // TODO: Implement missile ignore logic
            return 0; 
        }
        
        public float GetWalkableZ() 
        { 
            // TODO: Implement walkable Z calculation
            return Object?.Position?.PositionZ ?? 0; 
        }
        
        public bool IsValidWalkable(Vector normal) 
        { 
            // TODO: Implement walkable validation
            return PhysicsGlobals.IsWalkableNormal((Vector3)normal); 
        }
        
        public TransitionState ValidateWalkable(CSphere checkPos, Plane contactPlane, int isWater, float waterDepth, SPHEREPATH path, ACE.Server.Physics.Alt.CollisionInfo collisions, uint landCellId) 
        { 
            // TODO: Implement walkable validation logic
            return TransitionState.OK_TS; 
        }
    }
} 